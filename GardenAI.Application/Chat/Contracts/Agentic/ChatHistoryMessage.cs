namespace GardenAI.Application.Chat.Contracts.Agentic;

/// <summary>Represents one history message that is forwarded to the LLM.</summary>
public sealed record ChatHistoryMessage(string Role, string Content);

