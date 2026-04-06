using HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools;

namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps protocol/tool endpoints for planner-orchestrated workflows.</summary>
public static class ProtocolToolsRouteBuilder
{
    /// <summary>Maps protocol tool endpoints under <c>/api/garden/planner/functions</c>.</summary>
    public static IEndpointRouteBuilder MapProtocolToolsRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGardenPlannerToolEndpoints();

        return endpoints;
    }
}

