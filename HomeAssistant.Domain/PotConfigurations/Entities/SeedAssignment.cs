namespace HomeAssistant.Domain.PotConfigurations.Entities;

/// <summary>Represents a single seed or plant assignment within a pot during a lifecycle stage.</summary>
public sealed class SeedAssignment
{
    /// <summary>Unique identifier for this seed assignment.</summary>
    public Guid Id { get; init; }

    /// <summary>Common plant name (e.g., "Tomato", "Basil", "Lettuce").</summary>
    public string PlantName { get; init; } = string.Empty;

    /// <summary>Specific seed/cultivar name (e.g., "Moneymaker", "Genovese").</summary>
    public string SeedName { get; init; } = string.Empty;

    /// <summary>Date when the seed was sown/planted in this pot.</summary>
    public DateTimeOffset PlantedDate { get; init; }

    /// <summary>Expected harvest or maturity date, if known.</summary>
    public DateTimeOffset? ExpectedHarvestDate { get; init; }

    /// <summary>Current lifecycle status: "growing", "mature", "harvested", "removed".</summary>
    public string Status { get; init; } = "growing";

    /// <summary>Optional notes, e.g., companion planting info or special care instructions.</summary>
    public string Notes { get; init; }
}

