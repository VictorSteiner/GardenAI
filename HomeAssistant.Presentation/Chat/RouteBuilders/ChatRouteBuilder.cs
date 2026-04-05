using HomeAssistant.Presentation.Chat.Endpoints.CreateChatSession;
using HomeAssistant.Presentation.Chat.Endpoints.GetChatSession;
using HomeAssistant.Presentation.Chat.Endpoints.ListChatSessions;
using HomeAssistant.Presentation.Chat.Endpoints.PostChatPrompt;
using HomeAssistant.Presentation.Chat.Endpoints.PostChatSessionMessage;

namespace HomeAssistant.Presentation.Chat.RouteBuilders;

/// <summary>Builds all chat-related routes for the presentation layer.</summary>
public static class ChatRouteBuilder
{
    /// <summary>Maps all chat endpoints under the <c>/api/chat</c> route group.</summary>
    public static IEndpointRouteBuilder MapChatRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var chatGroup = endpoints.MapGroup("/api/chat")
            .WithTags("Chat");

        PostChatPromptEndpoint.Map(chatGroup);

        var sessionsGroup = chatGroup.MapGroup("/sessions");
        CreateChatSessionEndpoint.Map(sessionsGroup);
        ListChatSessionsEndpoint.Map(sessionsGroup);
        GetChatSessionEndpoint.Map(sessionsGroup);
        PostChatSessionMessageEndpoint.Map(sessionsGroup);

        return endpoints;
    }
}

