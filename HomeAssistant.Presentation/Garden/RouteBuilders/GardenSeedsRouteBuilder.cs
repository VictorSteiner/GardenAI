using HomeAssistant.Presentation.Garden.Seeds.HarvestReadiness;
using HomeAssistant.Presentation.Garden.Contracts;

namespace HomeAssistant.Presentation.Garden.RouteBuilders;

/// <summary>Maps /api/garden/seeds routes.</summary>
internal static class GardenSeedsRouteBuilder
{
    /// <summary>Maps seed harvest-readiness endpoints under <c>/api/garden/seeds</c>.</summary>
    internal static IEndpointRouteBuilder MapGardenSeedsRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var seedsGroup = endpoints.MapGroup("/api/garden/seeds")
            .WithTags("GardenPlanner");

        seedsGroup
            .MapGet("/harvest-readiness", GetHarvestReadinessEndpoint.Handle)
            .WithName("GetHarvestReadiness")
            .Produces<IReadOnlyList<HarvestReadinessResponse>>();

        return endpoints;
    }
}

