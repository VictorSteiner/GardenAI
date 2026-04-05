using HomeAssistant.Presentation.GardenAdvisor.Abstractions;

namespace HomeAssistant.Presentation.GardenAdvisor.Services;

/// <summary>
/// Thread-safe, in-memory rolling window of the garden planner conversation.
/// Registered as a singleton so history persists for the lifetime of the process.
/// </summary>
public sealed class GardenPlannerHistoryStore : IGardenPlannerHistoryStore
{
    /// <summary>Maximum number of messages retained (user + assistant combined).</summary>
    private const int MaxMessages = 30;

    private readonly List<GardenPlannerChatMessage> _messages = [];
    private readonly Lock _lock = new();

    /// <inheritdoc/>
    public void AddMessage(string role, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        lock (_lock)
        {
            _messages.Add(new GardenPlannerChatMessage(role, content, DateTimeOffset.UtcNow));
            while (_messages.Count > MaxMessages)
                _messages.RemoveAt(0);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<GardenPlannerChatMessage> GetHistory()
    {
        lock (_lock)
            return _messages.ToList().AsReadOnly();
    }

    /// <inheritdoc/>
    public void Clear()
    {
        lock (_lock)
            _messages.Clear();
    }
}

