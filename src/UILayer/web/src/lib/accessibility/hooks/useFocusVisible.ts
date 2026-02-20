import { useState, useEffect, useCallback } from 'react';

/**
 * Return type for the useFocusVisible hook.
 */
interface UseFocusVisibleResult {
  /**
   * Whether focus-visible styling should be applied.
   * `true` when the element is focused AND the focus was triggered by keyboard
   * navigation (Tab, Shift+Tab, arrow keys, etc.), `false` otherwise.
   */
  isFocusVisible: boolean;
  /**
   * Event handlers to attach to the target element.
   * Spread these onto your component: `{...focusVisibleProps}`.
   */
  focusVisibleProps: {
    onFocus: (e: React.FocusEvent) => void;
    onBlur: (e: React.FocusEvent) => void;
  };
}

/**
 * Keys that should trigger focus-visible behavior.
 */
const KEYBOARD_FOCUS_KEYS = new Set([
  'Tab',
  'ArrowUp',
  'ArrowDown',
  'ArrowLeft',
  'ArrowRight',
  'Home',
  'End',
  'PageUp',
  'PageDown',
  'Enter',
  ' ',
]);

/**
 * Tracks whether the last interaction was keyboard-based at the document level.
 * This is a module-level singleton to avoid duplicating listeners.
 */
let hadKeyboardEvent = false;
let isListenerAttached = false;

function attachGlobalListeners(): void {
  if (isListenerAttached || typeof document === 'undefined') {
    return;
  }

  document.addEventListener(
    'keydown',
    (e: KeyboardEvent) => {
      if (KEYBOARD_FOCUS_KEYS.has(e.key)) {
        hadKeyboardEvent = true;
      }
    },
    true
  );

  document.addEventListener(
    'mousedown',
    () => {
      hadKeyboardEvent = false;
    },
    true
  );

  document.addEventListener(
    'pointerdown',
    () => {
      hadKeyboardEvent = false;
    },
    true
  );

  document.addEventListener(
    'touchstart',
    () => {
      hadKeyboardEvent = false;
    },
    true
  );

  isListenerAttached = true;
}

/**
 * Hook that distinguishes keyboard focus from mouse/pointer focus.
 *
 * Implements the `:focus-visible` behavior as a React hook for environments
 * where the CSS pseudo-class is not sufficient (e.g., when you need to
 * conditionally render different UI based on focus source, or for older
 * browser support).
 *
 * This hook tracks document-level keyboard and pointer events to determine
 * whether focus was triggered by keyboard navigation. It returns a boolean
 * `isFocusVisible` flag and a set of event handlers to attach to your element.
 *
 * The `isFocusVisible` flag is `true` only when:
 * 1. The element is focused, AND
 * 2. The focus was triggered by a keyboard event (Tab, arrow keys, etc.)
 *
 * WCAG 2.1 Success Criteria: 2.4.7 (Focus Visible), 2.4.11 (Focus Not Obscured).
 *
 * @returns An object containing the `isFocusVisible` flag and `focusVisibleProps`
 *          event handlers to spread onto the target element.
 *
 * @example
 * ```tsx
 * const { isFocusVisible, focusVisibleProps } = useFocusVisible();
 *
 * <button
 *   {...focusVisibleProps}
 *   style={{
 *     outline: isFocusVisible ? '2px solid #005a9e' : 'none',
 *   }}
 * >
 *   Click me
 * </button>
 * ```
 */
export function useFocusVisible(): UseFocusVisibleResult {
  const [isFocusVisible, setIsFocusVisible] = useState(false);

  // Attach global listeners on first use
  useEffect(() => {
    attachGlobalListeners();
  }, []);

  const onFocus = useCallback((_e: React.FocusEvent) => {
    if (hadKeyboardEvent) {
      setIsFocusVisible(true);
    }
  }, []);

  const onBlur = useCallback((_e: React.FocusEvent) => {
    setIsFocusVisible(false);
  }, []);

  return {
    isFocusVisible,
    focusVisibleProps: {
      onFocus,
      onBlur,
    },
  };
}
