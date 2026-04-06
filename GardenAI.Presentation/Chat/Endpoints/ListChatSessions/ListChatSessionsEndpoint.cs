using GardenAI.Presentation.Chat.Contracts;
using GardenAI.Domain.Assistant.Abstractions;

namespace GardenAI.Presentation.Chat.Endpoints.ListChatSessions;

/// <summary>Maps the endpoint that lists persisted chat sessions.</summary>
internal static class ListChatSessionsEndpoint
{
    /// <summary>Maps the list chat sessions endpoint.</summary>
    internal static RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);

        return group.MapGet(
                string.Empty,
                async Task<IResult>(int? limit, IChatSessionRepository sessions, CancellationToken ct) =>
                {
                    var resolvedLimit = limit is > 0 ? limit.Value : 50;
                    var items = await sessions.ListSessionsAsync(resolvedLimit, ct);
                    var response = items
                        .Select(s => new ChatSessionSummaryResponse(s.Id, s.Title, s.Capability, s.CreatedAt, s.UpdatedAt))
                        .ToList();

                    return TypedResults.Ok(response);
                })
            .WithName("ListChatSessions")
            .Produces<IReadOnlyList<ChatSessionSummaryResponse>>();
    }
}

