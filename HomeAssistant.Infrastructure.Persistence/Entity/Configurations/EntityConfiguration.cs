using HomeAssistant.Domain.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeAssistant.Infrastructure.Persistence.Entity.Configurations;

/// <summary>EF Core configuration for the ha_entities table.</summary>
public sealed class EntityConfiguration : IEntityTypeConfiguration<EntityRecord>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<EntityRecord> builder)
    {
        builder.ToTable("ha_entities");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(x => x.DeviceId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.AreaId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.Platform).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.OriginalName).HasMaxLength(200);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
            .ValueGeneratedOnAdd();
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
            .ValueGeneratedOnAddOrUpdate();

        // Device FK: CASCADE — entity is removed when its device is removed
        builder.HasOne<HomeAssistant.Domain.Device.Entities.DeviceEntity>()
            .WithMany()
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Area FK: SET NULL — entity survives if its area override is removed
        builder.HasOne<HomeAssistant.Domain.Area.Entities.AreaEntity>()
            .WithMany()
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(x => x.DeviceId);
        builder.HasIndex(x => x.AreaId);
    }
}

