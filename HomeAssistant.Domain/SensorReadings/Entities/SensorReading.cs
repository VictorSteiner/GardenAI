namespace HomeAssistant.Domain.SensorReadings.Entities;

/// <summary>A single sensor reading captured from a plant pot at a point in time.</summary>
public sealed class SensorReading
{
    /// <summary>Unique identifier for this reading.</summary>
    public Guid Id { get; init; }

    /// <summary>The pot this reading belongs to.</summary>
    public Guid PotId { get; init; }

    /// <summary>UTC timestamp when the reading was captured.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Soil moisture percentage (0–100).</summary>
    public double SoilMoisture { get; init; }

    /// <summary>Soil temperature in degrees Celsius.</summary>
    public double TemperatureC { get; init; }
}
