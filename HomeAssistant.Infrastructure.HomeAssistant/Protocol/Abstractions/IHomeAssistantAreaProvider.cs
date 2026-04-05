using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Contracts;

namespace HomeAssistant.Infrastructure.HomeAssistant.Protocol.Abstractions;

/// <summary>Reads Home Assistant area metadata for planner room assignment workflows.</summary>
public interface IHomeAssistantAreaProvider
{
    /// <summary>Returns the available Home Assistant areas.</summary>
    Task<IReadOnlyList<HomeAssistantArea>> GetAvailableAreasAsync(CancellationToken ct = default);
}

