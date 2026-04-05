using HomeAssistant.Presentation.Chat;
using HomeAssistant.Presentation.Chat.Services;

namespace HomeAssistant.Presentation.Configuration;

/// <summary>Extension methods for presentation-owned external client configuration.</summary>
internal static class ExternalClientsConfiguration
{
    /// <summary>Registers the Ollama-backed chat assistant client.</summary>
    internal static IServiceCollection AddExternalClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

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
