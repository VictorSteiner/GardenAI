using System.Text.Json.Serialization;

namespace GardenAI.Presentation.Chat.Services;

/// <summary>Represents one Ollama chat message payload entry.</summary>
internal sealed record OllamaMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

