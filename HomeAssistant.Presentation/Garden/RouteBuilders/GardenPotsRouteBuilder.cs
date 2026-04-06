using HomeAssistant.Presentation.Garden.Pots.Contracts;
using HomeAssistant.Presentation.Garden.Pots.Endpoints.GetPotConfiguration;
using HomeAssistant.Presentation.Garden.Pots.Endpoints.PostSavePotConfiguration;
using HomeAssistant.Presentation.Garden.Pots.Endpoints.PostSavePotConfiguration.Contracts;
using HomeAssistant.Presentation.Garden.Pots.Endpoints.PostUpdateSeedStatus;
using HomeAssistant.Presentation.Garden.Pots.Endpoints.PostUpdateSeedStatus.Contracts;
using HomeAssistant.Presentation.Garden.Pots.Dashboard.Endpoints.GetDashboard;
using HomeAssistant.Presentation.Garden.Contracts;

namespace HomeAssistant.Presentation.Garden.RouteBuilders;

/// <summary>Maps /api/garden/pots routes.</summary>
internal static class GardenPotsRouteBuilder
{
    /// <summary>Maps pot configuration, seed-status, and dashboard endpoints under <c>/api/garden/pots</c>.</summary>
    internal static IEndpointRouteBuilder MapGardenPotsRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var potsGroup = endpoints.MapGroup("/api/garden/pots")
            .WithTags("GardenPlanner");

        // Configuration
        potsGroup
            .MapGet("/{potId:guid}/configuration", GetPotConfigurationEndpoint.Handle)
            .WithName("GetPotConfiguration")
            .WithOpenApi()
            .Produces<PotConfigurationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        potsGroup
            .MapPost("/{potId:guid}/configuration", PostSavePotConfigurationEndpoint.Handle)
            .WithName("SavePotConfiguration")
            .WithOpenApi()
            .Accepts<SavePotConfigurationRequest>("application/json")
            .Produces<PotConfigurationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        // Seeds/Status
        potsGroup
            .MapPost("/{potId:guid}/seeds/{seedId:guid}/status", PostUpdateSeedStatusEndpoint.Handle)
            .WithName("UpdateSeedStatus")
            .WithOpenApi()
            .Accepts<UpdateSeedStatusRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        // Dashboard
        potsGroup
            .MapGet("/dashboard", GetDashboardEndpoint.Handle)
            .WithName("GetDashboard")
            .Produces<DashboardAggregationResponse>();

        return endpoints;
    }
}

