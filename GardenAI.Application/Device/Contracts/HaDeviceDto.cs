using System.Text.Json.Serialization;

namespace GardenAI.Application.Device.Contracts;

/// <summary>DTO for a device received from the Home Assistant REST API.</summary>
public sealed record HaDeviceDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("area_id")]
    public string? AreaId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("name_by_user")]
    public string? NameByUser { get; init; }

    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }
}


