using HomeAssistant.Domain.PotConfigurations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeAssistant.Infrastructure.Persistence.PotConfigurations.Configurations;

/// <summary>EF Core entity configuration for PotConfiguration and nested SeedAssignment.</summary>
internal sealed class PotConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<PotConfiguration>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PotConfiguration> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.PotId)
            .IsRequired();

        builder.Property(pc => pc.RoomAreaId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pc => pc.RoomName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(pc => pc.LastUpdated)
            .IsRequired();

        // Configure nested SeedAssignment collection as JSON
        builder.OwnsMany(pc => pc.CurrentSeeds, navigationBuilder =>
        {
            navigationBuilder.ToJson();

            navigationBuilder.WithOwner().HasForeignKey();

            navigationBuilder.Property(sa => sa.Id)
                .IsRequired();

            navigationBuilder.Property(sa => sa.PlantName)
                .HasMaxLength(100)
                .IsRequired();

            navigationBuilder.Property(sa => sa.SeedName)
                .HasMaxLength(100)
                .IsRequired();

            navigationBuilder.Property(sa => sa.PlantedDate)
                .IsRequired();

            navigationBuilder.Property(sa => sa.ExpectedHarvestDate);

            navigationBuilder.Property(sa => sa.Status)
                .HasMaxLength(50)
                .IsRequired();

            navigationBuilder.Property(sa => sa.Notes)
                .HasMaxLength(500);
        });

        // Ensure uniqueness: one configuration per pot
        builder.HasIndex(pc => pc.PotId)
            .IsUnique();
    }
}
