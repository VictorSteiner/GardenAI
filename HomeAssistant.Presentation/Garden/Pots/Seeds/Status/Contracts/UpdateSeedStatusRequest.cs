namespace HomeAssistant.Presentation.Garden.Pots.Endpoints.PostUpdateSeedStatus.Contracts;

/// <summary>Request model for updating a seed's lifecycle status.</summary>
/// <param name="NewStatus">The new lifecycle status (<c>growing</c>, <c>mature</c>, <c>harvested</c>, or <c>removed</c>).</param>
public sealed record UpdateSeedStatusRequest(string NewStatus);

