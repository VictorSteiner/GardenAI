namespace HomeAssistant.Application.Chat.Contracts;

/// <summary>JSON-Schema fragment for a single tool parameter.</summary>
public sealed record ChatToolParameterSchema(
    string Type,
    string? Description = null,
    IReadOnlyList<string>? Enum = null);

