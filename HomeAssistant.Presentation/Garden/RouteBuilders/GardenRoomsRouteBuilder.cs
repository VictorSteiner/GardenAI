using HomeAssistant.Presentation.Garden.Contracts;
using HomeAssistant.Presentation.Garden.Rooms.Endpoints.GetAvailableRooms;
using HomeAssistant.Presentation.Garden.Rooms.Endpoints.GetRoomSummary;

namespace HomeAssistant.Presentation.Garden.RouteBuilders;

/// <summary>Maps /api/garden/rooms routes.</summary>
internal static class GardenRoomsRouteBuilder
{
    /// <summary>Maps room endpoints under <c>/api/garden/rooms</c>.</summary>
    internal static IEndpointRouteBuilder MapGardenRoomsRoutes(this IEndpointRouteBuilder endpoints)
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

