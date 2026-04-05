using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.GetPotConfiguration;

/// <summary>Endpoint to retrieve a pot's configuration (room + seeds).</summary>
public sealed class GetPotConfigurationEndpoint
{
    /// <summary>Handles the GET request.</summary>
    public static async Task<Results<Ok<PotConfigurationResponse>, NotFound>> Handle(
        Guid potId,
        IPotConfigurationRepository repository,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (potId == Guid.Empty)
            return TypedResults.NotFound();

        var config = await repository.GetByPotIdAsync(potId, ct);
        if (config is null)
            return TypedResults.NotFound();

        var response = new PotConfigurationResponse(
            config.PotId,
            config.RoomAreaId,
            config.RoomName,
            config.CurrentSeeds
                .Select(s => new SeedAssignmentResponse(
                    s.Id,
                    s.PlantName,
                    s.SeedName,
                    s.PlantedDate,
                    s.ExpectedHarvestDate,
                    s.Status,
                    s.Notes))
                .ToList(),
            config.LastUpdated);

        return TypedResults.Ok(response);
    }
}

