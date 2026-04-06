namespace GardenAI.Domain.Assistant.Entities;

/// <summary>Represents one message in a chat session timeline.</summary>
public sealed class ChatMessage
{
    /// <summary>Unique identifier for this message.</summary>
    public Guid Id { get; init; }

    /// <summary>The parent session id this message belongs to.</summary>
    public Guid SessionId { get; init; }

    /// <summary>Message role. Expected values: <c>system</c>, <c>user</c>, <c>assistant</c>.</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>Message content.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>UTC timestamp when this message was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
