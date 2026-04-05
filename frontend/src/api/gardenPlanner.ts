import type { GardenPlannerChatRequest, GardenPlannerChatResponse } from '../types'

const BASE = '/api/garden/planner'

export async function sendChatMessage(
  request: GardenPlannerChatRequest,
  signal?: AbortSignal,
): Promise<GardenPlannerChatResponse> {
  const response = await fetch(`${BASE}/chat`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
    signal,
  })

  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText)
    throw new Error(`Request failed (${response.status}): ${text}`)
  }

  return response.json() as Promise<GardenPlannerChatResponse>
}

