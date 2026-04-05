using HomeAssistant.Presentation.GardenAdvisor.Contracts;

namespace HomeAssistant.Presentation.GardenAdvisor.Abstractions;

/// <summary>Stores the latest generated garden advice in-memory for read endpoints.</summary>
public interface IGardenAdviceStateStore
{
    /// <summary>Gets the latest generated advice, if any.</summary>
    GardenAdviceResponse? GetLatest();

    /// <summary>Updates the latest generated advice snapshot.</summary>
    void SetLatest(GardenAdviceResponse advice);
}

