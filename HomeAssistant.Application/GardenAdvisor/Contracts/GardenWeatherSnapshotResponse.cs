namespace HomeAssistant.Application.GardenAdvisor.Contracts;

/// <summary>Condensed weather snapshot used for garden decision support.</summary>
public sealed record GardenWeatherSnapshotResponse(
    double? TemperatureC,
    double? WindSpeedKph,
    double? WindGustsKph,
    double? PrecipitationMm,
    string Region,
    string Timezone);

