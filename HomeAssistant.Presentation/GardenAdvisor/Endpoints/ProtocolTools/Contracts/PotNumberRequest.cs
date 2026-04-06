namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools.Contracts;

/// <summary>Request contract for pot-number-based planner tool queries.</summary>
public sealed record PotNumberRequest(int PotNumber);

