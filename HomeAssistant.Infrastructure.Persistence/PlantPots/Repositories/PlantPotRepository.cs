using HomeAssistant.Domain.PlantPots.Abstractions;
using HomeAssistant.Domain.PlantPots.Entities;
using HomeAssistant.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Infrastructure.Persistence.PlantPots.Repositories;

/// <summary>EF Core implementation of <see cref="IPlantPotRepository"/>.</summary>
public sealed class PlantPotRepository : IPlantPotRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PlantPotRepository> _logger;

    /// <summary>Initialises the repository with the provided database context.</summary>
    public PlantPotRepository(AppDbContext context, ILogger<PlantPotRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PlantPot?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("Pot id must not be empty.", nameof(id));
        _logger.LogDebug("Fetching plant pot {PotId}.", id);
        return await _context.PlantPots
            .Include(p => p.Species)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantPot>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching all plant pots.");
        return await _context.PlantPots
            .Include(p => p.Species)
            .OrderBy(p => p.Position)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantPot pot, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pot);
        _context.PlantPots.Add(pot);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Plant pot {PotId} added.", pot.Id);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantPot pot, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pot);
        _context.PlantPots.Update(pot);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Plant pot {PotId} updated.", pot.Id);
    }
}
