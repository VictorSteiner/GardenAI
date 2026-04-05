import { useEffect, useMemo, useRef, useState, type FormEvent, type KeyboardEvent } from 'react'
import type { HomeAssistant } from '../ha-types'
import { gardenPlannerStyles } from '../styles/gardenPlannerStyles'

interface PlannerHistoryMessage {
  role: 'user' | 'assistant'
  content: string
  timestamp: string
}

interface GardenPlannerChatProps {
  hass: HomeAssistant
  title?: string
}

const HISTORY_ENTITY_ID = 'sensor.garden_planner_history'
const REPLY_ENTITY_ID = 'sensor.garden_planner_reply'
const ACTIONS_ENTITY_ID = 'sensor.garden_planner_actions'

export default function GardenPlannerChat({ hass, title = 'Garden Planner' }: GardenPlannerChatProps) {
  const [input, setInput] = useState('')
  const [sending, setSending] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [pendingUserMessage, setPendingUserMessage] = useState<string | null>(null)
  const [pendingTargetCount, setPendingTargetCount] = useState<number | null>(null)
  const textareaRef = useRef<HTMLTextAreaElement>(null)
  const bottomRef = useRef<HTMLDivElement>(null)

  const historyEntity = hass.states[HISTORY_ENTITY_ID]
  const replyEntity = hass.states[REPLY_ENTITY_ID]
  const actionsEntity = hass.states[ACTIONS_ENTITY_ID]

  const backendMessages = useMemo<PlannerHistoryMessage[]>(() => {
    const raw = historyEntity?.attributes?.messages
    if (!Array.isArray(raw)) return []

    return raw
      .filter((msg): msg is PlannerHistoryMessage =>
        typeof msg === 'object' &&
        msg !== null &&
        typeof (msg as Record<string, unknown>).role === 'string' &&
        typeof (msg as Record<string, unknown>).content === 'string' &&
        typeof (msg as Record<string, unknown>).timestamp === 'string',
      )
      .map((msg) => ({
        role: msg.role === 'assistant' ? 'assistant' : 'user',
        content: msg.content,
        timestamp: msg.timestamp,
      }))
  }, [historyEntity])

  const latestActions = useMemo<string[]>(() => {
    const raw = replyEntity?.attributes?.actions_executed
    return Array.isArray(raw) ? raw.filter((a): a is string => typeof a === 'string') : []
  }, [replyEntity])

  const displayMessages = useMemo(() => {
    const mapped = backendMessages.map((msg, index) => ({
      id: `${msg.timestamp}-${index}`,
      role: msg.role,
      content: msg.content,
      timestamp: new Date(msg.timestamp),
      actionsExecuted:
        msg.role === 'assistant' && index === backendMessages.length - 1 && latestActions.length > 0
          ? latestActions
          : undefined,
    }))

    if (pendingUserMessage) {
      mapped.push({
        id: 'pending-user',
        role: 'user' as const,
        content: pendingUserMessage,
        timestamp: new Date(),
        actionsExecuted: undefined,
      })
    }

    return mapped
  }, [backendMessages, latestActions, pendingUserMessage])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [displayMessages, sending])

  useEffect(() => {
    if (!sending || pendingTargetCount === null) return
    if (backendMessages.length >= pendingTargetCount) {
      setSending(false)
      setPendingUserMessage(null)
      setPendingTargetCount(null)
      setError(null)
    }
  }, [backendMessages.length, pendingTargetCount, sending])

  const send = async () => {
    const message = input.trim()
    if (!message || sending) return

    setError(null)
    setInput('')
    setSending(true)
    setPendingUserMessage(message)
    setPendingTargetCount(backendMessages.length + 2)

    try {
      await hass.callService('rest_command', 'send_garden_planner_chat', { message })
      setTimeout(() => textareaRef.current?.focus(), 50)
    } catch (err) {
      setSending(false)
      setPendingUserMessage(null)
      setPendingTargetCount(null)
      setError(err instanceof Error ? err.message : 'Failed to send message.')
    }
  }

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    void send()
  }

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      void send()
    }
  }

  const isUnavailable = !historyEntity || !replyEntity || !actionsEntity

  return (
    <ha-card>
      <style>{gardenPlannerStyles}</style>
      <div className="gp-root">
        <div className="gp-header">
          <div className="gp-title-wrap">
            <span className="gp-title-icon">🌱</span>
            <span className="gp-title">{title}</span>
          </div>
          <span className="gp-status">{sending ? 'Thinking…' : 'Ready'}</span>
        </div>

        <div className="gp-messages chat-scroll">
          {isUnavailable ? (
            <div className="gp-empty">
              <div className="gp-empty-icon">⚠️</div>
              <div className="gp-empty-title">Planner entities unavailable</div>
              <div className="gp-empty-subtitle">
                Check sensor.garden_planner_history, sensor.garden_planner_reply, and sensor.garden_planner_actions.
              </div>
            </div>
          ) : displayMessages.length === 0 ? (
            <div className="gp-empty">
              <div className="gp-empty-icon">🪴</div>
              <div className="gp-empty-title">No conversation yet</div>
              <div className="gp-empty-subtitle">
                Try: <em>Plant basil in pot 1 today</em>
                <br />
                Or: <em>What is the moisture in pot 3?</em>
              </div>
            </div>
          ) : (
            displayMessages.map((message) => (
              <MessageBubble
                key={message.id}
                role={message.role}
                content={message.content}
                timestamp={message.timestamp}
                actionsExecuted={message.actionsExecuted}
              />
            ))
          )}

          {sending && <TypingIndicator />}
          <div ref={bottomRef} />
        </div>

        {error && <div className="gp-error">{error}</div>}

        {actionsEntity?.state && actionsEntity.state !== 'No actions' && !latestActions.length && (
          <div className="gp-actions-row">{actionsEntity.state}</div>
        )}

        <form className="gp-form" onSubmit={handleSubmit}>
          <textarea
            ref={textareaRef}
            className="gp-input"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            rows={1}
            placeholder='Ask the AI or say: "Plant tomatoes in pot 2 on the balcony"'
            disabled={sending || isUnavailable}
          />
          <button className="gp-send" type="submit" disabled={sending || !input.trim() || isUnavailable}>
            {sending ? '…' : '↑ Send'}
          </button>
        </form>
      </div>
    </ha-card>
  )
}

function MessageBubble(props: {
  role: 'user' | 'assistant'
  content: string
  timestamp: Date
  actionsExecuted?: string[]
}) {
  const isUser = props.role === 'user'

  return (
    <div className={`gp-message-row ${isUser ? 'gp-message-row-user' : 'gp-message-row-ai'}`}>
      <div className={`gp-bubble ${isUser ? 'gp-bubble-user' : 'gp-bubble-ai'}`}>
        {props.content}
      </div>

      {props.actionsExecuted && props.actionsExecuted.length > 0 && (
        <div className="gp-action-pill-wrap">
          {props.actionsExecuted.map((action, index) => (
            <span key={`${action}-${index}`} className="gp-action-pill">
              ✓ {action}
            </span>
          ))}
        </div>
      )}

      <div className="gp-time">
        {props.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
      </div>
    </div>
  )
}

function TypingIndicator() {
  return (
    <div className="gp-message-row gp-message-row-ai">
      <div className="gp-bubble gp-bubble-ai gp-typing">
        <span />
        <span />
        <span />
      </div>
    </div>
  )
}
