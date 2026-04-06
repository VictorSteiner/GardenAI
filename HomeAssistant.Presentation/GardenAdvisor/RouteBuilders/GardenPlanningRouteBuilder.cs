using HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostGardenPlannerChat;
using HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostGardenPlannerChat.Contracts;

namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps garden planning routes.</summary>
public static class GardenPlanningRouteBuilder
{
    /// <summary>Maps planning-related routes under <c>/api/garden/planner</c>.</summary>
    public static IEndpointRouteBuilder MapGardenPlanningRoutes(this IEndpointRouteBuilder endpoints)
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

