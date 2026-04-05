namespace HomeAssistant.Infrastructure.HomeAssistant.Protocol.Contracts;

/// <summary>Describes an internal Home Assistant protocol tool available to the garden planner.</summary>
public sealed record HomeAssistantProtocolToolDefinition(
    string Name,
    string Description,
    IReadOnlyList<HomeAssistantProtocolToolParameter> Parameters);

