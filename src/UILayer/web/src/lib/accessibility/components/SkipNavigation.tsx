import React, { CSSProperties } from 'react';

/**
 * Props for the SkipNavigation component.
 */
export interface SkipNavigationProps {
  /** The CSS selector or ID (without #) of the main content target. Defaults to 'main-content'. */
  mainContentId?: string;
  /** Custom label text for the skip link. */
  label?: string;
}

/**
 * Styles for the skip navigation link.
 * Visually hidden until focused, then positioned prominently at the top of the page.
 */
const skipLinkStyles: CSSProperties = {
  position: 'absolute',
  top: '-9999px',
  left: '-9999px',
  zIndex: 9999,
  padding: '12px 24px',
  backgroundColor: '#005a9e',
  color: '#ffffff',
  fontSize: '16px',
  fontWeight: 'bold',
  textDecoration: 'none',
  borderRadius: '0 0 4px 4px',
  boxShadow: '0 2px 8px rgba(0, 0, 0, 0.3)',
};

/**
 * Focus-visible styles applied when the skip link receives keyboard focus.
 */
const skipLinkFocusStyles: CSSProperties = {
  position: 'fixed',
  top: '0',
  left: '50%',
  transform: 'translateX(-50%)',
};

/**
 * Skip-to-content navigation link for WCAG 2.1 compliance (Success Criterion 2.4.1).
 *
 * This component renders a link that is visually hidden off-screen until the user
 * focuses it with the keyboard (typically the first Tab press on a page). When
 * focused, the link appears prominently at the top of the viewport, allowing
 * keyboard users to bypass repetitive navigation and jump directly to main content.
 *
 * Usage: Place this component as the first focusable element in your application
 * layout, and ensure the target element has a matching `id` attribute.
 *
 * @example
 * ```tsx
 * <SkipNavigation mainContentId="app-main" />
 * <nav>...</nav>
 * <main id="app-main">...</main>
 * ```
 */
const SkipNavigation: React.FC<SkipNavigationProps> = ({
  mainContentId = 'main-content',
  label = 'Skip to main content',
}) => {
  const handleFocus = (e: React.FocusEvent<HTMLAnchorElement>) => {
    Object.assign(e.currentTarget.style, skipLinkFocusStyles);
  };

  const handleBlur = (e: React.FocusEvent<HTMLAnchorElement>) => {
    e.currentTarget.style.position = 'absolute';
    e.currentTarget.style.top = '-9999px';
    e.currentTarget.style.left = '-9999px';
    e.currentTarget.style.transform = '';
  };

  return (
    <a
      href={`#${mainContentId}`}
      style={skipLinkStyles}
      onFocus={handleFocus}
      onBlur={handleBlur}
      className="skip-navigation"
    >
      {label}
    </a>
  );
};

export default SkipNavigation;
