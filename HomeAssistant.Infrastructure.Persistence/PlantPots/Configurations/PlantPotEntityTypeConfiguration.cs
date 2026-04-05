using HomeAssistant.Domain.PlantPots.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeAssistant.Infrastructure.Persistence.PlantPots.Configurations;

/// <summary>Entity mapping for <see cref="PlantPot"/>.</summary>
public sealed class PlantPotEntityTypeConfiguration : IEntityTypeConfiguration<PlantPot>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PlantPot> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Label).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Position).IsRequired();
        builder.HasOne(p => p.Species).WithMany().HasForeignKey("SpeciesId").IsRequired(false);
        builder.Ignore(p => p.Readings); // navigated separately via SensorReading.PotId
    }
}
