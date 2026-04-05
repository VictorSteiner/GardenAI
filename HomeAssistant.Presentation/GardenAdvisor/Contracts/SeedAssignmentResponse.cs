namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

/// <summary>Represents a single seed assignment in a pot configuration response.</summary>
/// <param name="Id">Unique identifier for this seed assignment.</param>
/// <param name="PlantName">Common plant name (e.g., "Tomato", "Basil").</param>
/// <param name="SeedName">Specific seed/cultivar name (e.g., "Moneymaker", "Genovese").</param>
/// <param name="PlantedDate">When the seed was sown (ISO 8601 format).</param>
/// <param name="ExpectedHarvestDate">Expected harvest date, if known (ISO 8601 format).</param>
/// <param name="Status">Current status: "growing", "mature", "harvested", "removed".</param>
/// <param name="Notes">Optional companion planting or care notes.</param>
public record SeedAssignmentResponse(
    Guid Id,
    string PlantName,
    string SeedName,
    DateTimeOffset PlantedDate,
    DateTimeOffset? ExpectedHarvestDate,
    string Status,
    string? Notes);

