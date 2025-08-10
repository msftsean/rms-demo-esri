import { defineConfig } from 'vite'

export default defineConfig({
  server: {
    port: 5173
  },
  optimizeDeps: {
    exclude: ['@arcgis/core']
  },
  build: {
    target: 'es2020'
  }
})
