namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.ProtocolTools.Contracts;

/// <summary>Request contract for generate-advice planner tool operations.</summary>
public sealed record GenerateAdviceRequest(bool PublishToMqtt = false);

