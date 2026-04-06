namespace HomeAssistant.Presentation.Garden.RouteBuilders;

/// <summary>Maps all /api/garden routes through URL-aligned sub-builders.</summary>
public static class GardenRouteBuilder
{
    /// <summary>Maps all garden routes under <c>/api/garden</c>.</summary>
    public static IEndpointRouteBuilder MapGardenRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGardenAdviceRoutes();
        endpoints.MapGardenPlannerRoutes();
        endpoints.MapGardenPlannerFunctionsRoutes();
        endpoints.MapGardenPotsRoutes();
        endpoints.MapGardenSeedsRoutes();
        endpoints.MapGardenRoomsRoutes();

        return endpoints;
    }
}

