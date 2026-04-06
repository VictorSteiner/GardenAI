namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps garden-insight route boundaries for GardenAdvisor.</summary>
public static class GardenInsightsRouteBuilder
{
    /// <summary>
    /// Maps garden-insight routes.
    /// Route paths are preserved by leaving this slice as a no-op until endpoint re-homing lands.
    /// </summary>
    public static IEndpointRouteBuilder MapGardenInsightsRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints;
    }
}

