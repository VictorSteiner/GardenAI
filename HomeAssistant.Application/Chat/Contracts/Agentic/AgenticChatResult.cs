namespace HomeAssistant.Application.Chat.Contracts.Agentic;

/// <summary>Result of an agentic chat turn.</summary>
public sealed record AgenticChatResult(
    string FinalReply,
    IReadOnlyList<ChatFunctionCall> ExecutedCalls);

