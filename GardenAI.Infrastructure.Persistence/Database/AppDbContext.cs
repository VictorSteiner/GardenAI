using GardenAI.Domain.Assistant.Entities;
using Microsoft.EntityFrameworkCore;

namespace GardenAI.Infrastructure.Persistence.Database;

/// <summary>EF Core database context for the GardenAI application.</summary>
public sealed class AppDbContext : DbContext
{
    /// <inheritdoc/>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Persisted assistant chat sessions.</summary>
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    /// <summary>Persisted assistant chat messages.</summary>
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Keep model setup distributed next to each feature repository/entity mapping.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
