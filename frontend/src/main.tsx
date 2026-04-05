import { createRoot, type Root } from 'react-dom/client'
import App from './App'
import type { HomeAssistant, LovelaceCardConfig } from './ha-types'

class GardenPlannerCard extends HTMLElement {
  private _root?: Root
  private _hass?: HomeAssistant
  private _config?: LovelaceCardConfig

  setConfig(config: LovelaceCardConfig) {
    this._config = config
    this.renderCard()
  }

  set hass(hass: HomeAssistant) {
    this._hass = hass
    this.renderCard()
  }

  connectedCallback() {
    if (!this._root) {
      this._root = createRoot(this)
    }
    this.renderCard()
  }

  disconnectedCallback() {
    // Keep root mounted; HA frequently detaches/reattaches cards during navigation.
  }

  getCardSize() {
    return 9
  }

  private renderCard() {
    if (!this._root || !this._hass) return

    this._root.render(
      <App
        hass={this._hass}
        title={typeof this._config?.title === 'string' ? this._config.title : 'Garden Planner'}
      />,
    )
  }
}

if (!customElements.get('garden-planner-card')) {
  customElements.define('garden-planner-card', GardenPlannerCard)
}

window.customCards = window.customCards || []
if (!window.customCards.some((card) => card.type === 'garden-planner-card')) {
  window.customCards.push({
    type: 'garden-planner-card',
    name: 'Garden Planner Card',
    description: 'Full-size garden planner chat card with Home Assistant-native service calls and message bubbles.',
    preview: true,
  })
}
