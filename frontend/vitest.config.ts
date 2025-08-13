import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',
    include: ['src/__tests__/**/*.test.ts?(x)'],
    globals: true,
    setupFiles: ['./src/__tests__/setup.ts'],
  },
});
