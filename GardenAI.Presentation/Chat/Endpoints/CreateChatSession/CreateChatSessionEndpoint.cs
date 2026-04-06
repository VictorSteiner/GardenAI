using GardenAI.Presentation.Chat.Contracts;
using GardenAI.Presentation.Chat.Endpoints.CreateChatSession.Contracts;
using GardenAI.Domain.Assistant.Abstractions;
using GardenAI.Domain.Assistant.Entities;

namespace GardenAI.Presentation.Chat.Endpoints.CreateChatSession;

/// <summary>Maps the endpoint that creates a persisted chat session.</summary>
internal static class CreateChatSessionEndpoint
{
    /// <summary>Maps the create chat session endpoint.</summary>
    internal static RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);

        return group.MapPost(
                string.Empty,
                async Task<IResult>(
                    CreateChatSessionRequest request,
                    IChatSessionRepository sessions,
                    CancellationToken ct) =>
                {
                    var now = DateTimeOffset.UtcNow;
                    var capability = string.IsNullOrWhiteSpace(request.Capability) ? "helper" : request.Capability.Trim();
                    var title = string.IsNullOrWhiteSpace(request.Title)
                        ? $"{capability} session {now:yyyy-MM-dd HH:mm}"
                        : request.Title.Trim();

                    var session = new ChatSession
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        Capability = capability,
                        CreatedAt = now,
                        UpdatedAt = now,
                    };

                    await sessions.CreateSessionAsync(session, ct);

                    var response = new ChatSessionSummaryResponse(
                        session.Id,
                        session.Title,
                        session.Capability,
                        session.CreatedAt,
                        session.UpdatedAt);

                    return TypedResults.Created($"/api/chat/sessions/{session.Id}", response);
                })
            .WithName("CreateChatSession")
            .Produces<ChatSessionSummaryResponse>(StatusCodes.Status201Created);
    }
}

