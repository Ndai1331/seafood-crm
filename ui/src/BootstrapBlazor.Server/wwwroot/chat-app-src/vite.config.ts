import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => ({
  plugins: [react()],
  base: mode === 'standalone' ? '/' : '/chat-app/',
  build: {
    outDir: mode === 'standalone'
      ? '../../../../sidecar/public'       // → sidecar/public/
      : '../chat-app',                     // → wwwroot/chat-app/
    emptyOutDir: true,
  },
}))
