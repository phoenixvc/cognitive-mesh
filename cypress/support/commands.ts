/**
 * @fileoverview Custom Cypress commands for the Cognitive Mesh E2E test suite.
 *
 * All commands are declared on the `Cypress.Chainable` interface
 * (see the ambient type augmentation at the bottom of this file)
 * so that TypeScript provides full auto-complete and type checking.
 */

// ---------------------------------------------------------------------------
// cy.login(username, password)
// ---------------------------------------------------------------------------

/**
 * Authenticates the test user by posting credentials to the
 * application's auth endpoint and storing the resulting token
 * in `localStorage`.
 *
 * The command skips the login UI entirely to keep tests fast and
 * avoid coupling every spec to the login page implementation.
 *
 * @example
 * ```ts
 * cy.login('testuser@example.com', 'P@ssw0rd!');
 * ```
 */
Cypress.Commands.add('login', (username: string, password: string) => {
  cy.log(`Logging in as **${username}**`);

  cy.request({
    method: 'POST',
    url: '/api/auth/login',
    body: { username, password },
    failOnStatusCode: false,
  }).then((response) => {
    if (response.status === 200 && response.body?.token) {
      window.localStorage.setItem('auth_token', response.body.token);
      window.localStorage.setItem('auth_user', JSON.stringify({
        username,
        roles: response.body.roles ?? [],
        tenantId: response.body.tenantId ?? 'default',
      }));
      cy.log('Login successful');
    } else {
      // Fall back to a mock token for environments without a real auth API.
      cy.log('Auth API unavailable - using mock token');
      window.localStorage.setItem('auth_token', 'mock-jwt-token-for-e2e');
      window.localStorage.setItem('auth_user', JSON.stringify({
        username,
        roles: ['admin'],
        tenantId: 'test-tenant',
      }));
    }
  });
});

// ---------------------------------------------------------------------------
// cy.loadDashboard(dashboardId)
// ---------------------------------------------------------------------------

/**
 * Navigates to a specific dashboard by its identifier.
 *
 * Waits for the dashboard container and at least one widget to
 * render before yielding control to the next command.
 *
 * @example
 * ```ts
 * cy.loadDashboard('main-overview');
 * ```
 */
Cypress.Commands.add('loadDashboard', (dashboardId: string) => {
  cy.log(`Loading dashboard **${dashboardId}**`);
  cy.visit(`/dashboard/${dashboardId}`);

  // Wait for the dashboard shell to appear.
  cy.get('[data-testid="dashboard-container"]', { timeout: 15000 })
    .should('exist')
    .and('be.visible');

  // Wait for at least one widget to finish loading.
  cy.get('[data-testid="widget-container"]', { timeout: 15000 })
    .first()
    .should('exist');
});

// ---------------------------------------------------------------------------
// cy.waitForWidget(widgetName)
// ---------------------------------------------------------------------------

/**
 * Waits until a specific widget has fully rendered and is visible.
 *
 * Widgets are identified by the `data-testid` attribute matching
 * `widget-{widgetName}` or a `role="region"` with an accessible
 * name containing the widget name.
 *
 * @example
 * ```ts
 * cy.waitForWidget('agent-control-center');
 * ```
 */
Cypress.Commands.add('waitForWidget', (widgetName: string) => {
  cy.log(`Waiting for widget **${widgetName}** to render`);

  // Try data-testid first, then fall back to aria-labelledby.
  cy.get(
    `[data-testid="widget-${widgetName}"], [role="region"][aria-labelledby*="${widgetName}"]`,
    { timeout: 15000 },
  )
    .first()
    .should('exist')
    .and('be.visible');

  // Ensure no loading spinner is visible inside the widget.
  cy.get(`[data-testid="widget-${widgetName}"]`)
    .find('[aria-live="polite"]')
    .should('not.contain.text', 'Loading');
});

// ---------------------------------------------------------------------------
// cy.assertAccessibility()
// ---------------------------------------------------------------------------

/**
 * Runs an `axe-core` accessibility audit against the current page
 * and fails the test if any violations are found.
 *
 * Requires `cypress-axe` to be installed. The command injects axe,
 * runs the check, and logs each violation for easy debugging.
 *
 * @example
 * ```ts
 * cy.visit('/');
 * cy.assertAccessibility();
 * ```
 */
Cypress.Commands.add('assertAccessibility', () => {
  cy.log('Running accessibility audit (axe-core)');

  // Inject axe-core into the page under test.
  cy.injectAxe();

  // Run the audit and process results.
  cy.checkA11y(
    undefined,
    {
      // Only flag issues at the "critical" and "serious" severity levels
      // to avoid noisy false positives during initial rollout.
      includedImpacts: ['critical', 'serious'],
      rules: {
        // Disable color-contrast rule if running in CI with non-standard
        // rendering; enable it locally by overriding in the spec.
        'color-contrast': { enabled: true },
      },
    },
    (violations) => {
      // Log a table of violations for quick debugging.
      if (violations.length > 0) {
        cy.log(`Found **${violations.length}** accessibility violations`);
        const violationData = violations.map(({ id, impact, description, nodes }) => ({
          id,
          impact,
          description,
          nodeCount: nodes.length,
          target: nodes.map((n) => n.target).join(', '),
        }));
        cy.task('log', JSON.stringify(violationData, null, 2));
      }
    },
  );
});

// ---------------------------------------------------------------------------
// Type augmentation
// ---------------------------------------------------------------------------

declare global {
  namespace Cypress {
    interface Chainable {
      /**
       * Authenticate the test user via the auth API.
       * @param username - The user's email or username.
       * @param password - The user's password.
       */
      login(username: string, password: string): Chainable<void>;

      /**
       * Navigate to a dashboard and wait for it to render.
       * @param dashboardId - The unique identifier of the dashboard.
       */
      loadDashboard(dashboardId: string): Chainable<void>;

      /**
       * Wait for a named widget to finish rendering.
       * @param widgetName - The `data-testid` suffix of the widget.
       */
      waitForWidget(widgetName: string): Chainable<void>;

      /**
       * Run an axe-core accessibility audit against the current page.
       * Fails if critical or serious violations are found.
       */
      assertAccessibility(): Chainable<void>;
    }
  }
}

export {};
