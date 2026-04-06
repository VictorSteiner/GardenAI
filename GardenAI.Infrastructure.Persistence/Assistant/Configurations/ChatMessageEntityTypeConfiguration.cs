using GardenAI.Domain.Assistant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GardenAI.Infrastructure.Persistence.Assistant.Configurations;

/// <summary>Entity mapping for <see cref="ChatMessage"/>.</summary>
public sealed class ChatMessageEntityTypeConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.SessionId).IsRequired();
        builder.Property(m => m.Role).IsRequired().HasMaxLength(32);
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.HasIndex(m => new { m.SessionId, m.CreatedAt });
        builder.HasOne<ChatSession>()
            .WithMany()
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

