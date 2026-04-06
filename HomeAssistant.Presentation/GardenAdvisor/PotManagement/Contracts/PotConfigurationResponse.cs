namespace HomeAssistant.Presentation.GardenAdvisor.PotManagement.Contracts;

/// <summary>Response model for a pot's configuration (room + seeds).</summary>
/// <param name="PotId">The pot ID.</param>
/// <param name="RoomAreaId">Home Assistant area ID (e.g., "living_room").</param>
/// <param name="RoomName">Display name of the room (e.g., "Living Room").</param>
/// <param name="CurrentSeeds">List of active seed assignments.</param>
/// <param name="LastUpdated">When this configuration was last updated (ISO 8601 format).</param>
public record PotConfigurationResponse(
    Guid PotId,
    string RoomAreaId,
    string RoomName,
    IReadOnlyList<SeedAssignmentResponse> CurrentSeeds,
    DateTimeOffset LastUpdated);

