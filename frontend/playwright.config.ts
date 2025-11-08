import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : 2,
  reporter: 'html',

  // Global timeouts
  timeout: 60000, // 60s per test
  expect: {
    timeout: 10000 // 10s for assertions
  },

  use: {
    baseURL: 'http://localhost:3000/portalforge/fe',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    actionTimeout: 15000, // 15s for actions (click, fill, etc)
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  // Auto-start dev server before tests
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:3000/portalforge/fe',
    reuseExistingServer: !process.env.CI,
    timeout: 120000, // 2 min for server startup
  },
})
