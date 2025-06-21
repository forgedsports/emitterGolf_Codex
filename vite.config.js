import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  base: './',
  plugins: [react()],
  build: {
    outDir: 'dist',
  },
  define: {
    'import.meta.env.PUBLISH_DATETIME': JSON.stringify(new Date().toISOString()),
  },
});
