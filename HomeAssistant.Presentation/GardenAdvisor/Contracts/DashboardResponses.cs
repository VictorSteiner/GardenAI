using HomeAssistant.Application.PotConfigurations.DTOs;

namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

/// <summary>Response model for harvest readiness.</summary>
public sealed record HarvestReadinessResponse(
    Guid SeedId,
    Guid PotId,
    string PlantName,
    string SeedName,
    string Status,
    int ReadinessScore,
    string ReadinessCategory,
    DateTimeOffset PlantedDate,
    DateTimeOffset? ExpectedHarvestDate,
    int DaysSincePlanting,
    int? DaysUntilHarvest)
{
    /// <summary>Factory method to create from DTO.</summary>
    public static HarvestReadinessResponse FromDto(HarvestReadinessDto dto)
    {
        return new HarvestReadinessResponse(
            dto.SeedId,
            dto.PotId,
            dto.PlantName,
            dto.SeedName,
            dto.Status,
            dto.ReadinessScore,
            dto.ReadinessCategory,
            dto.PlantedDate,
            dto.ExpectedHarvestDate,
            dto.DaysSincePlanting,
            dto.DaysUntilHarvest);
    }
}

/// <summary>Response model for a seed in dashboard context.</summary>
public sealed record SeedDashboardResponse(
    Guid SeedId,
    string PlantName,
    string SeedName,
    string Status,
    int ReadinessScore,
    string ReadinessCategory)
{
    /// <summary>Factory method to create from DTO.</summary>
    public static SeedDashboardResponse FromDto(SeedDashboardDto dto)
    {
        return new SeedDashboardResponse(
            dto.SeedId,
            dto.PlantName,
            dto.SeedName,
            dto.Status,
            dto.ReadinessScore,
            dto.ReadinessCategory);
    }
}

/// <summary>Response model for a pot in dashboard context.</summary>
public sealed record PotDashboardResponse(
    Guid PotId,
    string PotLabel,
    IReadOnlyList<SeedDashboardResponse> Seeds,
    int AverageReadiness,
    string HealthStatus)
{
    /// <summary>Factory method to create from DTO.</summary>
    public static PotDashboardResponse FromDto(PotDashboardDto dto)
    {
        return new PotDashboardResponse(
            dto.PotId,
            dto.PotLabel,
            dto.Seeds.Select(SeedDashboardResponse.FromDto).ToList().AsReadOnly(),
            dto.AverageReadiness,
            dto.HealthStatus);
    }
}

/// <summary>Response model for a room in dashboard context.</summary>
public sealed record RoomDashboardResponse(
    string RoomAreaId,
    string RoomName,
    IReadOnlyList<PotDashboardResponse> Pots)
{
    /// <summary>Factory method to create from DTO.</summary>
    public static RoomDashboardResponse FromDto(RoomDashboardDto dto)
    {
        return new RoomDashboardResponse(
            dto.RoomAreaId,
            dto.RoomName,
            dto.Pots.Select(PotDashboardResponse.FromDto).ToList().AsReadOnly());
    }
}

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

/// <summary>Response model for room summary.</summary>
public sealed record RoomSummaryResponse(
    string RoomAreaId,
    string RoomName,
    int PotCount,
    int ActiveSeedCount,
    int AverageReadiness,
    string HealthStatus,
    int ReadyToHarvestCount,
    int RipeningCount)
{
    /// <summary>Factory method to create from DTO.</summary>
    public static RoomSummaryResponse FromDto(RoomSummaryDto dto)
    {
        return new RoomSummaryResponse(
            dto.RoomAreaId,
            dto.RoomName,
            dto.PotCount,
            dto.ActiveSeedCount,
            dto.AverageReadiness,
            dto.HealthStatus,
            dto.ReadyToHarvestCount,
            dto.RipeningCount);
    }
}

