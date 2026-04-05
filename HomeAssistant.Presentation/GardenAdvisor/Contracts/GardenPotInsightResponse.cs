namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

/// <summary>Current insight snapshot for a single pot.</summary>
public sealed record GardenPotInsightResponse(
    Guid PotId,
    int Position,
    string PotLabel,
    string PlantName,
    string SeedName,
    double SoilMoisture,
    double TemperatureC,
    string MoistureBand,
    string TemperatureBand);

