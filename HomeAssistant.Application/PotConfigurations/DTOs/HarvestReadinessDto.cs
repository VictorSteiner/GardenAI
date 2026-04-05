namespace HomeAssistant.Application.PotConfigurations.DTOs;

/// <summary>DTO representing harvest readiness for a single seed.</summary>
public sealed record HarvestReadinessDto(
    /// <summary>Unique seed assignment ID.</summary>
    Guid SeedId,
    /// <summary>Pot ID this seed is in.</summary>
    Guid PotId,
    /// <summary>Common plant name (e.g., "Tomato").</summary>
    string PlantName,
    /// <summary>Specific seed/cultivar name (e.g., "Moneymaker").</summary>
    string SeedName,
    /// <summary>Current lifecycle status ("growing", "mature", "harvested", "removed").</summary>
    string Status,
    /// <summary>Readiness score (0–100).</summary>
    int ReadinessScore,
    /// <summary>Readiness category ("not-ready", "ripening", "ready-soon", "ready-now").</summary>
    string ReadinessCategory,
    /// <summary>Date the seed was planted.</summary>
    DateTimeOffset PlantedDate,
    /// <summary>Expected harvest date (if known).</summary>
    DateTimeOffset? ExpectedHarvestDate,
    /// <summary>Days since planting.</summary>
    int DaysSincePlanting,
    /// <summary>Days until expected harvest (if known).</summary>
    int? DaysUntilHarvest);

/// <summary>DTO for a single seed in dashboard context.</summary>
public sealed record SeedDashboardDto(
    /// <summary>Unique seed assignment ID.</summary>
    Guid SeedId,
    /// <summary>Plant name.</summary>
    string PlantName,
    /// <summary>Seed name.</summary>
    string SeedName,
    /// <summary>Current status.</summary>
    string Status,
    /// <summary>Readiness score (0–100).</summary>
    int ReadinessScore,
    /// <summary>Readiness category.</summary>
    string ReadinessCategory);

/// <summary>DTO for a single pot in dashboard context.</summary>
public sealed record PotDashboardDto(
    /// <summary>Pot ID.</summary>
    Guid PotId,
    /// <summary>Pot position/label.</summary>
    string PotLabel,
    /// <summary>List of active seeds.</summary>
    IReadOnlyList<SeedDashboardDto> Seeds,
    /// <summary>Average readiness across all seeds (0–100).</summary>
    int AverageReadiness,
    /// <summary>Overall health status ("healthy", "warning", "critical").</summary>
    string HealthStatus);

/// <summary>DTO for a room's pot aggregation.</summary>
public sealed record RoomDashboardDto(
    /// <summary>Home Assistant area ID.</summary>
    string RoomAreaId,
    /// <summary>Display name of the room.</summary>
    string RoomName,
    /// <summary>List of pots in this room.</summary>
    IReadOnlyList<PotDashboardDto> Pots);

/// <summary>Dashboard aggregation across all rooms and pots.</summary>
public sealed record DashboardAggregationDto(
    /// <summary>Snapshot timestamp (UTC).</summary>
    DateTimeOffset SnapshotAtUtc,
    /// <summary>List of rooms with their pot/seed aggregations.</summary>
    IReadOnlyList<RoomDashboardDto> Rooms,
    /// <summary>Overall system health status.</summary>
    string OverallHealthStatus,
    /// <summary>Count of seeds ready to harvest.</summary>
    int ReadyToHarvestCount,
    /// <summary>Count of seeds ripening.</summary>
    int RipeningCount,
    /// <summary>Count of seeds still growing.</summary>
    int GrowingCount,
    /// <summary>Count of pots in critical condition.</summary>
    int CriticalPotsCount);

/// <summary>DTO for a room summary (aggregated metrics).</summary>
public sealed record RoomSummaryDto(
    /// <summary>Home Assistant area ID.</summary>
    string RoomAreaId,
    /// <summary>Display name of the room.</summary>
    string RoomName,
    /// <summary>Number of pots in the room.</summary>
    int PotCount,
    /// <summary>Number of active seeds.</summary>
    int ActiveSeedCount,
    /// <summary>Average readiness across all seeds in the room (0–100).</summary>
    int AverageReadiness,
    /// <summary>Overall room health status.</summary>
    string HealthStatus,
    /// <summary>Seeds ready to harvest in this room.</summary>
    int ReadyToHarvestCount,
    /// <summary>Seeds ripening in this room.</summary>
    int RipeningCount);

