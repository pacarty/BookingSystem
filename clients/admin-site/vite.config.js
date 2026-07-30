import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Fixed at 5174 (not Vite's default 5173) so it doesn't collide with the
// public-site dev server, and so it matches Clients:AdminSiteUrl in the
// API's appsettings.json for CORS.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5174,
  },
})
