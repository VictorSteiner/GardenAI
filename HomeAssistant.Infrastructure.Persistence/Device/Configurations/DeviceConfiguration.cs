using HomeAssistant.Domain.Device.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeAssistant.Infrastructure.Persistence.Device.Configurations;

/// <summary>EF Core configuration for the ha_devices table.</summary>
public sealed class DeviceConfiguration : IEntityTypeConfiguration<DeviceEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<DeviceEntity> builder)
    {
        builder.ToTable("ha_devices");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(x => x.AreaId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameByUser).HasMaxLength(200);
        builder.Property(x => x.Manufacturer).HasMaxLength(200);
        builder.Property(x => x.Model).HasMaxLength(200);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
            .ValueGeneratedOnAdd();
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
            .ValueGeneratedOnAddOrUpdate();

        // Area FK: SET NULL — device survives if area is removed
        builder.HasOne<HomeAssistant.Domain.Area.Entities.AreaEntity>()
            .WithMany()
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(x => x.AreaId);
    }
}

