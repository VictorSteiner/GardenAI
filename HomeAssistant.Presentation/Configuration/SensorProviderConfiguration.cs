using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Infrastructure.Messaging.Configuration;
using HomeAssistant.Infrastructure.Messaging.Messaging.Abstractions;
using HomeAssistant.Infrastructure.Sensors.Sensors.BackgroundServices;
using HomeAssistant.Infrastructure.Sensors.Sensors.Providers;
using HomeAssistant.Domain.PotConfigurations.Abstractions;

namespace HomeAssistant.Presentation.Configuration;

/// <summary>Extension methods for sensor provider configuration.</summary>
internal static class SensorProviderConfiguration
{
    /// <summary>Registers appropriate sensor provider based on environment (mock in Development, Zigbee2MQTT in Production).</summary>
    internal static IServiceCollection AddSensorProvider(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(configuration);

        if (environment.IsDevelopment())
        {
            // Mock sensor provider for development
            services.AddScoped<ISensorProvider>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<MockSensorProvider>>();
                var potRepository = sp.GetRequiredService<IPotConfigurationRepository>();
                var mqttClient = sp.GetRequiredService<IMqttClient>();
                var config = sp.GetRequiredService<IConfiguration>();
                return new MockSensorProvider(logger, potRepository, mqttClient, config);
            });

            // Optionally publish mock readings to MQTT for integration testing
            var mqttOptions = new MqttClientOptions();
            configuration.GetSection("Mqtt").Bind(mqttOptions);
            if (mqttOptions.PublishMockReadings)
            {
                services.AddHostedService<MockSensorMqttPublisherBackgroundService>();
            }
        }
        else
        {
            // Real Zigbee2MQTT provider for production
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
}

