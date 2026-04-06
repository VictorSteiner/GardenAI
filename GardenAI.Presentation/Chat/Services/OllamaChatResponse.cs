using System.Text.Json.Serialization;

namespace GardenAI.Presentation.Chat.Services;

/// <summary>Represents the response payload returned by Ollama chat completions.</summary>
internal sealed record OllamaChatResponse(
    [property: JsonPropertyName("message")] OllamaMessage Message);

