using System.Text.Json;
using GardenAI.Application.Area.Commands;
using GardenAI.Application.Common.Sync.Abstractions;
using GardenAI.Application.Common.Sync.Contracts;
using GardenAI.Application.Dispatching.Abstractions;
using Microsoft.Extensions.Logging;

namespace GardenAI.Infrastructure.HomeAssistant.Events.Handlers;

/// <summary>Handles area_registry_updated events from Home Assistant.</summary>
public sealed class AreaRegistryEventHandler : IRegistryEventHandler
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly ILogger<AreaRegistryEventHandler> _logger;

    public AreaRegistryEventHandler(
        ICommandDispatcher dispatcher,
        ILogger<AreaRegistryEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(HaRegistryEvent evt, CancellationToken ct = default)
    {
        var data = evt.Data.Deserialize<HaAreaRegistryEventData>();
        if (data is null)
        {
            _logger.LogWarning("Skipping malformed area registry event");
            return;
        }

        if (string.Equals(data.Action, "create", StringComparison.Ordinal)
            || string.Equals(data.Action, "update", StringComparison.Ordinal))
        {
            await _dispatcher.DispatchAsync(
                new UpsertAreaCommand(data.AreaId, data.AreaId, null, null),
                ct).ConfigureAwait(false);
            return;
        }

        _logger.LogInformation("Area remove event received for {AreaId}; delete command not wired yet", data.AreaId);
    }
}
