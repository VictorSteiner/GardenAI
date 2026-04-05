namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

/// <summary>Represents an available Home Assistant area for room assignment.</summary>
/// <param name="AreaId">Unique area identifier (e.g., "living_room").</param>
/// <param name="AreaName">Display name of the area (e.g., "Living Room").</param>
public record RoomResponse(
    string AreaId,
    string AreaName);

