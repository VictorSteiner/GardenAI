namespace HomeAssistant.Domain.PotConfigurations.Entities;

/// <summary>Represents the persistent configuration of a pot, including its location (room) and active seed assignments.</summary>
public sealed class PotConfiguration
{
    /// <summary>Unique identifier for this configuration record.</summary>
    public Guid Id { get; init; }

    /// <summary>The pot this configuration applies to.</summary>
    public Guid PotId { get; init; }

    /// <summary>Home Assistant area ID (e.g., "living_room", "balcony"). Must match a HA area_registry entry.</summary>
    public string RoomAreaId { get; init; } = string.Empty;

    /// <summary>Display name of the room (e.g., "Living Room", "Balcony"). Denormalized for easy reference in UI.</summary>
    public string RoomName { get; init; } = string.Empty;

    /// <summary>List of active seed assignments in this pot (0..N seeds, supporting sequential or companion planting).</summary>
    public List<SeedAssignment> CurrentSeeds { get; init; } = [];

    /// <summary>Timestamp of the last configuration update.</summary>
    public DateTimeOffset LastUpdated { get; init; }
}

