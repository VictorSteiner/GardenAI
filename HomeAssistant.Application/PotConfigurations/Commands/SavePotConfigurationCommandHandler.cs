using HomeAssistant.Domain.Common.Handlers;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Entities;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Application.PotConfigurations.Commands;

/// <summary>Handles the SavePotConfigurationCommand by persisting the configuration to the repository.</summary>
public sealed class SavePotConfigurationCommandHandler : ICommandHandler<SavePotConfigurationCommand>
{
    private readonly IPotConfigurationRepository _repository;
    private readonly ILogger<SavePotConfigurationCommandHandler> _logger;

    /// <summary>Initialises the handler.</summary>
    public SavePotConfigurationCommandHandler(
        IPotConfigurationRepository repository,
        ILogger<SavePotConfigurationCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task HandleAsync(SavePotConfigurationCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = await _repository.GetByPotIdAsync(command.PotId, ct);

        var seeds = command.Request.Seeds
            .Select(s => new SeedAssignment
            {
                Id = Guid.NewGuid(),
                PlantName = s.PlantName,
                SeedName = s.SeedName,
                PlantedDate = s.PlantedDate,
                ExpectedHarvestDate = s.ExpectedHarvestDate,
                Status = s.Status,
                Notes = s.Notes,
            })
            .ToList();

        var configuration = new PotConfiguration
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            PotId = command.PotId,
            RoomAreaId = command.Request.RoomAreaId,
            RoomName = command.Request.RoomName,
            CurrentSeeds = seeds,
            LastUpdated = DateTimeOffset.UtcNow,
        };

        if (existing is null)
        {
            await _repository.AddAsync(configuration, ct);
            _logger.LogInformation("New pot configuration created for pot {PotId}.", command.PotId);
        }
        else
        {
            await _repository.UpdateAsync(configuration, ct);
            _logger.LogInformation("Pot configuration updated for pot {PotId}.", command.PotId);
        }
    }
}
