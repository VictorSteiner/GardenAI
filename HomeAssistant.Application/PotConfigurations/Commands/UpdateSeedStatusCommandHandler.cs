using System.Diagnostics.Metrics;
using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Constants;
using HomeAssistant.Domain.PotConfigurations.Entities;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Application.PotConfigurations.Commands;

/// <summary>Handles UpdateSeedStatusCommand by persisting the status change and logging the transition.</summary>
public sealed class UpdateSeedStatusCommandHandler : ICommandHandler<UpdateSeedStatusCommand>
{
    private readonly IPotConfigurationRepository _repository;
    private readonly ILogger<UpdateSeedStatusCommandHandler> _logger;
    private readonly Counter<int> _statusTransitionCounter;

    /// <summary>Initialises the handler.</summary>
    public UpdateSeedStatusCommandHandler(
        IPotConfigurationRepository repository,
        ILogger<UpdateSeedStatusCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize metrics counter
        var meter = new Meter("HomeAssistant.Application.PotConfigurations");
        _statusTransitionCounter = meter.CreateCounter<int>("seed_status_transitions", description: "Count of seed status transitions");
    }

    /// <inheritdoc/>
    public async Task HandleAsync(UpdateSeedStatusCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Validate the new status
        if (!SeedStatusConstants.ValidStatuses.Contains(command.NewStatus))
        {
            throw new ArgumentException(
                $"Invalid seed status '{command.NewStatus}'. Valid statuses are: {string.Join(", ", SeedStatusConstants.ValidStatuses)}",
                nameof(command.NewStatus));
        }

        var existing = await _repository.GetByPotIdAsync(command.PotId, ct);
        if (existing is null)
        {
            _logger.LogWarning("Attempted to update seed status in pot {PotId}, but pot configuration does not exist.", command.PotId);
            return;
        }

        // Find the seed
        var seedList = existing.CurrentSeeds.ToList();
        var seed = seedList.FirstOrDefault(s => s.Id == command.SeedId);
        if (seed is null)
        {
            _logger.LogWarning("Attempted to update seed {SeedId} in pot {PotId}, but seed does not exist.", command.SeedId, command.PotId);
            return;
        }

        var oldStatus = seed.Status;

        // Create updated seed with new status
        var updatedSeed = new SeedAssignment
        {
            Id = seed.Id,
            PlantName = seed.PlantName,
            SeedName = seed.SeedName,
            PlantedDate = seed.PlantedDate,
            ExpectedHarvestDate = seed.ExpectedHarvestDate,
            Status = command.NewStatus,
            Notes = seed.Notes,
        };

        // Replace in list
        seedList[seedList.IndexOf(seed)] = updatedSeed;

        // Update the configuration
        var updatedConfiguration = new PotConfiguration
        {
            Id = existing.Id,
            PotId = existing.PotId,
            RoomAreaId = existing.RoomAreaId,
            RoomName = existing.RoomName,
            CurrentSeeds = seedList,
            LastUpdated = DateTimeOffset.UtcNow,
        };

        await _repository.UpdateAsync(updatedConfiguration, ct);

        // Log the status transition
        _logger.LogInformation(
            "Seed status transitioned: pot={PotId}, seed={SeedId}, plant={PlantName}, oldStatus={OldStatus}, newStatus={NewStatus}",
            command.PotId,
            command.SeedId,
            seed.PlantName,
            oldStatus,
            command.NewStatus);

        // Emit metrics
        _statusTransitionCounter.Add(1);
    }
}
