namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PlannerFunctions.Contracts;

/// <summary>Request contract for save-configuration planner functions.</summary>
public sealed record SavePlannerPotConfigurationFunctionRequest(
    int PotNumber,
    string PlantName,
    string SeedName,
    string RoomAreaId,
    string Status = "growing",
    DateTimeOffset? PlantedDate = null,
    string? Notes = null);

