namespace GardenAI.Presentation.Chat.Endpoints.PostChatSessionMessage.Contracts;

/// <summary>Request payload for posting a message to an existing session.</summary>
public sealed record PostChatSessionMessageRequest(string Prompt);

