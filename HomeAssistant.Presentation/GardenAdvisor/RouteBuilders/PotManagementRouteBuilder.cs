namespace HomeAssistant.Presentation.GardenAdvisor.RouteBuilders;

/// <summary>Maps pot-management route boundaries for GardenAdvisor.</summary>
public static class PotManagementRouteBuilder
{
    /// <summary>
    /// Maps pot-management routes.
    /// Route paths are preserved by leaving this slice as a no-op until endpoint re-homing lands.
    /// </summary>
    public static IEndpointRouteBuilder MapPotManagementRoutes(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints;
    }
}

