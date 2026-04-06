using HomeAssistant.Infrastructure.Messaging.Messaging.Metrics;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using AppMqttOptions = HomeAssistant.Application.Messaging.Configuration.MqttClientOptions;

namespace HomeAssistant.Infrastructure.Messaging.Messaging.Services;

/// <summary>
/// Handles the reconnection/backoff concern for the MQTT client.
/// Subscribes to <see cref="MqttConnectionManager.Disconnected"/> and tracks reconnection
/// state. This class is the designated future home for exponential backoff and
/// auto-reconnect logic.
/// </summary>
public sealed class MqttReconnectPolicy
{
    private readonly AppMqttOptions _options;
    private readonly ILogger<MqttReconnectPolicy> _logger;
    private int _reconnectAttempts;

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
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        connectionManager.Disconnected += OnDisconnectedAsync;
    }

    /// <summary>
    /// Handles a disconnection event from the broker.
    /// Normal disconnections are logged at Information level; unexpected ones increment
    /// the reconnect counter and emit a warning with the attempt number.
    /// </summary>
    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        if (args.Reason == MqttClientDisconnectReason.NormalDisconnection)
        {
            _logger.LogInformation("MQTT client disconnected normally.");
        }
        else
        {
            _reconnectAttempts++;
            _logger.LogWarning(
                "MQTT client disconnected unexpectedly (attempt #{Attempt}, delay {Delay}s). Reason: {Reason}. Exception: {Exception}",
                _reconnectAttempts,
                _options.ReconnectDelaySeconds,
                args.Reason,
                args.Exception?.Message);

            MqttMetrics.ReconnectionAttempts.Add(1);

            // TODO: implement exponential backoff + auto-reconnect here.
        }

        return Task.CompletedTask;
    }
}
