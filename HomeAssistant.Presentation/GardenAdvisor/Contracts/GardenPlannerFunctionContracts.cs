namespace HomeAssistant.Presentation.GardenAdvisor.Contracts;

/// <summary>Common request contract for pot-number based planner functions.</summary>
public sealed record PotNumberFunctionRequest(
    /// <summary>Pot number in the user-facing range 1–6.</summary>
    int PotNumber);

/// <summary>Request contract for room-based planner functions.</summary>
public sealed record RoomAreaFunctionRequest(
    /// <summary>Home Assistant area identifier such as <c>living_room</c>.</summary>
    string RoomAreaId);

/// <summary>Request contract for save-configuration planner functions.</summary>
public sealed record SavePlannerPotConfigurationFunctionRequest(
    int PotNumber,
    string PlantName,
    string SeedName,
    string RoomAreaId,
    string Status = "growing",
    DateTimeOffset? PlantedDate = null,
    string? Notes = null);

/// <summary>Request contract for updating seed status through planner functions.</summary>
public sealed record UpdatePlannerSeedStatusFunctionRequest(
    int PotNumber,
    string NewStatus);

/// <summary>Request contract for harvest-readiness planner functions.</summary>
public sealed record HarvestReadinessFunctionRequest(
    string? FilterByStatus = null);

/// <summary>Request contract for planner-driven advice generation.</summary>
public sealed record GeneratePlannerAdviceFunctionRequest(
    bool PublishToMqtt = true);

