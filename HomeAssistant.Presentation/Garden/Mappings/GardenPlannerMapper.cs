using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Presentation.Garden.Contracts;

namespace HomeAssistant.Presentation.Garden.Mappings;

/// <summary>
/// Maps Application-layer DTOs to Presentation-layer response contracts.
/// Centralises all Domain ? Contract translations so contracts remain clean records.
/// </summary>
public sealed class GardenPlannerMapper : IGardenPlannerMapper
{
    /// <inheritdoc/>
    public HarvestReadinessResponse ToResponse(HarvestReadinessDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return HarvestReadinessResponse.FromDto(dto);
    }

    /// <inheritdoc/>
    public IReadOnlyList<HarvestReadinessResponse> ToResponse(IReadOnlyList<HarvestReadinessDto> dtos)
    {
        ArgumentNullException.ThrowIfNull(dtos);
        return dtos.Select(HarvestReadinessResponse.FromDto).ToList().AsReadOnly();
    }

    /// <inheritdoc/>
    public DashboardAggregationResponse ToResponse(DashboardAggregationDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return DashboardAggregationResponse.FromDto(dto);
    }

    /// <inheritdoc/>
    public RoomSummaryResponse ToResponse(RoomSummaryDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return RoomSummaryResponse.FromDto(dto);
    }
}

