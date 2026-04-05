using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HomeAssistant.Application.Weather.Abstractions;
using HomeAssistant.Application.Weather.Configuration;
using HomeAssistant.Application.Weather.Contracts;
using HomeAssistant.Integrations.OpenMeteo.Forecast.Exceptions;

namespace HomeAssistant.Integrations.OpenMeteo.Forecast.Clients;

/// <summary>HTTP client for Open-Meteo forecast endpoint data.</summary>
public sealed class OpenMeteoForecastClient : IOpenMeteoForecastClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;

    /// <summary>Initializes a new client instance.</summary>
    public OpenMeteoForecastClient(HttpClient httpClient, OpenMeteoClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            throw new ArgumentException("Open-Meteo base URL must be provided.", nameof(options));

        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri(options.BaseUrl, UriKind.Absolute);
    }

    /// <inheritdoc />
    public async Task<OpenMeteoForecastResponse> GetForecastAsync(OpenMeteoForecastRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = new Dictionary<string, string?>
        {
            ["latitude"] = request.Latitude.ToString(CultureInfo.InvariantCulture),
            ["longitude"] = request.Longitude.ToString(CultureInfo.InvariantCulture),
            ["current"] = JoinCsv(request.Current),
            ["hourly"] = JoinCsv(request.Hourly),
            ["daily"] = JoinCsv(request.Daily),
            ["timezone"] = request.Timezone,
            ["forecast_days"] = request.ForecastDays?.ToString(CultureInfo.InvariantCulture),
            ["past_days"] = request.PastDays?.ToString(CultureInfo.InvariantCulture),
            ["start_date"] = request.StartDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["end_date"] = request.EndDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["temperature_unit"] = request.TemperatureUnit,
            ["wind_speed_unit"] = request.WindSpeedUnit,
            ["precipitation_unit"] = request.PrecipitationUnit,
            ["cell_selection"] = request.CellSelection,
        };

        const string requestPath = "/v1/forecast";
        using var response = await _httpClient.GetAsync(BuildPathWithQuery(requestPath, query), ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new OpenMeteoApiException(response.StatusCode, requestPath, body);
        }

        var payload = await response.Content.ReadFromJsonAsync<OpenMeteoForecastResponse>(JsonOptions, ct).ConfigureAwait(false);
        if (payload is null)
            throw new InvalidOperationException("Open-Meteo returned an empty forecast payload.");

        return payload;
    }

    private static string? JoinCsv(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
            return null;

        var filtered = values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray();
        return filtered.Length == 0 ? null : string.Join(',', filtered);
    }

    private static string BuildPathWithQuery(string path, IReadOnlyDictionary<string, string?> query)
    {
        var builder = new StringBuilder(path);
        var hasAny = false;

        foreach (var (key, value) in query)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            builder.Append(hasAny ? '&' : '?');
            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
            hasAny = true;
        }

        return builder.ToString();
    }
}
