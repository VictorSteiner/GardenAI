using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeAssistant.Application.Weather.Contracts;

/// <summary>
/// Container for timeseries blocks where metric keys are dynamic per request.
/// </summary>
public sealed record OpenMeteoTimeSeriesBlock
{
    [JsonPropertyName("time")]
    public IReadOnlyList<string> Time { get; init; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Values { get; init; }
}

