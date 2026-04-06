namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PlannerFunctions.Contracts;

/// <summary>Request contract for room-based planner functions.</summary>
public sealed record RoomAreaFunctionRequest(
    /// <summary>Home Assistant area identifier such as <c>living_room</c>.</summary>
    string RoomAreaId);

