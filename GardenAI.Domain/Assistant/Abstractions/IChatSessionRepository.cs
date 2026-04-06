using GardenAI.Domain.Assistant.Entities;

namespace GardenAI.Domain.Assistant.Abstractions;

/// <summary>Defines persistence operations for assistant chat sessions and messages.</summary>
public interface IChatSessionRepository
{
    /// <summary>Creates a new chat session.</summary>
    Task CreateSessionAsync(ChatSession session, CancellationToken ct = default);

    /// <summary>Returns the chat session by id, or <c>null</c> when not found.</summary>
    Task<ChatSession> GetSessionAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>Returns recent chat sessions ordered by latest activity.</summary>
    Task<IReadOnlyList<ChatSession>> ListSessionsAsync(int limit = 50, CancellationToken ct = default);

    /// <summary>Returns messages for a session, ordered oldest to newest.</summary>
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid sessionId, int take = 100, CancellationToken ct = default);

    /// <summary>Appends a message to a session and updates session activity timestamp.</summary>
    Task AddMessageAsync(ChatMessage message, CancellationToken ct = default);
}
