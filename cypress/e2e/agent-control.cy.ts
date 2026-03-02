/**
 * @fileoverview E2E tests for the Cognitive Mesh agent control features.
 *
 * Covers:
 * - Agent status banner rendering
 * - Authority consent modal flow
 * - Agent action audit trail display
 * - Agent registration notification
 */

describe('Agent Control', () => {
  beforeEach(() => {
    cy.login('testuser@example.com', 'P@ssw0rd!');
  });

  // -----------------------------------------------------------------------
  // Agent Status Banner
  // -----------------------------------------------------------------------

  describe('Agent Status Banner', () => {
    it('should render the agent status banner at the top of the page', () => {
      cy.visit('/');
      cy.get('[role="status"]', { timeout: 10000 })
        .should('be.visible');
    });

    it('should display the current agent status text', () => {
      cy.visit('/');
      cy.get('[role="status"]', { timeout: 10000 }).within(() => {
        // The banner should contain status text like "Agent Idle", "Agent Executing", etc.
        cy.get('span').should('exist').and('not.be.empty');
      });
    });

    it('should display action buttons appropriate to the agent status', () => {
      cy.visit('/');
      cy.get('[role="status"]', { timeout: 10000 }).within(() => {
        // At minimum, the "Control Center" button should be present.
        cy.contains('button', /control center/i).should('be.visible');
      });
    });

    it('should show a retry button when the agent status fetch fails', () => {
      cy.intercept('GET', '/api/agents/*/status', {
        statusCode: 500,
        body: { errorCode: 'API_ERROR', message: 'Failed to fetch agent status', canRetry: true },
      }).as('statusFetch');

      cy.visit('/');
      cy.get('[role="status"]', { timeout: 10000 }).within(() => {
        cy.contains('button', /retry/i).should('be.visible');
      });
    });

    it('should update the banner color based on agent status', () => {
      cy.visit('/');
      cy.get('[role="status"]', { timeout: 10000 }).then(($banner) => {
        const bgColor = $banner.css('background-color');
        // The banner should have a non-default background color.
        expect(bgColor).to.not.equal('rgba(0, 0, 0, 0)');
        expect(bgColor).to.not.equal('transparent');
      });
    });

    it('should show the escalate button when agent is in error state', () => {
      cy.intercept('GET', '/api/agents/*/status', {
        statusCode: 200,
        body: { data: 'error', isStale: false, lastSyncTimestamp: new Date().toISOString(), lastError: null },
      }).as('statusFetch');

      cy.visit('/');
      cy.get('[role="status"]', { timeout: 10000 }).within(() => {
        cy.contains('button', /escalate/i).should('be.visible');
      });
    });
  });

  // -----------------------------------------------------------------------
  // Authority Consent Modal
  // -----------------------------------------------------------------------

  describe('Authority Consent Modal', () => {
    /**
     * Helper to trigger the consent modal. In a real test this might
     * navigate to an agent action that triggers the modal; here we
     * simulate by visiting a route that shows the modal or by
     * interacting with the agent control center.
     */
    function openConsentModal(): void {
      cy.visit('/agents/agent-001/actions');
      cy.get('[role="dialog"]', { timeout: 10000 }).should('be.visible');
    }

    it('should display the consent modal with action details', () => {
      openConsentModal();
      cy.get('[role="dialog"]').within(() => {
        cy.get('h2').should('contain.text', 'Approval');
        cy.contains('Action Details').should('be.visible');
        cy.contains('Risk Level').should('be.visible');
      });
    });

    it('should display Approve and Deny buttons', () => {
      openConsentModal();
      cy.get('[role="dialog"]').within(() => {
        cy.contains('button', /approve/i).should('be.visible');
        cy.contains('button', /deny/i).should('be.visible');
      });
    });

    it('should close the modal when the close button is clicked', () => {
      openConsentModal();
      cy.get('[aria-label="Close modal"]').click();
      cy.get('[role="dialog"]').should('not.exist');
    });

    it('should close the modal when Escape is pressed', () => {
      openConsentModal();
      cy.get('[role="dialog"]').type('{esc}');
      cy.get('[role="dialog"]').should('not.exist');
    });

    it('should submit an approval decision and close the modal', () => {
      cy.intercept('POST', '/api/consent/**', {
        statusCode: 200,
        body: true,
      }).as('consentSubmission');

      openConsentModal();
      cy.get('[role="dialog"]').within(() => {
        cy.contains('button', /approve/i).click();
      });
      // Modal should close after successful submission.
      cy.get('[role="dialog"]').should('not.exist');
    });

    it('should submit a deny decision and close the modal', () => {
      cy.intercept('POST', '/api/consent/**', {
        statusCode: 200,
        body: true,
      }).as('consentSubmission');

      openConsentModal();
      cy.get('[role="dialog"]').within(() => {
        cy.contains('button', /deny/i).click();
      });
      cy.get('[role="dialog"]').should('not.exist');
    });

    it('should show an error when consent submission fails', () => {
      cy.intercept('POST', '/api/consent/**', {
        statusCode: 500,
        body: false,
      }).as('consentSubmission');

      openConsentModal();
      cy.get('[role="dialog"]').within(() => {
        cy.contains('button', /approve/i).click();
        cy.get('[role="alert"]', { timeout: 5000 }).should('be.visible');
      });
    });

    it('should show a confirmation step for high-risk actions', () => {
      // Navigate to a high-risk action that triggers a two-step confirmation.
      cy.visit('/agents/agent-001/actions?risk=High');
      cy.get('[role="dialog"]', { timeout: 10000 }).within(() => {
        cy.contains('button', /approve/i).click();
        cy.contains('Confirm').should('be.visible');
        cy.contains('button', /confirm approval/i).should('be.visible');
      });
    });

    it('should display the timeout countdown for time-sensitive actions', () => {
      openConsentModal();
      // The countdown should appear when time is running low.
      cy.get('[role="dialog"]').within(() => {
        cy.get('[aria-live="polite"]').should('exist');
      });
    });
  });

  // -----------------------------------------------------------------------
  // Agent Action Audit Trail
  // -----------------------------------------------------------------------

  describe('Agent Action Audit Trail', () => {
    beforeEach(() => {
      cy.visit('/agents/audit-trail');
    });

    it('should render the audit trail panel', () => {
      cy.get('[role="region"][aria-labelledby*="audit"]', { timeout: 10000 })
        .should('be.visible');
    });

    it('should display the audit trail title', () => {
      cy.contains('h2', /audit trail/i).should('be.visible');
    });

    it('should render filter controls', () => {
      cy.get('[aria-label="Filter by event type"]').should('be.visible');
      cy.get('[aria-label="Filter by outcome"]').should('be.visible');
      cy.get('[aria-label="Filter by correlation ID"]').should('be.visible');
    });

    it('should filter audit events by outcome', () => {
      cy.get('[aria-label="Filter by outcome"]').select('Success');
      // All visible outcome badges should say "Success".
      cy.get('[role="log"]').within(() => {
        cy.get('span').filter(':contains("Success")').should('have.length.greaterThan', 0);
      });
    });

    it('should expand an audit event group to show details', () => {
      cy.get('[role="button"][aria-expanded]', { timeout: 10000 })
        .first()
        .click();
      cy.get('[role="button"][aria-expanded="true"]').should('exist');
    });

    it('should paginate audit events', () => {
      cy.contains('button', /next/i).should('exist');
      cy.contains('button', /previous/i).should('exist');
      cy.get('[aria-live="polite"]').contains(/page/i).should('exist');
    });

    it('should update the page when pagination buttons are clicked', () => {
      cy.get('[aria-live="polite"]').contains(/page 1/i).should('exist');
      cy.contains('button', /next/i).click();
      cy.get('[aria-live="polite"]').contains(/page 2/i).should('exist');
    });
  });

  // -----------------------------------------------------------------------
  // Agent Registration Notification
  // -----------------------------------------------------------------------

  describe('Agent Registration Notification', () => {
    it('should display a notification when a new agent is registered', () => {
      cy.visit('/');

      // Simulate a notification via the app's notification system.
      cy.window().then((win) => {
        const event = new CustomEvent('agent-notification', {
          detail: {
            notificationId: 'test-notification-001',
            title: 'New Agent Registered',
            message: 'Agent "ComplianceAuditor v2.1" has been registered.',
            severity: 'Info',
            timestamp: new Date().toISOString(),
          },
        });
        win.dispatchEvent(event);
      });

      // The notification should appear in the status banner or a toast.
      cy.contains(/new agent registered|complianceauditor/i, { timeout: 10000 })
        .should('be.visible');
    });

    it('should auto-dismiss the notification after a timeout', () => {
      cy.visit('/');

      cy.window().then((win) => {
        const event = new CustomEvent('agent-notification', {
          detail: {
            notificationId: 'test-notification-002',
            title: 'Agent Status Change',
            message: 'Agent is now executing a task',
            severity: 'Info',
            timestamp: new Date().toISOString(),
          },
        });
        win.dispatchEvent(event);
      });

      // Should appear first.
      cy.contains(/agent.*executing|status change/i, { timeout: 10000 })
        .should('be.visible');

      // Should auto-dismiss after 5 seconds.
      cy.contains(/agent.*executing|status change/i, { timeout: 10000 })
        .should('not.exist');
    });

    it('should navigate to the agent details when a notification is clicked', () => {
      cy.visit('/');

      cy.window().then((win) => {
        const event = new CustomEvent('agent-notification', {
          detail: {
            notificationId: 'test-notification-003',
            title: 'Approval Required',
            message: 'Agent requires approval to proceed',
            severity: 'Warning',
            timestamp: new Date().toISOString(),
          },
        });
        win.dispatchEvent(event);
      });

      cy.contains(/approval required/i, { timeout: 10000 }).click();
      // Should navigate or open the relevant agent view.
      cy.url().should('include', '/agent');
    });
  });
});
