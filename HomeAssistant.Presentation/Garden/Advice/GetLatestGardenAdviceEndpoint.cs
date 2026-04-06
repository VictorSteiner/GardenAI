using HomeAssistant.Application.GardenAdvisor.Abstractions;
using HomeAssistant.Application.GardenAdvisor.Contracts.Advice;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeAssistant.Presentation.Garden.Advice.Endpoints.GetLatestGardenAdvice;

/// <summary>Maps endpoint for reading the latest generated garden advice.</summary>
internal static class GetLatestGardenAdviceEndpoint
{
    /// <summary>Maps the route handler.</summary>
    internal static RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);

        return group.MapGet(
                "/latest",
                Results<Ok<GardenAdviceResponse>, NotFound> (IGardenAdviceStateStore stateStore) =>
                {
                    var latest = stateStore.GetLatest();
                    return latest is null
                        ? TypedResults.NotFound()
                        : TypedResults.Ok(latest);
                })
            .WithName("GetLatestGardenAdvice")
            .WithSummary("Returns the latest generated garden advice summary")
            .WithDescription("Reads the in-memory advisory snapshot generated manually or by the scheduler.")
            .Produces<GardenAdviceResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}

