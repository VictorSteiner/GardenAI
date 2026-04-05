namespace HomeAssistant.Integrations.OpenMeteo.Forecast.Contracts;

/// <summary>Query options for the Open-Meteo forecast endpoint.</summary>
public sealed record OpenMeteoForecastRequest
{
    /// <summary>Latitude in decimal degrees.</summary>
    public required double Latitude { get; init; }

    /// <summary>Longitude in decimal degrees.</summary>
    public required double Longitude { get; init; }

    /// <summary>Current weather variables to include (for example: temperature_2m, rain).</summary>
    public IReadOnlyList<string> Current { get; init; } = [];

    /// <summary>Hourly weather variables to include.</summary>
    public IReadOnlyList<string> Hourly { get; init; } = [];

    /// <summary>Daily weather variables to include.</summary>
    public IReadOnlyList<string> Daily { get; init; } = [];

    /// <summary>Timezone identifier or <c>auto</c>.</summary>
    public string? Timezone { get; init; } = "auto";

    /// <summary>Number of forecast days to return.</summary>
    public int? ForecastDays { get; init; }

    /// <summary>Number of past days to include.</summary>
    public int? PastDays { get; init; }

    /// <summary>Optional inclusive start date override.</summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>Optional inclusive end date override.</summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>Optional temperature unit (for example: celsius, fahrenheit).</summary>
    public string? TemperatureUnit { get; init; }

    /// <summary>Optional wind speed unit.</summary>
    public string? WindSpeedUnit { get; init; }

    /// <summary>Optional precipitation unit.</summary>
    public string? PrecipitationUnit { get; init; }

    /// <summary>Optional grid-cell selection strategy.</summary>
    public string? CellSelection { get; init; }
}

