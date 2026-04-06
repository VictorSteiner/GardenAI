using HomeAssistant.Integrations.HomeAssistant.Protocol.Contracts;

namespace HomeAssistant.Integrations.HomeAssistant.Protocol.Abstractions;

/// <summary>Discovers and invokes Semantic Kernel attributed protocol functions for the planner.</summary>
public interface IHomeAssistantProtocolToolRegistry
{
    /// <summary>Returns all available internal tool definitions.</summary>
    IReadOnlyList<HomeAssistantProtocolToolDefinition> GetToolDefinitions();

    /// <summary>Invokes a registered tool by name using the supplied JSON arguments.</summary>
    Task<string> InvokeAsync(string functionName, string argumentsJson, CancellationToken ct = default);
}


