using HomeAssistant.Application.Messaging.Abstractions;
using HomeAssistant.Application.Messaging.Configuration;
using HomeAssistant.Application.Weather.Abstractions;
using HomeAssistant.Application.Weather.Configuration;
using HomeAssistant.Domain.Assistant.Abstractions;
using HomeAssistant.Domain.Common.Abstractions;
using HomeAssistant.Domain.PlantPots.Abstractions;
using HomeAssistant.Domain.PotConfigurations.Abstractions;
using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Abstractions;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Functions;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Services;
using HomeAssistant.Infrastructure.Messaging.Messaging.Services;
using HomeAssistant.Infrastructure.Persistence.Assistant.Repositories;
using HomeAssistant.Infrastructure.Persistence.Database;
using HomeAssistant.Infrastructure.Persistence.Database.Repositories;
using HomeAssistant.Infrastructure.Persistence.PlantPots.Repositories;
using HomeAssistant.Infrastructure.Persistence.PotConfigurations.Repositories;
using HomeAssistant.Infrastructure.Persistence.SensorReadings.Repositories;
using HomeAssistant.Infrastructure.Sensors.Sensors.BackgroundServices;
using HomeAssistant.Infrastructure.Sensors.Sensors.Providers;
using HomeAssistant.Integrations.OpenMeteo.Forecast.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Composition.DependencyInjection;

/// <summary>
/// Registers infrastructure and integration adapters used by the application host.
/// </summary>
public static class InfrastructureCompositionServiceCollectionExtensions
{
    /// <summary>
    /// Registers persistence, messaging, sensor providers, Open-Meteo client, and Home Assistant protocol tooling.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="environment">Current host environment.</param>
    /// <param name="configuration">Application configuration root.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureComposition(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddPersistence(configuration);
        services.AddExternalAdapters(configuration);
        services.AddSensorProviders(environment, configuration);
        services.AddHomeAssistantProtocolServices();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPlantPotRepository, PlantPotRepository>();
        services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IPotConfigurationRepository, PotConfigurationRepository>();
        services.AddScoped<IPersistedDataResetRepository, PersistedDataResetRepository>();

        return services;
    }

    private static IServiceCollection AddExternalAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        var mqttOptions = new MqttClientOptions();
        configuration.GetSection("Mqtt").Bind(mqttOptions);
        services.AddSingleton(mqttOptions);
        services.AddSingleton<IMqttClient, MqttClientService>();

        var openMeteoOptions = new OpenMeteoClientOptions();
        configuration.GetSection("OpenMeteo").Bind(openMeteoOptions);
        services.AddSingleton(openMeteoOptions);
        services.AddHttpClient<IOpenMeteoForecastClient, OpenMeteoForecastClient>((_, client) =>
        {
            client.BaseAddress = new Uri(openMeteoOptions.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        return services;
    }

    private static IServiceCollection AddSensorProviders(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsDevelopment())
        {
            services.AddScoped<ISensorProvider>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<MockSensorProvider>>();
                var potRepository = sp.GetRequiredService<IPotConfigurationRepository>();
                var mqttClient = sp.GetRequiredService<IMqttClient>();
                var config = sp.GetRequiredService<IConfiguration>();
                return new MockSensorProvider(logger, potRepository, mqttClient, config);
            });

            var mqttOptions = new MqttClientOptions();
            configuration.GetSection("Mqtt").Bind(mqttOptions);
            if (mqttOptions.PublishMockReadings)
            {
                services.AddHostedService<MockSensorMqttPublisherBackgroundService>();
            }
        }
        else
        {
            services.AddScoped<ISensorProvider>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Zigbee2MqttSensorProvider>>();
                var mqttClient = sp.GetRequiredService<IMqttClient>();
                var options = sp.GetRequiredService<MqttClientOptions>();
                return new Zigbee2MqttSensorProvider(mqttClient, options, logger);
            });
        }

        return services;
    }

    private static IServiceCollection AddHomeAssistantProtocolServices(this IServiceCollection services)
    {
        services.AddScoped<IHomeAssistantAreaProvider, HomeAssistantAreaProvider>();
        services.AddScoped<GardenPlannerKernelFunctions>();
        services.AddScoped<IHomeAssistantProtocolToolRegistry, HomeAssistantProtocolToolRegistry>();

        return services;
    }
}

