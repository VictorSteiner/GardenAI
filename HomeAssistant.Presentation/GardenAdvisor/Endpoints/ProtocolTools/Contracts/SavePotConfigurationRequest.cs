namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools.Contracts;

/// <summary>Request contract for save-configuration planner tool operations.</summary>
public sealed record SavePotConfigurationRequest(
    int PotNumber,
    string PlantName,
    string SeedName,
    string RoomAreaId,
    string Status = "growing",
    DateTimeOffset? PlantedDate = null,
    string? Notes = null);

