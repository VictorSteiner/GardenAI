using HomeAssistant.Application.Chat.Contracts.Agentic;
using HomeAssistant.Application.Chat.Contracts.Completions;

namespace HomeAssistant.Application.Chat.Abstractions;

/// <summary>Provides chat completions for helper-style conversational responses.</summary>
public interface IChatAssistant
{
    /// <summary>Sends a contextual completion request to the configured LLM and returns its response text.</summary>
    Task<string> GetReplyAsync(ChatCompletionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Runs an agentic loop and may execute tool calls until a final response is produced.
    /// </summary>
    Task<AgenticChatResult> GetAgenticReplyAsync(
        string systemPrompt,
        IReadOnlyList<ChatHistoryMessage> history,
        string userMessage,
        IReadOnlyList<ChatToolDefinition> tools,
        Func<ChatFunctionCall, CancellationToken, Task<string>> toolExecutor,
        int maxIterations = 5,
        CancellationToken ct = default);
}

