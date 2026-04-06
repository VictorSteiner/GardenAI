
using GardenAI.Domain.Area.Entities;

namespace GardenAI.Domain.Area.Abstractions;

/// <summary>Repository interface for Home Assistant Area entities.</summary>
public interface IAreaRepository
{
    /// <summary>Get an area by its Home Assistant area_id.</summary>
    Task<AreaEntity?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Get all areas.</summary>
    Task<IReadOnlyList<AreaEntity>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Create a new area.</summary>
    Task<AreaEntity> CreateAsync(AreaEntity area, CancellationToken ct = default);

    /// <summary>Update an existing area.</summary>
    Task UpdateAsync(AreaEntity area, CancellationToken ct = default);

    /// <summary>Delete an area by id (cascade delete applies to associated devices and entities).</summary>
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>Upsert an area (create or update).</summary>
    Task<AreaEntity> UpsertAsync(AreaEntity area, CancellationToken ct = default);
}

