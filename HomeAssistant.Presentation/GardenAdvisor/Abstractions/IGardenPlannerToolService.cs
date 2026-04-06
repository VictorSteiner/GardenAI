using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools.Contracts;
using HomeAssistant.Application.GardenAdvisor.Contracts.Advice;
using AppGardenAdviceResponse = HomeAssistant.Application.GardenAdvisor.Contracts.Advice.GardenAdviceResponse;
namespace HomeAssistant.Presentation.GardenAdvisor.Abstractions;
public interface IGardenPlannerToolService
{
    Task<string> SavePotConfigurationAsync(SavePotConfigurationRequest request, CancellationToken ct = default);
    Task<string> UpdateSeedStatusAsync(UpdateSeedStatusRequest request, CancellationToken ct = default);
    Task<string> GetPotStatusAsync(PotNumberRequest request, CancellationToken ct = default);
    Task<string> GetAllPotsStatusAsync(CancellationToken ct = default);
    Task<string> GetSensorReadingsAsync(PotNumberRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<RoomResponse>> GetAvailableRoomsAsync(CancellationToken ct = default);
    Task<RoomSummaryResponse> GetRoomSummaryAsync(RoomAreaRequest request, CancellationToken ct = default);
    Task<DashboardAggregationResponse> GetDashboardAsync(CancellationToken ct = default);
    Task<IReadOnlyList<HarvestReadinessResponse>> GetHarvestReadinessAsync(HarvestReadinessRequest request, CancellationToken ct = default);
    AppGardenAdviceResponse? GetLatestAdvice();
    Task<AppGardenAdviceResponse> GenerateAdviceAsync(GenerateAdviceRequest request, CancellationToken ct = default);
    string ClearPlannerHistory();
}
