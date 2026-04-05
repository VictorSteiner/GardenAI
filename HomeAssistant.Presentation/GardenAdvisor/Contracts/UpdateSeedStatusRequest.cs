namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

/// <summary>Request model for updating a seed's lifecycle status.</summary>
/// <param name="NewStatus">The new lifecycle status ("growing", "mature", "harvested", "removed").</param>
public sealed record UpdateSeedStatusRequest(string NewStatus);

