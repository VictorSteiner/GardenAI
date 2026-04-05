namespace HomeAssistant.Presentation.GardenAdvisor.Abstractions;

/// <summary>Represents a single message in the garden planner conversation history.</summary>
/// <param name="Role">"user" or "assistant".</param>
/// <param name="Content">The message text.</param>
/// <param name="Timestamp">When the message was added.</param>
public sealed record GardenPlannerChatMessage(string Role, string Content, DateTimeOffset Timestamp);

/// <summary>In-memory store for the rolling garden planner conversation history.</summary>
public interface IGardenPlannerHistoryStore
{
    /// <summary>Appends a message to the history, evicting oldest if at capacity.</summary>
    void AddMessage(string role, string content);

    /// <summary>Returns the full conversation history, oldest first.</summary>
    IReadOnlyList<GardenPlannerChatMessage> GetHistory();

    /// <summary>Clears the entire conversation history.</summary>
    void Clear();
}

