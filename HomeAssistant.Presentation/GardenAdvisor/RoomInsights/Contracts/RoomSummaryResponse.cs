using HomeAssistant.Application.PotConfigurations.DTOs;

namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

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

