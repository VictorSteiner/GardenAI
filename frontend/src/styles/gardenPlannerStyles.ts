export const gardenPlannerStyles = `
  .gp-root {
    display: flex;
    flex-direction: column;
    min-height: 72vh;
    background: var(--ha-card-background, var(--card-background-color, #1c1c1c));
    color: var(--primary-text-color, #ffffff);
  }

  .gp-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 18px 12px;
    border-bottom: 1px solid var(--divider-color, rgba(255,255,255,0.12));
  }

  .gp-title-wrap {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .gp-title-icon { font-size: 20px; }
  .gp-title {
    font-size: 1rem;
    font-weight: 600;
    letter-spacing: 0.01em;
  }

  .gp-status {
    font-size: 0.75rem;
    color: var(--secondary-text-color, #9aa0a6);
  }

  .gp-messages {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
    display: flex;
    flex-direction: column;
    gap: 12px;
    background: var(--primary-background-color, #111315);
  }

  .gp-empty {
    margin: auto;
    text-align: center;
    color: var(--secondary-text-color, #9aa0a6);
    max-width: 420px;
    line-height: 1.6;
  }

  .gp-empty-icon { font-size: 2rem; margin-bottom: 10px; }
  .gp-empty-title { font-weight: 600; margin-bottom: 6px; color: var(--primary-text-color, #fff); }
  .gp-empty-subtitle { font-size: 0.88rem; }

  .gp-message-row {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .gp-message-row-user { align-items: flex-end; }
  .gp-message-row-ai { align-items: flex-start; }

  .gp-bubble {
    max-width: min(82%, 720px);
    padding: 10px 14px;
    border-radius: 18px;
    font-size: 0.95rem;
    line-height: 1.5;
    white-space: pre-wrap;
    word-break: break-word;
    box-sizing: border-box;
  }

  .gp-bubble-user {
    background: var(--primary-color, #1976d2);
    color: var(--text-primary-color, #fff);
    border-bottom-right-radius: 6px;
  }

  .gp-bubble-ai {
    background: color-mix(in srgb, var(--card-background-color, #2c2c2c) 85%, var(--primary-color, #1976d2) 15%);
    color: var(--primary-text-color, #fff);
    border: 1px solid var(--divider-color, rgba(255,255,255,0.12));
    border-bottom-left-radius: 6px;
  }

  .gp-time {
    font-size: 0.72rem;
    color: var(--secondary-text-color, #9aa0a6);
    padding: 0 4px;
  }

  .gp-action-pill-wrap {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    max-width: min(82%, 720px);
  }

  .gp-action-pill {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    padding: 4px 10px;
    border-radius: 999px;
    font-size: 0.75rem;
    background: color-mix(in srgb, var(--success-color, #43a047) 20%, transparent);
    color: var(--success-color, #81c784);
    border: 1px solid color-mix(in srgb, var(--success-color, #43a047) 35%, transparent);
  }

  .gp-error {
    margin: 0 16px 10px;
    padding: 10px 12px;
    border-radius: 12px;
    background: rgba(211, 47, 47, 0.18);
    color: #ffb4b4;
    border: 1px solid rgba(211, 47, 47, 0.35);
    font-size: 0.82rem;
  }

  .gp-actions-row {
    margin: 0 16px 10px;
    padding: 8px 12px;
    border-radius: 12px;
    background: rgba(67, 160, 71, 0.12);
    color: var(--success-color, #81c784);
    font-size: 0.8rem;
  }

  .gp-form {
    display: flex;
    gap: 10px;
    align-items: flex-end;
    padding: 14px 16px 16px;
    border-top: 1px solid var(--divider-color, rgba(255,255,255,0.12));
    background: var(--ha-card-background, var(--card-background-color, #1c1c1c));
  }

  .gp-input {
    flex: 1;
    min-height: 48px;
    max-height: 160px;
    resize: vertical;
    padding: 12px 14px;
    border-radius: 14px;
    border: 1px solid var(--divider-color, rgba(255,255,255,0.18));
    background: var(--primary-background-color, #111315);
    color: var(--primary-text-color, #fff);
    font: inherit;
    line-height: 1.45;
    box-sizing: border-box;
  }

  .gp-input::placeholder {
    color: var(--secondary-text-color, #9aa0a6);
  }

  .gp-input:focus {
    outline: none;
    border-color: var(--primary-color, #1976d2);
    box-shadow: 0 0 0 1px var(--primary-color, #1976d2);
  }

  .gp-send {
    min-width: 112px;
    height: 48px;
    border: none;
    border-radius: 14px;
    background: var(--primary-color, #1976d2);
    color: white;
    font: inherit;
    font-weight: 600;
    cursor: pointer;
    padding: 0 18px;
  }

  .gp-send:disabled,
  .gp-input:disabled {
    opacity: 0.55;
    cursor: not-allowed;
  }

  .gp-typing {
    display: inline-flex;
    align-items: center;
    gap: 6px;
  }

  .gp-typing span {
    width: 7px;
    height: 7px;
    border-radius: 999px;
    background: var(--secondary-text-color, #9aa0a6);
    animation: gp-bounce 1.2s infinite ease-in-out;
  }

  .gp-typing span:nth-child(2) { animation-delay: 0.15s; }
  .gp-typing span:nth-child(3) { animation-delay: 0.3s; }

  @keyframes gp-bounce {
    0%, 80%, 100% { transform: scale(0.7); opacity: 0.65; }
    40% { transform: scale(1); opacity: 1; }
  }

  .chat-scroll::-webkit-scrollbar { width: 6px; }
  .chat-scroll::-webkit-scrollbar-track { background: transparent; }
  .chat-scroll::-webkit-scrollbar-thumb {
    background: color-mix(in srgb, var(--secondary-text-color, #9aa0a6) 45%, transparent);
    border-radius: 999px;
  }
`

