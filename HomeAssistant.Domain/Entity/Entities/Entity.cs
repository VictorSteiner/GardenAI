
namespace HomeAssistant.Domain.Entity.Entities;

/// <summary>Represents a Home Assistant Entity (a specific capability/sensor on a Device).</summary>
public sealed class EntityRecord
{
    /// <summary>Home Assistant entity_id (string identifier from HA).</summary>
    public required string Id { get; set; }

    /// <summary>Foreign key to Device. Nullable because virtual entities have no physical device.</summary>
    public string? DeviceId { get; set; }

    /// <summary>Foreign key to Area (area override). Nullable because entities may not have a direct area assignment.</summary>
    public string? AreaId { get; set; }

    /// <summary>Platform/domain of the entity (e.g., "sensor", "climate", "light").</summary>
    public required string Platform { get; set; }

    /// <summary>User-friendly name for the entity.</summary>
    public required string Name { get; set; }

    /// <summary>Original/default name from Home Assistant.</summary>
    public string? OriginalName { get; set; }

    /// <summary>Timestamp when the entity was first synced into local DB.</summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of last update from Home Assistant.</summary>
    public required DateTime UpdatedAt { get; set; }
}

