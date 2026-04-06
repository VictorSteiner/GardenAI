using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Application.PotConfigurations.Queries;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Presentation.Garden.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.Garden.Pots.Dashboard.Endpoints.GetDashboard;

/// <summary>Endpoint: GET /api/garden/pots/dashboard – Get dashboard aggregation across all rooms and pots.</summary>
public sealed class GetDashboardEndpoint
{
    /// <summary>Handles the GET request to retrieve dashboard aggregation data.</summary>
    public static async Task<Ok<DashboardAggregationResponse>> Handle(
        IQueryHandler<GetDashboardAggregationQuery, DashboardAggregationDto> handler,
        ILogger<GetDashboardEndpoint> logger,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(logger);

        var query = new GetDashboardAggregationQuery();
        var dashboardDto = await handler.HandleAsync(query, ct);
        var response = DashboardAggregationResponse.FromDto(dashboardDto);

        logger.LogInformation("Retrieved dashboard aggregation: {RoomCount} rooms, overallStatus={OverallStatus}",
            response.Rooms.Count, response.OverallHealthStatus);

        return TypedResults.Ok(response);
    }
}

