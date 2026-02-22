/**
 * @fileoverview Cypress E2E support file for Cognitive Mesh.
 *
 * This file runs **before** every E2E spec file. It is the single
 * entry point for importing custom commands, configuring global
 * hooks, and injecting third-party Cypress plugins.
 */

// ---------------------------------------------------------------------------
// Custom commands
// ---------------------------------------------------------------------------

import './commands';

// ---------------------------------------------------------------------------
// Third-party plugins
// ---------------------------------------------------------------------------

// cypress-axe provides `cy.injectAxe()` and `cy.checkA11y()`.
import 'cypress-axe';

// ---------------------------------------------------------------------------
// Global hooks
// ---------------------------------------------------------------------------

/**
 * Runs once before each test.
 * Clears application state so tests start from a clean baseline.
 */
beforeEach(() => {
  // Clear all cookies, localStorage, and sessionStorage.
  cy.clearCookies();
  cy.clearLocalStorage();
  cy.window().then((win) => {
    win.sessionStorage.clear();
  });
});

/**
 * Suppress uncaught exceptions that originate from the application
 * under test. Some third-party scripts (analytics, hot-reload)
 * throw benign errors that would otherwise fail every spec.
 *
 * Override on a per-test basis when you _want_ to assert that
 * the application does not throw:
 *
 * ```ts
 * Cypress.on('uncaught:exception', () => { throw err; });
 * ```
 */
Cypress.on('uncaught:exception', (err) => {
  // Ignore ResizeObserver errors which are common and benign.
  if (err.message.includes('ResizeObserver loop')) {
    return false;
  }

  // Ignore chunk-loading errors caused by stale service workers
  // during hot-reload development sessions.
  if (err.message.includes('Loading chunk') || err.message.includes('ChunkLoadError')) {
    return false;
  }

  // Let all other errors fail the test.
  return true;
});

// ---------------------------------------------------------------------------
// Custom task registration for logging
// ---------------------------------------------------------------------------

/**
 * Register a `log` task so that `cy.task('log', message)` prints
 * to the Node process stdout. Useful for debugging accessibility
 * violations and other structured data.
 *
 * Note: Task registration happens in `cypress.config.ts` via
 * `setupNodeEvents`. The command is called from `commands.ts`.
 */
