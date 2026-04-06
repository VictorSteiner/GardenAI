using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Application.GardenAdvisor.Contracts.Advice;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools.Contracts;
using AppGardenAdviceResponse = HomeAssistant.Application.GardenAdvisor.Contracts.Advice.GardenAdviceResponse;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools;

/// <summary>Maps dedicated planner tool endpoints under <c>/api/garden/planner/functions</c>.</summary>
public static class GardenPlannerToolEndpoints
{
    /// <summary>Maps planner tool endpoints.</summary>
    public static IEndpointRouteBuilder MapGardenPlannerToolEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/garden/planner/functions")
            .WithTags("GardenPlannerTools");

        group.MapPost("/pots/save", SavePotConfiguration)
            .WithName("PlannerToolSavePotConfiguration")
            .WithOpenApi()
            .Accepts<SavePotConfigurationRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapPost("/pots/update-seed-status", UpdateSeedStatus)
            .WithName("PlannerToolUpdateSeedStatus")
            .WithOpenApi()
            .Accepts<UpdateSeedStatusRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapPost("/pots/status", GetPotStatus)
            .WithName("PlannerToolGetPotStatus")
            .WithOpenApi()
            .Accepts<PotNumberRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/pots", GetAllPotsStatus)
            .WithName("PlannerToolGetAllPotsStatus")
            .WithOpenApi()
            .Produces<string>(StatusCodes.Status200OK);

        group.MapPost("/pots/sensors", GetSensorReadings)
            .WithName("PlannerToolGetSensorReadings")
            .WithOpenApi()
            .Accepts<PotNumberRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/rooms", GetAvailableRooms)
            .WithName("PlannerToolGetAvailableRooms")
            .WithOpenApi()
            .Produces<IReadOnlyList<RoomResponse>>(StatusCodes.Status200OK);

        group.MapGet("/rooms/{roomAreaId}", GetRoomSummary)
            .WithName("PlannerToolGetRoomSummary")
            .WithOpenApi()
            .Produces<RoomSummaryResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/dashboard", GetDashboard)
            .WithName("PlannerToolGetDashboard")
            .WithOpenApi()
            .Produces<DashboardAggregationResponse>(StatusCodes.Status200OK);

        group.MapGet("/harvest-readiness", GetHarvestReadiness)
            .WithName("PlannerToolGetHarvestReadiness")
            .WithOpenApi()
            .Produces<IReadOnlyList<HarvestReadinessResponse>>(StatusCodes.Status200OK);

        group.MapGet("/advice/latest", GetLatestAdvice)
            .WithName("PlannerToolGetLatestAdvice")
            .WithOpenApi()
            .Produces<AppGardenAdviceResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/advice/generate", GenerateAdvice)
            .WithName("PlannerToolGenerateAdvice")
            .WithOpenApi()
            .Accepts<GenerateAdviceRequest>("application/json")
            .Produces<AppGardenAdviceResponse>(StatusCodes.Status200OK);

        group.MapPost("/history/clear", ClearHistory)
            .WithName("PlannerToolClearHistory")
            .WithOpenApi()
            .Produces<string>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> SavePotConfiguration(
        SavePotConfigurationRequest request,
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(service);

        if (request.PotNumber < 1 || request.PotNumber > 6 || string.IsNullOrWhiteSpace(request.PlantName) || string.IsNullOrWhiteSpace(request.SeedName))
            return TypedResults.BadRequest("Valid pot number, plant name, and seed name are required.");

        return TypedResults.Ok(await service.SavePotConfigurationAsync(request, ct));
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> UpdateSeedStatus(
        UpdateSeedStatusRequest request,
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(service);

        if (request.PotNumber < 1 || request.PotNumber > 6 || string.IsNullOrWhiteSpace(request.NewStatus))
            return TypedResults.BadRequest("Valid pot number and new status are required.");

        return TypedResults.Ok(await service.UpdateSeedStatusAsync(request, ct));
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> GetPotStatus(
        PotNumberRequest request,
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(service);

        if (request.PotNumber < 1 || request.PotNumber > 6)
            return TypedResults.BadRequest("Pot number must be between 1 and 6.");

        return TypedResults.Ok(await service.GetPotStatusAsync(request, ct));
    }

    private static async Task<Ok<string>> GetAllPotsStatus(
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GetAllPotsStatusAsync(ct));
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> GetSensorReadings(
        PotNumberRequest request,
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(service);

        if (request.PotNumber < 1 || request.PotNumber > 6)
            return TypedResults.BadRequest("Pot number must be between 1 and 6.");

        return TypedResults.Ok(await service.GetSensorReadingsAsync(request, ct));
    }

    private static async Task<Ok<IReadOnlyList<RoomResponse>>> GetAvailableRooms(
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GetAvailableRoomsAsync(ct));
    }

    private static async Task<Results<Ok<RoomSummaryResponse>, BadRequest<string>>> GetRoomSummary(
        string roomAreaId,
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (string.IsNullOrWhiteSpace(roomAreaId))
            return TypedResults.BadRequest("Room area ID is required.");

        return TypedResults.Ok(await service.GetRoomSummaryAsync(new RoomAreaRequest(roomAreaId), ct));
    }

    private static async Task<Ok<DashboardAggregationResponse>> GetDashboard(
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GetDashboardAsync(ct));
    }

    private static async Task<Ok<IReadOnlyList<HarvestReadinessResponse>>> GetHarvestReadiness(
        string? filterByStatus,
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GetHarvestReadinessAsync(new HarvestReadinessRequest(filterByStatus), ct));
    }

    private static Results<Ok<AppGardenAdviceResponse>, NotFound> GetLatestAdvice(IGardenPlannerToolService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var latest = service.GetLatestAdvice();
        return latest is null ? TypedResults.NotFound() : TypedResults.Ok(latest);
    }

    private static async Task<Ok<AppGardenAdviceResponse>> GenerateAdvice(
        GenerateAdviceRequest? request,
        IGardenPlannerToolService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GenerateAdviceAsync(request ?? new GenerateAdviceRequest(), ct));
    }

    private static Ok<string> ClearHistory(IGardenPlannerToolService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(service.ClearPlannerHistory());
    }
}

