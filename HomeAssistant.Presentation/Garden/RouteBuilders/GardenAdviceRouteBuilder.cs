using HomeAssistant.Presentation.Garden.Advice.Endpoints.GetLatestGardenAdvice;
using HomeAssistant.Presentation.Garden.Advice.Endpoints.PostGenerateGardenAdvice;

namespace HomeAssistant.Presentation.Garden.RouteBuilders;

/// <summary>Maps /api/garden/advice routes.</summary>
internal static class GardenAdviceRouteBuilder
{
    /// <summary>Maps garden advice endpoints under <c>/api/garden/advice</c>.</summary>
    internal static IEndpointRouteBuilder MapGardenAdviceRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var adviceGroup = endpoints.MapGroup("/api/garden/advice")
            .WithTags("GardenPlanner");

        PostGenerateGardenAdviceEndpoint.Map(adviceGroup);
        GetLatestGardenAdviceEndpoint.Map(adviceGroup);

        return endpoints;
    }
}

