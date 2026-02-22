/**
 * @fileoverview Cypress configuration for the Cognitive Mesh E2E test suite.
 *
 * Configures both end-to-end and component testing, including retry
 * strategies, viewport defaults, and custom support file paths.
 */

import { defineConfig } from 'cypress';

export default defineConfig({
  e2e: {
    /** Base URL of the ASP.NET Core + React dev server. */
    baseUrl: 'http://localhost:5000',

    /** Path to the E2E support file that runs before every spec. */
    supportFile: 'cypress/support/e2e.ts',

    /** Glob pattern for locating E2E spec files. */
    specPattern: 'cypress/e2e/**/*.cy.ts',

    /** Default viewport width in pixels. */
    viewportWidth: 1280,

    /** Default viewport height in pixels. */
    viewportHeight: 720,

    /** Disable video recording to speed up CI runs. */
    video: false,

    /** Capture a screenshot when a test fails during `cypress run`. */
    screenshotOnRunFailure: true,

    /** Retry configuration: retries in headless (run) mode, none in open mode. */
    retries: {
      runMode: 2,
      openMode: 0,
    },

    /** Default command timeout in milliseconds. */
    defaultCommandTimeout: 10000,

    /** Timeout for page-load transitions. */
    pageLoadTimeout: 30000,

    /** Timeout for `cy.request()` network calls. */
    responseTimeout: 15000,

    /**
     * Hook that runs once before all specs.
     * Can be extended with database seeding, auth token generation, etc.
     */
    setupNodeEvents(on, config) {
      // Register any plugins here (e.g. code coverage, visual regression).
      return config;
    },
  },

  component: {
    /** Dev server configuration for component testing. */
    devServer: {
      framework: 'react',
      bundler: 'webpack',
    },
  },
});
