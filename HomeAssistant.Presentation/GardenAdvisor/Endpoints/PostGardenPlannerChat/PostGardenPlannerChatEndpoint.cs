using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.GardenAdvisor.Endpoints.PostGardenPlannerChat;

/// <summary>Endpoint: POST /api/garden/planner/chat – Send a natural-language message to the AI garden planner.</summary>
public sealed class PostGardenPlannerChatEndpoint
{
    /// <summary>
    /// Handles the POST request: enriches the message with live garden context, calls the AI,
    /// executes any detected planting actions, and returns the AI reply plus executed actions.
    /// </summary>
    public static async Task<Results<Ok<GardenPlannerChatResponse>, BadRequest<string>>> Handle(
        GardenPlannerChatRequest request,
        IGardenPlannerService plannerService,
        ILogger<PostGardenPlannerChatEndpoint> logger,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(plannerService);
        ArgumentNullException.ThrowIfNull(logger);

        if (string.IsNullOrWhiteSpace(request.Message))
            return TypedResults.BadRequest("Message must not be empty.");

        logger.LogInformation("Garden planner chat request received ({Length} chars).", request.Message.Length);

        var response = await plannerService.ChatAsync(request.Message, ct);

        logger.LogInformation(
            "Garden planner chat completed. ActionsExecuted={Count}.",
            response.ActionsExecuted.Count);

        return TypedResults.Ok(response);
    }
}

