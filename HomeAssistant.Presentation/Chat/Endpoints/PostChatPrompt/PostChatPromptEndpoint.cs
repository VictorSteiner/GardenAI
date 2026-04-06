using HomeAssistant.Application.Chat.Abstractions;
using HomeAssistant.Application.Chat.Contracts;
using HomeAssistant.Presentation.Chat.Services;

namespace HomeAssistant.Presentation.Chat.Endpoints.PostChatPrompt;

/// <summary>Maps the endpoint that sends a single prompt to the chat assistant.</summary>
internal static class PostChatPromptEndpoint
{
    /// <summary>Maps the post chat prompt endpoint.</summary>
    internal static RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);

        return group.MapPost(
                string.Empty,
                async Task<IResult>(
                    ChatRequest request,
                    HomeAssistant.Application.Chat.Abstractions.IChatAssistant assistant,
                    IConfiguration configuration,
                    CancellationToken ct) =>
                {
                    if (string.IsNullOrWhiteSpace(request.Prompt))
                        return TypedResults.BadRequest("Prompt must not be empty.");

                    try
                    {
                        var completion = new HomeAssistant.Application.Chat.Contracts.ChatCompletionRequest(
                            ChatSystemPromptBuilder.Build(configuration, "helper"),
                            request.Prompt,
                            [],
                            "helper");

                        var reply = await assistant.GetReplyAsync(completion, ct);
                        var model = configuration["Ollama:Model"] ?? "llama3.2:3b";
                        return TypedResults.Ok(new ChatResponse(reply, model));
                    }
                    catch (Exception ex)
                    {
                        return TypedResults.Problem(
                            detail: ex.Message,
                            title: "Chat assistant request failed",
                            statusCode: StatusCodes.Status502BadGateway);
                    }
                })
            .WithName("PostChatPrompt")
            .WithSummary("Gets a helper response from the Ollama chatbot")
            .WithDescription("Sends a single prompt to Ollama and returns the generated reply.")
            .Produces<ChatResponse>()
            .Produces<string>(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status502BadGateway);
    }
}

