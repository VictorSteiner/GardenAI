namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PlannerFunctions.Contracts;

/// <summary>Request contract for updating seed status through planner functions.</summary>
public sealed record UpdatePlannerSeedStatusFunctionRequest(
	int PotNumber,
	string NewStatus);

