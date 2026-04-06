using HomeAssistant.Presentation.Garden.Planner.Chat.Contracts;
using HomeAssistant.Presentation.Garden.Planner.Chat;

namespace HomeAssistant.Presentation.Garden.RouteBuilders;

/// <summary>Maps /api/garden/planner routes.</summary>
internal static class GardenPlannerRouteBuilder
{
    /// <summary>Maps planner chat endpoints under <c>/api/garden/planner</c>.</summary>
    internal static IEndpointRouteBuilder MapGardenPlannerRoutes(this IEndpointRouteBuilder endpoints)
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

