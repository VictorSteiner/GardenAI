using GardenAI.Application.Common.Sync.Abstractions;
using GardenAI.Application.Common.Sync.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GardenAI.Infrastructure.HomeAssistant.Sync.Services;

/// <summary>Background service that owns HA connection lifecycle and event processing loop.</summary>
public sealed class HomeAssistantSyncBackgroundService : BackgroundService
{
    private readonly IHomeAssistantWebSocketClient _webSocketClient;
    private readonly ISyncOrchestrator _syncOrchestrator;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HomeAssistantOptions _options;
    private readonly ILogger<HomeAssistantSyncBackgroundService> _logger;

    public HomeAssistantSyncBackgroundService(
        IHomeAssistantWebSocketClient webSocketClient,
        ISyncOrchestrator syncOrchestrator,
        IServiceScopeFactory scopeFactory,
        HomeAssistantOptions options,
        ILogger<HomeAssistantSyncBackgroundService> logger)
    {
        _webSocketClient = webSocketClient;
        _syncOrchestrator = syncOrchestrator;
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HomeAssistant sync background service started");

        var attempts = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _webSocketClient.ConnectAsync(stoppingToken).ConfigureAwait(false);
                await _syncOrchestrator.RunInitialSyncAsync(stoppingToken).ConfigureAwait(false);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var evt = await _webSocketClient.ReadNextEventAsync(stoppingToken).ConfigureAwait(false);
                    if (evt is null)
                    {
                        break;
                    }

                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var handler = scope.ServiceProvider.GetRequiredKeyedService<IRegistryEventHandler>(evt.EventType);
                    await handler.HandleAsync(evt, stoppingToken).ConfigureAwait(false);
                }

                attempts = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                attempts++;
                if (attempts > _options.ReconnectMaxRetries)
                {
                    _logger.LogError(ex, "Stopping HA sync after max reconnect attempts: {Attempts}", attempts);
                    break;
                }

                var delay = ComputeBackoff(attempts);
                _logger.LogWarning(ex, "HA sync reconnect attempt {Attempt}. Waiting {Delay}", attempts, delay);
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
            }
        }

        await _webSocketClient.DisconnectAsync(stoppingToken).ConfigureAwait(false);
        _logger.LogInformation("HomeAssistant sync background service stopped");
    }

    private static TimeSpan ComputeBackoff(int attempt)
    {
        var seconds = Math.Min(60, (int)Math.Pow(2, Math.Min(attempt, 5)));
        return TimeSpan.FromSeconds(seconds);
    }
}

