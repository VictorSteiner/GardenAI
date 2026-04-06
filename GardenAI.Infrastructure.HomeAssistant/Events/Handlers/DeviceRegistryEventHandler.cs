using System.Text.Json;
using GardenAI.Application.Common.Sync.Abstractions;
using GardenAI.Application.Common.Sync.Contracts;
using GardenAI.Application.Device.Commands;
using GardenAI.Application.Dispatching.Abstractions;
using Microsoft.Extensions.Logging;

namespace GardenAI.Infrastructure.HomeAssistant.Events.Handlers;

/// <summary>Handles device_registry_updated events from Home Assistant.</summary>
public sealed class DeviceRegistryEventHandler : IRegistryEventHandler
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly ILogger<DeviceRegistryEventHandler> _logger;

    public DeviceRegistryEventHandler(
        ICommandDispatcher dispatcher,
        ILogger<DeviceRegistryEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task HandleAsync(HaRegistryEvent evt, CancellationToken ct = default)
    {
        var data = evt.Data.Deserialize<HaDeviceRegistryEventData>();
        if (data is null)
        {
            _logger.LogWarning("Skipping malformed device registry event");
            return;
        }

        if (string.Equals(data.Action, "create", StringComparison.Ordinal)
            || string.Equals(data.Action, "update", StringComparison.Ordinal))
        {
            await _dispatcher.DispatchAsync(
                new UpsertDeviceCommand(data.DeviceId, data.AreaId, data.DeviceId, null, null, null),
                ct).ConfigureAwait(false);
            return;
        }

        _logger.LogInformation("Device remove event received for {DeviceId}; delete command not wired yet", data.DeviceId);
    }
}
