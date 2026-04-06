using GardenAI.Domain.Area.Abstractions;
using GardenAI.Domain.Area.Entities;
using GardenAI.Domain.Common.Handlers;

namespace GardenAI.Application.Area.Commands;

/// <summary>Handles <see cref="UpsertAreaCommand"/>.</summary>
public sealed class UpsertAreaCommandHandler : ICommandHandler<UpsertAreaCommand>
{
    private readonly IAreaRepository _repo;

    public UpsertAreaCommandHandler(IAreaRepository repo)
    {
        _repo = repo;
    }

    public async Task HandleAsync(UpsertAreaCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = DateTime.UtcNow;
        var entity = new AreaEntity
        {
            Id = command.AreaId,
            Name = command.Name,
            Icon = command.Icon,
            Aliases = command.AliasesJson,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repo.UpsertAsync(entity, ct);
    }
}

