namespace HomeAssistant.Domain.PotConfigurations.Constants;

/// <summary>Constants for seed lifecycle status values.</summary>
public static class SeedStatusConstants
{
    /// <summary>Seed is actively growing.</summary>
    public const string Growing = "growing";

    /// <summary>Seed has reached maturity.</summary>
    public const string Mature = "mature";

    /// <summary>Seed has been harvested.</summary>
    public const string Harvested = "harvested";

    /// <summary>Seed/plant has been removed.</summary>
    public const string Removed = "removed";

    /// <summary>All valid status values.</summary>
    public static readonly IReadOnlySet<string> ValidStatuses = new HashSet<string>
    {
        Growing,
        Mature,
        Harvested,
        Removed,
    };
}

