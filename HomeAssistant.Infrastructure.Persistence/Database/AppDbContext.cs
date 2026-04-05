using HomeAssistant.Domain.Assistant.Entities;
using HomeAssistant.Domain.PlantPots.Entities;
using HomeAssistant.Domain.PotConfigurations.Entities;
using HomeAssistant.Domain.SensorReadings.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeAssistant.Infrastructure.Persistence.Database;

/// <summary>EF Core database context for the HomeAssistant application.</summary>
public sealed class AppDbContext : DbContext
{
    /// <inheritdoc/>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Plant pot entities.</summary>
    public DbSet<PlantPot> PlantPots => Set<PlantPot>();

    /// <summary>Plant species definitions.</summary>
    public DbSet<PlantSpecies> PlantSpecies => Set<PlantSpecies>();

    /// <summary>Sensor readings from all pots.</summary>
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

    /// <summary>Persisted assistant chat sessions.</summary>
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    /// <summary>Persisted assistant chat messages.</summary>
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    /// <summary>Pot configuration and room assignments.</summary>
    public DbSet<PotConfiguration> PotConfigurations => Set<PotConfiguration>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Keep model setup distributed next to each feature repository/entity mapping.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
