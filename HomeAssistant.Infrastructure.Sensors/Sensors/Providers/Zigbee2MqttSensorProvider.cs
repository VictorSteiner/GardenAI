using HomeAssistant.Domain.SensorReadings.Abstractions;
using HomeAssistant.Domain.SensorReadings.Entities;
using HomeAssistant.Infrastructure.Messaging.Configuration;
using HomeAssistant.Infrastructure.Messaging.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace HomeAssistant.Infrastructure.Sensors.Sensors.Providers;

/// <summary>
/// Production sensor provider that fetches soil moisture and temperature readings
/// from Zigbee2MQTT over MQTT.
/// Connects to the MQTT broker and subscribes to Zigbee device topics.
/// </summary>
public sealed class Zigbee2MqttSensorProvider : ISensorProvider, IAsyncDisposable
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _mqttOptions;
    private readonly ILogger<Zigbee2MqttSensorProvider> _logger;
    private readonly ConcurrentDictionary<Guid, SensorReading> _latestReadings = new();
    private bool _initialized;

    /// <summary>Initialises the Zigbee2MQTT sensor provider.</summary>
    /// <param name="mqttClient">MQTT client for broker communication.</param>
    /// <param name="mqttOptions">MQTT configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public Zigbee2MqttSensorProvider(
        IMqttClient mqttClient,
        MqttClientOptions mqttOptions,
        ILogger<Zigbee2MqttSensorProvider> logger)
    {
        _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
        _mqttOptions = mqttOptions ?? throw new ArgumentNullException(nameof(mqttOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to MQTT messages
        _mqttClient.MessageReceivedAsync += OnMqttMessageReceivedAsync;
    }

    /// <summary>Ensures the provider is initialized with subscriptions.</summary>
    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_initialized)
            return;

        try
        {
            // Build topic patterns for all configured Zigbee devices
            var topics = _mqttOptions.SensorTopicMappings.Keys
                .Select(deviceId => $"{_mqttOptions.Zigbee2MqttTopicPrefix}/{deviceId}/+/update")
                .Distinct()
                .ToList();

            if (topics.Count > 0)
            {
                await _mqttClient.SubscribeAsync(topics, ct);
                _logger.LogInformation("Subscribed to {TopicCount} Zigbee2MQTT topics.", topics.Count);
            }
            else
            {
                _logger.LogWarning("No sensor mappings configured in Mqtt:SensorTopicMappings.");
            }

            _initialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Zigbee2MQTT subscriptions.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SensorReading>> GetLatestReadingsAsync(CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct);
        var readings = _latestReadings.Values.ToList().AsReadOnly();
        _logger.LogDebug("Returning {ReadingCount} cached sensor readings.", readings.Count);
        return readings;
    }

    /// <summary>Handles incoming MQTT messages from Zigbee2MQTT.</summary>
    private Task OnMqttMessageReceivedAsync(string topic, string payload)
    {
        try
        {
            // Topic format: zigbee2mqtt/<device-id>/<property>/update
            var parts = topic.Split('/');
            if (parts.Length < 2)
            {
                _logger.LogWarning("Received malformed topic: {Topic}", topic);
                return Task.CompletedTask;
            }

            var deviceId = parts[1];

            // Look up the PlantPot GUID for this device
            if (!_mqttOptions.SensorTopicMappings.TryGetValue(deviceId, out var potIdStr) ||
                !Guid.TryParse(potIdStr, out var potId))
            {
                _logger.LogWarning("Unknown or invalid Zigbee device ID: {DeviceId}", deviceId);
                return Task.CompletedTask;
            }

            // Parse the JSON payload
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            // Extract soil moisture (assuming "humidity" property)
            double soilMoisture = 0;
            if (root.TryGetProperty("humidity", out var humidityProp) && humidityProp.TryGetDouble(out var humidity))
            {
                soilMoisture = humidity;
            }

            // Extract temperature (assuming "temperature" property)
            double temperatureC = 0;
            if (root.TryGetProperty("temperature", out var tempProp) && tempProp.TryGetDouble(out var temp))
            {
                temperatureC = temp;
            }

            // Create and cache the reading
            var reading = new SensorReading
            {
                Id = Guid.NewGuid(),
                PotId = potId,
                Timestamp = DateTimeOffset.UtcNow,
                SoilMoisture = soilMoisture,
                TemperatureC = temperatureC,
            };

            _latestReadings[potId] = reading;

            _logger.LogDebug(
                "Processed Zigbee2MQTT reading for pot {PotId}: moisture={Moisture}%, temp={Temp}°C",
                potId, soilMoisture, temperatureC);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Zigbee2MQTT JSON payload for topic {Topic}: {Payload}",
                topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message on topic {Topic}.", topic);
        }

        return Task.CompletedTask;
    }

    /// <summary>Releases resources.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_mqttClient is not null)
        {
            _mqttClient.MessageReceivedAsync -= OnMqttMessageReceivedAsync;
        }
        await ValueTask.CompletedTask;
    }
}

