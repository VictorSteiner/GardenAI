using System.ComponentModel;
using System.Text;
using HomeAssistant.Application.Dispatching;
using HomeAssistant.Application.Maintenance.Commands.ResetPersistedData;
using HomeAssistant.Application.PotConfigurations.Commands;
using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Application.PotConfigurations.Queries;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace HomeAssistant.Infrastructure.HomeAssistant.Protocol.Functions;

/// <summary>Semantic Kernel tool methods used internally by the garden planner.</summary>
public sealed class GardenPlannerKernelFunctions
{
    private static readonly IReadOnlyDictionary<int, Guid> PotNumberToId = new Dictionary<int, Guid>
    {
        [1] = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        [2] = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        [3] = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        [4] = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        [5] = Guid.Parse("55555555-5555-5555-5555-555555555555"),
        [6] = Guid.Parse("66666666-6666-6666-6666-666666666666"),
    };

    private readonly IPotConfigurationRepository _potRepository;
    private readonly ISensorReadingRepository _sensorRepository;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryHandler<GetDashboardAggregationQuery, DashboardAggregationDto> _dashboardHandler;
    private readonly IQueryHandler<GetRoomSummaryQuery, RoomSummaryDto> _roomSummaryHandler;
    private readonly IQueryHandler<GetHarvestReadinessQuery, IReadOnlyList<HarvestReadinessDto>> _harvestReadinessHandler;
    private readonly IHomeAssistantAreaProvider _areaProvider;
    private readonly ILogger<GardenPlannerKernelFunctions> _logger;

    /// <summary>Creates a new <see cref="GardenPlannerKernelFunctions"/> instance.</summary>
    public GardenPlannerKernelFunctions(
        IPotConfigurationRepository potRepository,
        ISensorReadingRepository sensorRepository,
        ICommandDispatcher commandDispatcher,
        IQueryHandler<GetDashboardAggregationQuery, DashboardAggregationDto> dashboardHandler,
        IQueryHandler<GetRoomSummaryQuery, RoomSummaryDto> roomSummaryHandler,
        IQueryHandler<GetHarvestReadinessQuery, IReadOnlyList<HarvestReadinessDto>> harvestReadinessHandler,
        IHomeAssistantAreaProvider areaProvider,
        ILogger<GardenPlannerKernelFunctions> logger)
    {
        _potRepository = potRepository ?? throw new ArgumentNullException(nameof(potRepository));
        _sensorRepository = sensorRepository ?? throw new ArgumentNullException(nameof(sensorRepository));
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        _dashboardHandler = dashboardHandler ?? throw new ArgumentNullException(nameof(dashboardHandler));
        _roomSummaryHandler = roomSummaryHandler ?? throw new ArgumentNullException(nameof(roomSummaryHandler));
        _harvestReadinessHandler = harvestReadinessHandler ?? throw new ArgumentNullException(nameof(harvestReadinessHandler));
        _areaProvider = areaProvider ?? throw new ArgumentNullException(nameof(areaProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [KernelFunction("save_pot_configuration")]
    [Description("Plant a seed in a pot by saving plant, seed, room assignment, lifecycle status, and optional planting metadata.")]
    public async Task<string> SavePotConfigurationAsync(
        [Description("Pot number in the range 1 to 6.")] int pot_number,
        [Description("Common plant name, for example Tomato or Basil.")] string plant_name,
        [Description("Seed or cultivar name, for example Moneymaker or Genovese.")] string seed_name,
        [Description("Home Assistant room area identifier, for example living_room, balcony, or greenhouse.")] string room_area_id,
        [Description("Lifecycle status. Use growing, mature, harvested, or removed.")] string status = "growing",
        [Description("ISO 8601 planted date, optional.")] DateTimeOffset? planted_date = null,
        [Description("Optional notes about the planting.")] string? notes = null,
        CancellationToken ct = default)
    {
        if (!TryResolvePotId(pot_number, out var potId))
        {
            return "Invalid pot number. Must be 1–6.";
        }

        if (string.IsNullOrWhiteSpace(plant_name) || string.IsNullOrWhiteSpace(seed_name) || string.IsNullOrWhiteSpace(room_area_id))
        {
            return "Pot number, plant name, seed name, and room area id are required.";
        }

        var roomName = await ResolveRoomNameAsync(room_area_id, ct).ConfigureAwait(false);
        await _commandDispatcher.DispatchAsync(
            new SavePotConfigurationCommand(
                potId,
                new SavePotConfigurationRequest(
                    room_area_id,
                    roomName,
                    [new SeedAssignmentRequest(
                        plant_name,
                        seed_name,
                        planted_date ?? DateTimeOffset.UtcNow,
                        null,
                        status,
                        notes)])),
            ct).ConfigureAwait(false);

        _logger.LogInformation("Saved pot configuration via Semantic Kernel function for pot {PotNumber}.", pot_number);
        return $"Saved {plant_name} ({seed_name}) in pot {pot_number} for room {roomName} with status {status}.";
    }

    [KernelFunction("update_seed_status")]
    [Description("Update the lifecycle status of the active seed currently assigned to a pot.")]
    public async Task<string> UpdateSeedStatusAsync(
        [Description("Pot number in the range 1 to 6.")] int pot_number,
        [Description("New lifecycle status. Use growing, mature, harvested, or removed.")] string new_status,
        CancellationToken ct = default)
    {
        if (!TryResolvePotId(pot_number, out var potId))
        {
            return "Invalid pot number. Must be 1–6.";
        }

        var config = await _potRepository.GetByPotIdAsync(potId, ct).ConfigureAwait(false);
        if (config is null)
        {
            return $"Pot {pot_number} has no configuration.";
        }

        var seed = config.CurrentSeeds.FirstOrDefault(s => s.Status == "growing") ?? config.CurrentSeeds.FirstOrDefault();
        if (seed is null)
        {
            return $"Pot {pot_number} has no seeds to update.";
        }

        await _commandDispatcher.DispatchAsync(new UpdateSeedStatusCommand(potId, seed.Id, new_status), ct).ConfigureAwait(false);
        return $"Updated pot {pot_number} to status {new_status}.";
    }

    [KernelFunction("get_pot_status")]
    [Description("Get the current planting configuration and latest sensor reading for a single pot.")]
    public async Task<string> GetPotStatusAsync(
        [Description("Pot number in the range 1 to 6.")] int pot_number,
        CancellationToken ct = default)
    {
        if (!TryResolvePotId(pot_number, out var potId))
        {
            return "Invalid pot number. Must be 1–6.";
        }

        var config = await _potRepository.GetByPotIdAsync(potId, ct).ConfigureAwait(false);
        var reading = await _sensorRepository.GetLatestByPotAsync(potId, ct).ConfigureAwait(false);
        if (config is null)
        {
            return $"Pot {pot_number}: no configuration yet. {FormatReading(reading)}.";
        }

        var seed = config.CurrentSeeds.FirstOrDefault(s => s.Status == "growing") ?? config.CurrentSeeds.FirstOrDefault();
        return $"Pot {pot_number} [{config.RoomName}]: {seed?.PlantName ?? "empty"}{(seed is not null ? $"/{seed.SeedName} ({seed.Status})" : string.Empty)}. {FormatReading(reading)}.";
    }

    [KernelFunction("get_all_pots_status")]
    [Description("Get an overview of all six pots including assignments and latest sensor readings.")]
    public async Task<string> GetAllPotsStatusAsync(CancellationToken ct = default)
    {
        var configs = await _potRepository.GetAllAsync(ct).ConfigureAwait(false);
        var configByPot = configs.ToDictionary(c => c.PotId);
        var builder = new StringBuilder();

        foreach (var (potNumber, potId) in PotNumberToId.OrderBy(x => x.Key))
        {
            configByPot.TryGetValue(potId, out var config);
            var reading = await _sensorRepository.GetLatestByPotAsync(potId, ct).ConfigureAwait(false);
            var seed = config?.CurrentSeeds.FirstOrDefault(s => s.Status == "growing") ?? config?.CurrentSeeds.FirstOrDefault();
            builder.AppendLine($"Pot {potNumber} [{config?.RoomName ?? "unassigned"}]: {seed?.PlantName ?? "empty"}{(seed is not null ? $"/{seed.SeedName} ({seed.Status})" : string.Empty)}. {FormatReading(reading)}.");
        }

        return builder.ToString().TrimEnd();
    }

    [KernelFunction("get_sensor_readings")]
    [Description("Get the latest soil moisture and temperature reading for a specific pot.")]
    public async Task<string> GetSensorReadingsAsync(
        [Description("Pot number in the range 1 to 6.")] int pot_number,
        CancellationToken ct = default)
    {
        if (!TryResolvePotId(pot_number, out var potId))
        {
            return "Invalid pot number. Must be 1–6.";
        }

        var reading = await _sensorRepository.GetLatestByPotAsync(potId, ct).ConfigureAwait(false);
        return reading is null
            ? $"Pot {pot_number}: no sensor data yet."
            : $"Pot {pot_number}: moisture {reading.SoilMoisture:0.0}%, temperature {reading.TemperatureC:0.0}°C ({reading.Timestamp:HH:mm} UTC).";
    }

    [KernelFunction("get_available_rooms")]
    [Description("List the available Home Assistant areas that can be assigned to pots.")]
    public async Task<string> GetAvailableRoomsAsync(CancellationToken ct = default)
    {
        var areas = await _areaProvider.GetAvailableAreasAsync(ct).ConfigureAwait(false);
        return areas.Count == 0
            ? "No rooms are currently available."
            : $"Available rooms: {string.Join(", ", areas.Select(area => $"{area.AreaName} ({area.AreaId})"))}.";
    }

    [KernelFunction("get_room_summary")]
    [Description("Get an aggregated room summary for a Home Assistant area.")]
    public async Task<string> GetRoomSummaryAsync(
        [Description("Home Assistant room area identifier, for example living_room.")] string room_area_id,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(room_area_id))
        {
            return "Room area id is required.";
        }

        var summary = await _roomSummaryHandler.HandleAsync(new GetRoomSummaryQuery(room_area_id), ct).ConfigureAwait(false);
        return $"Room {summary.RoomName} ({summary.RoomAreaId}): {summary.PotCount} pots, {summary.ActiveSeedCount} active seeds, average readiness {summary.AverageReadiness}, health {summary.HealthStatus}, ready now {summary.ReadyToHarvestCount}, ripening {summary.RipeningCount}.";
    }

    [KernelFunction("get_dashboard_snapshot")]
    [Description("Get the aggregated dashboard snapshot across all rooms and pots.")]
    public async Task<string> GetDashboardSnapshotAsync(CancellationToken ct = default)
    {
        var dashboard = await _dashboardHandler.HandleAsync(new GetDashboardAggregationQuery(), ct).ConfigureAwait(false);
        return $"Dashboard snapshot: {dashboard.Rooms.Count} rooms, overall health {dashboard.OverallHealthStatus}, ready to harvest {dashboard.ReadyToHarvestCount}, ripening {dashboard.RipeningCount}, growing {dashboard.GrowingCount}, critical pots {dashboard.CriticalPotsCount}.";
    }

    [KernelFunction("get_harvest_readiness")]
    [Description("Get harvest-readiness details for all seeds or optionally filter by lifecycle status.")]
    public async Task<string> GetHarvestReadinessAsync(
        [Description("Optional lifecycle status filter. Use growing, mature, harvested, or removed.")] string? filter_by_status = null,
        CancellationToken ct = default)
    {
        var items = await _harvestReadinessHandler.HandleAsync(new GetHarvestReadinessQuery(filter_by_status), ct).ConfigureAwait(false);
        return items.Count == 0
            ? "No harvest-readiness items were found."
            : string.Join(" ", items.Select(item => $"{item.PlantName}/{item.SeedName} in pot {item.PotId}: score {item.ReadinessScore}, category {item.ReadinessCategory}."));
    }

    [KernelFunction("reset_persisted_data")]
    [Description("Dangerous admin operation that clears all persisted application data while preserving the schema and migration history.")]
    public async Task<string> ResetPersistedDataAsync(CancellationToken ct = default)
    {
        await _commandDispatcher.DispatchAsync(new ResetPersistedDataCommand(), ct).ConfigureAwait(false);
        return "Persisted application data reset has been queued.";
    }

    private async Task<string> ResolveRoomNameAsync(string roomAreaId, CancellationToken ct)
    {
        var areas = await _areaProvider.GetAvailableAreasAsync(ct).ConfigureAwait(false);
        var match = areas.FirstOrDefault(area => string.Equals(area.AreaId, roomAreaId, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            return match.AreaName;
        }

        return roomAreaId.Replace('_', ' ');
    }

    private static bool TryResolvePotId(int potNumber, out Guid potId)
        => PotNumberToId.TryGetValue(potNumber, out potId);

    private static string FormatReading(Domain.SensorReadings.Entities.SensorReading? reading)
        => reading is null
            ? "no sensor data"
            : $"moisture {reading.SoilMoisture:0.0}%, temp {reading.TemperatureC:0.0}°C";
}

