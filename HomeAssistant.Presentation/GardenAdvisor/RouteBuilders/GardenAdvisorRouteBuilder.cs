using HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostGardenPlannerChat;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;

namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps all garden advisor endpoints.</summary>
public static class GardenAdvisorRouteBuilder
{
    /// <summary>Maps garden advisor routes under <c>/api/garden</c>.</summary>
    public static IEndpointRouteBuilder MapGardenAdvisorRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var plannerGroup = endpoints.MapGroup("/api/garden/planner")
            .WithTags("GardenPlanner");

        plannerGroup
            .MapPost("/chat", PostGardenPlannerChatEndpoint.Handle)
            .WithName("GardenPlannerChat")
            .WithOpenApi()
            .Accepts<GardenPlannerChatRequest>("application/json")
            .Produces<GardenPlannerChatResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);


        return endpoints;
    }
}

