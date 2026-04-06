namespace GardenAI.Application.Common.Sync.Configuration;

/// <summary>Configuration options for the Home Assistant connection.</summary>
public sealed class HomeAssistantOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "HomeAssistant";

    /// <summary>Base URL of the Home Assistant instance (e.g. http://homeassistant.local:8123).</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Long-lived access token for authenticating with the HA WebSocket and REST API.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Interval in seconds between WebSocket keepalive ping messages.</summary>
    public int PingIntervalSeconds { get; set; } = 30;

    /// <summary>Maximum number of reconnect attempts before giving up.</summary>
    public int ReconnectMaxRetries { get; set; } = 10;
}


