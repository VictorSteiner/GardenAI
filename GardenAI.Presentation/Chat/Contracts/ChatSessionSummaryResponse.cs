namespace GardenAI.Presentation.Chat.Contracts;

/// <summary>Summary information returned for chat sessions.</summary>
public sealed record ChatSessionSummaryResponse(
	Guid SessionId,
	string Title,
	string Capability,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt);

