namespace GardenAI.Application.Common.Sync.Abstractions;

/// <summary>
/// Orchestrates the initial HA registry sync using the Subscribe-Before-Fetch pattern
/// to eliminate the race condition between REST fetch completion and WebSocket event activation.
/// </summary>
public interface ISyncOrchestrator
{
    /// <summary>Gets the current sync state.</summary>
    SyncState State { get; }

    /// <summary>
    /// Runs the full initial sync:
    /// 1. Subscribe to registry events (buffer incoming events).
    /// 2. Fetch current state via REST and upsert into DB (Areas ? Devices ? Entities).
    /// 3. Drain buffered events received during the fetch.
    /// 4. Transition to live event processing.
    /// </summary>
    Task RunInitialSyncAsync(CancellationToken ct = default);
}


