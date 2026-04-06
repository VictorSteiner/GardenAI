using GardenAI.Domain.Common.Handlers;
using GardenAI.Domain.Device.Abstractions;
using GardenAI.Domain.Device.Entities;

namespace GardenAI.Application.Device.Commands;

/// <summary>Handles <see cref="UpsertDeviceCommand"/>.</summary>
public sealed class UpsertDeviceCommandHandler : ICommandHandler<UpsertDeviceCommand>
{
    private readonly IDeviceRepository _repo;

    public UpsertDeviceCommandHandler(IDeviceRepository repo)
    {
        _repo = repo;
    }

    public async Task HandleAsync(UpsertDeviceCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = DateTime.UtcNow;
        var entity = new DeviceEntity
        {
            Id = command.DeviceId,
            AreaId = command.AreaId,
            Name = command.Name,
            NameByUser = command.NameByUser,
            Manufacturer = command.Manufacturer,
            Model = command.Model,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repo.UpsertAsync(entity, ct);
    }
}

