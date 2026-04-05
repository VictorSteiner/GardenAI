import GardenPlannerChat from './components/GardenPlannerChat'
import type { HomeAssistant } from './ha-types'

interface AppProps {
  hass: HomeAssistant
  title?: string
}

export default function App({ hass, title }: AppProps) {
  return <GardenPlannerChat hass={hass} title={title} />
}
