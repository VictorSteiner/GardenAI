using HomeAssistant.Application.PotConfigurations.DTOs;

namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

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

