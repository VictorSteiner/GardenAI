using GardenAI.Domain.Common.Markers;

namespace GardenAI.Application.Device.Commands;

/// <summary>Creates or updates a device in the local DB from a HA event or REST fetch.</summary>
public sealed record UpsertDeviceCommand(
    string DeviceId,
    string? AreaId,
    string Name,
    string? NameByUser,
    string? Manufacturer,
    string? Model) : ICommand;

