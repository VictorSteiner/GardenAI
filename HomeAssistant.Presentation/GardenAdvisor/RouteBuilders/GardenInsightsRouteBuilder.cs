using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using HomeAssistant.Presentation.GardenAdvisor.GardenInsights.Endpoints.GetDashboard;
using HomeAssistant.Presentation.GardenAdvisor.GardenInsights.Endpoints.GetHarvestReadiness;

namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps garden-insight route boundaries for GardenAdvisor.</summary>
public static class GardenInsightsRouteBuilder
{
    /// <summary>Maps garden-insight routes while preserving the existing API paths.</summary>
    public static IEndpointRouteBuilder MapGardenInsightsRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var potsGroup = endpoints.MapGroup("/api/garden/pots")
            .WithTags("GardenPlanner");

        potsGroup
            .MapGet("/dashboard", GetDashboardEndpoint.Handle)
            .WithName("GetDashboard")
            .Produces<DashboardAggregationResponse>();

        var seedsGroup = endpoints.MapGroup("/api/garden/seeds")
            .WithTags("GardenPlanner");

        seedsGroup
            .MapGet("/harvest-readiness", GetHarvestReadinessEndpoint.Handle)
            .WithName("GetHarvestReadiness")
            .Produces<IReadOnlyList<HarvestReadinessResponse>>();

        return endpoints;
    }
}

