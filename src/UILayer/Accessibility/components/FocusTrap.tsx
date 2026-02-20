import React, { useEffect, useRef, useCallback } from 'react';

/**
 * Props for the FocusTrap component.
 */
export interface FocusTrapProps {
  /** Whether the focus trap is currently active. */
  active: boolean;
  /** Callback invoked when the user presses Escape to close the trap. */
  onClose: () => void;
  /** Child elements to render within the trap container. */
  children: React.ReactNode;
  /** Optional ref to the element that triggered the trap, so focus can return on close. */
  returnFocusRef?: React.RefObject<HTMLElement | null>;
  /** Whether pressing Escape should invoke onClose. Defaults to true. */
  closeOnEscape?: boolean;
}

/** CSS selector for all focusable elements. */
const FOCUSABLE_SELECTOR = [
  'a[href]',
  'button:not([disabled])',
  'textarea:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
  '[contenteditable]',
].join(', ');

/**
 * Returns all focusable elements within a container, sorted by DOM order.
 */
function getFocusableElements(container: HTMLElement): HTMLElement[] {
  return Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR)).filter(
    (el) => !el.hasAttribute('disabled') && el.offsetParent !== null
  );
}

/**
 * Modal focus trap component for WCAG 2.1 compliance (Success Criterion 2.4.3, 2.1.2).
 *
 * When active, this component traps keyboard focus within its child tree.
 * Pressing Tab at the last focusable element wraps to the first, and pressing
 * Shift+Tab at the first wraps to the last. Pressing Escape invokes the onClose
 * callback (configurable via closeOnEscape).
 *
 * On activation, focus moves to the first focusable element within the trap.
 * On deactivation, focus returns to the element referenced by returnFocusRef
 * (if provided), ensuring the user's keyboard position is preserved.
 *
 * @example
 * ```tsx
 * const triggerRef = useRef<HTMLButtonElement>(null);
 * const [isOpen, setIsOpen] = useState(false);
 *
 * <button ref={triggerRef} onClick={() => setIsOpen(true)}>Open Modal</button>
 * <FocusTrap active={isOpen} onClose={() => setIsOpen(false)} returnFocusRef={triggerRef}>
 *   <div role="dialog" aria-modal="true">
 *     <h2>Modal Title</h2>
 *     <button onClick={() => setIsOpen(false)}>Close</button>
 *   </div>
 * </FocusTrap>
 * ```
 */
const FocusTrap: React.FC<FocusTrapProps> = ({
  active,
  onClose,
  children,
  returnFocusRef,
  closeOnEscape = true,
}) => {
  const trapRef = useRef<HTMLDivElement | null>(null);

  // Focus the first focusable element on activation
  useEffect(() => {
    if (!active || !trapRef.current) {
      return;
    }

    const focusableElements = getFocusableElements(trapRef.current);
    if (focusableElements.length > 0) {
      focusableElements[0].focus();
    }
  }, [active]);

  // Return focus to trigger element on deactivation
  useEffect(() => {
    return () => {
      if (returnFocusRef?.current) {
        // Use setTimeout to allow the DOM to update before refocusing
        setTimeout(() => {
          returnFocusRef.current?.focus();
        }, 0);
      }
    };
  }, [returnFocusRef]);

  // Handle keydown for tab trapping and escape
  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLDivElement>) => {
      if (!active || !trapRef.current) {
        return;
      }

      // Handle Escape
      if (e.key === 'Escape' && closeOnEscape) {
        e.preventDefault();
        onClose();
        return;
      }

      // Handle Tab wrapping
      if (e.key === 'Tab') {
        const focusableElements = getFocusableElements(trapRef.current);
        if (focusableElements.length === 0) {
          e.preventDefault();
          return;
        }

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        if (e.shiftKey) {
          // Shift+Tab: wrap from first to last
          if (document.activeElement === firstElement) {
            e.preventDefault();
            lastElement.focus();
          }
        } else {
          // Tab: wrap from last to first
          if (document.activeElement === lastElement) {
            e.preventDefault();
            firstElement.focus();
          }
        }
      }
    },
    [active, closeOnEscape, onClose]
  );

  if (!active) {
    return null;
  }

  return (
    <div ref={trapRef} onKeyDown={handleKeyDown}>
      {children}
    </div>
  );
};

export default FocusTrap;
