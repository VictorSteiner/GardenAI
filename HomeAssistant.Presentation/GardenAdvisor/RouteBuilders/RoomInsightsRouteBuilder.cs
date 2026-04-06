using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using HomeAssistant.Presentation.GardenAdvisor.RoomInsights.Endpoints.GetAvailableRooms;
using HomeAssistant.Presentation.GardenAdvisor.RoomInsights.Endpoints.GetRoomSummary;

namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps room-insight route boundaries for GardenAdvisor.</summary>
public static class RoomInsightsRouteBuilder
{
    /// <summary>Maps room-insight routes while preserving the existing API paths.</summary>
    public static IEndpointRouteBuilder MapRoomInsightsRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var roomsGroup = endpoints.MapGroup("/api/garden/rooms")
            .WithTags("GardenPlanner");

        roomsGroup
            .MapGet(string.Empty, GetAvailableRoomsEndpoint.Handle)
            .WithName("GetAvailableRooms")
            .Produces<IReadOnlyList<RoomResponse>>();

        roomsGroup
            .MapGet("/{roomAreaId}/summary", GetRoomSummaryEndpoint.Handle)
            .WithName("GetRoomSummary")
            .Produces<RoomSummaryResponse>()
            .Produces<string>(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}

