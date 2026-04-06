namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools.Contracts;

/// <summary>Request contract for harvest-readiness planner tool queries.</summary>
public sealed record HarvestReadinessRequest(string? FilterByStatus = null);

