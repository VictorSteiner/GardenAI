using HomeAssistant.Presentation.GardenAdvisor.Services;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.GetAvailableRooms;

/// <summary>Endpoint: GET /api/garden/rooms – Retrieve all available Home Assistant rooms (areas).</summary>
public sealed class GetAvailableRoomsEndpoint
{
    /// <summary>Handles the GET request to retrieve available rooms.</summary>
    public static async Task<Ok<IReadOnlyList<RoomResponse>>> Handle(
        IHomeAssistantAreaProvider provider,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var rooms = await provider.GetAvailableRoomsAsync(ct);
        return TypedResults.Ok(rooms);
    }
}

