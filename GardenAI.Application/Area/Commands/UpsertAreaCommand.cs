using GardenAI.Domain.Common.Markers;

namespace GardenAI.Application.Area.Commands;

/// <summary>Creates or updates an area in the local DB from a HA event or REST fetch.</summary>
public sealed record UpsertAreaCommand(
    string AreaId,
    string Name,
    string? Icon,
    string? AliasesJson) : ICommand;

