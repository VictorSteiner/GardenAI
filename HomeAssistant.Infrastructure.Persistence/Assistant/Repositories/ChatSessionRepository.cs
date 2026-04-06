using HomeAssistant.Domain.Assistant.Abstractions;
using HomeAssistant.Domain.Assistant.Entities;
using HomeAssistant.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Infrastructure.Persistence.Assistant.Repositories;

/// <summary>EF Core implementation of <see cref="IChatSessionRepository"/>.</summary>
public sealed class ChatSessionRepository : IChatSessionRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChatSessionRepository> _logger;

    /// <summary>Initialises the repository with the provided database context.</summary>
    public ChatSessionRepository(AppDbContext context, ILogger<ChatSessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task CreateSessionAsync(ChatSession session, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created chat session {SessionId} with capability {Capability}.", session.Id, session.Capability);
    }

    /// <inheritdoc/>
    public async Task<ChatSession> GetSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException("Session id must not be empty.", nameof(sessionId));

        return await _context.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChatSession>> ListSessionsAsync(int limit = 50, CancellationToken ct = default)
    {
        if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit));

        var boundedLimit = Math.Min(limit, 200);
        return await _context.ChatSessions
            .AsNoTracking()
            .OrderByDescending(s => s.UpdatedAt)
            .Take(boundedLimit)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid sessionId, int take = 100, CancellationToken ct = default)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException("Session id must not be empty.", nameof(sessionId));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        var boundedTake = Math.Min(take, 500);

        return await _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(boundedTake)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task AddMessageAsync(ChatMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var session = await _context.ChatSessions.FirstOrDefaultAsync(s => s.Id == message.SessionId, ct);
        if (session is null)
            throw new InvalidOperationException($"Session '{message.SessionId}' was not found.");

        _context.ChatMessages.Add(message);
        session.UpdatedAt = message.CreatedAt;

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Added {Role} message {MessageId} to session {SessionId}.", message.Role, message.Id, message.SessionId);
    }
}

