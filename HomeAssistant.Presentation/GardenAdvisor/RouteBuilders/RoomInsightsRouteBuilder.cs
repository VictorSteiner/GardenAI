namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps room-insight route boundaries for GardenAdvisor.</summary>
public static class RoomInsightsRouteBuilder
{
    /// <summary>
    /// Maps room-insight routes.
    /// Route paths are preserved by leaving this slice as a no-op until endpoint re-homing lands.
    /// </summary>
    public static IEndpointRouteBuilder MapRoomInsightsRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints;
    }
}

