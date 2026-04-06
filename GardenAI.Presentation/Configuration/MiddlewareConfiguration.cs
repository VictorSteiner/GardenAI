using GardenAI.Application.Messaging.Abstractions;
using GardenAI.Presentation.Chat.RouteBuilders;
using Scalar.AspNetCore;

namespace GardenAI.Presentation.Configuration;

/// <summary>Extension methods for middleware and routing configuration.</summary>
/// <remarks>
/// These extension methods are called from Program.cs in the following order:
/// 1. ConfigureMiddlewareAsync() – Initialize external services (MQTT, database, etc.)
/// 2. ConfigurePipeline() – Configure middleware (OpenAPI, error handlers, etc.)
/// 3. MapRoutes() – Map all HTTP endpoints by feature
/// </remarks>
internal static class MiddlewareConfiguration
{
    /// <summary>Configures middleware pipeline and initializes external connections.</summary>
    internal static async Task ConfigureMiddlewareAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Initialize MQTT connection
        try
        {
            var mqttClient = app.Services.GetRequiredService<IMqttClient>();
            await mqttClient.ConnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to initialize MQTT client on startup. Continuing without MQTT.");
            // Don't fail startup if MQTT is unavailable; allow graceful degradation
        }
    }

    /// <summary>Configures OpenAPI documentation in development and sets up middleware pipeline.</summary>
    internal static IApplicationBuilder ConfigurePipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // OpenAPI in development only
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            // DisableDefaultFonts: avoids loading Inter/JetBrains Mono from Scalar's CDN (important for Pi)
            app.MapScalarApiReference(options => options
                .WithTitle("GardenAI API")
                .DisableDefaultFonts());
        }

        app.UseHttpsRedirection();

        return app;
    }

    /// <summary>Maps all application routes.</summary>
    internal static IApplicationBuilder MapRoutes(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapChatRoutes();

        return app;
    }
}
