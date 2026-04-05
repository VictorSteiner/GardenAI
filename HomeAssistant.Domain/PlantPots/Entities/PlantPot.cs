using HomeAssistant.Domain.SensorReadings.Entities;

namespace HomeAssistant.Domain.PlantPots.Entities;

/// <summary>Represents one of the six monitored plant pots in the garden system.</summary>
public sealed class PlantPot
{
    /// <summary>Unique identifier for this pot.</summary>
    public Guid Id { get; init; }

    /// <summary>Human-readable label for the pot (e.g. "Pot 1 – Tomatoes").</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Physical position index (1–6).</summary>
    public int Position { get; init; }

    /// <summary>The plant species currently growing in this pot, if assigned.</summary>
    public PlantSpecies? Species { get; init; }

    /// <summary>Sensor readings recorded for this pot, ordered oldest to newest.</summary>
    public IReadOnlyList<SensorReading> Readings { get; init; } = [];
}

