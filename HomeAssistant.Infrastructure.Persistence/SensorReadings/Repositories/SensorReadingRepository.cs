using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Domain.SensorReadings.Entities;
using HomeAssistant.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Infrastructure.Persistence.SensorReadings.Repositories;

/// <summary>EF Core implementation of <see cref="ISensorReadingRepository"/>.</summary>
public sealed class SensorReadingRepository : ISensorReadingRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<SensorReadingRepository> _logger;

    /// <summary>Initialises the repository with the provided database context.</summary>
    public SensorReadingRepository(AppDbContext context, ILogger<SensorReadingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task AppendAsync(SensorReading reading, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reading);
        _context.SensorReadings.Add(reading);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Sensor reading {ReadingId} appended for pot {PotId}.", reading.Id, reading.PotId);
    }

    /// <inheritdoc/>
    public async Task<SensorReading?> GetLatestByPotAsync(Guid potId, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching latest sensor reading for pot {PotId}.", potId);
        return await _context.SensorReadings
            .Where(r => r.PotId == potId)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SensorReading>> GetByPotAsync(Guid potId, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching all sensor readings for pot {PotId}.", potId);
        return await _context.SensorReadings
            .Where(r => r.PotId == potId)
            .OrderBy(r => r.Timestamp)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SensorReading>> GetLatestReadingsByPotIdAsync(Guid potId, int limit = 10, CancellationToken ct = default)
    {
        if (potId == Guid.Empty)
            throw new ArgumentException("Pot ID must not be empty.", nameof(potId));

        if (limit <= 0)
            throw new ArgumentException("Limit must be greater than 0.", nameof(limit));

        _logger.LogDebug("Fetching latest {Limit} sensor readings for pot {PotId}.", limit, potId);
        return await _context.SensorReadings
            .Where(r => r.PotId == potId)
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
    }
}

