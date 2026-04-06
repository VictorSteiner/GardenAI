namespace HomeAssistant.Application.Chat.Contracts;

/// <summary>A single function call the AI decided to make.</summary>
public sealed record ChatFunctionCall(string FunctionName, string ArgumentsJson);

