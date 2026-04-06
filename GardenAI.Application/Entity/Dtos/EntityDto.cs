namespace GardenAI.Application.Entity.Dtos;

/// <summary>Read model for a synced Home Assistant entity.</summary>
public sealed record EntityDto(string Id, string? DeviceId, string? AreaId, string Platform, string Name);

