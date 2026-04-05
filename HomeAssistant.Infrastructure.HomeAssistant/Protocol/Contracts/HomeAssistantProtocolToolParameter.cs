namespace HomeAssistant.Infrastructure.HomeAssistant.Protocol.Contracts;

/// <summary>Describes a single Semantic Kernel tool parameter exposed to the garden planner.</summary>
public sealed record HomeAssistantProtocolToolParameter(
    string Name,
    string Type,
    string Description,
    bool IsRequired);

