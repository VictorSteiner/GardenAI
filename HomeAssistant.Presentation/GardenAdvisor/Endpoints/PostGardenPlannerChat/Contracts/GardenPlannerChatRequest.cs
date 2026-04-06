namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostGardenPlannerChat.Contracts;

/// <summary>Request body for the garden planner chat endpoint.</summary>
public sealed record GardenPlannerChatRequest(
    /// <summary>The user's natural-language message (for example <c>I'm planting basil in pot 1</c>).</summary>
    string Message);

