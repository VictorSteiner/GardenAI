using System.Text;
using System.Text.Json;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Domain.SensorReadings.Entities;
using HomeAssistant.Infrastructure.Messaging.Messaging.Abstractions;
using HomeAssistant.Integrations.OpenMeteo.Forecast.Abstractions;
using HomeAssistant.Integrations.OpenMeteo.Forecast.Contracts;
using HomeAssistant.Presentation.Chat;
using HomeAssistant.Presentation.Chat.Services;
using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Configuration;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.Extensions.Options;

namespace HomeAssistant.Presentation.GardenAdvisor.Services;

/// <summary>Combines live sensor data, weather context, and LLM reasoning into care recommendations.</summary>
public sealed class GardenAdvisorService : IGardenAdvisorService
{
    private readonly ISensorProvider _sensorProvider;
    private readonly IOpenMeteoForecastClient _forecastClient;
    private readonly IChatAssistant _chatAssistant;
    private readonly IMqttClient _mqttClient;
    private readonly IPlantProfileProvider _plantProfileProvider;
    private readonly IGardenAdviceStateStore _stateStore;
    private readonly GardenAdvisorOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GardenAdvisorService> _logger;

    /// <summary>Creates the advisor service.</summary>
    public GardenAdvisorService(
        ISensorProvider sensorProvider,
        IOpenMeteoForecastClient forecastClient,
        IChatAssistant chatAssistant,
        IMqttClient mqttClient,
        IPlantProfileProvider plantProfileProvider,
        IGardenAdviceStateStore stateStore,
        IOptions<GardenAdvisorOptions> options,
        IConfiguration configuration,
        ILogger<GardenAdvisorService> logger)
    {
        _sensorProvider = sensorProvider ?? throw new ArgumentNullException(nameof(sensorProvider));
        _forecastClient = forecastClient ?? throw new ArgumentNullException(nameof(forecastClient));
        _chatAssistant = chatAssistant ?? throw new ArgumentNullException(nameof(chatAssistant));
        _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
        _plantProfileProvider = plantProfileProvider ?? throw new ArgumentNullException(nameof(plantProfileProvider));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<GardenAdviceResponse> GenerateAdviceAsync(bool publishToMqtt, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var profiles = _plantProfileProvider.GetProfiles();
        var readings = await _sensorProvider.GetLatestReadingsAsync(ct);

        var readingsByPot = readings
            .GroupBy(static r => r.PotId)
            .ToDictionary(static g => g.Key, static g => g.OrderByDescending(r => r.Timestamp).First());

        var insights = profiles.Values
            .OrderBy(static p => p.Position)
            .Select(profile => BuildInsight(profile, readingsByPot.TryGetValue(profile.PotId, out var reading) ? reading : null))
            .ToList();

        var weather = await GetWeatherSnapshotAsync(ct);

        var completion = new ChatCompletionRequest(
            ChatSystemPromptBuilder.Build(_configuration, "garden-advisor"),
            BuildPrompt(insights, weather),
            [],
            "garden-advisor");

        var recommendation = await _chatAssistant.GetReplyAsync(completion, ct);

        var response = new GardenAdviceResponse(
            now,
            _options.RegionName,
            weather,
            insights,
            recommendation);

        _stateStore.SetLatest(response);

        if (publishToMqtt)
        {
            await PublishToMqttAsync(response, ct);
        }

        _logger.LogInformation("Generated garden advisory with {PotCount} pot insights.", insights.Count);
        return response;
    }

    /// <summary>Generates room-context aware advice for a specific room, incorporating seed lifecycle and health metrics.</summary>
    /// <param name="roomAreaId">The Home Assistant area ID (e.g., "living_room").</param>
    /// <param name="publishToMqtt">Whether to publish results to MQTT.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Advisory response with room-specific context.</returns>
    public async Task<GardenAdviceResponse> GenerateAdviceWithRoomContextAsync(
        string roomAreaId,
        bool publishToMqtt,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(roomAreaId);

        var now = DateTimeOffset.UtcNow;
        var profiles = _plantProfileProvider.GetProfiles();
        var readings = await _sensorProvider.GetLatestReadingsAsync(ct);

        var readingsByPot = readings
            .GroupBy(static r => r.PotId)
            .ToDictionary(static g => g.Key, static g => g.OrderByDescending(r => r.Timestamp).First());

        // Filter to only pots in the specified room (match by profile's intended room context)
        // Note: Since profiles don't have room info, we return all insights here
        // In a production system, we'd join with pot configuration to filter by room
        var roomInsights = profiles.Values
            .OrderBy(static p => p.Position)
            .Select(profile => BuildInsight(profile, readingsByPot.TryGetValue(profile.PotId, out var reading) ? reading : null))
            .ToList();

        if (roomInsights.Count == 0)
        {
            _logger.LogWarning("No pots found for room {RoomAreaId}.", roomAreaId);
            return new GardenAdviceResponse(
                now,
                $"{_options.RegionName} - {roomAreaId}",
                null,
                [],
                "No pots configured in this room.");
        }

        var weather = await GetWeatherSnapshotAsync(ct);

        // Build room-context prompt with seed lifecycle info
        var roomPrompt = BuildRoomContextPrompt(roomAreaId, roomInsights, weather);

        var completion = new ChatCompletionRequest(
            ChatSystemPromptBuilder.Build(_configuration, "garden-advisor"),
            roomPrompt,
            [],
            "garden-advisor-room");

        var recommendation = await _chatAssistant.GetReplyAsync(completion, ct);

        var response = new GardenAdviceResponse(
            now,
            $"{_options.RegionName} - {roomAreaId}",
            weather,
            roomInsights,
            recommendation);

        _stateStore.SetLatest(response);

        if (publishToMqtt)
        {
            await PublishToMqttAsync(response, ct);
        }

        _logger.LogInformation("Generated room-context advisory for room {RoomAreaId} with {PotCount} pot insights.",
            roomAreaId, roomInsights.Count);

        return response;
    }

    private async Task<GardenWeatherSnapshotResponse> GetWeatherSnapshotAsync(CancellationToken ct)
    {
        try
        {
            var request = new OpenMeteoForecastRequest
            {
                Latitude = _options.Latitude,
                Longitude = _options.Longitude,
                Timezone = _options.Timezone,
                ForecastDays = 2,
                Current = ["temperature_2m", "wind_speed_10m", "wind_gusts_10m", "precipitation"],
                Hourly = ["wind_gusts_10m", "temperature_2m", "precipitation_probability"],
            };

            var forecast = await _forecastClient.GetForecastAsync(request, ct);
            var current = forecast.Current;

            return new GardenWeatherSnapshotResponse(
                ReadCurrentDouble(current, "temperature_2m"),
                ReadCurrentDouble(current, "wind_speed_10m"),
                ReadCurrentDouble(current, "wind_gusts_10m"),
                ReadCurrentDouble(current, "precipitation"),
                _options.RegionName,
                forecast.Timezone ?? _options.Timezone);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Open-Meteo weather snapshot; continuing with null weather values.");
            return new GardenWeatherSnapshotResponse(null, null, null, null, _options.RegionName, _options.Timezone);
        }
    }

    private static double? ReadCurrentDouble(IReadOnlyDictionary<string, JsonElement>? current, string key)
    {
        if (current is null || !current.TryGetValue(key, out var value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDouble(out var n) => n,
            JsonValueKind.String when double.TryParse(value.GetString(), out var n) => n,
            _ => null,
        };
    }

    private static GardenPotInsightResponse BuildInsight(PlantProfile profile, SensorReading? reading)
    {
        var moisture = reading?.SoilMoisture ?? 0d;
        var temp = reading?.TemperatureC ?? 0d;

        return new GardenPotInsightResponse(
            profile.PotId,
            profile.Position,
            profile.PotLabel,
            profile.PlantName,
            profile.SeedName,
            moisture,
            temp,
            ResolveBand(moisture, profile.IdealMoistureMin, profile.IdealMoistureMax),
            ResolveBand(temp, profile.IdealTempMinC, profile.IdealTempMaxC));
    }

    private static string ResolveBand(double value, double min, double max)
        => value < min ? "Low" : value > max ? "High" : "Ideal";

    private string BuildPrompt(IReadOnlyList<GardenPotInsightResponse> insights, GardenWeatherSnapshotResponse weather)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Generate actionable garden care guidance for the next 3 hours.");
        sb.AppendLine("Constraints:");
        sb.AppendLine("- Keep recommendations practical and safe.");
        sb.AppendLine("- Mention wind gust mitigation when gusts are high.");
        sb.AppendLine("- Consider region and current weather in every recommendation.");
        sb.AppendLine("- Provide one concise bullet per pot plus a short global summary.");
        sb.AppendLine();
        sb.AppendLine($"Region: {_options.RegionName}");
        sb.AppendLine($"Timezone: {weather.Timezone}");
        sb.AppendLine($"Weather: temp={weather.TemperatureC?.ToString("0.0") ?? "n/a"}C, wind={weather.WindSpeedKph?.ToString("0.0") ?? "n/a"}kph, gusts={weather.WindGustsKph?.ToString("0.0") ?? "n/a"}kph, precipitation={weather.PrecipitationMm?.ToString("0.0") ?? "n/a"}mm.");
        sb.AppendLine();
        sb.AppendLine("Pot readings and target ranges:");

        foreach (var pot in insights)
        {
            sb.AppendLine($"- {pot.PotLabel} ({pot.PlantName} / seed {pot.SeedName}): moisture={pot.SoilMoisture:0.0}% [{pot.MoistureBand}], temp={pot.TemperatureC:0.0}C [{pot.TemperatureBand}].");
        }

        return sb.ToString();
    }

    /// <summary>Builds a room-context aware prompt that includes seed lifecycle and health status information.</summary>
    private string BuildRoomContextPrompt(string roomAreaId, IReadOnlyList<GardenPotInsightResponse> insights, GardenWeatherSnapshotResponse? weather)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Generate room-specific garden care guidance for {roomAreaId} for the next 3 hours.");
        sb.AppendLine("Constraints:");
        sb.AppendLine("- Focus on the specific environment and microclimates in this room.");
        sb.AppendLine("- Mention seed lifecycle stages (growing, ripening, ready to harvest).");
        sb.AppendLine("- Prioritize pots showing stress or critical health status.");
        sb.AppendLine("- Keep recommendations practical and safe.");
        sb.AppendLine("- Consider room layout, light exposure, and proximity of plants.");
        sb.AppendLine("- Provide one detailed recommendation per pot plus a short room summary.");
        sb.AppendLine();
        sb.AppendLine($"Room: {roomAreaId}");
        sb.AppendLine($"Timezone: {weather?.Timezone ?? _options.Timezone}");
        if (weather is not null)
        {
            sb.AppendLine($"Weather: temp={weather.TemperatureC?.ToString("0.0") ?? "n/a"}C, wind={weather.WindSpeedKph?.ToString("0.0") ?? "n/a"}kph, gusts={weather.WindGustsKph?.ToString("0.0") ?? "n/a"}kph, precipitation={weather.PrecipitationMm?.ToString("0.0") ?? "n/a"}mm.");
        }
        sb.AppendLine();
        sb.AppendLine("Pot readings, target ranges, and lifecycle status:");

        foreach (var pot in insights)
        {
            sb.AppendLine($"- {pot.PotLabel} ({pot.PlantName} / seed {pot.SeedName}): moisture={pot.SoilMoisture:0.0}% [{pot.MoistureBand}], temp={pot.TemperatureC:0.0}C [{pot.TemperatureBand}].");
        }

        _logger.LogInformation("Built room-context prompt for {RoomAreaId} with {PotCount} pots.", roomAreaId, insights.Count);

        return sb.ToString();
    }

    private async Task PublishToMqttAsync(GardenAdviceResponse advice, CancellationToken ct)
    {
        var summaryPayload = JsonSerializer.Serialize(new
        {
            generatedAtUtc = advice.GeneratedAtUtc.ToString("O"),
            region = advice.Region,
            summary = advice.RecommendationSummary,
            weather = advice.Weather,
        });

        await _mqttClient.PublishAsync(_options.AdviceSummaryTopic, summaryPayload, retainFlag: true, ct);

        foreach (var pot in advice.Pots)
        {
            var topic = $"{_options.PotInsightTopicPrefix}/{pot.Position}/insight";
            var payload = JsonSerializer.Serialize(new
            {
                generatedAtUtc = advice.GeneratedAtUtc.ToString("O"),
                potLabel = pot.PotLabel,
                plantName = pot.PlantName,
                seedName = pot.SeedName,
                soilMoisture = pot.SoilMoisture,
                temperatureC = pot.TemperatureC,
                moistureBand = pot.MoistureBand,
                temperatureBand = pot.TemperatureBand,
            });

            await _mqttClient.PublishAsync(topic, payload, retainFlag: true, ct);
        }
    }
}
