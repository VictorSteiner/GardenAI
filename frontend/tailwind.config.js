/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        'ha-bg':      '#111827',
        'ha-surface': '#1f2937',
        'ha-border':  '#374151',
        'ha-text':    '#f9fafb',
        'ha-muted':   '#9ca3af',
        'user-bubble':'#1d4ed8',
        'ai-bubble':  '#1e3a5f',
      },
    },
  },
  plugins: [],
}

