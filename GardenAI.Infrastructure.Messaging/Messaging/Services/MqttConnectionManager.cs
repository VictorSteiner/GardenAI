using GardenAI.Infrastructure.Messaging.Messaging.Metrics;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using AppMqttOptions = GardenAI.Application.Messaging.Configuration.MqttClientOptions;
using NetMqttClient = MQTTnet.Client.IMqttClient;

namespace GardenAI.Infrastructure.Messaging.Messaging.Services;

/// <summary>
/// Manages the MQTT connection lifecycle: option building, client creation, connect,
/// disconnect, and disposal of the underlying MQTTnet client.
/// </summary>
/// <remarks>
/// Fires forwarding events (<see cref="Connected"/>, <see cref="Disconnected"/>,
/// <see cref="ApplicationMessageReceived"/>) so consumers register once at
/// construction time and receive events for the lifetime of the connection.
/// </remarks>
public sealed class MqttConnectionManager : IAsyncDisposable
{
    private readonly AppMqttOptions _options;
    private readonly ILogger<MqttConnectionManager> _logger;
    private readonly SemaphoreSlim _connectionGate = new(1, 1);
    private NetMqttClient _client;
    private bool _isStopping;

    /// <summary>The connected raw MQTTnet client. <c>null</c> before first connect.</summary>
    public NetMqttClient Client => _client;

    /// <summary>Whether the client is currently connected to the broker.</summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Whether shutdown/disconnect was requested intentionally.
    /// Reconnect policy uses this to avoid reconnect attempts during normal shutdown.
    /// </summary>
    public bool IsStopping => _isStopping;

    /// <summary>Raised when a connection to the broker is successfully established.</summary>
    public event Func<MqttClientConnectedEventArgs, Task> Connected;

    /// <summary>Raised when the connection to the broker is lost or closed.</summary>
    public event Func<MqttClientDisconnectedEventArgs, Task> Disconnected;

    /// <summary>Raised when a message arrives on a subscribed topic.</summary>
    public event Func<MqttApplicationMessageReceivedEventArgs, Task> ApplicationMessageReceived;

    /// <summary>Initialises the connection manager with broker options and a logger.</summary>
    public MqttConnectionManager(AppMqttOptions options, ILogger<MqttConnectionManager> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Builds the MQTTnet client, wires lifecycle and message events, and connects to the broker.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _connectionGate.WaitAsync(ct);
        try
        {
            _isStopping = false;

            if (_client is null)
                _client = CreateAndWireClient();

            if (_client.IsConnected)
            {
                IsConnected = true;
                return;
            }

            _logger.LogInformation("Connecting to MQTT broker at {Host}:{Port}...", _options.Host, _options.Port);
            await _client.ConnectAsync(BuildClientOptions(), ct);

            IsConnected = true;
            _logger.LogInformation("Successfully connected to MQTT broker.");
        }
        catch (Exception ex)
        {
            MqttMetrics.ConnectionFailures.Add(1);
            _logger.LogError(ex, "Failed to connect to MQTT broker.");
            throw;
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    /// <summary>Gracefully closes the active connection to the broker.</summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        await _connectionGate.WaitAsync(ct);
        try
        {
            _isStopping = true;

            if (_client is null)
            {
                IsConnected = false;
                return;
            }

            if (!_client.IsConnected)
            {
                IsConnected = false;
                return;
            }

            _logger.LogInformation("Disconnecting from MQTT broker...");
            await _client.DisconnectAsync(null, ct);
            IsConnected = false;
            _logger.LogInformation("Successfully disconnected from MQTT broker.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while disconnecting from MQTT broker.");
            throw;
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    private NetMqttClient CreateAndWireClient()
    {
        var client = new MqttFactory().CreateMqttClient();

        // Forward raw MQTTnet events to this manager's events.
        client.ConnectedAsync += e => Connected?.Invoke(e) ?? Task.CompletedTask;
        client.DisconnectedAsync += e =>
        {
            IsConnected = false;
            return Disconnected?.Invoke(e) ?? Task.CompletedTask;
        };
        client.ApplicationMessageReceivedAsync += e =>
            ApplicationMessageReceived?.Invoke(e) ?? Task.CompletedTask;

        return client;
    }

    private MQTTnet.Client.MqttClientOptions BuildClientOptions()
    {
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId(_options.ClientId)
            .WithCleanSession(true)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveSeconds));

        if (!string.IsNullOrEmpty(_options.Username))
            builder.WithCredentials(_options.Username, _options.Password ?? string.Empty);

        return builder.Build();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        try
        {
            await DisconnectAsync();

            if (_client is not null)
            {
                _client.Dispose();
                _client = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing MQTT connection manager.");
        }
        finally
        {
            _connectionGate.Dispose();
        }
    }
}
