namespace GardenAI.Application.Common.Sync.Abstractions;

/// <summary>Represents the current state of the HA sync pipeline.</summary>
public enum SyncState
{
    /// <summary>Sync has not started yet.</summary>
    Unsynced,

    /// <summary>Initial sync is in progress.</summary>
    Syncing,

    /// <summary>Initial sync completed successfully and the event loop is active.</summary>
    Synced,

    /// <summary>Sync failed due to an unrecoverable error.</summary>
    Error
}


