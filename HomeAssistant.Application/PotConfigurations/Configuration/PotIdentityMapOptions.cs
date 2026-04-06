namespace HomeAssistant.Application.PotConfigurations.Configuration;

/// <summary>Config section used to map stable pot numbers to persistent pot identifiers.</summary>
public sealed class PotIdentityMapOptions
{
    /// <summary>Configured mapping where key is pot number (1..N) and value is the pot identifier.</summary>
    public Dictionary<int, Guid> PotNumberToId { get; init; } = [];

    /// <summary>
    /// Optional explicit pot numbers expected to exist. Leave empty to avoid fixed-size assumptions.
    /// </summary>
    public List<int> ExpectedPotNumbers { get; init; } = [];
}

