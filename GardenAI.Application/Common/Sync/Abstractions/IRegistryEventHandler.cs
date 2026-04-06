using GardenAI.Application.Common.Sync.Contracts;

namespace GardenAI.Application.Common.Sync.Abstractions;

/// <summary>Handles a specific type of Home Assistant registry change event.</summary>
public interface IRegistryEventHandler
{
    /// <summary>Processes the event and applies the appropriate DB mutations.</summary>
    Task HandleAsync(HaRegistryEvent evt, CancellationToken ct = default);
}


