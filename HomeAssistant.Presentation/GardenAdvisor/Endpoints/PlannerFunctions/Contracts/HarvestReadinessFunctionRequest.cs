namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PlannerFunctions.Contracts;

/// <summary>Request contract for harvest-readiness planner functions.</summary>
public sealed record HarvestReadinessFunctionRequest(
    string? FilterByStatus = null);

