using HomeAssistant.Application.GardenAdvisor.Abstractions;
using HomeAssistant.Application.GardenAdvisor.Configuration;
using HomeAssistant.Application.GardenAdvisor.Services;
using HomeAssistant.Presentation.GardenAdvisor.BackgroundServices;
using HomeAssistant.Presentation.GardenAdvisor.Services;

namespace HomeAssistant.Presentation.Configuration;

/// <summary>Extension methods for garden advisor and AI agent configuration.</summary>
internal static class GardenAdvisorConfiguration
{
    /// <summary>Registers application-owned garden advice services plus presentation-owned planner and background components.</summary>
    internal static IServiceCollection AddGardenAdvisorServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Garden advisor options
        services.Configure<GardenAdvisorOptions>(configuration.GetSection("GardenAdvisor"));

        // Singleton stores and providers
        services.AddSingleton<IPlantProfileProvider, HomeAssistant.Application.GardenAdvisor.Services.PlantProfileProvider>();
        services.AddSingleton<IGardenAdviceStateStore, HomeAssistant.Application.GardenAdvisor.Services.GardenAdviceStateStore>();
        services.AddSingleton<HomeAssistant.Presentation.GardenAdvisor.Abstractions.IGardenPlannerHistoryStore, GardenPlannerHistoryStore>();

        // Scoped services
        services.AddScoped<IGardenAdvisorService, HomeAssistant.Application.GardenAdvisor.Services.GardenAdvisorService>();
        services.AddScoped<HomeAssistant.Presentation.GardenAdvisor.Services.IHomeAssistantAreaProvider,
            HomeAssistant.Presentation.GardenAdvisor.Services.HomeAssistantAreaProvider>();
        services.AddScoped<HomeAssistant.Presentation.GardenAdvisor.Abstractions.IGardenPlannerService, GardenPlannerService>();

        // Background services
        services.AddHostedService<GardenAdviceScheduleBackgroundService>();

        return services;
    }
}
