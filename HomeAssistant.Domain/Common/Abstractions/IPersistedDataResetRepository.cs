namespace HomeAssistant.Domain.Common.Abstractions;

/// <summary>Provides a safe way to clear persisted application data without removing schema or migrations.</summary>
public interface IPersistedDataResetRepository
{
    /// <summary>Clears all persisted application table data while preserving schema and migration history.</summary>
    Task ResetAsync(CancellationToken ct = default);
}

