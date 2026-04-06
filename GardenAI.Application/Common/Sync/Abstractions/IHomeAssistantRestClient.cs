using GardenAI.Application.Area.Contracts;
using GardenAI.Application.Device.Contracts;
using GardenAI.Application.Entity.Contracts;

namespace GardenAI.Application.Common.Sync.Abstractions;

/// <summary>
/// REST client for the Home Assistant API.
/// Used to fetch the initial full registry snapshot at startup.
/// </summary>
public interface IHomeAssistantRestClient
{
    /// <summary>Fetches all areas from GET /api/config/area_registry/list.</summary>
    Task<IReadOnlyList<HaAreaDto>> GetAreasAsync(CancellationToken ct = default);

    /// <summary>Fetches all devices from GET /api/config/device_registry/list.</summary>
    Task<IReadOnlyList<HaDeviceDto>> GetDevicesAsync(CancellationToken ct = default);

    /// <summary>Fetches all entities from GET /api/config/entity_registry/list.</summary>
    Task<IReadOnlyList<HaEntityDto>> GetEntitiesAsync(CancellationToken ct = default);
}


