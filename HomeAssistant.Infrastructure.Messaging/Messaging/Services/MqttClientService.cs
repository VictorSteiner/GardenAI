using HomeAssistant.Application.Messaging.Abstractions;
using HomeAssistant.Application.Messaging.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Diagnostics.Metrics;
using AppMqttClient = HomeAssistant.Application.Messaging.Abstractions.IMqttClient;
using AppMqttOptions = HomeAssistant.Application.Messaging.Configuration.MqttClientOptions;
using NetMqttClient = MQTTnet.Client.IMqttClient;

namespace HomeAssistant.Infrastructure.Messaging.Messaging.Services;

/// <summary>
/// Concrete MQTT client implementation using MQTTnet.
/// Handles connection, publish, subscribe, and automatic reconnection.
/// </summary>
public sealed class MqttClientService : AppMqttClient, IAsyncDisposable
{
    private readonly AppMqttOptions _options;
    private readonly ILogger<MqttClientService> _logger;
    private NetMqttClient? _client;
    private int _reconnectAttempts;
    private bool _isConnected;

    // Metrics
    private static readonly Meter MeterInstance = new("HomeAssistant.Messaging");
    private static readonly Counter<long> MessagesPublishedCounter = MeterInstance.CreateCounter<long>("mqtt.messages.published");
    private static readonly Counter<long> MessagesReceivedCounter = MeterInstance.CreateCounter<long>("mqtt.messages.received");
    private static readonly Counter<long> ReconnectionAttemptsCounter = MeterInstance.CreateCounter<long>("mqtt.reconnection.attempts");
    private static readonly Counter<long> ConnectionFailuresCounter = MeterInstance.CreateCounter<long>("mqtt.connection.failures");

    /// <inheritdoc/>
    public event Func<string, string, Task>? MessageReceivedAsync;

    /// <summary>Initialises the MQTT client service.</summary>
    /// <param name="options">MQTT configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public MqttClientService(AppMqttOptions options, ILogger<MqttClientService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Connecting to MQTT broker at {Host}:{Port}...", _options.Host, _options.Port);

            var factory = new MqttFactory();

            var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Host, _options.Port)
                .WithClientId(_options.ClientId)
                .WithCleanSession(true)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveSeconds));

            if (!string.IsNullOrEmpty(_options.Username))
                clientOptions.WithCredentials(_options.Username, _options.Password ?? string.Empty);

            _client = factory.CreateMqttClient();

            // Wire up event handlers
            _client.ConnectedAsync += OnConnectedAsync;
            _client.DisconnectedAsync += OnDisconnectedAsync;
            _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

            await _client.ConnectAsync(clientOptions.Build(), ct);

            _isConnected = true;
            _reconnectAttempts = 0;
            _logger.LogInformation("Successfully connected to MQTT broker.");
        }
        catch (Exception ex)
        {
            ConnectionFailuresCounter.Add(1);
            _logger.LogError(ex, "Failed to connect to MQTT broker.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        try
        {
            if (_client is not null && _isConnected)
            {
                _logger.LogInformation("Disconnecting from MQTT broker...");
                await _client.DisconnectAsync(null, ct);
                _isConnected = false;
                _logger.LogInformation("Successfully disconnected from MQTT broker.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while disconnecting from MQTT broker.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task PublishAsync(string topic, string payload, bool retainFlag = false, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(payload);

        try
        {
            if (_client is null || !_isConnected)
            {
                throw new InvalidOperationException("MQTT client is not connected.");
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retainFlag)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _client.PublishAsync(message, ct);

            _logger.LogDebug("Published message to topic {Topic} (size: {Size} bytes).", topic, payload.Length);
            MessagesPublishedCounter.Add(1, new KeyValuePair<string, object?>("topic", topic));
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
            if (_client is null || !_isConnected)
            {
                throw new InvalidOperationException("MQTT client is not connected.");
            }

            var topicList = topics.Where(static t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            if (topicList.Count > 0)
            {
                var subscribeOptionsBuilder = new MqttClientSubscribeOptionsBuilder();
                foreach (var topic in topicList)
                {
                    subscribeOptionsBuilder.WithTopicFilter(topic, MqttQualityOfServiceLevel.AtLeastOnce);
                }

                await _client.SubscribeAsync(subscribeOptionsBuilder.Build(), ct);
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
            if (_client is null || !_isConnected)
            {
                throw new InvalidOperationException("MQTT client is not connected.");
            }

            var topicList = topics.Where(static t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            if (topicList.Count > 0)
            {
                var unsubscribeOptionsBuilder = new MqttClientUnsubscribeOptionsBuilder();
                foreach (var topic in topicList)
                {
                    unsubscribeOptionsBuilder.WithTopicFilter(topic);
                }

                await _client.UnsubscribeAsync(unsubscribeOptionsBuilder.Build(), ct);
                _logger.LogInformation("Unsubscribed from {TopicCount} topics.", topicList.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe from topics.");
            throw;
        }
    }

    /// <summary>Raised when connection is successfully established.</summary>
    private Task OnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client connected event received.");
        return Task.CompletedTask;
    }

    /// <summary>Raised when connection is lost.</summary>
    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _isConnected = false;

        if (arg.Reason == MqttClientDisconnectReason.NormalDisconnection)
        {
            _logger.LogInformation("MQTT client disconnected normally.");
        }
        else
        {
            _reconnectAttempts++;
            _logger.LogWarning("MQTT client disconnected unexpectedly (attempt #{Attempt}). Exception: {Exception}",
                _reconnectAttempts, arg.Exception?.Message);
            ReconnectionAttemptsCounter.Add(1);
        }

        return Task.CompletedTask;
    }

    /// <summary>Raised when a message is received on a subscribed topic.</summary>
    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        try
        {
            var topic = arg.ApplicationMessage.Topic;
            var payload = arg.ApplicationMessage.ConvertPayloadToString();

            _logger.LogDebug("Received MQTT message on topic {Topic} (size: {Size} bytes).", topic, payload.Length);
            MessagesReceivedCounter.Add(1, new KeyValuePair<string, object?>("topic", topic));

            // Invoke registered event handler if any
            if (MessageReceivedAsync is not null)
            {
                await MessageReceivedAsync.Invoke(topic, payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received MQTT message.");
        }
    }

    /// <summary>Releases the MQTT client resources.</summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_client is not null)
            {
                await DisconnectAsync();
                _client.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing MQTT client.");
        }
    }
}

