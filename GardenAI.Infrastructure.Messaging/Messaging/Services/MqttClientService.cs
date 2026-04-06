using GardenAI.Infrastructure.Messaging.Messaging.Metrics;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using AppMqttClient = GardenAI.Application.Messaging.Abstractions.IMqttClient;

namespace GardenAI.Infrastructure.Messaging.Messaging.Services;

/// <summary>
/// Application-layer adapter that implements <see cref="AppMqttClient"/>.
/// Responsible for publish/subscribe operations and message routing only.
/// Connection lifecycle is delegated to <see cref="MqttConnectionManager"/>;
/// reconnection policy is handled by <see cref="MqttReconnectPolicy"/>.
/// </summary>
public sealed class MqttClientService : AppMqttClient, IAsyncDisposable
{
    private readonly MqttConnectionManager _connectionManager;
    private readonly ILogger<MqttClientService> _logger;

    /// <inheritdoc/>
    public event Func<string, string, Task> MessageReceivedAsync;

    /// <summary>
    /// Initialises the service and wires connection and message events from
    /// <paramref name="connectionManager"/>.
    /// </summary>
    public MqttClientService(
        MqttConnectionManager connectionManager,
        MqttReconnectPolicy reconnectPolicy,
        ILogger<MqttClientService> logger)
    {
        ArgumentNullException.ThrowIfNull(connectionManager);
        ArgumentNullException.ThrowIfNull(reconnectPolicy);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionManager = connectionManager;
        _logger = logger;

        // Keep policy dependency alive through DI and constructor graph.
        _ = reconnectPolicy;

        // Wire event handlers once at construction time.
        _connectionManager.Connected += OnConnectedAsync;
        _connectionManager.ApplicationMessageReceived += OnMessageReceivedAsync;
    }

    /// <inheritdoc/>
    public Task ConnectAsync(CancellationToken ct = default)
        => _connectionManager.ConnectAsync(ct);

    /// <inheritdoc/>
    public Task DisconnectAsync(CancellationToken ct = default)
        => _connectionManager.DisconnectAsync(ct);

    /// <inheritdoc/>
    public async Task PublishAsync(string topic, string payload, bool retainFlag = false, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(payload);

        try
        {
            if (_connectionManager.Client is null || !_connectionManager.IsConnected)
                throw new InvalidOperationException("MQTT client is not connected.");

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retainFlag)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _connectionManager.Client.PublishAsync(message, ct);

            _logger.LogDebug("Published message to topic {Topic} (size: {Size} bytes).", topic, payload.Length);
            MqttMetrics.MessagesPublished.Add(1, new KeyValuePair<string, object>("topic", topic));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}.", topic);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SubscribeAsync(IEnumerable<string> topics, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(topics);

        try
        {
            if (_connectionManager.Client is null || !_connectionManager.IsConnected)
                throw new InvalidOperationException("MQTT client is not connected.");

            var topicList = topics.Where(static t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            if (topicList.Count > 0)
            {
                var builder = new MqttClientSubscribeOptionsBuilder();
                foreach (var topic in topicList)
                    builder.WithTopicFilter(topic, MqttQualityOfServiceLevel.AtLeastOnce);

                await _connectionManager.Client.SubscribeAsync(builder.Build(), ct);
                _logger.LogInformation("Subscribed to {TopicCount} topics.", topicList.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to topics.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(topics);

        try
        {
            if (_connectionManager.Client is null || !_connectionManager.IsConnected)
                throw new InvalidOperationException("MQTT client is not connected.");

            var topicList = topics.Where(static t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            if (topicList.Count > 0)
            {
                var builder = new MqttClientUnsubscribeOptionsBuilder();
                foreach (var topic in topicList)
                    builder.WithTopicFilter(topic);

                await _connectionManager.Client.UnsubscribeAsync(builder.Build(), ct);
                _logger.LogInformation("Unsubscribed from {TopicCount} topics.", topicList.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe from topics.");
            throw;
        }
    }

    /// <summary>Logs when a connection to the broker is successfully established.</summary>
    private Task OnConnectedAsync(MqttClientConnectedEventArgs _)
    {
        _logger.LogInformation("MQTT client connected event received.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Forwards a received broker message to all <see cref="MessageReceivedAsync"/> subscribers.
    /// </summary>
    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        try
        {
            var topic = arg.ApplicationMessage.Topic;
            var payload = arg.ApplicationMessage.ConvertPayloadToString();

            _logger.LogDebug("Received MQTT message on topic {Topic} (size: {Size} bytes).", topic, payload.Length);
            MqttMetrics.MessagesReceived.Add(1, new KeyValuePair<string, object>("topic", topic));

            if (MessageReceivedAsync is not null)
                await MessageReceivedAsync.Invoke(topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received MQTT message.");
        }
    }

    /// <summary>Delegates disposal to the underlying connection manager.</summary>
    public ValueTask DisposeAsync() => _connectionManager.DisposeAsync();
}

