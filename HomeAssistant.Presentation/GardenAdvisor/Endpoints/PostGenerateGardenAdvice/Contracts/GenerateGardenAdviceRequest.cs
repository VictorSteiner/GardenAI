namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostGenerateGardenAdvice.Contracts;

/// <summary>Request payload for manual garden advice generation.</summary>
public sealed record GenerateGardenAdviceRequest(bool PublishToMqtt = true);

