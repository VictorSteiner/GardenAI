# HomeAssistant.Infrastructure.Messaging

MQTT messaging adapter for HomeAssistant.

## What it provides

- `IMqttClient` abstraction
- `MqttClientService` implementation (MQTTnet)
- `MqttClientOptions` configuration bound from `Mqtt` section

## Local development flow

1. Start Mosquitto with Docker Compose.
2. Run API in Development (`MockSensorProvider` + periodic MQTT publishing).
3. Subscribe to `homeassistant/test/mock-sensors/#` to observe mock readings.

See `HomeAssistant.Presentation/HomeAssistant.Presentation.http` for quick verification commands.

