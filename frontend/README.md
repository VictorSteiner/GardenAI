# Garden Planner Home Assistant Card

This frontend builds a Home Assistant custom Lovelace card named `custom:garden-planner-card`.

## What it does

- Renders a real chat UI with separate user / assistant bubbles
- Calls the Home Assistant service `rest_command.send_garden_planner_chat`
- Reads live planner history from:
  - `sensor.garden_planner_history`
  - `sensor.garden_planner_reply`
  - `sensor.garden_planner_actions`

## Build output

The build writes the card module to:

- `../homeassistant-config/www/garden-planner-card/garden-planner-card.js`

Home Assistant serves that file as:

- `/local/garden-planner-card/garden-planner-card.js`

## Development

```powershell
cd frontend
npm install
npm run build
```

Then reload the Lovelace dashboard resources in Home Assistant.

