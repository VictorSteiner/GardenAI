namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

/// <summary>Request body for the garden planner chat endpoint.</summary>
public sealed record GardenPlannerChatRequest(
    /// <summary>The user's natural-language message (e.g. "I'm planting basil in pot 1").</summary>
    string Message);

/// <summary>Response from the garden planner chat endpoint.</summary>
public sealed record GardenPlannerChatResponse(
    /// <summary>The AI assistant's reply with action blocks stripped out.</summary>
    string Reply,
    /// <summary>Human-readable summaries of each pot-configuration action that was executed.</summary>
    IReadOnlyList<string> ActionsExecuted,
    /// <summary>UTC timestamp when the reply was generated.</summary>
    DateTimeOffset GeneratedAtUtc);

