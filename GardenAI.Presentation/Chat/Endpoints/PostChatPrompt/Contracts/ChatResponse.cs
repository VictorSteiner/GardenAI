namespace GardenAI.Presentation.Chat.Endpoints.PostChatPrompt.Contracts;

/// <summary>Response payload returned by the chatbot helper endpoint.</summary>
public sealed record ChatResponse(string Reply, string Model);

