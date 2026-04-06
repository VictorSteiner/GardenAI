namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps garden-advice route boundaries for GardenAdvisor.</summary>
public static class GardenAdviceRouteBuilder
{
    /// <summary>
    /// Maps garden-advice routes.
    /// Route paths are preserved by leaving this slice as a no-op until endpoint re-homing lands.
    /// </summary>
    public static IEndpointRouteBuilder MapGardenAdviceRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints;
    }
}

