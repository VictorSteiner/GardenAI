namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools.Contracts;

/// <summary>Request contract for update-seed-status planner tool operations.</summary>
public sealed record UpdateSeedStatusRequest(int PotNumber, string NewStatus);

