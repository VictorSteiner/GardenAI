namespace HomeAssistant.Presentation.Chat;

/// <summary>Provides chat completions for helper-style conversational responses.</summary>
public interface IChatAssistant
{
    /// <summary>Sends a contextual completion request to the configured LLM and returns its response text.</summary>
    Task<string> GetReplyAsync(ChatCompletionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Runs an agentic loop: sends the conversation with tool definitions, executes any tool
    /// calls via <paramref name="toolExecutor"/>, feeds results back, and repeats until the
    /// model returns a plain text response or <paramref name="maxIterations"/> is reached.
    /// </summary>
    /// <param name="systemPrompt">System-role instruction for the model.</param>
    /// <param name="history">Prior conversation turns (user + assistant pairs).</param>
    /// <param name="userMessage">The new user message for this turn.</param>
    /// <param name="tools">Tool definitions the model may invoke.</param>
    /// <param name="toolExecutor">Delegate that executes a tool call and returns its result string.</param>
    /// <param name="maxIterations">Safety cap on tool-calling rounds (default 5).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<AgenticChatResult> GetAgenticReplyAsync(
        string systemPrompt,
        IReadOnlyList<ChatHistoryMessage> history,
        string userMessage,
        IReadOnlyList<ChatToolDefinition> tools,
        Func<ChatFunctionCall, CancellationToken, Task<string>> toolExecutor,
        int maxIterations = 5,
        CancellationToken ct = default);
}

