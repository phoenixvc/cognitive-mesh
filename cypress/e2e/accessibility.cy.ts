/**
 * @fileoverview WCAG compliance E2E tests for the Cognitive Mesh UI.
 *
 * Uses axe-core via cypress-axe to validate:
 * - Automated WCAG 2.1 AA compliance on each page
 * - Color contrast ratios
 * - Keyboard navigation and focus management
 * - Screen reader landmarks and ARIA attributes
 */

describe('Accessibility (WCAG 2.1 AA)', () => {
  beforeEach(() => {
    cy.login('testuser@example.com', 'P@ssw0rd!');
  });

  // -----------------------------------------------------------------------
  // Axe-core audits on each page
  // -----------------------------------------------------------------------

  describe('Automated axe-core Audits', () => {
    const pages = [
      { name: 'Home / Dashboard', path: '/' },
      { name: 'Dashboard Overview', path: '/dashboard/main-overview' },
      { name: 'Agent Audit Trail', path: '/agents/audit-trail' },
      { name: 'Agent Registry', path: '/agents/registry' },
      { name: 'Settings', path: '/settings' },
    ];

    pages.forEach(({ name, path }) => {
      it(`should have no critical or serious a11y violations on ${name}`, () => {
        cy.visit(path);
        // Wait for the page to stabilize.
        cy.get('body', { timeout: 10000 }).should('be.visible');
        cy.assertAccessibility();
      });
    });
  });

  // -----------------------------------------------------------------------
  // Color contrast
  // -----------------------------------------------------------------------

  describe('Color Contrast', () => {
    it('should pass the axe color-contrast rule on the dashboard', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');
      cy.injectAxe();
      cy.checkA11y(undefined, {
        runOnly: {
          type: 'rule',
          values: ['color-contrast'],
        },
      });
    });

    it('should maintain contrast on status badges', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      // Status badges use white text on colored backgrounds.
      // Verify at least minimal contrast ratio via computed styles.
      cy.get('span').filter('[style*="color: white"], [style*="color:white"]').each(($el) => {
        const bgColor = $el.css('background-color');
        // Ensure the background is not white or transparent (would fail contrast).
        expect(bgColor).to.not.match(/rgba?\(255,\s*255,\s*255/);
        expect(bgColor).to.not.equal('transparent');
      });
    });

    it('should maintain contrast in dark mode', () => {
      cy.visit('/dashboard/main-overview');
      // Trigger dark mode if the app supports it via a toggle.
      cy.window().then((win) => {
        win.localStorage.setItem('theme', 'Dark');
      });
      cy.reload();
      cy.get('body', { timeout: 10000 }).should('be.visible');
      cy.injectAxe();
      cy.checkA11y(undefined, {
        runOnly: {
          type: 'rule',
          values: ['color-contrast'],
        },
      });
    });
  });

  // -----------------------------------------------------------------------
  // Keyboard navigation
  // -----------------------------------------------------------------------

  describe('Keyboard Navigation', () => {
    it('should allow tabbing through interactive elements on the dashboard', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      // Start tabbing from the body.
      cy.get('body').tab();
      cy.focused().should('exist');

      // Tab through several elements and verify focus moves.
      const focusedElements: string[] = [];
      for (let i = 0; i < 10; i++) {
        cy.focused().then(($el) => {
          const tagName = $el.prop('tagName');
          const role = $el.attr('role');
          focusedElements.push(`${tagName}[role=${role}]`);
        });
        cy.focused().tab();
      }

      // At least some interactive elements should have received focus.
      cy.wrap(focusedElements).should('have.length.greaterThan', 0);
    });

    it('should allow Enter key to activate table rows', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      cy.get('table tbody tr[tabindex]').first().focus();
      cy.focused().type('{enter}');

      // Pressing Enter on a row should open a details view.
      cy.get('[role="dialog"]', { timeout: 5000 }).should('exist');
    });

    it('should trap focus inside modals', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      // Open a modal by clicking a table row.
      cy.get('table tbody tr').first().click();
      cy.get('[role="dialog"]', { timeout: 5000 }).should('be.visible');

      // Tab forward many times and verify focus stays inside the modal.
      for (let i = 0; i < 20; i++) {
        cy.focused().tab();
        cy.focused().closest('[role="dialog"]').should('exist');
      }
    });

    it('should close modals with the Escape key', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      cy.get('table tbody tr').first().click();
      cy.get('[role="dialog"]', { timeout: 5000 }).should('be.visible');
      cy.get('body').type('{esc}');
      cy.get('[role="dialog"]').should('not.exist');
    });

    it('should have visible focus indicators on interactive elements', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      cy.get('button, a, input, select, [tabindex]').first().focus();
      cy.focused().then(($el) => {
        // The focused element should have a visible outline or box-shadow.
        const outline = $el.css('outline');
        const boxShadow = $el.css('box-shadow');
        const outlineVisible = outline && !outline.includes('none') && !outline.includes('0px');
        const shadowVisible = boxShadow && !boxShadow.includes('none');
        // At least one of outline or box-shadow should provide a focus indicator.
        // Note: Some browsers may handle this differently.
        expect(outlineVisible || shadowVisible).to.be.true;
      });
    });
  });

  // -----------------------------------------------------------------------
  // Screen reader landmarks
  // -----------------------------------------------------------------------

  describe('Screen Reader Landmarks', () => {
    it('should have a main content landmark', () => {
      cy.visit('/');
      cy.get('body', { timeout: 10000 }).should('be.visible');
      cy.get('main, [role="main"]').should('exist');
    });

    it('should use proper heading hierarchy', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      // There should be at least one h1 or h2.
      cy.get('h1, h2').should('have.length.greaterThan', 0);

      // Headings should not skip levels (h1 -> h3 without h2).
      cy.get('h1, h2, h3, h4, h5, h6').then(($headings) => {
        let previousLevel = 0;
        $headings.each((_, el) => {
          const level = parseInt(el.tagName.replace('H', ''), 10);
          // A heading can be at the same level, one level deeper, or any
          // level shallower than the previous heading.
          if (previousLevel > 0) {
            expect(level).to.be.at.most(previousLevel + 1);
          }
          previousLevel = level;
        });
      });
    });

    it('should have labeled regions for widgets', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      cy.get('[role="region"]').each(($region) => {
        // Each region should have an accessible name via aria-labelledby or aria-label.
        const hasLabelledBy = !!$region.attr('aria-labelledby');
        const hasLabel = !!$region.attr('aria-label');
        expect(hasLabelledBy || hasLabel).to.be.true;
      });
    });

    it('should have accessible labels on form controls', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      cy.get('input, select, textarea').each(($control) => {
        const hasAriaLabel = !!$control.attr('aria-label');
        const hasAriaLabelledBy = !!$control.attr('aria-labelledby');
        const id = $control.attr('id');
        const hasAssociatedLabel = id ? Cypress.$(`label[for="${id}"]`).length > 0 : false;

        // Each form control should have at least one labelling mechanism.
        expect(hasAriaLabel || hasAriaLabelledBy || hasAssociatedLabel).to.be.true;
      });
    });

    it('should use role="alert" for error messages', () => {
      cy.intercept('GET', '/api/**', {
        statusCode: 500,
        body: { errorCode: 'SERVER_ERROR', message: 'Internal Server Error' },
      });

      cy.visit('/dashboard/main-overview');
      cy.get('[role="alert"]', { timeout: 15000 }).should('exist');
    });

    it('should use aria-live regions for dynamic content updates', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');
      // Loading indicators and status updates should use aria-live.
      cy.get('[aria-live]').should('have.length.greaterThan', 0);
    });

    it('should have alt text or aria-hidden on decorative images', () => {
      cy.visit('/dashboard/main-overview');
      cy.get('body', { timeout: 10000 }).should('be.visible');

      cy.get('img').each(($img) => {
        const alt = $img.attr('alt');
        const ariaHidden = $img.attr('aria-hidden');
        const role = $img.attr('role');
        // Each image should either have alt text, be marked decorative,
        // or have role="presentation".
        const isAccessible = (alt !== undefined) || ariaHidden === 'true' || role === 'presentation';
        expect(isAccessible).to.be.true;
      });
    });
  });
});
