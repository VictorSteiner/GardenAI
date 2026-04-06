using System.Text.Json.Serialization;

namespace HomeAssistant.Presentation.Chat.Services;

/// <summary>Represents the response payload returned by Ollama chat completions.</summary>
internal sealed record OllamaChatResponse(
    [property: JsonPropertyName("message")] OllamaMessage Message);

