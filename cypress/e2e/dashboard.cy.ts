/**
 * @fileoverview E2E tests for the Cognitive Mesh dashboard.
 *
 * Covers:
 * - Dashboard layout loading
 * - Widget grid rendering
 * - Widget interactions (click, expand, collapse)
 * - Error state handling
 * - Responsive layout at different viewports
 */

describe('Dashboard', () => {
  beforeEach(() => {
    cy.login('testuser@example.com', 'P@ssw0rd!');
  });

  // -----------------------------------------------------------------------
  // Layout loading
  // -----------------------------------------------------------------------

  describe('Layout Loading', () => {
    it('should load the default dashboard layout', () => {
      cy.loadDashboard('main-overview');
      cy.get('[data-testid="dashboard-container"]').should('be.visible');
    });

    it('should display a loading indicator while the dashboard loads', () => {
      cy.visit('/dashboard/main-overview');
      // The loading indicator should appear briefly.
      cy.get('[aria-live="polite"]').should('exist');
      // Then the dashboard container should render.
      cy.get('[data-testid="dashboard-container"]', { timeout: 15000 })
        .should('be.visible');
    });

    it('should display the dashboard title', () => {
      cy.loadDashboard('main-overview');
      cy.get('h2, h1').first().should('not.be.empty');
    });
  });

  // -----------------------------------------------------------------------
  // Widget grid rendering
  // -----------------------------------------------------------------------

  describe('Widget Grid', () => {
    beforeEach(() => {
      cy.loadDashboard('main-overview');
    });

    it('should render at least one widget in the grid', () => {
      cy.get('[data-testid="widget-container"], [role="region"]')
        .should('have.length.greaterThan', 0);
    });

    it('should render widgets with identifiable headers', () => {
      cy.get('[role="region"]').each(($region) => {
        // Each region should have a heading or labelledby attribute.
        const labelledBy = $region.attr('aria-labelledby');
        if (labelledBy) {
          cy.get(`#${labelledBy}`).should('exist').and('not.be.empty');
        }
      });
    });

    it('should display widget data once loading completes', () => {
      // Widgets should not permanently show "Loading".
      cy.get('[role="region"]', { timeout: 15000 }).first().within(() => {
        cy.get('[aria-live="polite"]')
          .should('not.contain.text', 'Loading');
      });
    });
  });

  // -----------------------------------------------------------------------
  // Widget interactions
  // -----------------------------------------------------------------------

  describe('Widget Interactions', () => {
    beforeEach(() => {
      cy.loadDashboard('main-overview');
    });

    it('should expand a widget when clicked', () => {
      cy.get('[role="region"]').first().as('widget');
      cy.get('@widget').click();
      // After clicking, expect some expanded content or modal.
      cy.get('[role="dialog"], [data-testid="widget-expanded"]')
        .should('exist');
    });

    it('should collapse a widget when the close button is clicked', () => {
      cy.get('[role="region"]').first().click();
      // Close the expanded view.
      cy.get('[aria-label="Close modal"], [aria-label="Close agent details"], button')
        .filter(':contains("×")')
        .first()
        .click();
      cy.get('[role="dialog"]').should('not.exist');
    });

    it('should support keyboard navigation between widgets', () => {
      cy.get('[role="region"]').first().focus();
      cy.focused().should('have.attr', 'role', 'region');
      cy.focused().type('{tab}');
      // The next focusable element should receive focus.
      cy.focused().should('exist');
    });

    it('should handle table row clicks to show agent details', () => {
      // Navigate to the agent control center.
      cy.get('table[aria-label]').first().within(() => {
        cy.get('tbody tr').first().click();
      });
      // A details modal or expanded section should appear.
      cy.get('[role="dialog"]', { timeout: 5000 }).should('be.visible');
    });
  });

  // -----------------------------------------------------------------------
  // Error state handling
  // -----------------------------------------------------------------------

  describe('Error States', () => {
    it('should display an error message when the API fails', () => {
      // Intercept the API call and force a failure.
      cy.intercept('GET', '/api/**', {
        statusCode: 500,
        body: { errorCode: 'SERVER_ERROR', message: 'Internal Server Error' },
      }).as('apiFailure');

      cy.visit('/dashboard/main-overview');
      cy.get('[role="alert"]', { timeout: 15000 }).should('be.visible');
    });

    it('should show a retry button when the error is retryable', () => {
      cy.intercept('GET', '/api/**', {
        statusCode: 503,
        body: { errorCode: 'SERVICE_UNAVAILABLE', message: 'Service Unavailable', canRetry: true },
      }).as('apiFailure');

      cy.visit('/dashboard/main-overview');
      cy.get('[role="alert"]', { timeout: 15000 }).within(() => {
        cy.contains('button', /retry/i).should('be.visible');
      });
    });

    it('should reload data when the retry button is clicked', () => {
      let callCount = 0;
      cy.intercept('GET', '/api/**', (req) => {
        callCount += 1;
        if (callCount <= 1) {
          req.reply({ statusCode: 500, body: { errorCode: 'FETCH_ERROR', message: 'Error' } });
        } else {
          req.reply({ statusCode: 200, body: { data: [] } });
        }
      }).as('apiCall');

      cy.visit('/dashboard/main-overview');
      cy.get('[role="alert"]', { timeout: 15000 }).within(() => {
        cy.contains('button', /retry/i).click();
      });
      // After retry, the error should be gone.
      cy.get('[role="alert"]').should('not.exist');
    });
  });

  // -----------------------------------------------------------------------
  // Responsive layout
  // -----------------------------------------------------------------------

  describe('Responsive Layout', () => {
    const viewports: Array<{ name: string; width: number; height: number }> = [
      { name: 'Desktop (1280x720)', width: 1280, height: 720 },
      { name: 'Tablet (768x1024)', width: 768, height: 1024 },
      { name: 'Mobile (375x667)', width: 375, height: 667 },
    ];

    viewports.forEach(({ name, width, height }) => {
      it(`should render correctly at ${name}`, () => {
        cy.viewport(width, height);
        cy.loadDashboard('main-overview');
        cy.get('[data-testid="dashboard-container"], [role="region"]')
          .should('be.visible');

        // Ensure no horizontal overflow.
        cy.window().then((win) => {
          const bodyWidth = win.document.body.scrollWidth;
          expect(bodyWidth).to.be.at.most(width + 20); // small tolerance for scrollbar
        });
      });
    });

    it('should stack widgets vertically on mobile viewports', () => {
      cy.viewport(375, 667);
      cy.loadDashboard('main-overview');
      cy.get('[role="region"]').then(($widgets) => {
        if ($widgets.length > 1) {
          const firstTop = $widgets.eq(0).position().top;
          const secondTop = $widgets.eq(1).position().top;
          // Second widget should be below the first (stacked).
          expect(secondTop).to.be.greaterThan(firstTop);
        }
      });
    });
  });
});
