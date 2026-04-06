namespace HomeAssistant.Application.Chat.Contracts;

/// <summary>Describes a single function the AI may call.</summary>
public sealed record ChatToolDefinition(
    string Name,
    string Description,
    IReadOnlyDictionary<string, ChatToolParameterSchema> Properties,
    IReadOnlyList<string> Required);

