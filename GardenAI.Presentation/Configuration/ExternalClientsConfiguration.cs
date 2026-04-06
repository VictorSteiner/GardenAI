using GardenAI.Application.Chat.Abstractions;
using GardenAI.Presentation.Chat.Services;
using Microsoft.Extensions.Logging;

namespace GardenAI.Presentation.Configuration;

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

        services.AddHttpClient("ollama-chat", client =>
        {
            var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434/";
            client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<IChatAssistant>(sp =>
            new OllamaChatAssistant(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("ollama-chat"),
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<ILogger<OllamaChatAssistant>>()));

        return services;
    }
}
