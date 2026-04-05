using HomeAssistant.Domain.Common.Markers;

namespace HomeAssistant.Application.Maintenance.Commands.ResetPersistedData;

/// <summary>Command that clears all persisted application data while preserving the database schema.</summary>
public sealed record ResetPersistedDataCommand : ICommand;

