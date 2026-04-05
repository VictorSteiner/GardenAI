using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Entities;
using HomeAssistant.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Infrastructure.Persistence.PotConfigurations.Repositories;

/// <summary>EF Core implementation of <see cref="IPotConfigurationRepository"/>.</summary>
public sealed class PotConfigurationRepository : IPotConfigurationRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PotConfigurationRepository> _logger;

    /// <summary>Initialises the repository with the provided database context.</summary>
    public PotConfigurationRepository(AppDbContext context, ILogger<PotConfigurationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<PotConfiguration?> GetByPotIdAsync(Guid potId, CancellationToken ct = default)
    {
        if (potId == Guid.Empty)
            throw new ArgumentException("Pot ID must not be empty.", nameof(potId));

        _logger.LogDebug("Fetching pot configuration for pot {PotId}.", potId);
        return await _context.PotConfigurations
            .FirstOrDefaultAsync(pc => pc.PotId == potId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PotConfiguration>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching all pot configurations.");
        return await _context.PotConfigurations
            .OrderBy(pc => pc.PotId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PotConfiguration configuration, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _context.PotConfigurations.Add(configuration);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Pot configuration created for pot {PotId} in room {RoomAreaId}.", 
            configuration.PotId, configuration.RoomAreaId);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PotConfiguration configuration, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _context.PotConfigurations.Update(configuration);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Pot configuration updated for pot {PotId} in room {RoomAreaId}.", 
            configuration.PotId, configuration.RoomAreaId);
    }

    /// <inheritdoc/>
    public async Task DeleteByPotIdAsync(Guid potId, CancellationToken ct = default)
    {
        if (potId == Guid.Empty)
            throw new ArgumentException("Pot ID must not be empty.", nameof(potId));

        var configuration = await GetByPotIdAsync(potId, ct);
        if (configuration is null)
        {
            _logger.LogWarning("Attempted to delete pot configuration for pot {PotId}, but it does not exist.", potId);
            return;
        }

        _context.PotConfigurations.Remove(configuration);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Pot configuration deleted for pot {PotId}.", potId);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PotConfiguration>> GetByRoomAreaIdAsync(string roomAreaId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(roomAreaId);

        _logger.LogDebug("Fetching pot configurations for room {RoomAreaId}.", roomAreaId);
        return await _context.PotConfigurations
            .Where(pc => pc.RoomAreaId == roomAreaId)
            .OrderBy(pc => pc.PotId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PotConfiguration>> GetByStatusAsync(string seedStatus, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(seedStatus);

        _logger.LogDebug("Fetching pot configurations with seeds at status {SeedStatus}.", seedStatus);
        return await _context.PotConfigurations
            .Where(pc => pc.CurrentSeeds.Any(s => s.Status == seedStatus))
            .OrderBy(pc => pc.PotId)
            .ToListAsync(ct);
    }
}

