using HomeAssistant.Domain.SensorReadings.Entities;

namespace HomeAssistant.Domain.SensorReadings.Abstractions;

/// <summary>Defines data access operations for sensor readings.</summary>
public interface ISensorReadingRepository
{
    /// <summary>Appends a new sensor reading to the store.</summary>
    Task AppendAsync(SensorReading reading, CancellationToken ct = default);

    /// <summary>Returns the single most recent reading for the specified pot, or <c>null</c>.</summary>
    Task<SensorReading> GetLatestByPotAsync(Guid potId, CancellationToken ct = default);

    /// <summary>Returns all readings for the specified pot, ordered oldest to newest.</summary>
    Task<IReadOnlyList<SensorReading>> GetByPotAsync(Guid potId, CancellationToken ct = default);

    /// <summary>Returns the most recent N readings for the specified pot, ordered by timestamp descending (newest first).</summary>
    /// <param name="potId">The pot ID to retrieve readings for.</param>
    /// <param name="limit">Maximum number of readings to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Latest readings for the pot (up to limit), ordered newest first.</returns>
    Task<IReadOnlyList<SensorReading>> GetLatestReadingsByPotIdAsync(Guid potId, int limit = 10, CancellationToken ct = default);
}


