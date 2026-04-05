using HomeAssistant.Domain.PlantPots.Entities;

namespace HomeAssistant.Domain.PlantPots.Abstractions;

/// <summary>Defines data access operations for plant pots.</summary>
public interface IPlantPotRepository
{
    /// <summary>Returns the pot with the given identifier, or <c>null</c> if not found.</summary>
    Task<PlantPot?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all plant pots.</summary>
    Task<IReadOnlyList<PlantPot>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Persists a new plant pot.</summary>
    Task AddAsync(PlantPot pot, CancellationToken ct = default);

    /// <summary>Persists changes to an existing plant pot.</summary>
    Task UpdateAsync(PlantPot pot, CancellationToken ct = default);
}
