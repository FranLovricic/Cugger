// @ts-check
const { defineConfig } = require('@playwright/test');

/**
 * Playwright konfiguracija za Cugger.
 * Testovi gađaju http://localhost:5017 — ako aplikacija već ne radi,
 * webServer je automatski pokreće (dotnet run).
 */
module.exports = defineConfig({
  testDir: './tests',
  timeout: 60_000,
  fullyParallel: false,
  workers: 1,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: 'http://localhost:5017',
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure',
    ignoreHTTPSErrors: true,
    locale: 'hr-HR',
  },
  webServer: {
    command: 'dotnet run --project ../Cugger.csproj --launch-profile http',
    url: 'http://localhost:5017',
    reuseExistingServer: true,
    timeout: 120_000,
  },
  projects: [
    { name: 'chromium', use: { browserName: 'chromium' } },
  ],
});
