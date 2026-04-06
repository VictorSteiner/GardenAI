using HomeAssistant.Domain.PotConfigurations.Entities;

namespace HomeAssistant.Domain.PotConfigurations.Abstractions;

/// <summary>Repository abstraction for persisting and retrieving pot configuration data.</summary>
public interface IPotConfigurationRepository
{
    /// <summary>Retrieves the configuration for a given pot, if it exists.</summary>
    /// <param name="potId">The pot ID to retrieve configuration for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The pot configuration, or null if not found.</returns>
    Task<PotConfiguration> GetByPotIdAsync(Guid potId, CancellationToken ct = default);

    /// <summary>Retrieves all pot configurations.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All pot configurations in the system.</returns>
    Task<IReadOnlyList<PotConfiguration>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Saves a new pot configuration.</summary>
    /// <param name="configuration">The configuration to save.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(PotConfiguration configuration, CancellationToken ct = default);

    /// <summary>Updates an existing pot configuration.</summary>
    /// <param name="configuration">The updated configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateAsync(PotConfiguration configuration, CancellationToken ct = default);

    /// <summary>Deletes a pot configuration by pot ID.</summary>
    /// <param name="potId">The pot ID to delete configuration for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteByPotIdAsync(Guid potId, CancellationToken ct = default);

    /// <summary>Retrieves all pot configurations in a given room (Home Assistant area).</summary>
    /// <param name="roomAreaId">The Home Assistant area ID (e.g., "living_room").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All configurations in the room, or empty if none found.</returns>
    Task<IReadOnlyList<PotConfiguration>> GetByRoomAreaIdAsync(string roomAreaId, CancellationToken ct = default);

    /// <summary>Retrieves all pot configurations with seeds at a specific status.</summary>
    /// <param name="seedStatus">The seed status to filter by (e.g., "growing", "mature", "harvested").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All configurations containing at least one seed with the given status.</returns>
    Task<IReadOnlyList<PotConfiguration>> GetByStatusAsync(string seedStatus, CancellationToken ct = default);
}

