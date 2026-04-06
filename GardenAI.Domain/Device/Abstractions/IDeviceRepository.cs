using GardenAI.Domain.Device.Entities;

namespace GardenAI.Domain.Device.Abstractions;

/// <summary>Repository interface for Home Assistant Device entities.</summary>
public interface IDeviceRepository
{
    /// <summary>Get a device by its Home Assistant device_id.</summary>
    Task<DeviceEntity?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Get all devices.</summary>
    Task<IReadOnlyList<DeviceEntity>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Get all devices in a specific area.</summary>
    Task<IReadOnlyList<DeviceEntity>> GetByAreaIdAsync(string areaId, CancellationToken ct = default);

    /// <summary>Create a new device.</summary>
    Task<DeviceEntity> CreateAsync(DeviceEntity device, CancellationToken ct = default);

    /// <summary>Update an existing device.</summary>
    Task UpdateAsync(DeviceEntity device, CancellationToken ct = default);

    /// <summary>Delete a device by id (cascade delete applies to associated entities).</summary>
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>Upsert a device (create or update).</summary>
    Task<DeviceEntity> UpsertAsync(DeviceEntity device, CancellationToken ct = default);
}

