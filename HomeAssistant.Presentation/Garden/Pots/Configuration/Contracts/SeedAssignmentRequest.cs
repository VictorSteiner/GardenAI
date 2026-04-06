namespace HomeAssistant.Presentation.Garden.Pots.Endpoints.PostSavePotConfiguration.Contracts;

/// <summary>Represents a single seed assignment submitted when saving a pot configuration.</summary>
public sealed record SeedAssignmentRequest(
    /// <summary>Common plant name (for example <c>Tomato</c> or <c>Basil</c>).</summary>
    string PlantName,
    /// <summary>Specific seed or cultivar name (for example <c>Moneymaker</c> or <c>Genovese</c>).</summary>
    string SeedName,
    /// <summary>Date when the seed was sown.</summary>
    DateTimeOffset PlantedDate,
    /// <summary>Expected harvest or maturity date, if known.</summary>
    DateTimeOffset? ExpectedHarvestDate,
    /// <summary>Current lifecycle status.</summary>
    string Status,
    /// <summary>Optional companion planting or care notes.</summary>
    string? Notes);

