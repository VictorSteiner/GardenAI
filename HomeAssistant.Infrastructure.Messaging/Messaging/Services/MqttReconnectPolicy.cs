using HomeAssistant.Infrastructure.Messaging.Messaging.Metrics;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using AppMqttOptions = HomeAssistant.Application.Messaging.Configuration.MqttClientOptions;

namespace HomeAssistant.Infrastructure.Messaging.Messaging.Services;

/// <summary>
/// Handles reconnection/backoff for the MQTT client.
/// Subscribes to <see cref="MqttConnectionManager.Disconnected"/> and starts a reconnect
/// loop for unexpected disconnects.
/// </summary>
public sealed class MqttReconnectPolicy
{
    private readonly MqttConnectionManager _connectionManager;
    private readonly AppMqttOptions _options;
    private readonly ILogger<MqttReconnectPolicy> _logger;
    private readonly SemaphoreSlim _reconnectLoopGate = new(1, 1);
    private readonly Random _random = new();

    /// <summary>
    /// Initialises the reconnect policy and attaches to the connection manager's
    /// <see cref="MqttConnectionManager.Disconnected"/> event.
    /// </summary>
    public MqttReconnectPolicy(
        MqttConnectionManager connectionManager,
        AppMqttOptions options,
        ILogger<MqttReconnectPolicy> logger)
    {
        ArgumentNullException.ThrowIfNull(connectionManager);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionManager = connectionManager;
        _options = options;
        _logger = logger;

        _connectionManager.Disconnected += OnDisconnectedAsync;
    }

    /// <summary>
    /// Handles disconnection events.
    /// Normal disconnections (or explicit shutdown) are ignored by auto-reconnect.
    /// Unexpected disconnections start a background reconnect loop.
    /// </summary>
    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        var isNormal = args.Reason == MqttClientDisconnectReason.NormalDisconnection;
        if (isNormal || _connectionManager.IsStopping)
        {
            _logger.LogInformation("MQTT client disconnected intentionally or normally.");
            return Task.CompletedTask;
        }

        if (!_options.EnableAutoReconnect)
        {
            _logger.LogInformation("MQTT auto-reconnect is disabled. Skipping reconnect loop.");
            return Task.CompletedTask;
        }

        _ = Task.Run(RunReconnectLoopAsync);
        return Task.CompletedTask;
    }

    private async Task RunReconnectLoopAsync()
    {
        if (!await _reconnectLoopGate.WaitAsync(0))
            return;

        try
        {
            var attempt = 0;

            while (!_connectionManager.IsStopping && !_connectionManager.IsConnected)
            {
                attempt++;

                if (_options.MaxReconnectAttempts > 0 && attempt > _options.MaxReconnectAttempts)
                {
                    _logger.LogError("Reached max reconnect attempts ({MaxAttempts}). Giving up.", _options.MaxReconnectAttempts);
                    break;
                }

                var delay = CalculateDelay(attempt);
                MqttMetrics.ReconnectionAttempts.Add(1);

                _logger.LogWarning(
                    "MQTT disconnected unexpectedly. Reconnect attempt #{Attempt} in {DelayMs} ms.",
                    attempt,
                    (int)delay.TotalMilliseconds);

                try
                {
                    await Task.Delay(delay);

                    if (_connectionManager.IsStopping)
                        break;

                    await _connectionManager.ConnectAsync();

                    if (_connectionManager.IsConnected)
                    {
                        MqttMetrics.ReconnectionSucceeded.Add(1);
                        _logger.LogInformation("MQTT reconnect succeeded after {AttemptCount} attempts.", attempt);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MqttMetrics.ReconnectionFailed.Add(1);
                    _logger.LogWarning(ex, "MQTT reconnect attempt #{Attempt} failed.", attempt);
                }
            }
        }
        finally
        {
            _reconnectLoopGate.Release();
        }
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        var baseSeconds = Math.Max(1, _options.ReconnectDelaySeconds);
        var maxSeconds = Math.Max(baseSeconds, _options.MaxReconnectDelaySeconds);
        var jitterPercent = Math.Clamp(_options.ReconnectJitterPercent, 0, 100);

        var exponentialSeconds = Math.Min(maxSeconds, baseSeconds * Math.Pow(2, attempt - 1));
        var jitterRangeSeconds = exponentialSeconds * jitterPercent / 100d;
        var jitterSeconds = jitterRangeSeconds <= 0
            ? 0
            : (_random.NextDouble() * 2d * jitterRangeSeconds) - jitterRangeSeconds;

        var finalSeconds = Math.Clamp(exponentialSeconds + jitterSeconds, 1, maxSeconds);
        return TimeSpan.FromSeconds(finalSeconds);
    }
}
