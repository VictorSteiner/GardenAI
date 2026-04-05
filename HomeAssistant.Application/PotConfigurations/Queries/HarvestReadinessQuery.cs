using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Domain.Common.Markers;

namespace HomeAssistant.Application.PotConfigurations.Queries;

/// <summary>Query to retrieve harvest readiness for all seeds, optionally filtered by status.</summary>
/// <param name="FilterByStatus">Optional: filter seeds by status (e.g., "growing"). If null, returns all seeds.</param>
public sealed record GetHarvestReadinessQuery(string? FilterByStatus = null) : IQuery<IReadOnlyList<HarvestReadinessDto>>;

/// <summary>Query to retrieve a dashboard aggregation of all rooms, pots, and seeds with health statuses.</summary>
public sealed record GetDashboardAggregationQuery : IQuery<DashboardAggregationDto>;

/// <summary>Query to retrieve aggregated metrics for a specific room.</summary>
/// <param name="RoomAreaId">The Home Assistant area ID (e.g., "living_room").</param>
public sealed record GetRoomSummaryQuery(string RoomAreaId) : IQuery<RoomSummaryDto>;

