using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using HomeAssistant.Presentation.GardenAdvisor.Endpoints.PlannerFunctions.Contracts;
using HomeAssistant.Application.GardenAdvisor.Contracts.Advice;
using AppGardenAdviceResponse = HomeAssistant.Application.GardenAdvisor.Contracts.Advice.GardenAdviceResponse;

namespace HomeAssistant.Presentation.GardenAdvisor.Abstractions;

/// <summary>
/// Provides discrete planner workflows that can be exposed both as AI-invokable tools and
/// as Kestrel-hosted Minimal API endpoints.
/// </summary>
public interface IGardenPlannerFunctionService
{
    /// <summary>Saves a pot configuration for planting or re-planting.</summary>
    Task<string> SavePotConfigurationAsync(SavePlannerPotConfigurationFunctionRequest request, CancellationToken ct = default);

    /// <summary>Updates the lifecycle status of the active seed in a pot.</summary>
    Task<string> UpdateSeedStatusAsync(UpdatePlannerSeedStatusFunctionRequest request, CancellationToken ct = default);

    /// <summary>Returns planting and sensor status for a specific pot.</summary>
    Task<string> GetPotStatusAsync(PotNumberFunctionRequest request, CancellationToken ct = default);

    /// <summary>Returns an overview of all configured pots.</summary>
    Task<string> GetAllPotsStatusAsync(CancellationToken ct = default);

    /// <summary>Returns latest sensor readings for a specific pot.</summary>
    Task<string> GetSensorReadingsAsync(PotNumberFunctionRequest request, CancellationToken ct = default);

    /// <summary>Returns available Home Assistant rooms.</summary>
    Task<IReadOnlyList<RoomResponse>> GetAvailableRoomsAsync(CancellationToken ct = default);

    /// <summary>Returns a room summary for the given area identifier.</summary>
    Task<RoomSummaryResponse> GetRoomSummaryAsync(RoomAreaFunctionRequest request, CancellationToken ct = default);

    /// <summary>Returns the dashboard aggregation snapshot.</summary>
    Task<DashboardAggregationResponse> GetDashboardAsync(CancellationToken ct = default);

    /// <summary>Returns harvest readiness across seeds.</summary>
    Task<IReadOnlyList<HarvestReadinessResponse>> GetHarvestReadinessAsync(HarvestReadinessFunctionRequest request, CancellationToken ct = default);

    /// <summary>Returns the latest in-memory garden advice, if available.</summary>
    AppGardenAdviceResponse? GetLatestAdvice();

    /// <summary>Generates fresh garden advice and optionally publishes MQTT updates.</summary>
    Task<AppGardenAdviceResponse> GenerateAdviceAsync(GeneratePlannerAdviceFunctionRequest request, CancellationToken ct = default);

    /// <summary>Clears the in-memory planner chat history.</summary>
    string ClearPlannerHistory();
}

