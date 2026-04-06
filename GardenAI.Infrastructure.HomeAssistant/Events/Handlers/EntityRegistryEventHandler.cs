using System.Text.Json;
using GardenAI.Application.Common.Sync.Abstractions;
using GardenAI.Application.Common.Sync.Contracts;
using Microsoft.Extensions.Logging;

namespace GardenAI.Infrastructure.HomeAssistant.Events.Handlers;

/// <summary>Handles entity_registry_updated events from Home Assistant.</summary>
public sealed class EntityRegistryEventHandler : IRegistryEventHandler
{
    private readonly ILogger<EntityRegistryEventHandler> _logger;

    public EntityRegistryEventHandler(ILogger<EntityRegistryEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(HaRegistryEvent evt, CancellationToken ct = default)
    {
        var data = evt.Data.Deserialize<HaEntityRegistryEventData>();
        if (data is null)
        {
            _logger.LogWarning("Skipping malformed entity registry event");
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Entity registry event received: action={Action}, entityId={EntityId}",
            data.Action,
            data.EntityId);

        return Task.CompletedTask;
    }
}
