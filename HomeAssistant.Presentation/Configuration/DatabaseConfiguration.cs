using HomeAssistant.Domain.Assistant.Abstractions;
using HomeAssistant.Domain.Common.Abstractions;
using HomeAssistant.Domain.PlantPots.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Infrastructure.Persistence.Assistant.Repositories;
using HomeAssistant.Infrastructure.Persistence.Database;
using HomeAssistant.Infrastructure.Persistence.Database.Repositories;
using HomeAssistant.Infrastructure.Persistence.PlantPots.Repositories;
using HomeAssistant.Infrastructure.Persistence.PotConfigurations.Repositories;
using HomeAssistant.Infrastructure.Persistence.SensorReadings.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeAssistant.Presentation.Configuration;

/// <summary>Extension methods for database and repository configuration.</summary>
internal static class DatabaseConfiguration
{
    /// <summary>Registers database context and all repository implementations.</summary>
    internal static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Database context
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IPlantPotRepository, PlantPotRepository>();
        services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IPotConfigurationRepository, PotConfigurationRepository>();
        services.AddScoped<IPersistedDataResetRepository, PersistedDataResetRepository>();

        return services;
    }
}

