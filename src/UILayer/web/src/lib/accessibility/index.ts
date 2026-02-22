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
export { FocusTrap } from './components/FocusTrap';
export { LiveRegion } from './components/LiveRegion';
export { SkipNavigation } from './components/SkipNavigation';
export { VisuallyHidden } from './components/VisuallyHidden';

// Audit
export { axeConfig } from './axeConfig';
