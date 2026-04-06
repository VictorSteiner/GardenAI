using System.Text.Json;
using System.Text.Json.Serialization;

namespace GardenAI.Application.Common.Sync.Contracts;

/// <summary>Represents a registry change event received from the Home Assistant WebSocket API.</summary>
public sealed record HaRegistryEvent
{
    /// <summary>The event type (e.g. "area_registry_updated").</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>Raw JSON data payload from HA. Deserialised by each event handler.</summary>
    public JsonElement Data { get; init; }
}

/// <summary>Payload for area_registry_updated events.</summary>
public sealed record HaAreaRegistryEventData
{
    [JsonPropertyName("action")]
    public string Action { get; init; } = string.Empty;

    [JsonPropertyName("area_id")]
    public string AreaId { get; init; } = string.Empty;
}

/// <summary>Payload for device_registry_updated events.</summary>
public sealed record HaDeviceRegistryEventData
{
    [JsonPropertyName("action")]
    public string Action { get; init; } = string.Empty;

    [JsonPropertyName("device_id")]
    public string DeviceId { get; init; } = string.Empty;

    [JsonPropertyName("area_id")]
    public string? AreaId { get; init; }
}

/// <summary>Payload for entity_registry_updated events.</summary>
public sealed record HaEntityRegistryEventData
{
    [JsonPropertyName("action")]
    public string Action { get; init; } = string.Empty;

    [JsonPropertyName("entity_id")]
    public string EntityId { get; init; } = string.Empty;

    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }

    [JsonPropertyName("area_id")]
    public string? AreaId { get; init; }
}


