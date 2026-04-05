using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Entities;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Domain.SensorReadings.Entities;
using HomeAssistant.Infrastructure.Messaging.Configuration;
using HomeAssistant.Infrastructure.Messaging.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HomeAssistant.Infrastructure.Sensors.Sensors.Providers;

/// <summary>
/// Development-only sensor provider that discovers configured pots from the database and generates
/// realistic mock sensor readings. Pots are discovered at startup and when new sensors are registered.
/// Optionally publishes readings to MQTT for local integration testing.
/// </summary>
public sealed class MockSensorProvider : ISensorProvider
{
    /// <summary>Default parameter profiles for generating realistic mock readings.</summary>
    private static readonly IReadOnlyDictionary<string, SensorSimulationProfile> DefaultProfiles =
        new Dictionary<string, SensorSimulationProfile>(StringComparer.OrdinalIgnoreCase)
        {
            ["tomato"] = new(52, 8, 22, 4),
            ["cucumber"] = new(58, 10, 23, 3.5),
            ["basil"] = new(47, 7, 24, 3),
            ["carrot"] = new(43, 6, 19, 2.5),
            ["lettuce"] = new(55, 8, 18, 2.8),
            ["pepper"] = new(50, 9, 24, 4.2),
        };

    private readonly ILogger<MockSensorProvider> _logger;
    private readonly IMqttClient? _mqttClient;
    private readonly IPotConfigurationRepository _potRepository;
    private readonly bool _publishToMqtt;

    /// <summary>Initialises the mock sensor provider with dynamic pot discovery from the database.</summary>
    public MockSensorProvider(
        ILogger<MockSensorProvider> logger,
        IPotConfigurationRepository potRepository,
        IMqttClient? mqttClient = null,
        IConfiguration? configuration = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _potRepository = potRepository ?? throw new ArgumentNullException(nameof(potRepository));
        _mqttClient = mqttClient;

        var options = new MqttClientOptions();
        configuration?.GetSection("Mqtt").Bind(options);
        _publishToMqtt = options.PublishMockReadings && _mqttClient is not null;

        _logger.LogInformation("MockSensorProvider initialized. Pots will be discovered from database.");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SensorReading>> GetLatestReadingsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Discover configured pots from database
        var configurations = await _potRepository.GetAllAsync(ct).ConfigureAwait(false);
        
        if (configurations.Count == 0)
        {
            _logger.LogDebug("No pots found in database. Returning empty readings.");
            return [];
        }

        var readings = new List<SensorReading>(configurations.Count);

        foreach (var config in configurations)
        {
            var reading = BuildReading(config, now);
            readings.Add(reading);
        }

        _logger.LogDebug("MockSensorProvider generated {ReadingCount} readings from {ConfigCount} discovered pots.", readings.Count, configurations.Count);

        if (_publishToMqtt && _mqttClient is not null)
        {
            await PublishMockReadingsToMqttAsync(readings, configurations, now, ct).ConfigureAwait(false);
        }

        return readings.AsReadOnly();
    }

    /// <summary>Generates a realistic mock sensor reading for a configured pot based on plant type and time-of-day simulation.</summary>
    /// <param name="config">The pot configuration with plant assignment.</param>
    /// <param name="now">The current timestamp for time-of-day calculations.</param>
    /// <returns>A <see cref="SensorReading"/> with simulated moisture and temperature values.</returns>
    private SensorReading BuildReading(PotConfiguration config, DateTimeOffset now)
    {
        // Use plant name from config to get simulation profile, fallback to defaults
        var plantName = config.CurrentSeeds.FirstOrDefault()?.PlantName ?? "tomato";
        var profile = DefaultProfiles.TryGetValue(plantName, out var p) ? p : DefaultProfiles["tomato"];

        // Create pseudo-stable position from pot ID for deterministic variance
        var potPosition = Math.Abs(config.PotId.GetHashCode() % 100) + 1;

        // Diurnal cycle: coolest near dawn, warmest late afternoon.
        var dayPhase = Math.Sin((now.TimeOfDay.TotalHours - 6) / 24d * 2d * Math.PI);

        // Pot-specific variance so pots do not move in perfect lockstep.
        var potPhase = Math.Sin((now.ToUnixTimeSeconds() / 37d) + potPosition);
        var moisture = profile.BaseMoisture + (potPhase * profile.MoistureSwing);

        // Simulate slow dry-down and occasional manual watering spikes.
        var minutesSinceEpoch = now.ToUnixTimeSeconds() / 60d;
        var dryDown = ((minutesSinceEpoch + potPosition * 11) % 120d) / 120d;
        moisture -= dryDown * 6d;
        if (((minutesSinceEpoch + potPosition * 13) % 97d) < 0.01d)
        {
            moisture += 7d;
        }

        var temperature = profile.BaseTemperatureC + (dayPhase * profile.TemperatureSwingC) + (potPhase * 0.7d);

        return new SensorReading
        {
            Id = Guid.NewGuid(),
            PotId = config.PotId,
            Timestamp = now,
            SoilMoisture = Math.Round(Math.Clamp(moisture, 20d, 85d), 1),
            TemperatureC = Math.Round(Math.Clamp(temperature, 12d, 35d), 1),
        };
    }

    private async Task PublishMockReadingsToMqttAsync(
        IReadOnlyList<SensorReading> readings,
        IReadOnlyList<PotConfiguration> configurations,
        DateTimeOffset now,
        CancellationToken ct)
    {
        try
        {
            var configByPotId = configurations.ToDictionary(c => c.PotId);

            foreach (var reading in readings)
            {
                if (!configByPotId.TryGetValue(reading.PotId, out var config))
                {
                    continue;
                }

                var seed = config.CurrentSeeds.FirstOrDefault();
                var payload = JsonSerializer.Serialize(new
                {
                    soilMoisture = reading.SoilMoisture,
                    temperatureC = reading.TemperatureC,
                    timestamp = reading.Timestamp.ToString("O"),
                    potId = reading.PotId,
                    roomName = config.RoomName,
                    plantName = seed?.PlantName ?? "unassigned",
                    seedName = seed?.SeedName ?? string.Empty,
                    readingWindow = now.ToString("yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture),
                });

                var topic = $"homeassistant/test/mock-sensors/{reading.PotId:N}";
                await _mqttClient!.PublishAsync(topic, payload, retainFlag: true, ct).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish mock readings to MQTT.");
        }
    }

    /// <summary>Simulation parameters for realistic sensor readings by plant type.</summary>
    /// <param name="BaseMoisture">Baseline soil moisture percentage.</param>
    /// <param name="MoistureSwing">Amplitude of moisture oscillation.</param>
    /// <param name="BaseTemperatureC">Baseline temperature in Celsius.</param>
    /// <param name="TemperatureSwingC">Amplitude of temperature oscillation.</param>
    private sealed record SensorSimulationProfile(
        double BaseMoisture,
        double MoistureSwing,
        double BaseTemperatureC,
        double TemperatureSwingC);
}
