using HomeAssistant.Integrations.HomeAssistant.Protocol.Contracts;

namespace HomeAssistant.Integrations.HomeAssistant.Protocol.Abstractions;

/// <summary>Reads Home Assistant area metadata for planner room assignment workflows.</summary>
public interface IHomeAssistantAreaProvider
{
    /// <summary>Returns the available Home Assistant areas.</summary>
    Task<IReadOnlyList<HomeAssistantArea>> GetAvailableAreasAsync(CancellationToken ct = default);
}


