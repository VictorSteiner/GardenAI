using HomeAssistant.Application.PotConfigurations.DTOs;

namespace HomeAssistant.Presentation.Garden.Contracts;

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

