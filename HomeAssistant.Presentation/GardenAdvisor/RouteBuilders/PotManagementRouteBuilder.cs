using HomeAssistant.Presentation.GardenAdvisor.PotManagement.Contracts;
using HomeAssistant.Presentation.GardenAdvisor.PotManagement.Endpoints.GetPotConfiguration;
using HomeAssistant.Presentation.GardenAdvisor.PotManagement.Endpoints.PostSavePotConfiguration;
using HomeAssistant.Presentation.GardenAdvisor.PotManagement.Endpoints.PostSavePotConfiguration.Contracts;
using HomeAssistant.Presentation.GardenAdvisor.PotManagement.Endpoints.PostUpdateSeedStatus;
using HomeAssistant.Presentation.GardenAdvisor.PotManagement.Endpoints.PostUpdateSeedStatus.Contracts;

namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps pot-management route boundaries for GardenAdvisor.</summary>
public static class PotManagementRouteBuilder
{
    /// <summary>Maps pot-management routes while preserving existing API paths.</summary>
    public static IEndpointRouteBuilder MapPotManagementRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var potsGroup = endpoints.MapGroup("/api/garden/pots")
            .WithTags("GardenPlanner");

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

        potsGroup
            .MapPost("/{potId:guid}/seeds/{seedId:guid}/status", PostUpdateSeedStatusEndpoint.Handle)
            .WithName("UpdateSeedStatus")
            .WithOpenApi()
            .Accepts<UpdateSeedStatusRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}

