namespace HomeAssistant.Application.PotConfigurations.Abstractions;

/// <summary>Provides a configured mapping from user-facing pot numbers to persistent pot identifiers.</summary>
public interface IPotIdentityMapProvider
{
    /// <summary>Resolves a pot identifier for a given pot number, or null when no mapping exists.</summary>
    Task<Guid?> ResolvePotIdAsync(int potNumber, CancellationToken ct = default);

    /// <summary>Returns the full configured map keyed by pot number.</summary>
    Task<IReadOnlyDictionary<int, Guid>> GetMapAsync(CancellationToken ct = default);
}

