using HomeAssistant.Presentation.GardenAdvisor.GardenAdvice.Endpoints.GetLatestGardenAdvice;
using HomeAssistant.Presentation.GardenAdvisor.GardenAdvice.Endpoints.PostGenerateGardenAdvice;

namespace HomeAssistant.Presentation.GardenAdvisor.GardenAdvice.RouteBuilders;

/// <summary>Maps garden-advice route boundaries for GardenAdvisor.</summary>
public static class GardenAdviceRouteBuilder
{
    /// <summary>Maps garden-advice routes while preserving the existing API paths.</summary>
    public static IEndpointRouteBuilder MapGardenAdviceRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var adviceGroup = endpoints.MapGroup("/api/garden/advice")
            .WithTags("GardenPlanner");

        PostGenerateGardenAdviceEndpoint.Map(adviceGroup);
        GetLatestGardenAdviceEndpoint.Map(adviceGroup);

        return endpoints;
    }
}


