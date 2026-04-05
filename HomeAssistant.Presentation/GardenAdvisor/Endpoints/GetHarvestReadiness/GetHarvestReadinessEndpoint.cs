using HomeAssistant.Application.PotConfigurations.DTOs;
using HomeAssistant.Application.PotConfigurations.Queries;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.GetHarvestReadiness;

/// <summary>Endpoint: GET /api/garden/seeds/harvest-readiness – Get harvest readiness for all seeds.</summary>
public sealed class GetHarvestReadinessEndpoint
{
    /// <summary>Handles the GET request to retrieve harvest readiness data.</summary>
    public static async Task<Ok<IReadOnlyList<HarvestReadinessResponse>>> Handle(
        IQueryHandler<GetHarvestReadinessQuery, IReadOnlyList<HarvestReadinessDto>> handler,
        ILogger<GetHarvestReadinessEndpoint> logger,
        string? filterByStatus = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(logger);

        var query = new GetHarvestReadinessQuery(filterByStatus);
        var readinessList = await handler.HandleAsync(query, ct);

        var responses = readinessList
            .Select(HarvestReadinessResponse.FromDto)
            .ToList();

        logger.LogInformation("Retrieved harvest readiness for {Count} seeds (filter: {Filter})",
            responses.Count, filterByStatus ?? "none");

        return TypedResults.Ok(responses.AsReadOnly() as IReadOnlyList<HarvestReadinessResponse> ?? responses);
    }
}

