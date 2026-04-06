namespace HomeAssistant.Presentation.Garden.Planner.Functions.Contracts;

/// <summary>Request contract for update-seed-status planner tool operations.</summary>
public sealed record UpdateSeedStatusRequest(int PotNumber, string NewStatus);

