using HomeAssistant.Domain.Common.Markers;

namespace HomeAssistant.Application.PotConfigurations.Commands;

/// <summary>Command to update the lifecycle status of a seed assignment.</summary>
/// <param name="PotId">The pot ID containing the seed.</param>
/// <param name="SeedId">The seed assignment ID to update.</param>
/// <param name="NewStatus">The new lifecycle status ("growing", "mature", "harvested", "removed").</param>
public sealed record UpdateSeedStatusCommand(
    Guid PotId,
    Guid SeedId,
    string NewStatus) : ICommand;

