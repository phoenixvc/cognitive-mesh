/**
 * Axe-core configuration for automated WCAG 2.1 AA accessibility testing.
 *
 * Provides a pre-configured axe-core setup targeting WCAG 2.1 Level AA criteria
 * with impact-level filtering. Use {@link runAccessibilityAudit} to execute an
 * audit against a DOM container and receive structured results.
 *
 * @see https://github.com/dequelabs/axe-core
 */

import type { AxeResults, RunOptions, Spec } from 'axe-core';

/**
 * Impact severity levels as classified by axe-core.
 */
export type ImpactLevel = 'critical' | 'serious' | 'moderate' | 'minor';

/**
 * Structured result returned by {@link runAccessibilityAudit}.
 */
export interface AccessibilityAuditResult {
  /** Total number of violations found. */
  violationCount: number;
  /** Violations grouped by impact severity. */
  violationsByImpact: Record<ImpactLevel, number>;
  /** Total number of passing rules. */
  passCount: number;
  /** Total number of rules that could not be evaluated (incomplete). */
  incompleteCount: number;
  /** Total number of rules that were not applicable to the tested content. */
  inapplicableCount: number;
  /** Full axe-core results object for detailed inspection. */
  rawResults: AxeResults;
}

/**
 * Default axe-core run options targeting WCAG 2.1 Level AA.
 *
 * Configures axe to only run rule tags relevant to WCAG 2.1 AA compliance,
 * including the base WCAG 2.0 A/AA rules and the WCAG 2.1 additions.
 */
export const defaultAxeRunOptions: RunOptions = {
  runOnly: {
    type: 'tag',
    values: [
      'wcag2a',
      'wcag2aa',
      'wcag21a',
      'wcag21aa',
      'best-practice',
    ],
  },
  resultTypes: ['violations', 'passes', 'incomplete', 'inapplicable'],
};

/**
 * Default axe-core spec configuration.
 *
 * Sets the reporting locale and branding for audit output.
 */
export const defaultAxeSpec: Spec = {
  branding: {
    brand: 'Cognitive Mesh',
    application: 'UILayer Accessibility Audit',
  },
};

/**
 * Runs an axe-core accessibility audit on the provided DOM container.
 *
 * This function dynamically imports axe-core to avoid bundling it in production
 * builds unless explicitly invoked. It configures axe for WCAG 2.1 AA testing
 * and returns a structured result summarizing violations by impact level.
 *
 * @param container - The HTML element to audit. Typically the root of a
 *                    component tree or the document body.
 * @param options - Optional axe-core run options override. Defaults to
 *                  {@link defaultAxeRunOptions}.
 * @returns A promise resolving to an {@link AccessibilityAuditResult} with
 *          violation counts and the raw axe-core output.
 *
 * @example
 * ```ts
 * const result = await runAccessibilityAudit(document.getElementById('app')!);
 * console.log(`Found ${result.violationCount} violations`);
 * console.log('Critical:', result.violationsByImpact.critical);
 * ```
 */
export async function runAccessibilityAudit(
  container: HTMLElement,
  options: RunOptions = defaultAxeRunOptions
): Promise<AccessibilityAuditResult> {
  // Dynamic import to avoid bloating production bundles
  const axe = await import('axe-core');

  // Configure axe branding
  axe.default.configure(defaultAxeSpec);

  // Run the audit
  const results: AxeResults = await axe.default.run(container, options);

  // Aggregate violations by impact
  const violationsByImpact: Record<ImpactLevel, number> = {
    critical: 0,
    serious: 0,
    moderate: 0,
    minor: 0,
  };

  for (const violation of results.violations) {
    const impact = (violation.impact as ImpactLevel) ?? 'minor';
    violationsByImpact[impact] += violation.nodes.length;
  }

  return {
    violationCount: results.violations.length,
    violationsByImpact,
    passCount: results.passes.length,
    incompleteCount: results.incomplete.length,
    inapplicableCount: results.inapplicable.length,
    rawResults: results,
  };
}
