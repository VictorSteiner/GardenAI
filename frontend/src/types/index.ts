export interface ChatMessage {
  id: string
  role: 'user' | 'assistant'
  content: string
  actionsExecuted?: string[]
  timestamp: Date
}

export interface GardenPlannerChatRequest {
  message: string
}

export interface GardenPlannerChatResponse {
  reply: string
  actionsExecuted: string[]
  generatedAtUtc: string
}

