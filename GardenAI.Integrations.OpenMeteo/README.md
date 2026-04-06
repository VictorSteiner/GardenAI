# HomeAssistant.Integrations.OpenMeteo

Typed client for Open-Meteo forecast endpoint data.

## Implemented endpoint

- `GET /v1/forecast`

## Quick usage

```csharp
var httpClient = new HttpClient();
var client = new OpenMeteoForecastClient(
    httpClient,
    new OpenMeteoClientOptions());

var forecast = await client.GetForecastAsync(new OpenMeteoForecastRequest
{
    Latitude = 55.6761,
    Longitude = 12.5683,
    Current = ["temperature_2m", "relative_humidity_2m"],
    Hourly = ["temperature_2m", "precipitation"],
    Daily = ["temperature_2m_max", "temperature_2m_min"],
    Timezone = "auto",
});
```

This project is intentionally not wired into other layers yet.
