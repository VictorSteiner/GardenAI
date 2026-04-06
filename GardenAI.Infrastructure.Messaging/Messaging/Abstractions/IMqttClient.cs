namespace GardenAI.Infrastructure.Messaging.Messaging.Abstractions;

/// <summary>
/// Abstraction for MQTT client communication.
/// Provides a seam for testing and swapping implementations.
/// </summary>
public interface IMqttClient
{
    /// <summary>
    /// Establishes a connection to the MQTT broker.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task ConnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Closes the connection to the MQTT broker.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task DisconnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Publishes a message to an MQTT topic.
    /// </summary>
    /// <param name="topic">MQTT topic name.</param>
    /// <param name="payload">Message payload (string or JSON).</param>
    /// <param name="retainFlag">Whether the broker should retain the message.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync(string topic, string payload, bool retainFlag = false, CancellationToken ct = default);

    /// <summary>
    /// Subscribes to one or more MQTT topics.
    /// </summary>
    /// <param name="topics">Topic names or patterns (supports + and # wildcards).</param>
    /// <param name="ct">Cancellation token.</param>
    Task SubscribeAsync(IEnumerable<string> topics, CancellationToken ct = default);

    /// <summary>
    /// Unsubscribes from one or more MQTT topics.
    /// </summary>
    /// <param name="topics">Topic names to unsubscribe from.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UnsubscribeAsync(IEnumerable<string> topics, CancellationToken ct = default);

    /// <summary>
    /// Event raised when a message is received on a subscribed topic.
    /// </summary>
    event Func<string, string, Task> MessageReceivedAsync;
}

