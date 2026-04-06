namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PlannerFunctions.Contracts;

/// <summary>Request contract for planner-driven advice generation.</summary>
public sealed record GeneratePlannerAdviceFunctionRequest(
    bool PublishToMqtt = true);

