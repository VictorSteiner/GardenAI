using HomeAssistant.Domain.PlantPots.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeAssistant.Infrastructure.Persistence.PlantPots.Configurations;

/// <summary>Entity mapping for <see cref="PlantSpecies"/>.</summary>
public sealed class PlantSpeciesEntityTypeConfiguration : IEntityTypeConfiguration<PlantSpecies>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PlantSpecies> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
    }
}
