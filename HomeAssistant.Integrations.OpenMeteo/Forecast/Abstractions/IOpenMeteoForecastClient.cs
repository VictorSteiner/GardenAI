using HomeAssistant.Integrations.OpenMeteo.Forecast.Contracts;

namespace HomeAssistant.Integrations.OpenMeteo.Forecast.Abstractions;

/// <summary>Typed client contract for Open-Meteo weather forecast data.</summary>
public interface IOpenMeteoForecastClient
{
    /// <summary>
    /// Fetches forecast data from Open-Meteo for the given coordinates and requested variable sets.
    /// </summary>
    /// <param name="request">Forecast query options, including coordinates and requested fields.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A typed forecast payload with dynamic metric groups.</returns>
    Task<OpenMeteoForecastResponse> GetForecastAsync(OpenMeteoForecastRequest request, CancellationToken ct = default);
}
