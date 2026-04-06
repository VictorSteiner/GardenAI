
namespace GardenAI.Domain.Area.Entities;

/// <summary>Represents a Home Assistant Area (a named room or zone).</summary>
public sealed class AreaEntity
{
    /// <summary>Home Assistant area_id (string identifier from HA).</summary>
    public required string Id { get; set; }

    /// <summary>User-friendly name of the area (e.g., "Living Room").</summary>
    public required string Name { get; set; }

    /// <summary>Optional icon identifier for the area.</summary>
    public string? Icon { get; set; }

    /// <summary>JSON array of aliases for this area.</summary>
    public string? Aliases { get; set; }

    /// <summary>Timestamp when the area was first synced into local DB.</summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of last update from Home Assistant.</summary>
    public required DateTime UpdatedAt { get; set; }
}

