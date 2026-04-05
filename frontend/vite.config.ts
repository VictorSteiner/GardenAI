import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  define: {
    'process.env.NODE_ENV': '"production"',
  },
  build: {
    outDir: '../homeassistant-config/www/garden-planner-card',
    emptyOutDir: true,
    cssCodeSplit: false,
    lib: {
      entry: './src/main.tsx',
      formats: ['es'],
      fileName: () => 'garden-planner-card.js',
    },
  },
})
