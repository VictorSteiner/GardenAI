using System.Diagnostics.Metrics;

namespace GardenAI.Infrastructure.Messaging.Messaging.Metrics;

/// <summary>
/// Centralised metric instruments for the MQTT messaging layer.
/// Shared across all MQTT components to avoid duplicate <see cref="Meter"/> instances.
/// </summary>
internal static class MqttMetrics
{
    private static readonly Meter MeterInstance = new("GardenAI.Messaging");

    /// <summary>Number of messages successfully published to the broker.</summary>
    internal static readonly Counter<long> MessagesPublished =
        MeterInstance.CreateCounter<long>("mqtt.messages.published");

    /// <summary>Number of messages received from subscribed topics.</summary>
    internal static readonly Counter<long> MessagesReceived =
        MeterInstance.CreateCounter<long>("mqtt.messages.received");

    /// <summary>Number of unexpected disconnection events that triggered a reconnect attempt.</summary>
    internal static readonly Counter<long> ReconnectionAttempts =
        MeterInstance.CreateCounter<long>("mqtt.reconnection.attempts");

    /// <summary>Number of reconnect loops that eventually restored connectivity.</summary>
    internal static readonly Counter<long> ReconnectionSucceeded =
        MeterInstance.CreateCounter<long>("mqtt.reconnection.succeeded");

    /// <summary>Number of reconnect attempts that failed due to errors.</summary>
    internal static readonly Counter<long> ReconnectionFailed =
        MeterInstance.CreateCounter<long>("mqtt.reconnection.failed");

    /// <summary>Number of connection attempts that resulted in an exception.</summary>
    internal static readonly Counter<long> ConnectionFailures =
        MeterInstance.CreateCounter<long>("mqtt.connection.failures");
}
