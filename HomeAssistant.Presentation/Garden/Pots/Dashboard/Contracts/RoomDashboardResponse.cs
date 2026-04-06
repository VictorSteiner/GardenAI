using HomeAssistant.Application.PotConfigurations.DTOs;

namespace HomeAssistant.Presentation.Garden.Contracts;

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

