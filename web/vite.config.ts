import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// In development the Vite server forwards /api and /health to the .NET API,
// so the browser sees one origin and no CORS setup is needed.
const apiTarget = 'http://localhost:5080'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': apiTarget,
      '/health': apiTarget,
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    css: false,
  },
})
