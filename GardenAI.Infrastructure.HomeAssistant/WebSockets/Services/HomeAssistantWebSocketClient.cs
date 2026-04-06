using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using GardenAI.Application.Common.Sync.Abstractions;
using GardenAI.Application.Common.Sync.Configuration;
using GardenAI.Application.Common.Sync.Contracts;
using Microsoft.Extensions.Logging;

namespace GardenAI.Infrastructure.HomeAssistant.WebSockets.Services;

/// <summary>WebSocket client for Home Assistant realtime events.</summary>
public sealed class HomeAssistantWebSocketClient : IHomeAssistantWebSocketClient, IDisposable
{
    private readonly HomeAssistantOptions _options;
    private readonly ILogger<HomeAssistantWebSocketClient> _logger;
    private readonly Channel<HaRegistryEvent> _eventChannel;
    private readonly ClientWebSocket _socket = new();
    private readonly CancellationTokenSource _receiveCts = new();

    private int _messageId;
    private Task? _receiveTask;

    public HomeAssistantWebSocketClient(
        HomeAssistantOptions options,
        ILogger<HomeAssistantWebSocketClient> logger)
    {
        _options = options;
        _logger = logger;
        _eventChannel = Channel.CreateBounded<HaRegistryEvent>(
            new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = true
            });
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (_socket.State == WebSocketState.Open)
        {
            return;
        }

        var wsUri = BuildWebSocketUri(_options.BaseUrl);
        _logger.LogInformation("Connecting to Home Assistant websocket at {Uri}", wsUri);

        await _socket.ConnectAsync(wsUri, ct).ConfigureAwait(false);
        _receiveTask = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token), CancellationToken.None);
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_socket.State == WebSocketState.Open)
        {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutdown", ct).ConfigureAwait(false);
        }

        _receiveCts.Cancel();

        if (_receiveTask is not null)
        {
            await _receiveTask.ConfigureAwait(false);
        }
    }

    public async Task<int> SendAsync<T>(T message, CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _messageId);
        var payload = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(payload);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct).ConfigureAwait(false);
        return id;
    }

    public Task SubscribeToEventAsync(string eventType, CancellationToken ct = default)
    {
        var message = new
        {
            id = Interlocked.Increment(ref _messageId),
            type = "subscribe_events",
            event_type = eventType
        };
        return SendTextAsync(JsonSerializer.Serialize(message), ct);
    }

    public Task UnsubscribeFromEventAsync(string eventType, CancellationToken ct = default)
    {
        var message = new
        {
            id = Interlocked.Increment(ref _messageId),
            type = "unsubscribe_events",
            event_type = eventType
        };
        return SendTextAsync(JsonSerializer.Serialize(message), ct);
    }

    public async Task<HaRegistryEvent?> ReadNextEventAsync(CancellationToken ct = default)
    {
        if (await _eventChannel.Reader.WaitToReadAsync(ct).ConfigureAwait(false)
            && _eventChannel.Reader.TryRead(out var evt))
        {
            return evt;
        }

        return null;
    }

    private async Task SendTextAsync(string payload, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct).ConfigureAwait(false);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[8 * 1024];

        while (!ct.IsCancellationRequested && _socket.State == WebSocketState.Open)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await _socket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                ms.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            var payload = Encoding.UTF8.GetString(ms.ToArray());
            TryPublishEvent(payload);
        }
    }

    private void TryPublishEvent(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (!root.TryGetProperty("type", out var typeElement)
                || !string.Equals(typeElement.GetString(), "event", StringComparison.Ordinal))
            {
                return;
            }

            var eventElement = root.GetProperty("event");
            var eventType = eventElement.GetProperty("event_type").GetString();
            if (string.IsNullOrWhiteSpace(eventType))
            {
                return;
            }

            var data = eventElement.TryGetProperty("data", out var dataElement)
                ? dataElement.Clone()
                : default;

            _eventChannel.Writer.TryWrite(new HaRegistryEvent
            {
                EventType = eventType,
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse websocket payload from Home Assistant");
        }
    }

    private static Uri BuildWebSocketUri(string baseUrl)
    {
        var normalized = baseUrl.TrimEnd('/');
        if (normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            normalized = "wss://" + normalized.Substring("https://".Length);
        }
        else if (normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            normalized = "ws://" + normalized.Substring("http://".Length);
        }

        return new Uri($"{normalized}/api/websocket", UriKind.Absolute);
    }

    public void Dispose()
    {
        _receiveCts.Cancel();
        _socket.Dispose();
        _receiveCts.Dispose();
    }
}

