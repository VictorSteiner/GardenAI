using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Presentation.Garden.Contracts;

namespace HomeAssistant.Presentation.Garden.Mappings;

/// <summary>Maps Application-layer DTOs to Presentation-layer response contracts.</summary>
public interface IGardenPlannerMapper
{
    /// <summary>Maps a <see cref="HarvestReadinessDto"/> to its Presentation contract.</summary>
    HarvestReadinessResponse ToResponse(HarvestReadinessDto dto);

    /// <summary>Maps a collection of <see cref="HarvestReadinessDto"/> to Presentation contracts.</summary>
    IReadOnlyList<HarvestReadinessResponse> ToResponse(IReadOnlyList<HarvestReadinessDto> dtos);

    /// <summary>Maps a <see cref="DashboardAggregationDto"/> to its Presentation contract.</summary>
    DashboardAggregationResponse ToResponse(DashboardAggregationDto dto);

    /// <summary>Maps a <see cref="RoomSummaryDto"/> to its Presentation contract.</summary>
    RoomSummaryResponse ToResponse(RoomSummaryDto dto);
}

