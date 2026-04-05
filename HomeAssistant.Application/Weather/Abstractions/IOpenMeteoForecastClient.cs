using HomeAssistant.Application.Weather.Contracts;

namespace HomeAssistant.Application.Weather.Abstractions;

/// <summary>
/// Typed client contract for Open-Meteo weather forecast data.
/// </summary>
public interface IOpenMeteoForecastClient
{
    /// <summary>
    /// Fetches forecast data from Open-Meteo for the given coordinates and variable sets.
    /// </summary>
    /// <param name="request">Forecast query options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Typed forecast payload.</returns>
    Task<OpenMeteoForecastResponse> GetForecastAsync(OpenMeteoForecastRequest request, CancellationToken ct = default);
}

