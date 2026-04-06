using HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostGardenPlannerChat.Contracts;

namespace HomeAssistant.Presentation.GardenAdvisor.Abstractions;

/// <summary>
/// Processes natural-language garden planning messages, enriches them with live garden context,
/// uses the AI to respond, and automatically executes any detected pot-configuration actions.
/// </summary>
public interface IGardenPlannerService
{
    /// <summary>
    /// Sends a natural-language message to the AI planner, executes any detected garden
    /// actions (e.g. "plant basil in pot 1"), and returns the AI's reply.
    /// </summary>
    /// <param name="message">The user's free-form message.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GardenPlannerChatResponse> ChatAsync(string message, CancellationToken ct = default);
}

