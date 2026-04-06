using System.Text.Json.Serialization;

namespace GardenAI.Application.Entity.Contracts;

/// <summary>DTO for an entity received from the Home Assistant REST API.</summary>
public sealed record HaEntityDto
{
    [JsonPropertyName("entity_id")]
    public string EntityId { get; init; } = string.Empty;

    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }

    [JsonPropertyName("area_id")]
    public string? AreaId { get; init; }

    [JsonPropertyName("platform")]
    public string Platform { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("original_name")]
    public string? OriginalName { get; init; }
}


