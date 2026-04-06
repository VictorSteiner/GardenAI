using HomeAssistant.Application.GardenAdvisor.Abstractions;
using HomeAssistant.Application.GardenAdvisor.Contracts.Advice;
using HomeAssistant.Presentation.Garden.Advice.Endpoints.PostGenerateGardenAdvice.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.Garden.Advice.Endpoints.PostGenerateGardenAdvice;

/// <summary>Maps endpoint that manually triggers new garden advice generation.</summary>
internal static class PostGenerateGardenAdviceEndpoint
{
    /// <summary>Maps the route handler.</summary>
    internal static RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);

        return group.MapPost(
                "/generate",
                async Task<Results<Ok<GardenAdviceResponse>, ProblemHttpResult>> (
                    GenerateGardenAdviceRequest? request,
                    IGardenAdvisorService advisorService,
                    CancellationToken ct) =>
                {
                    try
                    {
                        var result = await advisorService.GenerateAdviceAsync(request?.PublishToMqtt ?? true, ct);
                        return TypedResults.Ok(result);
                    }
                    catch (Exception ex)
                    {
                        return TypedResults.Problem(
                            detail: ex.Message,
                            title: "Garden advice generation failed",
                            statusCode: StatusCodes.Status502BadGateway);
                    }
                })
            .WithName("PostGenerateGardenAdvice")
            .WithSummary("Generates a new advice summary from latest sensor and weather data")
            .WithDescription("Triggers Ollama to evaluate the latest pot state with weather context and optional MQTT publication.")
            .Produces<GardenAdviceResponse>()
            .ProducesProblem(StatusCodes.Status502BadGateway);
    }
}

