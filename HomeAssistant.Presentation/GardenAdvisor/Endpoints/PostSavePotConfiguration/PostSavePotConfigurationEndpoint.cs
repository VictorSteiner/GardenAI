using HomeAssistant.Application.Dispatching;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostSavePotConfiguration.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using SavePotConfigurationCommand = HomeAssistant.Application.PotConfigurations.Commands.SavePotConfigurationCommand;
using SavePotConfigurationCommandRequest = HomeAssistant.Application.PotConfigurations.Commands.SavePotConfigurationRequest;
using SeedAssignmentCommandRequest = HomeAssistant.Application.PotConfigurations.Commands.SeedAssignmentRequest;

namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostSavePotConfiguration;

/// <summary>Endpoint to save/update a pot's configuration (room + seeds).</summary>
public sealed class PostSavePotConfigurationEndpoint
{
    /// <summary>Handles the POST request.</summary>
    public static async Task<Results<Ok<PotConfigurationResponse>, BadRequest>> Handle(
        Guid potId,
        SavePotConfigurationRequest request,
        ICommandDispatcher dispatcher,
        IPotConfigurationRepository repository,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(request);

        if (potId == Guid.Empty)
            return TypedResults.BadRequest();

        if (string.IsNullOrWhiteSpace(request.RoomAreaId) || string.IsNullOrWhiteSpace(request.RoomName))
            return TypedResults.BadRequest();

        // Convert presentation request to application request
        var appRequest = new SavePotConfigurationCommandRequest(
            request.RoomAreaId,
            request.RoomName,
            request.Seeds
                .Select(s => new SeedAssignmentCommandRequest(
                    s.PlantName,
                    s.SeedName,
                    s.PlantedDate,
                    s.ExpectedHarvestDate,
                    s.Status,
                    s.Notes))
                .ToList());

        // Dispatch the save command
        var command = new SavePotConfigurationCommand(potId, appRequest);
        await dispatcher.DispatchAsync(command, ct);

        // Retrieve the saved configuration. Command dispatch is channel-based,
        // so persistence may complete slightly after dispatch returns.
        HomeAssistant.Domain.PotConfigurations.Entities.PotConfiguration? config = null;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            config = await repository.GetByPotIdAsync(potId, ct);
            if (config is not null)
                break;

            await Task.Delay(50, ct);
        }

        if (config is null)
            return TypedResults.BadRequest();

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
