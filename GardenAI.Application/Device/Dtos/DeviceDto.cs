namespace GardenAI.Application.Device.Dtos;

/// <summary>Read model for a synced Home Assistant device.</summary>
public sealed record DeviceDto(string Id, string? AreaId, string Name, string? Manufacturer, string? Model);

