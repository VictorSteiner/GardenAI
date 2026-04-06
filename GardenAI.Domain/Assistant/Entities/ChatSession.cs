namespace GardenAI.Domain.Assistant.Entities;

/// <summary>Represents a persisted conversation session with the assistant.</summary>
public sealed class ChatSession
{
    /// <summary>Unique identifier for the chat session.</summary>
    public Guid Id { get; set; }

    /// <summary>Human-friendly title for the session.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Capability mode used by this session (for example: helper, seed-planning).</summary>
    public string Capability { get; set; } = "helper";

    /// <summary>UTC timestamp when the session was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>UTC timestamp when the session last received a message.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
