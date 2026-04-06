namespace GardenAI.Presentation.Chat.Services;

/// <summary>Builds the system prompt used for chat assistant capability modes.</summary>
internal static class ChatSystemPromptBuilder
{
    /// <summary>Builds a capability-specific system prompt from configuration.</summary>
    internal static string Build(IConfiguration configuration, string capability)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(capability);

        var defaultPrompt = configuration["Assistant:SystemPrompt"]
            ?? "You are GardenAI Helper for a Raspberry Pi garden automation system. Give practical, safe, concise guidance focused on plant care, seeding schedules, sensor interpretation, and home assistant troubleshooting.";

        return $"{defaultPrompt} Capability mode: {capability}.";
    }
}

