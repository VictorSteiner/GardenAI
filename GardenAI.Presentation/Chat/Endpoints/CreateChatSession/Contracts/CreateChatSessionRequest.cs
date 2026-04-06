namespace GardenAI.Presentation.Chat.Endpoints.CreateChatSession.Contracts;

/// <summary>Request payload used to create a persisted chat session.</summary>
public sealed record CreateChatSessionRequest(string Title, string Capability);

