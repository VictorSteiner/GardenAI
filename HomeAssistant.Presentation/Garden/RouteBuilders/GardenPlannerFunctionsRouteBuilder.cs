using HomeAssistant.Presentation.Garden.Planner.Functions;

namespace HomeAssistant.Presentation.Garden.RouteBuilders;

/// <summary>Maps /api/garden/planner/functions protocol-tool routes.</summary>
internal static class GardenPlannerFunctionsRouteBuilder
{
    /// <summary>Maps planner function (protocol-tool) endpoints under <c>/api/garden/planner/functions</c>.</summary>
    internal static IEndpointRouteBuilder MapGardenPlannerFunctionsRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGardenPlannerToolEndpoints();

        return endpoints;
    }
}

