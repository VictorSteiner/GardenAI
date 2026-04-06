using HomeAssistant.Application.GardenAdvisor.Contracts.PlantProfiles;

namespace HomeAssistant.Application.GardenAdvisor.Abstractions;

/// <summary>Provides plant profile metadata for each monitored pot.</summary>
public interface IPlantProfileProvider
{
    /// <summary>Returns all known pot profiles keyed by pot identifier.</summary>
    IReadOnlyDictionary<Guid, PlantProfile> GetProfiles();
}

