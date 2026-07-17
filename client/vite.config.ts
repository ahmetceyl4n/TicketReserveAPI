import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'node:path'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'),
    },
  },
  server: {
    // Mock modda bu proxy hiç devreye girmez; backend hazır olduğunda
    // VITE_USE_MOCKS=false yapmak yeterli, kodda değişiklik gerekmez.
    proxy: {
      '/api': {
        target: 'https://localhost:7233',
        changeOrigin: true,
        secure: false,
      },
      '/hubs': {
        target: 'https://localhost:7233',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
})
