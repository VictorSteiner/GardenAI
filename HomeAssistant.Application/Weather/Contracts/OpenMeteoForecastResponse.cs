using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeAssistant.Application.Weather.Contracts;

/// <summary>
/// Typed forecast response from Open-Meteo.
/// </summary>
public sealed record OpenMeteoForecastResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMs { get; init; }

    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; init; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; init; }

    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; init; }

    [JsonPropertyName("elevation")]
    public double? Elevation { get; init; }

    [JsonPropertyName("current_units")]
    public Dictionary<string, string> CurrentUnits { get; init; }

    [JsonPropertyName("current")]
    public Dictionary<string, JsonElement> Current { get; init; }

    [JsonPropertyName("hourly_units")]
    public Dictionary<string, string> HourlyUnits { get; init; }

    [JsonPropertyName("hourly")]
    public OpenMeteoTimeSeriesBlock Hourly { get; init; }

    [JsonPropertyName("daily_units")]
    public Dictionary<string, string> DailyUnits { get; init; }

    [JsonPropertyName("daily")]
    public OpenMeteoTimeSeriesBlock Daily { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionData { get; init; }
}

