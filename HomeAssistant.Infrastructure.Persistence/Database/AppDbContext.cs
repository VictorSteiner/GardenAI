using HomeAssistant.Domain.Area.Entities;
using HomeAssistant.Domain.Assistant.Entities;
using HomeAssistant.Domain.Device.Entities;
using HomeAssistant.Domain.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeAssistant.Infrastructure.Persistence.Database;

/// <summary>EF Core database context for the HomeAssistant application.</summary>
public sealed class AppDbContext : DbContext
{
    /// <inheritdoc/>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Persisted assistant chat sessions.</summary>
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    /// <summary>Persisted assistant chat messages.</summary>
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    /// <summary>Home Assistant sync areas.</summary>
    public DbSet<AreaEntity> Areas => Set<AreaEntity>();

    /// <summary>Home Assistant sync devices.</summary>
    public DbSet<DeviceEntity> Devices => Set<DeviceEntity>();

    /// <summary>Home Assistant sync entities.</summary>
    public DbSet<EntityRecord> Entities => Set<EntityRecord>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Keep model setup distributed next to each feature repository/entity mapping.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
