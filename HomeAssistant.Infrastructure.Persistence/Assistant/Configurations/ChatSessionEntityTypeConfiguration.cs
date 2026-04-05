using HomeAssistant.Domain.Assistant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeAssistant.Infrastructure.Persistence.Assistant.Configurations;

/// <summary>Entity mapping for <see cref="ChatSession"/>.</summary>
public sealed class ChatSessionEntityTypeConfiguration : IEntityTypeConfiguration<ChatSession>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Capability).IsRequired().HasMaxLength(100);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
        builder.HasIndex(s => s.UpdatedAt);
    }
}

