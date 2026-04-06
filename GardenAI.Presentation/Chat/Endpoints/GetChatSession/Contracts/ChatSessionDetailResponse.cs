using GardenAI.Presentation.Chat.Contracts;

namespace GardenAI.Presentation.Chat.Endpoints.GetChatSession.Contracts;

/// <summary>Response payload for one full session including message history.</summary>
public sealed record ChatSessionDetailResponse(
	Guid SessionId,
	string Title,
	string Capability,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt,
	IReadOnlyList<ChatSessionMessageResponse> Messages);

