using HomeAssistant.Application.PotConfigurations.DTOs;

namespace HomeAssistant.Presentation.Garden.Contracts;

/// <summary>Response model for the dashboard aggregation.</summary>
public sealed record DashboardAggregationResponse(
    DateTimeOffset SnapshotAtUtc,
    IReadOnlyList<RoomDashboardResponse> Rooms,
    string OverallHealthStatus,
    int ReadyToHarvestCount,
    int RipeningCount,
    int GrowingCount,
    int CriticalPotsCount)
{
    /// <summary>Factory method to create from DTO.</summary>
    public static DashboardAggregationResponse FromDto(DashboardAggregationDto dto)
    {
        return new DashboardAggregationResponse(
            dto.SnapshotAtUtc,
            dto.Rooms.Select(RoomDashboardResponse.FromDto).ToList().AsReadOnly(),
            dto.OverallHealthStatus,
            dto.ReadyToHarvestCount,
            dto.RipeningCount,
            dto.GrowingCount,
            dto.CriticalPotsCount);
    }
}

