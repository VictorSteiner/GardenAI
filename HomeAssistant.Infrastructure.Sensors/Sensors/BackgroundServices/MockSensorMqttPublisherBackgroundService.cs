using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Infrastructure.Messaging.Configuration;
using HomeAssistant.Infrastructure.Sensors.Sensors.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Infrastructure.Sensors.Sensors.BackgroundServices;

/// <summary>
/// Development background service that periodically triggers the mock sensor provider.
/// This causes mock readings to be published to MQTT when PublishMockReadings is enabled.
/// </summary>
public sealed class MockSensorMqttPublisherBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MockSensorMqttPublisherBackgroundService> _logger;
    private readonly int _intervalSeconds;

    /// <summary>Initialises the background publisher.</summary>
    public MockSensorMqttPublisherBackgroundService(
        IServiceScopeFactory scopeFactory,
        MqttClientOptions mqttOptions,
        ILogger<MockSensorMqttPublisherBackgroundService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _intervalSeconds = Math.Max(1, mqttOptions.MockPublishIntervalSeconds);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Mock MQTT publisher background service started with interval {IntervalSeconds}s.",
            _intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sensorProvider = scope.ServiceProvider.GetRequiredService<ISensorProvider>();

                if (sensorProvider is MockSensorProvider)
                {
                    await sensorProvider.GetLatestReadingsAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Mock MQTT publisher cycle failed; retrying on next interval.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}

