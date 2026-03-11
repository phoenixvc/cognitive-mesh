/**
 * Accessibility utilities for WCAG 2.1 AA compliance.
 *
 * Provides hooks, components, and auditing tools to ensure
 * the Cognitive Mesh UI meets accessibility standards.
 */

// Hooks
export { useReducedMotion } from './hooks/useReducedMotion';
export { useFocusVisible } from './hooks/useFocusVisible';

// Components
export { default as FocusTrap } from './components/FocusTrap';
export { default as LiveRegion } from './components/LiveRegion';
export { default as SkipNavigation } from './components/SkipNavigation';
export { default as VisuallyHidden } from './components/VisuallyHidden';

// Audit
export { runAccessibilityAudit, defaultAxeRunOptions, defaultAxeSpec } from './axeConfig';
export type { AccessibilityAuditResult, ImpactLevel } from './axeConfig';
