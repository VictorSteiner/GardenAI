
using HomeAssistant.Presentation.GardenAdvisor.GardenAdvice.RouteBuilders;

namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps all garden advisor routes through domain-sliced route builders.</summary>
public static class GardenAdvisorRouteBuilder
{
    /// <summary>Maps garden advisor routes under <c>/api/garden</c>.</summary>
    public static IEndpointRouteBuilder MapGardenAdvisorRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGardenPlanningRoutes();
        endpoints.MapPotManagementRoutes();
        endpoints.MapRoomInsightsRoutes();
        endpoints.MapGardenInsightsRoutes();
        endpoints.MapGardenAdviceRoutes();
        endpoints.MapProtocolToolsRoutes();

        return endpoints;
    }
}

