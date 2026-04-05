using HomeAssistant.Presentation.GardenAdvisor.Contracts;

namespace HomeAssistant.Presentation.GardenAdvisor.Abstractions;

/// <summary>Generates garden care advice from latest sensor and weather data.</summary>
public interface IGardenAdvisorService
{
    /// <summary>Generates advice and optionally publishes MQTT updates.</summary>
    Task<GardenAdviceResponse> GenerateAdviceAsync(bool publishToMqtt, CancellationToken ct = default);
}

