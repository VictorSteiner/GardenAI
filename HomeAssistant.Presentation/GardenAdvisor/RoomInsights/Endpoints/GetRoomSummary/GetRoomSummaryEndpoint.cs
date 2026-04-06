using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Application.PotConfigurations.Queries;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.GardenAdvisor.RoomInsights.Endpoints.GetRoomSummary;

/// <summary>Endpoint: GET /api/garden/rooms/{roomAreaId}/summary – Get aggregated summary for a room.</summary>
public sealed class GetRoomSummaryEndpoint
{
    /// <summary>Handles the GET request to retrieve room summary data.</summary>
    public static async Task<Results<Ok<RoomSummaryResponse>, BadRequest<string>>> Handle(
        string roomAreaId,
        IQueryHandler<GetRoomSummaryQuery, RoomSummaryDto> handler,
        ILogger<GetRoomSummaryEndpoint> logger,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(logger);

        if (string.IsNullOrWhiteSpace(roomAreaId))
            return TypedResults.BadRequest("Room area ID must not be empty.");

        var query = new GetRoomSummaryQuery(roomAreaId);
        var summaryDto = await handler.HandleAsync(query, ct);
        var response = RoomSummaryResponse.FromDto(summaryDto);

        logger.LogInformation("Retrieved room summary: room={RoomAreaId}, pots={PotCount}, avgReadiness={AvgReadiness}",
            roomAreaId, response.PotCount, response.AverageReadiness);

        return TypedResults.Ok(response);
    }
}

