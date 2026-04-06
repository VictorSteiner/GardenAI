namespace GardenAI.Application.Area.Dtos;

/// <summary>Read model for a synced Home Assistant area.</summary>
public sealed record AreaDto(string Id, string Name, string? Icon);

