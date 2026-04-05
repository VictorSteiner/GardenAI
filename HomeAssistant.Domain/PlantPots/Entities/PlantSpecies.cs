namespace HomeAssistant.Domain.PlantPots.Entities;

/// <summary>Defines the ideal growing conditions for a species of plant.</summary>
public sealed class PlantSpecies
{
    /// <summary>Unique identifier for this species.</summary>
    public Guid Id { get; init; }

    /// <summary>Common name of the plant species.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Minimum acceptable soil moisture percentage (0–100).</summary>
    public double IdealMoistureMin { get; init; }

    /// <summary>Maximum acceptable soil moisture percentage (0–100).</summary>
    public double IdealMoistureMax { get; init; }

    /// <summary>Minimum acceptable soil temperature in degrees Celsius.</summary>
    public double IdealTempMinC { get; init; }

    /// <summary>Maximum acceptable soil temperature in degrees Celsius.</summary>
    public double IdealTempMaxC { get; init; }
}
