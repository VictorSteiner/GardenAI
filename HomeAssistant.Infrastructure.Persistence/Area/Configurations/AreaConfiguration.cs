using HomeAssistant.Domain.Area.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeAssistant.Infrastructure.Persistence.Area.Configurations;

/// <summary>EF Core configuration for the ha_areas table.</summary>
public sealed class AreaConfiguration : IEntityTypeConfiguration<AreaEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AreaEntity> builder)
    {
        builder.ToTable("ha_areas");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.Aliases).HasColumnType("text").IsRequired(false);
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
            .ValueGeneratedOnAdd();
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(x => x.Name);
    }
}

