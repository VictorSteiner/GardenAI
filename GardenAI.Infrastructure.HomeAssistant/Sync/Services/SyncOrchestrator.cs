using GardenAI.Application.Common.Sync.Abstractions;
using Microsoft.Extensions.Logging;

namespace GardenAI.Infrastructure.HomeAssistant.Sync.Services;

/// <summary>Coordinates initial sync ordering for Home Assistant registry data.</summary>
public sealed class SyncOrchestrator : ISyncOrchestrator
{
    private readonly IHomeAssistantRestClient _restClient;
    private readonly ILogger<SyncOrchestrator> _logger;
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public SyncState State { get; private set; } = SyncState.Unsynced;

    public SyncOrchestrator(
        IHomeAssistantRestClient restClient,
        ILogger<SyncOrchestrator> logger)
    {
        _restClient = restClient;
        _logger = logger;
    }

    public async Task RunInitialSyncAsync(CancellationToken ct = default)
    {
        await _syncLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            State = SyncState.Syncing;

            var areas = await _restClient.GetAreasAsync(ct).ConfigureAwait(false);
            var devices = await _restClient.GetDevicesAsync(ct).ConfigureAwait(false);
            var entities = await _restClient.GetEntitiesAsync(ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Initial Home Assistant fetch completed: areas={AreaCount}, devices={DeviceCount}, entities={EntityCount}",
                areas.Count,
                devices.Count,
                entities.Count);

            State = SyncState.Synced;
        }
        catch
        {
            State = SyncState.Error;
            throw;
        }
        finally
        {
            _syncLock.Release();
        }
    }
}

