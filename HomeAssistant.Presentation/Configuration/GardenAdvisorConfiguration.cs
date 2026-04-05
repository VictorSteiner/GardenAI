using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.BackgroundServices;
using HomeAssistant.Presentation.GardenAdvisor.Configuration;
using HomeAssistant.Presentation.GardenAdvisor.Services;

namespace HomeAssistant.Presentation.Configuration;

/// <summary>Extension methods for garden advisor and AI agent configuration.</summary>
internal static class GardenAdvisorConfiguration
{
    /// <summary>Registers garden advisor services and background workers owned by the presentation layer.</summary>
    internal static IServiceCollection AddGardenAdvisorServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Garden advisor options
        services.Configure<GardenAdvisorOptions>(configuration.GetSection("GardenAdvisor"));

        // Singleton stores and providers
        services.AddSingleton<IPlantProfileProvider, PlantProfileProvider>();
        services.AddSingleton<IGardenAdviceStateStore, GardenAdviceStateStore>();
        services.AddSingleton<IGardenPlannerHistoryStore, GardenPlannerHistoryStore>();

        // Scoped services
        services.AddScoped<IGardenAdvisorService, GardenAdvisorService>();
        services.AddScoped<HomeAssistant.Presentation.GardenAdvisor.Services.IHomeAssistantAreaProvider,
            HomeAssistant.Presentation.GardenAdvisor.Services.HomeAssistantAreaProvider>();
        services.AddScoped<IGardenPlannerService, GardenPlannerService>();

        // Background services
        services.AddHostedService<GardenAdviceScheduleBackgroundService>();

        return services;
    }
}
