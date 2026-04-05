using HomeAssistant.Domain.Assistant.Abstractions;
using HomeAssistant.Domain.Assistant.Entities;
using HomeAssistant.Presentation.Chat.Services;

namespace HomeAssistant.Presentation.Chat.Endpoints.PostChatSessionMessage;

/// <summary>Maps the endpoint that posts a message into an existing chat session.</summary>
internal static class PostChatSessionMessageEndpoint
{
    /// <summary>Maps the post chat session message endpoint.</summary>
    internal static RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);

        return group.MapPost(
                "/{sessionId:guid}/messages",
                async Task<IResult>(
                    Guid sessionId,
                    PostChatSessionMessageRequest request,
                    IChatSessionRepository sessions,
                    IChatAssistant assistant,
                    IConfiguration configuration,
                    CancellationToken ct) =>
                {
                    if (string.IsNullOrWhiteSpace(request.Prompt))
                        return TypedResults.BadRequest("Prompt must not be empty.");

                    var session = await sessions.GetSessionAsync(sessionId, ct);
                    if (session is null)
                        return TypedResults.NotFound();

                    var now = DateTimeOffset.UtcNow;
                    var userMessage = new ChatMessage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = sessionId,
                        Role = "user",
                        Content = request.Prompt.Trim(),
                        CreatedAt = now,
                    };

                    await sessions.AddMessageAsync(userMessage, ct);

                    var maxHistory = int.TryParse(configuration["Assistant:MaxHistoryMessages"], out var parsedMax) ? parsedMax : 30;
                    var historyMessages = await sessions.GetMessagesAsync(sessionId, maxHistory, ct);

                    var completion = new ChatCompletionRequest(
                        ChatSystemPromptBuilder.Build(configuration, session.Capability),
                        request.Prompt.Trim(),
                        historyMessages
                            .Where(m => m.Role is "user" or "assistant")
                            .Select(m => new ChatHistoryMessage(m.Role, m.Content))
                            .ToList(),
                        session.Capability);

                    var reply = await assistant.GetReplyAsync(completion, ct);

                    var assistantMessage = new ChatMessage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = sessionId,
                        Role = "assistant",
                        Content = reply,
                        CreatedAt = DateTimeOffset.UtcNow,
                    };

                    await sessions.AddMessageAsync(assistantMessage, ct);

                    return TypedResults.Ok(new ChatSessionMessageResponse(
                        assistantMessage.Id,
                        assistantMessage.Role,
                        assistantMessage.Content,
                        assistantMessage.CreatedAt));
                })
            .WithName("PostChatSessionMessage")
            .WithSummary("Posts a message into an existing chat session and returns the assistant reply")
            .Produces<ChatSessionMessageResponse>()
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status502BadGateway);
    }
}

