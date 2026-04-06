using HomeAssistant.Application.Messaging.Abstractions;
using HomeAssistant.Application.Messaging.Configuration;
using HomeAssistant.Infrastructure.Messaging.Messaging.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAssistant.Infrastructure.Messaging;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for registering MQTT messaging services.
/// </summary>
public static class MessagingServiceExtensions
{
    /// <summary>
    /// Registers all MQTT messaging services required by the application.
    /// </summary>
    /// <remarks>
    /// Registers (all as singletons):
    /// <list type="bullet">
    ///   <item><see cref="MqttClientOptions"/> bound from the <c>Mqtt</c> configuration section.</item>
    ///   <item><see cref="MqttConnectionManager"/> – connection lifecycle.</item>
    ///   <item><see cref="MqttReconnectPolicy"/> – reconnection/backoff policy.</item>
    ///   <item><see cref="IMqttClient"/> → <see cref="MqttClientService"/> – pub/subscribe adapter.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddMqttClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new MqttClientOptions();
        configuration.GetSection("Mqtt").Bind(options);
        services.AddSingleton(options);

        services.AddSingleton<MqttConnectionManager>();
        services.AddSingleton<MqttReconnectPolicy>();
        services.AddSingleton<IMqttClient, MqttClientService>();

        return services;
    }
}

