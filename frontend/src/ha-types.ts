export interface HassEntityAttributeBase {
  [key: string]: unknown
}

export interface HassEntity {
  entity_id: string
  state: string
  attributes: HassEntityAttributeBase
  last_changed: string
  last_updated: string
}

export interface HomeAssistant {
  states: Record<string, HassEntity>
  callService(domain: string, service: string, serviceData?: Record<string, unknown>): Promise<void>
}

export interface LovelaceCardConfig {
  type: string
  title?: string
  [key: string]: unknown
}

declare global {
  interface Window {
    customCards?: Array<{
      type: string
      name: string
      description: string
      preview?: boolean
      documentationURL?: string
    }>
  }
}

