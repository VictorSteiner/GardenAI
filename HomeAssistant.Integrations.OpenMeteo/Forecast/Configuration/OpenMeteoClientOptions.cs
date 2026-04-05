namespace HomeAssistant.Integrations.OpenMeteo.Forecast.Configuration;

/// <summary>Configuration for the Open-Meteo forecast API client.</summary>
public sealed class OpenMeteoClientOptions
{
    /// <summary>Base URL for the Open-Meteo weather API.</summary>
    public string BaseUrl { get; init; } = "https://api.open-meteo.com";
}
