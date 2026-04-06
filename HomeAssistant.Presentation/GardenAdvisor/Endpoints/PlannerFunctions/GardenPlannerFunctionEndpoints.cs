using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PlannerFunctions;

/// <summary>Maps dedicated planner workflow endpoints under <c>/api/garden/planner/functions</c>.</summary>
public static class GardenPlannerFunctionEndpoints
{
    /// <summary>Maps planner function endpoints.</summary>
    public static IEndpointRouteBuilder MapGardenPlannerFunctionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/garden/planner/functions")
            .WithTags("GardenPlannerFunctions");

        group.MapPost("/pots/save", SavePotConfiguration)
            .WithName("PlannerFunctionSavePotConfiguration")
            .WithOpenApi()
            .Accepts<SavePlannerPotConfigurationFunctionRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapPost("/pots/update-seed-status", UpdateSeedStatus)
            .WithName("PlannerFunctionUpdateSeedStatus")
            .WithOpenApi()
            .Accepts<UpdatePlannerSeedStatusFunctionRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapPost("/pots/status", GetPotStatus)
            .WithName("PlannerFunctionGetPotStatus")
            .WithOpenApi()
            .Accepts<PotNumberFunctionRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/pots", GetAllPotsStatus)
            .WithName("PlannerFunctionGetAllPotsStatus")
            .WithOpenApi()
            .Produces<string>(StatusCodes.Status200OK);

        group.MapPost("/pots/sensors", GetSensorReadings)
            .WithName("PlannerFunctionGetSensorReadings")
            .WithOpenApi()
            .Accepts<PotNumberFunctionRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/rooms", GetAvailableRooms)
            .WithName("PlannerFunctionGetAvailableRooms")
            .WithOpenApi()
            .Produces<IReadOnlyList<RoomResponse>>(StatusCodes.Status200OK);

        group.MapGet("/rooms/{roomAreaId}", GetRoomSummary)
            .WithName("PlannerFunctionGetRoomSummary")
            .WithOpenApi()
            .Produces<RoomSummaryResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/dashboard", GetDashboard)
            .WithName("PlannerFunctionGetDashboard")
            .WithOpenApi()
            .Produces<DashboardAggregationResponse>(StatusCodes.Status200OK);

        group.MapGet("/harvest-readiness", GetHarvestReadiness)
            .WithName("PlannerFunctionGetHarvestReadiness")
            .WithOpenApi()
            .Produces<IReadOnlyList<HarvestReadinessResponse>>(StatusCodes.Status200OK);

        group.MapGet("/advice/latest", GetLatestAdvice)
            .WithName("PlannerFunctionGetLatestAdvice")
            .WithOpenApi()
            .Produces<HomeAssistant.Application.GardenAdvisor.Contracts.GardenAdviceResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/advice/generate", GenerateAdvice)
            .WithName("PlannerFunctionGenerateAdvice")
            .WithOpenApi()
            .Accepts<GeneratePlannerAdviceFunctionRequest>("application/json")
            .Produces<HomeAssistant.Application.GardenAdvisor.Contracts.GardenAdviceResponse>(StatusCodes.Status200OK);

        group.MapPost("/history/clear", ClearHistory)
            .WithName("PlannerFunctionClearHistory")
            .WithOpenApi()
            .Produces<string>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> SavePotConfiguration(
        SavePlannerPotConfigurationFunctionRequest request,
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(service);

        if (request.PotNumber < 1 || request.PotNumber > 6 || string.IsNullOrWhiteSpace(request.PlantName) || string.IsNullOrWhiteSpace(request.SeedName))
            return TypedResults.BadRequest("Valid pot number, plant name, and seed name are required.");

        return TypedResults.Ok(await service.SavePotConfigurationAsync(request, ct));
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> UpdateSeedStatus(
        UpdatePlannerSeedStatusFunctionRequest request,
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(service);

        if (request.PotNumber < 1 || request.PotNumber > 6 || string.IsNullOrWhiteSpace(request.NewStatus))
            return TypedResults.BadRequest("Valid pot number and new status are required.");

        return TypedResults.Ok(await service.UpdateSeedStatusAsync(request, ct));
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> GetPotStatus(
        PotNumberFunctionRequest request,
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(service);

        if (request.PotNumber < 1 || request.PotNumber > 6)
            return TypedResults.BadRequest("Pot number must be between 1 and 6.");

        return TypedResults.Ok(await service.GetPotStatusAsync(request, ct));
    }

    private static async Task<Ok<string>> GetAllPotsStatus(
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GetAllPotsStatusAsync(ct));
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> GetSensorReadings(
        PotNumberFunctionRequest request,
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(service);

        if (request.PotNumber < 1 || request.PotNumber > 6)
            return TypedResults.BadRequest("Pot number must be between 1 and 6.");

        return TypedResults.Ok(await service.GetSensorReadingsAsync(request, ct));
    }

    private static async Task<Ok<IReadOnlyList<RoomResponse>>> GetAvailableRooms(
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GetAvailableRoomsAsync(ct));
    }

    private static async Task<Results<Ok<RoomSummaryResponse>, BadRequest<string>>> GetRoomSummary(
        string roomAreaId,
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (string.IsNullOrWhiteSpace(roomAreaId))
            return TypedResults.BadRequest("Room area ID is required.");

        return TypedResults.Ok(await service.GetRoomSummaryAsync(new RoomAreaFunctionRequest(roomAreaId), ct));
    }

    private static async Task<Ok<DashboardAggregationResponse>> GetDashboard(
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GetDashboardAsync(ct));
    }

    private static async Task<Ok<IReadOnlyList<HarvestReadinessResponse>>> GetHarvestReadiness(
        string? filterByStatus,
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GetHarvestReadinessAsync(new HarvestReadinessFunctionRequest(filterByStatus), ct));
    }

    private static Results<Ok<HomeAssistant.Application.GardenAdvisor.Contracts.GardenAdviceResponse>, NotFound> GetLatestAdvice(IGardenPlannerFunctionService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var latest = service.GetLatestAdvice();
        return latest is null ? TypedResults.NotFound() : TypedResults.Ok(latest);
    }

    private static async Task<Ok<HomeAssistant.Application.GardenAdvisor.Contracts.GardenAdviceResponse>> GenerateAdvice(
        GeneratePlannerAdviceFunctionRequest? request,
        IGardenPlannerFunctionService service,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(await service.GenerateAdviceAsync(request ?? new GeneratePlannerAdviceFunctionRequest(), ct));
    }

    private static Ok<string> ClearHistory(IGardenPlannerFunctionService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return TypedResults.Ok(service.ClearPlannerHistory());
    }
}

