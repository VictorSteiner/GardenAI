
using HomeAssistant.Domain.Entity.Entities;

namespace HomeAssistant.Domain.Entity.Abstractions;

/// <summary>Repository interface for Home Assistant Entity records.</summary>
public interface IEntityRepository
{
    /// <summary>Get an entity by its Home Assistant entity_id.</summary>
    Task<EntityRecord?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Get all entities.</summary>
    Task<IReadOnlyList<EntityRecord>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Get all entities on a specific device.</summary>
    Task<IReadOnlyList<EntityRecord>> GetByDeviceIdAsync(string deviceId, CancellationToken ct = default);

    /// <summary>Get all entities in a specific area.</summary>
    Task<IReadOnlyList<EntityRecord>> GetByAreaIdAsync(string areaId, CancellationToken ct = default);

    /// <summary>Create a new entity.</summary>
    Task<EntityRecord> CreateAsync(EntityRecord entity, CancellationToken ct = default);

    /// <summary>Update an existing entity.</summary>
    Task UpdateAsync(EntityRecord entity, CancellationToken ct = default);

    /// <summary>Delete an entity by id.</summary>
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>Upsert an entity (create or update).</summary>
    Task<EntityRecord> UpsertAsync(EntityRecord entity, CancellationToken ct = default);
}

