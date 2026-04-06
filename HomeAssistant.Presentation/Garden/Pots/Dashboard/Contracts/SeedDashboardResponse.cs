using HomeAssistant.Application.PotConfigurations.DTOs;

namespace HomeAssistant.Presentation.Garden.Contracts;

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

