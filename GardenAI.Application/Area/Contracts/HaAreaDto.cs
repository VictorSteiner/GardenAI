using System.Text.Json.Serialization;

namespace GardenAI.Application.Area.Contracts;

/// <summary>DTO for an area received from the Home Assistant REST API.</summary>
public sealed record HaAreaDto
{
    [JsonPropertyName("area_id")]
    public string AreaId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonPropertyName("aliases")]
    public string[]? Aliases { get; init; }
}


