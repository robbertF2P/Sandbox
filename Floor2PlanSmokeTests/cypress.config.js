const { defineConfig } = require('cypress');

const defaultTargetUrl = 'https://2025-14-patch.floor2plan.com/Account/Login';

module.exports = defineConfig({
  e2e: {
    specPattern: 'cypress/e2e/**/*.cy.js',
    supportFile: false,
    defaultCommandTimeout: 10000,
    pageLoadTimeout: 60000,
    retries: {
      runMode: 1,
      openMode: 0,
    },
    setupNodeEvents() {
      // Node event hooks can be added here without changing the spec contract.
    },
  },
  env: {
    targetUrls: process.env.TARGET_URLS || process.env.TARGET_URL || defaultTargetUrl,
    smokeEmail: process.env.SMOKE_EMAIL || 'smoke@example.com',
    visualSettleMs: Number(process.env.SMOKE_VISUAL_SETTLE_MS || 0),
  },
  screenshotsFolder: 'artifacts/screenshots',
  videosFolder: 'artifacts/videos',
  video: true,
  viewportWidth: Number(process.env.CYPRESS_VIEWPORT_WIDTH || 1280),
  viewportHeight: Number(process.env.CYPRESS_VIEWPORT_HEIGHT || 720),
});
