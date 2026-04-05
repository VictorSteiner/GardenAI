using HomeAssistant.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Infrastructure.Persistence.Database.Repositories;

/// <summary>Clears persisted application data while preserving schema and migration metadata.</summary>
public sealed class PersistedDataResetRepository : IPersistedDataResetRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PersistedDataResetRepository> _logger;

    /// <summary>Creates a new <see cref="PersistedDataResetRepository"/>.</summary>
    public PersistedDataResetRepository(AppDbContext context, ILogger<PersistedDataResetRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task ResetAsync(CancellationToken ct = default)
    {
        var tables = _context.Model
            .GetEntityTypes()
            .Select(entityType => new
            {
                Schema = entityType.GetSchema() ?? "public",
                Table = entityType.GetTableName()
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Table))
            .Distinct()
            .Select(x => $"\"{x.Schema}\".\"{x.Table}\"")
            .ToList();

        if (tables.Count == 0)
        {
            _logger.LogInformation("No mapped entity tables were found to reset.");
            return;
        }

        var sql = $"TRUNCATE TABLE {string.Join(", ", tables)} RESTART IDENTITY CASCADE;";
        _logger.LogWarning("Executing persisted data reset across {TableCount} tables.", tables.Count);
        await _context.Database.ExecuteSqlRawAsync(sql, ct).ConfigureAwait(false);
    }
}

