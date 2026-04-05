using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Abstractions;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Functions;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Services;
using HomeAssistant.Presentation.GardenAdvisor.Abstractions;
using HomeAssistant.Presentation.GardenAdvisor.BackgroundServices;
using HomeAssistant.Presentation.GardenAdvisor.Configuration;
using HomeAssistant.Presentation.GardenAdvisor.Services;
using Microsoft.Extensions.Options;

namespace HomeAssistant.Presentation.Configuration;

/// <summary>Extension methods for garden advisor and AI agent configuration.</summary>
internal static class GardenAdvisorConfiguration
{
    /// <summary>Registers garden advisor services, Semantic Kernel functions, and background services.</summary>
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

        // Semantic Kernel functions and protocol
        // Note: garden planner functions are AI-only via [KernelFunction] attributes.
        // They are NOT exposed as HTTP endpoints – only GardenPlannerKernelFunctions invokes them.
        // See: HomeAssistant.Infrastructure.HomeAssistant.Protocol.Functions.GardenPlannerKernelFunctions
        services.AddScoped<GardenPlannerKernelFunctions>();
        services.AddScoped<IHomeAssistantProtocolToolRegistry, HomeAssistantProtocolToolRegistry>();
        services.AddScoped<IGardenPlannerService, GardenPlannerService>();

        // Background services
        services.AddHostedService<GardenAdviceScheduleBackgroundService>();

        return services;
    }
}

