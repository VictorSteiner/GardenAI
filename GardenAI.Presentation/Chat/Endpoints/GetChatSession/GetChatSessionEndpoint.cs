using GardenAI.Presentation.Chat.Contracts;
using GardenAI.Presentation.Chat.Endpoints.GetChatSession.Contracts;
using GardenAI.Domain.Assistant.Abstractions;

namespace GardenAI.Presentation.Chat.Endpoints.GetChatSession;

/// <summary>Maps the endpoint that retrieves a persisted chat session with its message history.</summary>
internal static class GetChatSessionEndpoint
{
    /// <summary>Maps the get chat session endpoint.</summary>
    internal static RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);

        return group.MapGet(
                "/{sessionId:guid}",
                async Task<IResult>(Guid sessionId, IChatSessionRepository sessions, CancellationToken ct) =>
                {
                    var session = await sessions.GetSessionAsync(sessionId, ct);
                    if (session is null)
                        return TypedResults.NotFound();

                    var messages = await sessions.GetMessagesAsync(sessionId, 200, ct);
                    var response = new ChatSessionDetailResponse(
                        session.Id,
                        session.Title,
                        session.Capability,
                        session.CreatedAt,
                        session.UpdatedAt,
                        messages.Select(m => new ChatSessionMessageResponse(m.Id, m.Role, m.Content, m.CreatedAt)).ToList());

                    return TypedResults.Ok(response);
                })
            .WithName("GetChatSession")
            .Produces<ChatSessionDetailResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}

