namespace HomeAssistant.Presentation.Chat;

/// <summary>Response payload returned by the chatbot helper endpoint.</summary>
public sealed record ChatResponse(string Reply, string Model);

