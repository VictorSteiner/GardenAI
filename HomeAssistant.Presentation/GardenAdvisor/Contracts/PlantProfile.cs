namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

/// <summary>Plant assignment and ideal condition band for a single pot.</summary>
public sealed record PlantProfile(
    Guid PotId,
    int Position,
    string PotLabel,
    string PlantName,
    string SeedName,
    double IdealMoistureMin,
    double IdealMoistureMax,
    double IdealTempMinC,
    double IdealTempMaxC);

