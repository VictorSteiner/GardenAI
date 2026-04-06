using GardenAI.Application.Common.Sync.Contracts;

namespace GardenAI.Application.Common.Sync.Abstractions;

/// <summary>
/// WebSocket client for the Home Assistant real-time API.
/// Handles authentication, message ID sequencing, event subscriptions, and keepalive pings.
/// </summary>
public interface IHomeAssistantWebSocketClient
{
    /// <summary>Connects to the HA WebSocket endpoint and completes the auth handshake.</summary>
    Task ConnectAsync(CancellationToken ct = default);

    /// <summary>Gracefully closes the WebSocket connection.</summary>
    Task DisconnectAsync(CancellationToken ct = default);

    /// <summary>Sends a typed message to HA and returns the next outbound message ID used.</summary>
    Task<int> SendAsync<T>(T message, CancellationToken ct = default);

    /// <summary>Subscribes to a HA event type and begins buffering events into the registry event channel.</summary>
    Task SubscribeToEventAsync(string eventType, CancellationToken ct = default);

    /// <summary>Unsubscribes from a previously subscribed event type.</summary>
    Task UnsubscribeFromEventAsync(string eventType, CancellationToken ct = default);

    /// <summary>Reads the next registry event from the incoming event channel. Returns null when closed.</summary>
    Task<HaRegistryEvent?> ReadNextEventAsync(CancellationToken ct = default);
}


