using System.Text;
using HomeAssistant.Application.Dispatching;
using HomeAssistant.Application.GardenAdvisor.Abstractions;
using HomeAssistant.Application.GardenAdvisor.Contracts;
using HomeAssistant.Application.PotConfigurations.Commands;
using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Application.PotConfigurations.Queries;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;

namespace HomeAssistant.Presentation.GardenAdvisor.Services;

/// <summary>
/// Default implementation of planner workflows exposed through AI tools and Kestrel endpoints.
/// </summary>
public sealed class GardenPlannerFunctionService : IGardenPlannerFunctionService
{
    private static readonly IReadOnlyDictionary<int, Guid> PotNumberToId =
        new Dictionary<int, Guid>
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
    private readonly HomeAssistant.Application.GardenAdvisor.Abstractions.IGardenAdviceStateStore _adviceStateStore;
    private readonly HomeAssistant.Application.GardenAdvisor.Abstractions.IGardenAdvisorService _gardenAdvisorService;
    private readonly IGardenPlannerHistoryStore _historyStore;
    private readonly ILogger<GardenPlannerFunctionService> _logger;

    /// <summary>Creates a new <see cref="GardenPlannerFunctionService"/>.</summary>
    public GardenPlannerFunctionService(
        IPotConfigurationRepository potRepository,
        ISensorReadingRepository sensorRepository,
        ICommandDispatcher commandDispatcher,
        IQueryHandler<GetDashboardAggregationQuery, DashboardAggregationDto> dashboardHandler,
        IQueryHandler<GetRoomSummaryQuery, RoomSummaryDto> roomSummaryHandler,
        IQueryHandler<GetHarvestReadinessQuery, IReadOnlyList<HarvestReadinessDto>> harvestReadinessHandler,
        IHomeAssistantAreaProvider areaProvider,
        HomeAssistant.Application.GardenAdvisor.Abstractions.IGardenAdviceStateStore adviceStateStore,
        HomeAssistant.Application.GardenAdvisor.Abstractions.IGardenAdvisorService gardenAdvisorService,
        IGardenPlannerHistoryStore historyStore,
        ILogger<GardenPlannerFunctionService> logger)
    {
        _potRepository = potRepository ?? throw new ArgumentNullException(nameof(potRepository));
        _sensorRepository = sensorRepository ?? throw new ArgumentNullException(nameof(sensorRepository));
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        _dashboardHandler = dashboardHandler ?? throw new ArgumentNullException(nameof(dashboardHandler));
        _roomSummaryHandler = roomSummaryHandler ?? throw new ArgumentNullException(nameof(roomSummaryHandler));
        _harvestReadinessHandler = harvestReadinessHandler ?? throw new ArgumentNullException(nameof(harvestReadinessHandler));
        _areaProvider = areaProvider ?? throw new ArgumentNullException(nameof(areaProvider));
        _adviceStateStore = adviceStateStore ?? throw new ArgumentNullException(nameof(adviceStateStore));
        _gardenAdvisorService = gardenAdvisorService ?? throw new ArgumentNullException(nameof(gardenAdvisorService));
        _historyStore = historyStore ?? throw new ArgumentNullException(nameof(historyStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<string> SavePotConfigurationAsync(SavePlannerPotConfigurationFunctionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TryResolvePotId(request.PotNumber, out var potId))
            return "Invalid pot number. Must be 1–6.";

        if (string.IsNullOrWhiteSpace(request.PlantName) || string.IsNullOrWhiteSpace(request.SeedName))
            return "Plant name and seed name are required.";

        var roomName = request.RoomAreaId switch
        {
            "balcony" => "Balcony",
            "greenhouse" => "Greenhouse",
            _ => "Living Room",
        };

        await _commandDispatcher.DispatchAsync(
            new SavePotConfigurationCommand(
                potId,
                new SavePotConfigurationRequest(
                    request.RoomAreaId,
                    roomName,
                    [new SeedAssignmentRequest(
                        request.PlantName,
                        request.SeedName,
                        request.PlantedDate ?? DateTimeOffset.UtcNow,
                        null,
                        request.Status,
                        request.Notes)])),
            ct);

        _logger.LogInformation(
            "Planner workflow saved pot configuration for pot {PotNumber} ({PlantName}/{SeedName}).",
            request.PotNumber,
            request.PlantName,
            request.SeedName);

        return $"✅ Saved {request.PlantName} ({request.SeedName}) in Pot {request.PotNumber} ({roomName}), status: {request.Status}.";
    }

    /// <inheritdoc/>
    public async Task<string> UpdateSeedStatusAsync(UpdatePlannerSeedStatusFunctionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TryResolvePotId(request.PotNumber, out var potId))
            return "Invalid pot number. Must be 1–6.";

        var config = await _potRepository.GetByPotIdAsync(potId, ct);
        if (config is null)
            return $"Pot {request.PotNumber} has no configuration.";

        var seed = config.CurrentSeeds.FirstOrDefault(s => s.Status == "growing") ?? config.CurrentSeeds.FirstOrDefault();
        if (seed is null)
            return $"Pot {request.PotNumber} has no seeds to update.";

        await _commandDispatcher.DispatchAsync(
            new UpdateSeedStatusCommand(potId, seed.Id, request.NewStatus),
            ct);

        _logger.LogInformation(
            "Planner workflow updated seed status for pot {PotNumber} to {Status}.",
            request.PotNumber,
            request.NewStatus);

        return $"✅ Pot {request.PotNumber} ({seed.PlantName}/{seed.SeedName}) status → '{request.NewStatus}'.";
    }

    /// <inheritdoc/>
    public async Task<string> GetPotStatusAsync(PotNumberFunctionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TryResolvePotId(request.PotNumber, out var potId))
            return "Invalid pot number. Must be 1–6.";

        var config = await _potRepository.GetByPotIdAsync(potId, ct);
        var reading = await _sensorRepository.GetLatestByPotAsync(potId, ct);

        if (config is null)
            return $"Pot {request.PotNumber}: no configuration yet. {FormatReading(reading)}.";

        var seeds = config.CurrentSeeds.Count > 0
            ? string.Join(", ", config.CurrentSeeds.Select(s => $"{s.PlantName}/{s.SeedName} ({s.Status})"))
            : "empty";

        return $"Pot {request.PotNumber} [{config.RoomName}]: {seeds}. {FormatReading(reading)}.";
    }

    /// <inheritdoc/>
    public async Task<string> GetAllPotsStatusAsync(CancellationToken ct = default)
    {
        var configs = await _potRepository.GetAllAsync(ct);
        var configByPot = configs.ToDictionary(c => c.PotId);
        var sb = new StringBuilder();

        foreach (var (potNumber, potId) in PotNumberToId.OrderBy(kv => kv.Key))
        {
            configByPot.TryGetValue(potId, out var config);
            var reading = await _sensorRepository.GetLatestByPotAsync(potId, ct);
            var seed = config?.CurrentSeeds.FirstOrDefault(s => s.Status == "growing") ?? config?.CurrentSeeds.FirstOrDefault();

            sb.AppendLine(
                $"Pot {potNumber} [{config?.RoomName ?? "unassigned"}]: {seed?.PlantName ?? "empty"}" +
                $"{(seed is not null ? $"/{seed.SeedName} ({seed.Status})" : string.Empty)}. {FormatReading(reading)}.");
        }

        return sb.ToString().TrimEnd();
    }

    /// <inheritdoc/>
    public async Task<string> GetSensorReadingsAsync(PotNumberFunctionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TryResolvePotId(request.PotNumber, out var potId))
            return "Invalid pot number. Must be 1–6.";

        var reading = await _sensorRepository.GetLatestByPotAsync(potId, ct);
        return reading is null
            ? $"Pot {request.PotNumber}: no sensor data yet."
            : $"Pot {request.PotNumber}: moisture {reading.SoilMoisture:0.0}%, temperature {reading.TemperatureC:0.0}°C ({reading.Timestamp:HH:mm} UTC).";
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<RoomResponse>> GetAvailableRoomsAsync(CancellationToken ct = default)
        => _areaProvider.GetAvailableRoomsAsync(ct);

    /// <inheritdoc/>
    public async Task<RoomSummaryResponse> GetRoomSummaryAsync(RoomAreaFunctionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RoomAreaId);

        var dto = await _roomSummaryHandler.HandleAsync(new GetRoomSummaryQuery(request.RoomAreaId), ct);
        return RoomSummaryResponse.FromDto(dto);
    }

    /// <inheritdoc/>
    public async Task<DashboardAggregationResponse> GetDashboardAsync(CancellationToken ct = default)
    {
        var dto = await _dashboardHandler.HandleAsync(new GetDashboardAggregationQuery(), ct);
        return DashboardAggregationResponse.FromDto(dto);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HarvestReadinessResponse>> GetHarvestReadinessAsync(HarvestReadinessFunctionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dtos = await _harvestReadinessHandler.HandleAsync(new GetHarvestReadinessQuery(request.FilterByStatus), ct);
        return dtos.Select(HarvestReadinessResponse.FromDto).ToList().AsReadOnly();
    }

    /// <inheritdoc/>
    public HomeAssistant.Application.GardenAdvisor.Contracts.GardenAdviceResponse? GetLatestAdvice() => _adviceStateStore.GetLatest();

    /// <inheritdoc/>
    public Task<HomeAssistant.Application.GardenAdvisor.Contracts.GardenAdviceResponse> GenerateAdviceAsync(GeneratePlannerAdviceFunctionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _gardenAdvisorService.GenerateAdviceAsync(request.PublishToMqtt, ct);
    }

    /// <inheritdoc/>
    public string ClearPlannerHistory()
    {
        _historyStore.Clear();
        _logger.LogInformation("Planner workflow cleared in-memory chat history.");
        return "✅ Planner history cleared.";
    }

    private static bool TryResolvePotId(int potNumber, out Guid potId)
        => PotNumberToId.TryGetValue(potNumber, out potId);

    private static string FormatReading(Domain.SensorReadings.Entities.SensorReading? reading)
        => reading is null
            ? "no sensor data"
            : $"moisture {reading.SoilMoisture:0.0}%, temp {reading.TemperatureC:0.0}°C";
}

