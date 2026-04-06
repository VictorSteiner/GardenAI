namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PlannerFunctions.Contracts;

/// <summary>Common request contract for pot-number based planner functions.</summary>
public sealed record PotNumberFunctionRequest(
    /// <summary>Pot number in the configured PotIdentityMap.</summary>
    int PotNumber);

