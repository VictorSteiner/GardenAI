using HomeAssistant.Domain.SensorReadings.Entities;

namespace HomeAssistant.Domain.SensorReadings.Abstractions;

/// <summary>Provides the latest sensor readings from the hardware layer (real or mock).</summary>
public interface ISensorProvider
{
    /// <summary>Returns the most recent reading for each of the six monitored plant pots.</summary>
    Task<IReadOnlyList<SensorReading>> GetLatestReadingsAsync(CancellationToken ct = default);
}
