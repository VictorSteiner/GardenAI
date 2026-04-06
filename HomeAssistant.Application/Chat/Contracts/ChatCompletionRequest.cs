namespace HomeAssistant.Application.Chat.Contracts;

/// <summary>Request for a context-aware LLM completion call.</summary>
public sealed record ChatCompletionRequest(
    string SystemPrompt,
    string Prompt,
    IReadOnlyList<ChatHistoryMessage> History,
    string Capability);

