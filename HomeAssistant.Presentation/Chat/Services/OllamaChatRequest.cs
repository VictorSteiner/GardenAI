using System.Text.Json.Serialization;

namespace HomeAssistant.Presentation.Chat.Services;

/// <summary>Represents the request payload sent to Ollama chat completions.</summary>
internal sealed record OllamaChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IReadOnlyList<OllamaMessage> Messages,
    [property: JsonPropertyName("stream")] bool Stream);

