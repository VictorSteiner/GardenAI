using HomeAssistant.Domain.SensorReadings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeAssistant.Infrastructure.Persistence.SensorReadings.Configurations;

/// <summary>Entity mapping for <see cref="SensorReading"/>.</summary>
public sealed class SensorReadingEntityTypeConfiguration : IEntityTypeConfiguration<SensorReading>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SensorReading> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.PotId).IsRequired();
        builder.Property(r => r.Timestamp).IsRequired();
        builder.Property(r => r.SoilMoisture).IsRequired();
        builder.Property(r => r.TemperatureC).IsRequired();
        builder.HasIndex(r => new { r.PotId, r.Timestamp });
    }
}

