namespace HomeAssistant.Presentation.Garden.Planner.Functions.Contracts;

/// <summary>Request contract for generate-advice planner tool operations.</summary>
public sealed record GenerateAdviceRequest(bool PublishToMqtt = false);

