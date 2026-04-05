# HomeAssistant Garden Add-on

Standalone garden automation add-on designed to run next to Home Assistant (typically on a Raspberry Pi). It consumes sensor data via MQTT, stores data in PostgreSQL, and offers a chat and automation API powered by Ollama.

## How the data flow works

- Zigbee sensors publish to Zigbee2MQTT topics on MQTT.
- Home Assistant can subscribe to those MQTT topics.
- This add-on also subscribes to the same broker/topics.
- In development, this add-on can generate and publish mock sensor readings itself.

Important: the add-on does not need Home Assistant to forward data. Both can read from the same MQTT broker.

## Prerequisites

- Docker + Docker Compose
- .NET SDK 10
- Optional: Home Assistant instance (for end-to-end testing with the HA UI)

## 1) Start infrastructure services (PostgreSQL, Mosquitto, Ollama)

```powershell
Copy-Item .env.example .env
docker compose up -d postgres mosquitto ollama
```

Optional model pull (first run):

```powershell
docker compose --profile init run --rm ollama-pull
```

## 2) Apply database migrations

```powershell
dotnet ef database update --project HomeAssistant.Infrastructure.Persistence --startup-project HomeAssistant.Presentation
```

## 3) Run the add-on API locally

```powershell
dotnet run --project HomeAssistant.Presentation --launch-profile http
```

API URLs (Development):

- OpenAPI JSON: `http://localhost:5064/openapi/v1.json`
- Scalar UI: `http://localhost:5064/scalar/v1`

## 4) Mock sensor data in local development

### Option A: built-in mock publisher (recommended)

`HomeAssistant.Presentation/appsettings.Development.json` already enables:

- `Mqtt.PublishMockReadings = true`
- `Mqtt.MockPublishIntervalSeconds = 10`

When the API runs in `Development`, `MockSensorProvider` publishes 6 pot readings to:

- `homeassistant/test/mock-sensors/<pot-guid>`

Watch messages:

```powershell
docker exec -it ha-mosquitto mosquitto_sub -h localhost -t "homeassistant/test/mock-sensors/#" -v
```

### Option B: manually publish Zigbee-like messages

This simulates real Zigbee2MQTT payload shape for mappings in `Mqtt.SensorTopicMappings`.

```powershell
docker exec -it ha-mosquitto mosquitto_pub -h localhost -t "zigbee2mqtt/pot-1/soil/update" -m '{"humidity":42.5,"temperature":21.4}'
```

## 5) Connect Home Assistant and the add-on to the same broker

In Home Assistant:

1. Add MQTT integration pointed to the same broker (`localhost:1883` or container host).
2. Create MQTT entities for relevant topics (either Zigbee2MQTT topics or mock topics).

Example `configuration.yaml` sensor snippet:

```yaml
mqtt:
  sensor:
    - name: "Pot 1 Humidity"
      state_topic: "zigbee2mqtt/pot-1/soil/update"
      unit_of_measurement: "%"
      value_template: "{{ value_json.humidity }}"

    - name: "Pot 1 Temperature"
      state_topic: "zigbee2mqtt/pot-1/soil/update"
      unit_of_measurement: "degC"
      value_template: "{{ value_json.temperature }}"
```

## Typical development workflow

```powershell
docker compose up -d postgres mosquitto ollama
dotnet ef database update --project HomeAssistant.Infrastructure.Persistence --startup-project HomeAssistant.Presentation
dotnet run --project HomeAssistant.Presentation --launch-profile http
```

Then use `HomeAssistant.Presentation/HomeAssistant.Presentation.http` for quick endpoint checks.

## Raspberry Pi deployment notes

- Keep MQTT, PostgreSQL, and Ollama in the same Docker network for low-latency local access.
- Use environment variables for secrets (`POSTGRES_PASSWORD`, MQTT credentials).
- Prefer production settings where the real `Zigbee2MqttSensorProvider` is used instead of the development mock provider.

## Troubleshooting

- If API startup logs MQTT connection errors: verify Mosquitto is running and host/port in `Mqtt` settings are correct.
- If DB connection fails: verify `.env` values and `ConnectionStrings:DefaultConnection`.
- If chat fails: ensure Ollama container is healthy and the model is pulled.

