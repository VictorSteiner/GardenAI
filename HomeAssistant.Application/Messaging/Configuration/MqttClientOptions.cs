namespace HomeAssistant.Application.Messaging.Configuration;

/// <summary>
/// Configuration options for MQTT connection and behavior.
/// </summary>
public sealed class MqttClientOptions
{
    /// <summary>MQTT broker hostname or IP address.</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>MQTT broker port.</summary>
    public int Port { get; set; } = 1883;

    /// <summary>MQTT client identifier.</summary>
    public string ClientId { get; set; } = "homeassistant-api";

    /// <summary>MQTT broker username, if required.</summary>
    public string? Username { get; set; }

    /// <summary>MQTT broker password, if required.</summary>
    public string? Password { get; set; }

    /// <summary>Keep-alive interval in seconds.</summary>
    public int KeepAliveSeconds { get; set; } = 60;

    /// <summary>Reconnection delay in seconds.</summary>
    public int ReconnectDelaySeconds { get; set; } = 5;

    /// <summary>Connection timeout in seconds.</summary>
    public int ConnectionTimeoutSeconds { get; set; } = 10;

    /// <summary>Whether mock readings should also be published to MQTT in development.</summary>
    public bool PublishMockReadings { get; set; }

    /// <summary>Polling interval in seconds for periodic mock MQTT publishing.</summary>
    public int MockPublishIntervalSeconds { get; set; } = 10;

    /// <summary>Topic prefix for Zigbee2MQTT messages.</summary>
    public string Zigbee2MqttTopicPrefix { get; set; } = "zigbee2mqtt";

    /// <summary>Map of Zigbee device IDs to pot IDs.</summary>
    public Dictionary<string, string> SensorTopicMappings { get; set; } = [];
}

