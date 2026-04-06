namespace GardenAI.Presentation.Chat.Contracts;

/// <summary>Describes a single function the AI may call.</summary>
/// <param name="Name">Machine-readable function name (snake_case).</param>
/// <param name="Description">Natural-language description for the model.</param>
/// <param name="Properties">Schema properties keyed by parameter name.</param>
/// <param name="Required">Parameters that must be present in any call.</param>
public sealed record ChatToolDefinition(
    string Name,
    string Description,
    IReadOnlyDictionary<string, ChatToolParameterSchema> Properties,
    IReadOnlyList<string> Required);

/// <summary>JSON-Schema fragment for a single tool parameter.</summary>
/// <param name="Type">JSON type: "string", "integer", "number", "boolean".</param>
/// <param name="Description">Human-readable parameter hint.</param>
/// <param name="Enum">Optional fixed value set.</param>
public sealed record ChatToolParameterSchema(
    string Type,
    string Description = null,
    IReadOnlyList<string> Enum = null);

/// <summary>A single function call the AI decided to make.</summary>
/// <param name="FunctionName">Name matching a <see cref="ChatToolDefinition.Name"/>.</param>
/// <param name="ArgumentsJson">Raw JSON object of arguments.</param>
public sealed record ChatFunctionCall(string FunctionName, string ArgumentsJson);

/// <summary>Result of an agentic (tool-capable) chat turn.</summary>
/// <param name="FinalReply">The model's final natural-language text response.</param>
/// <param name="ExecutedCalls">All tool calls that were executed during this turn.</param>
public sealed record AgenticChatResult(
    string FinalReply,
    IReadOnlyList<ChatFunctionCall> ExecutedCalls);

