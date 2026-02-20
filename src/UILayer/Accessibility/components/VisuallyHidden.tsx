import React, { CSSProperties } from 'react';

/**
 * Props for the VisuallyHidden component.
 */
export interface VisuallyHiddenProps {
  /** The content to render for screen readers. */
  children: React.ReactNode;
  /**
   * The HTML element to render. Defaults to 'span'.
   * Useful for semantic correctness (e.g., 'h2' for a hidden heading,
   * 'label' for a hidden form label).
   */
  as?: keyof React.JSX.IntrinsicElements;
  /** Optional additional CSS class name. */
  className?: string;
  /** Optional id attribute for label/describedby associations. */
  id?: string;
}

/**
 * CSS properties that visually hide an element while keeping it in the
 * accessibility tree for screen readers.
 *
 * This approach is preferred over `display: none` or `visibility: hidden`,
 * both of which remove elements from the accessibility tree entirely.
 *
 * @see https://www.a11yproject.com/posts/how-to-hide-content/
 */
const visuallyHiddenStyles: CSSProperties = {
  position: 'absolute',
  width: '1px',
  height: '1px',
  padding: 0,
  margin: '-1px',
  overflow: 'hidden',
  clip: 'rect(0, 0, 0, 0)',
  whiteSpace: 'nowrap',
  border: 0,
};

/**
 * Screen-reader-only text component for WCAG compliance.
 *
 * Renders content that is visually hidden from sighted users but remains
 * fully accessible to screen readers and other assistive technologies.
 * The element stays in the normal document flow and accessibility tree.
 *
 * Common use cases:
 * - Adding descriptive text to icon-only buttons
 * - Providing context for complex UI interactions
 * - Hidden headings for page structure
 * - Form labels that are visually implied by layout
 *
 * The `as` prop controls the rendered HTML element, defaulting to `<span>`.
 * This is important for maintaining proper semantic structure (e.g., using
 * `as="h2"` for a visually hidden section heading).
 *
 * @example
 * ```tsx
 * <button>
 *   <Icon name="close" />
 *   <VisuallyHidden>Close dialog</VisuallyHidden>
 * </button>
 *
 * <VisuallyHidden as="h2">Navigation section</VisuallyHidden>
 * ```
 */
const VisuallyHidden: React.FC<VisuallyHiddenProps> = ({
  children,
  as: Component = 'span',
  className,
  id,
}) => {
  return React.createElement(
    Component,
    {
      style: visuallyHiddenStyles,
      className,
      id,
    },
    children
  );
};

export default VisuallyHidden;
