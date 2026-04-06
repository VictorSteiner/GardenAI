using GardenAI.Application.Chat.Contracts.Agentic;

namespace GardenAI.Application.Chat.Contracts.Completions;

/// <summary>Request for a context-aware LLM completion call.</summary>
public sealed record ChatCompletionRequest(
    string SystemPrompt,
    string Prompt,
    IReadOnlyList<ChatHistoryMessage> History,
    string Capability);

