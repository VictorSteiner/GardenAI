namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostSavePotConfiguration.Contracts;

/// <summary>Request model for saving a pot's room assignment and seed configuration.</summary>
public sealed record SavePotConfigurationRequest(
    /// <summary>Home Assistant area identifier (for example <c>living_room</c>).</summary>
    string RoomAreaId,
    /// <summary>Display name for the assigned room.</summary>
    string RoomName,
    /// <summary>Seeds that should be persisted for the pot.</summary>
    IReadOnlyList<SeedAssignmentRequest> Seeds);

