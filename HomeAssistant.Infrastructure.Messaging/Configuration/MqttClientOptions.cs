namespace HomeAssistant.Infrastructure.Messaging.Configuration;

/// <summary>
/// Configuration options for the MQTT client connection and behavior.
/// Bound from appsettings.json "Mqtt" section.
/// </summary>
public sealed class MqttClientOptions
{
    /// <summary>MQTT broker hostname or IP address.</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>MQTT broker port (default 1883 for unencrypted).</summary>
    public int Port { get; set; } = 1883;

    /// <summary>MQTT client ID for identification on the broker.</summary>
    public string ClientId { get; set; } = "homeassistant-api";

    /// <summary>MQTT broker username (optional).</summary>
    public string Username { get; set; }

    /// <summary>MQTT broker password (optional; treated as a secret).</summary>
    public string Password { get; set; }

    /// <summary>MQTT keep-alive interval in seconds.</summary>
    public int KeepAliveSeconds { get; set; } = 60;

    /// <summary>Delay before attempting reconnection in seconds.</summary>
    public int ReconnectDelaySeconds { get; set; } = 5;

    /// <summary>Connection timeout in seconds.</summary>
    public int ConnectionTimeoutSeconds { get; set; } = 10;

    /// <summary>Enable mock sensor provider to publish readings to MQTT (development only).</summary>
    public bool PublishMockReadings { get; set; } = false;

    /// <summary>Polling interval in seconds for periodic mock MQTT publishing.</summary>
    public int MockPublishIntervalSeconds { get; set; } = 10;

    /// <summary>Topic prefix for Zigbee2MQTT messages (e.g., "zigbee2mqtt").</summary>
    public string Zigbee2MqttTopicPrefix { get; set; } = "zigbee2mqtt";

    /// <summary>Maps Zigbee device identifiers to PlantPot GUIDs.</summary>
    public Dictionary<string, string> SensorTopicMappings { get; set; } = [];
}

