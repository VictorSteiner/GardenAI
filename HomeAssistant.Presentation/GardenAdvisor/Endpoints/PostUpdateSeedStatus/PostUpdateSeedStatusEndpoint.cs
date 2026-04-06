using HomeAssistant.Application.PotConfigurations.Commands;
using HomeAssistant.Application.Dispatching;
using HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostUpdateSeedStatus.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostUpdateSeedStatus;

/// <summary>Endpoint: POST /api/garden/pots/{potId}/seeds/{seedId}/status – Update a seed's lifecycle status.</summary>
public sealed class PostUpdateSeedStatusEndpoint
{
    /// <summary>Handles the POST request to update a seed's status.</summary>
    public static async Task<Results<Ok, BadRequest<string>>> Handle(
        Guid potId,
        Guid seedId,
        UpdateSeedStatusRequest request,
        ICommandDispatcher dispatcher,
        ILogger<PostUpdateSeedStatusEndpoint> logger,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(logger);

        if (potId == Guid.Empty)
            return TypedResults.BadRequest("Pot ID must not be empty.");

        if (seedId == Guid.Empty)
            return TypedResults.BadRequest("Seed ID must not be empty.");

        if (string.IsNullOrWhiteSpace(request.NewStatus))
            return TypedResults.BadRequest("New status must not be empty.");

        var command = new UpdateSeedStatusCommand(potId, seedId, request.NewStatus);
        await dispatcher.DispatchAsync(command, ct);

        logger.LogInformation("Seed status updated: pot={PotId}, seed={SeedId}, newStatus={NewStatus}",
            potId, seedId, request.NewStatus);

        return TypedResults.Ok();
    }
}

