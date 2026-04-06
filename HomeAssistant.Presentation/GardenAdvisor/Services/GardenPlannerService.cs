using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;
using HomeAssistant.Application.Chat.Abstractions;
using HomeAssistant.Application.Chat.Contracts;
using HomeAssistant.Application.GardenAdvisor.Abstractions;
using HomeAssistant.Application.GardenAdvisor.Configuration;
using HomeAssistant.Application.GardenAdvisor.Contracts;
using HomeAssistant.Application.Messaging.Abstractions;
using HomeAssistant.Application.PotConfigurations.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.Extensions.Options;

namespace HomeAssistant.Presentation.GardenAdvisor.Services;

/// <summary>
/// Garden planner chat service using Ollama function/tool calling.
/// Defines typed tools for all garden actions — no regex parsing required.
/// </summary>
public sealed class GardenPlannerService : IGardenPlannerService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IPotConfigurationRepository _potRepository;
    private readonly IPotIdentityMapProvider _potIdentityMapProvider;
    private readonly IGardenPlannerFunctionService _functionService;
    private readonly IChatAssistant _chatAssistant;
    private readonly IMqttClient _mqttClient;
    private readonly IGardenPlannerHistoryStore _historyStore;
    private readonly GardenAdvisorOptions _options;
    private readonly ILogger<GardenPlannerService> _logger;
    private readonly Counter<int> _messagesReceivedCounter;
    private readonly Counter<int> _actionsExecutedCounter;

    /// <summary>Creates a new <see cref="GardenPlannerService"/>.</summary>
    public GardenPlannerService(
        IPotConfigurationRepository potRepository,
        IPotIdentityMapProvider potIdentityMapProvider,
        IGardenPlannerFunctionService functionService,
        IChatAssistant chatAssistant,
        IMqttClient mqttClient,
        IGardenPlannerHistoryStore historyStore,
        IOptions<GardenAdvisorOptions> options,
        ILogger<GardenPlannerService> logger)
    {
        _potRepository = potRepository ?? throw new ArgumentNullException(nameof(potRepository));
        _potIdentityMapProvider = potIdentityMapProvider ?? throw new ArgumentNullException(nameof(potIdentityMapProvider));
        _functionService = functionService ?? throw new ArgumentNullException(nameof(functionService));
        _chatAssistant = chatAssistant ?? throw new ArgumentNullException(nameof(chatAssistant));
        _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
        _historyStore = historyStore ?? throw new ArgumentNullException(nameof(historyStore));
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var meter = new Meter("HomeAssistant.Presentation.GardenPlanner");
        _messagesReceivedCounter = meter.CreateCounter<int>("garden_planner.messages_received");
        _actionsExecutedCounter = meter.CreateCounter<int>("garden_planner.actions_executed");
    }

    /// <inheritdoc/>
    public async Task<GardenPlannerChatResponse> ChatAsync(string message, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _messagesReceivedCounter.Add(1);
        _logger.LogInformation("Garden planner received message ({Length} chars).", message.Length);

        _historyStore.AddMessage("user", message);

        var configs = await _potRepository.GetAllAsync(ct);
        var systemPrompt = await BuildSystemPromptAsync(configs, ct);

        var fullHistory = _historyStore.GetHistory();
        var priorMessages = fullHistory.Count > 0
            ? fullHistory.Take(fullHistory.Count - 1)
            : [];

        var history = priorMessages
            .TakeLast(20)
            .Select(m => new ChatHistoryMessage(m.Role, m.Content))
            .ToList()
            .AsReadOnly();

        var result = await _chatAssistant.GetAgenticReplyAsync(
            systemPrompt,
            history,
            message,
            BuildToolDefinitions(),
            ExecuteToolAsync,
            maxIterations: Math.Clamp(_options.PlannerMaxIterations, 1, 5),
            ct);

        _historyStore.AddMessage("assistant", result.FinalReply);

        var actionsExecuted = result.ExecutedCalls.Select(FormatActionDescription).ToList().AsReadOnly();
        if (actionsExecuted.Count > 0)
            _actionsExecutedCounter.Add(actionsExecuted.Count);

        await PublishResponseAsync(result.FinalReply, actionsExecuted, ct);
        await PublishHistoryAsync(ct);

        _logger.LogInformation("Garden planner turn complete. Actions: {Count}.", actionsExecuted.Count);
        return new GardenPlannerChatResponse(result.FinalReply, actionsExecuted, DateTimeOffset.UtcNow);
    }

    // ── System prompt ─────────────────────────────────────────────────────

    private async Task<string> BuildSystemPromptAsync(
        IReadOnlyList<Domain.PotConfigurations.Entities.PotConfiguration> configs,
        CancellationToken ct)
    {
        var configByPot = configs.ToDictionary(c => c.PotId);
        var map = await _potIdentityMapProvider.GetMapAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("You are a knowledgeable garden planning assistant for a smart home system.");
        sb.AppendLine("Help the user plan plantings, suggest seeds, give care advice, and track what is growing.");
        sb.AppendLine("Use the available tools when the user wants to plant, update, or query a pot.");
        sb.AppendLine($"Today: {DateTimeOffset.UtcNow:yyyy-MM-dd}  |  Region: {_options.RegionName}");
        sb.AppendLine();
        sb.AppendLine("| Pot | Plant | Seed | Status | Room |");
        sb.AppendLine("|-----|-------|------|--------|------|");
        foreach (var (num, potId) in map.OrderBy(kv => kv.Key))
        {
            configByPot.TryGetValue(potId, out var cfg);
            var seed = cfg?.CurrentSeeds.FirstOrDefault(s => s.Status == "growing") ?? cfg?.CurrentSeeds.FirstOrDefault();
            sb.AppendLine($"| {num} | {seed?.PlantName ?? "empty"} | {seed?.SeedName ?? "—"} | {seed?.Status ?? "—"} | {cfg?.RoomName ?? "unassigned"} |");
        }
        sb.AppendLine("Rooms: living_room, balcony, greenhouse  |  Statuses: growing, mature, harvested, removed");
        return sb.ToString();
    }

    // ── Tool definitions ──────────────────────────────────────────────────

    private static IReadOnlyList<ChatToolDefinition> BuildToolDefinitions() =>
    [
        new("save_pot_configuration",
            "Plant a seed in a pot – saves plant/seed, room, and lifecycle status.",
            new Dictionary<string, ChatToolParameterSchema>
            {
                ["pot_number"]  = new("integer", "Configured pot number from PotIdentityMap."),
                ["plant_name"]  = new("string",  "Common plant name, e.g. Tomato, Basil."),
                ["seed_name"]   = new("string",  "Cultivar name, e.g. Moneymaker, Genovese."),
                ["room_area_id"]= new("string",  "Room ID.", ["living_room","balcony","greenhouse"]),
                ["status"]      = new("string",  "Lifecycle status.", ["growing","mature","harvested","removed"]),
                ["planted_date"]= new("string",  "ISO 8601 sowing date, optional."),
                ["notes"]       = new("string",  "Optional notes."),
            },
            ["pot_number","plant_name","seed_name","room_area_id"]),

        new("update_seed_status",
            "Update the lifecycle status of the seed currently in a pot.",
            new Dictionary<string, ChatToolParameterSchema>
            {
                ["pot_number"] = new("integer", "Configured pot number from PotIdentityMap."),
                ["new_status"] = new("string",  "New status.", ["growing","mature","harvested","removed"]),
            },
            ["pot_number","new_status"]),

        new("get_pot_status",
            "Get the current planting config and latest sensor reading for a pot.",
            new Dictionary<string, ChatToolParameterSchema>
            {
                ["pot_number"] = new("integer", "Configured pot number from PotIdentityMap."),
            },
            ["pot_number"]),

        new("get_all_pots_status",
            "Overview of all configured pots: plants, statuses, and sensor readings.",
            new Dictionary<string, ChatToolParameterSchema>(), []),

        new("get_sensor_readings",
            "Latest soil moisture and temperature for a specific pot.",
            new Dictionary<string, ChatToolParameterSchema>
            {
                ["pot_number"] = new("integer", "Configured pot number from PotIdentityMap."),
            },
            ["pot_number"]),

        new("get_available_rooms",
            "List the available Home Assistant rooms/areas that pots can be assigned to.",
            new Dictionary<string, ChatToolParameterSchema>(), []),

        new("get_room_summary",
            "Get aggregated readiness and pot counts for a room.",
            new Dictionary<string, ChatToolParameterSchema>
            {
                ["room_area_id"] = new("string", "Room ID.", ["living_room", "balcony", "greenhouse"]),
            },
            ["room_area_id"]),

        new("get_dashboard_snapshot",
            "Get the dashboard-wide snapshot across all rooms and pots.",
            new Dictionary<string, ChatToolParameterSchema>(), []),

        new("get_harvest_readiness",
            "Get harvest-readiness details for all seeds or optionally by status.",
            new Dictionary<string, ChatToolParameterSchema>
            {
                ["filter_by_status"] = new("string", "Optional status filter.", ["growing", "mature", "harvested", "removed"]),
            },
            []),

        new("get_latest_garden_advice",
            "Read the latest generated garden advice snapshot.",
            new Dictionary<string, ChatToolParameterSchema>(), []),

        new("generate_garden_advice",
            "Generate a fresh garden advice summary and optionally publish it to MQTT.",
            new Dictionary<string, ChatToolParameterSchema>
            {
                ["publish_to_mqtt"] = new("boolean", "Whether to publish the generated advice to MQTT."),
            },
            []),

        new("clear_planner_history",
            "Clear the in-memory planner chat history when the user asks to reset the conversation.",
            new Dictionary<string, ChatToolParameterSchema>(), []),
    ];

    // ── Tool executor dispatcher ──────────────────────────────────────────

    private async Task<string> ExecuteToolAsync(ChatFunctionCall call, CancellationToken ct)
    {
        _logger.LogInformation("Executing tool {Tool}.", call.FunctionName);
        try
        {
            return call.FunctionName switch
            {
                "save_pot_configuration" => await _functionService.SavePotConfigurationAsync(DeserializeArgs<SavePlannerPotConfigurationFunctionRequest>(call.ArgumentsJson), ct),
                "update_seed_status"     => await _functionService.UpdateSeedStatusAsync(DeserializeArgs<UpdatePlannerSeedStatusFunctionRequest>(call.ArgumentsJson), ct),
                "get_pot_status"         => await _functionService.GetPotStatusAsync(DeserializeArgs<PotNumberFunctionRequest>(call.ArgumentsJson), ct),
                "get_all_pots_status"    => await _functionService.GetAllPotsStatusAsync(ct),
                "get_sensor_readings"    => await _functionService.GetSensorReadingsAsync(DeserializeArgs<PotNumberFunctionRequest>(call.ArgumentsJson), ct),
                "get_available_rooms"    => FormatRooms(await _functionService.GetAvailableRoomsAsync(ct)),
                "get_room_summary"       => FormatRoomSummary(await _functionService.GetRoomSummaryAsync(DeserializeArgs<RoomAreaFunctionRequest>(call.ArgumentsJson), ct)),
                "get_dashboard_snapshot" => FormatDashboard(await _functionService.GetDashboardAsync(ct)),
                "get_harvest_readiness"  => FormatHarvestReadiness(await _functionService.GetHarvestReadinessAsync(new HarvestReadinessFunctionRequest(DeserializeOptionalStatus(call.ArgumentsJson, "filter_by_status")), ct)),
                "get_latest_garden_advice" => FormatLatestAdvice(_functionService.GetLatestAdvice()),
                "generate_garden_advice" => FormatAdvice(await _functionService.GenerateAdviceAsync(new GeneratePlannerAdviceFunctionRequest(DeserializeOptionalBoolean(call.ArgumentsJson, "publish_to_mqtt") ?? true), ct)),
                "clear_planner_history"  => _functionService.ClearPlannerHistory(),
                _                        => $"Unknown tool '{call.FunctionName}'.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool {Tool} failed.", call.FunctionName);
            return $"Error in {call.FunctionName}: {ex.Message}";
        }
    }

    private static T DeserializeArgs<T>(string argumentsJson)
        where T : class
        => JsonSerializer.Deserialize<T>(argumentsJson, JsonOptions)
           ?? throw new InvalidOperationException($"Failed to deserialize tool arguments into {typeof(T).Name}.");

    private static string? DeserializeOptionalStatus(string argumentsJson, string propertyName)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        return doc.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static bool? DeserializeOptionalBoolean(string argumentsJson, string propertyName)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        return doc.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : null;
    }

    private static string FormatRooms(IReadOnlyList<RoomResponse> rooms)
        => rooms.Count == 0
            ? "No rooms are currently available."
            : $"Available rooms: {string.Join(", ", rooms.Select(r => $"{r.AreaName} ({r.AreaId})"))}.";

    private static string FormatRoomSummary(RoomSummaryResponse summary)
        => $"Room {summary.RoomName} ({summary.RoomAreaId}): {summary.PotCount} pots, {summary.ActiveSeedCount} active seeds, average readiness {summary.AverageReadiness}, health {summary.HealthStatus}, ready now {summary.ReadyToHarvestCount}, ripening {summary.RipeningCount}.";

    private static string FormatDashboard(DashboardAggregationResponse dashboard)
        => $"Dashboard snapshot: {dashboard.Rooms.Count} rooms, overall health {dashboard.OverallHealthStatus}, ready to harvest {dashboard.ReadyToHarvestCount}, ripening {dashboard.RipeningCount}, growing {dashboard.GrowingCount}, critical pots {dashboard.CriticalPotsCount}.";

    private static string FormatHarvestReadiness(IReadOnlyList<HarvestReadinessResponse> items)
        => items.Count == 0
            ? "No harvest-readiness items were found."
            : string.Join(" ", items.Select(i => $"{i.PlantName}/{i.SeedName} in pot {i.PotId}: score {i.ReadinessScore}, category {i.ReadinessCategory}."));

    private static string FormatLatestAdvice(HomeAssistant.Application.GardenAdvisor.Contracts.GardenAdviceResponse? advice)
        => advice is null
            ? "No garden advice has been generated yet."
            : $"Latest advice ({advice.GeneratedAtUtc:O}): {advice.RecommendationSummary}";

    private static string FormatAdvice(HomeAssistant.Application.GardenAdvisor.Contracts.GardenAdviceResponse advice)
        => $"Generated advice ({advice.GeneratedAtUtc:O}): {advice.RecommendationSummary}";

    private static string FormatActionDescription(ChatFunctionCall call)
    {
        try
        {
            using var doc = JsonDocument.Parse(call.ArgumentsJson);
            var r   = doc.RootElement;
            var pot = r.TryGetProperty("pot_number", out var p) ? p.GetInt32().ToString() : "?";
            return call.FunctionName switch
            {
                "save_pot_configuration" =>
                    $"Planted {(r.TryGetProperty("plant_name", out var pl) ? pl.GetString() : "?")} " +
                    $"({(r.TryGetProperty("seed_name", out var sn) ? sn.GetString() : "?")}) in Pot {pot}",
                "update_seed_status" =>
                    $"Updated Pot {pot} → {(r.TryGetProperty("new_status", out var ns) ? ns.GetString() : "?")}",
                "get_pot_status"      => $"Queried Pot {pot}",
                "get_all_pots_status" => "Queried all pots",
                "get_sensor_readings" => $"Read sensors Pot {pot}",
                "get_available_rooms" => "Listed available rooms",
                "get_room_summary"    => $"Queried room {(r.TryGetProperty("room_area_id", out var room) ? room.GetString() : "?")}",
                "get_dashboard_snapshot" => "Queried dashboard snapshot",
                "get_harvest_readiness" => "Queried harvest readiness",
                "get_latest_garden_advice" => "Read latest garden advice",
                "generate_garden_advice" => "Generated fresh garden advice",
                "clear_planner_history" => "Cleared planner history",
                _ => call.FunctionName,
            };
        }
        catch { return call.FunctionName; }
    }

    // ── MQTT publish ──────────────────────────────────────────────────────

    private async Task PublishResponseAsync(string reply, IReadOnlyList<string> actions, CancellationToken ct)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { reply, actionsExecuted = actions, generatedAtUtc = DateTimeOffset.UtcNow.ToString("O") });
            await _mqttClient.PublishAsync(_options.PlannerResponseTopic, payload, retainFlag: true, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "MQTT publish failed (response)."); }
    }

    private async Task PublishHistoryAsync(CancellationToken ct)
    {
        try
        {
            var history = _historyStore.GetHistory();
            var payload = JsonSerializer.Serialize(new
            {
                messageCount = history.Count,
                messages = history.Select(m => new { role = m.Role, content = m.Content, timestamp = m.Timestamp.ToString("O") }),
            });
            await _mqttClient.PublishAsync(_options.PlannerHistoryTopic, payload, retainFlag: true, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "MQTT publish failed (history)."); }
    }
}

