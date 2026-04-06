using GardenAI.Presentation.Chat.Endpoints.CreateChatSession;
using GardenAI.Presentation.Chat.Endpoints.GetChatSession;
using GardenAI.Presentation.Chat.Endpoints.ListChatSessions;
using GardenAI.Presentation.Chat.Endpoints.PostChatPrompt;
using GardenAI.Presentation.Chat.Endpoints.PostChatSessionMessage;

namespace GardenAI.Presentation.Chat.RouteBuilders;

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

