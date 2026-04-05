using HomeAssistant.Infrastructure.Messaging.Configuration;
using HomeAssistant.Infrastructure.Messaging.Messaging.Abstractions;
using HomeAssistant.Infrastructure.Messaging.Messaging.Services;
using HomeAssistant.Integrations.OpenMeteo.Forecast.Abstractions;
using HomeAssistant.Integrations.OpenMeteo.Forecast.Clients;
using HomeAssistant.Integrations.OpenMeteo.Forecast.Configuration;
using HomeAssistant.Presentation.Chat;
using HomeAssistant.Presentation.Chat.Services;

namespace HomeAssistant.Presentation.Configuration;

/// <summary>Extension methods for external HTTP client and messaging configuration.</summary>
internal static class ExternalClientsConfiguration
{
    /// <summary>Registers MQTT client, Open-Meteo forecast client, and Ollama chat assistant.</summary>
    internal static IServiceCollection AddExternalClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // MQTT Client
        var mqttOptions = new MqttClientOptions();
        configuration.GetSection("Mqtt").Bind(mqttOptions);
        services.AddSingleton(mqttOptions);
        services.AddSingleton<IMqttClient, MqttClientService>();

        // Open-Meteo Forecast Client
        var openMeteoOptions = new OpenMeteoClientOptions();
        configuration.GetSection("OpenMeteo").Bind(openMeteoOptions);
        services.AddSingleton(openMeteoOptions);
        services.AddHttpClient<IOpenMeteoForecastClient, OpenMeteoForecastClient>((_, client) =>
        {
            client.BaseAddress = new Uri(openMeteoOptions.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        // Chat Assistant (Ollama)
        services.AddHttpClient<IChatAssistant, OllamaChatAssistant>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["Ollama:BaseUrl"] ?? "http://localhost:11434/";
            client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        return services;
    }
}

