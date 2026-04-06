
namespace HomeAssistant.Domain.Device.Entities;

/// <summary>Represents a Home Assistant Device assigned to an Area.</summary>
public sealed class DeviceEntity
{
    /// <summary>Home Assistant device_id (string identifier from HA).</summary>
    public required string Id { get; set; }

    /// <summary>Foreign key to Area. Nullable because devices can be unassigned.</summary>
    public string? AreaId { get; set; }

    /// <summary>User-friendly name of the device.</summary>
    public required string Name { get; set; }

    /// <summary>Optional name set by the user in Home Assistant.</summary>
    public string? NameByUser { get; set; }

    /// <summary>Optional manufacturer of the device.</summary>
    public string? Manufacturer { get; set; }

    /// <summary>Optional model identifier of the device.</summary>
    public string? Model { get; set; }

    /// <summary>Timestamp when the device was first synced into local DB.</summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of last update from Home Assistant.</summary>
    public required DateTime UpdatedAt { get; set; }
}

