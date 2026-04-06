namespace GardenAI.Presentation.Chat.Contracts;

/// <summary>Response payload for one persisted chat message.</summary>
public sealed record ChatSessionMessageResponse(
	Guid MessageId,
	string Role,
	string Content,
	DateTimeOffset CreatedAt);

