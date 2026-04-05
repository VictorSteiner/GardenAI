using HomeAssistant.Domain.Common.Markers;

namespace HomeAssistant.Application.PotConfigurations.Commands;

/// <summary>Request model for saving a pot's configuration (room + seeds).</summary>
public record SavePotConfigurationRequest(
    string RoomAreaId,
    string RoomName,
    IReadOnlyList<SeedAssignmentRequest> Seeds);

/// <summary>Represents a seed assignment in a save request.</summary>
public record SeedAssignmentRequest(
    /// <summary>Common plant name (e.g., "Tomato", "Basil").</summary>
    string PlantName,
    /// <summary>Specific seed/cultivar name (e.g., "Moneymaker", "Genovese").</summary>
    string SeedName,
    /// <summary>Date when the seed was sown/planted.</summary>
    DateTimeOffset PlantedDate,
    /// <summary>Expected harvest or maturity date, if known.</summary>
    DateTimeOffset? ExpectedHarvestDate,
    /// <summary>Current lifecycle status: "growing", "mature", "harvested", "removed".</summary>
    string Status,
    /// <summary>Optional notes, e.g., companion planting info.</summary>
    string? Notes);

/// <summary>Command to save or update a pot's configuration (room + seed assignments).</summary>
/// <param name="PotId">The pot ID to configure.</param>
/// <param name="Request">The configuration request data.</param>
public sealed record SavePotConfigurationCommand(Guid PotId, SavePotConfigurationRequest Request) : ICommand;

