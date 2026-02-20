import { useState, useEffect } from 'react';

/**
 * Custom hook that detects the user's `prefers-reduced-motion` media query preference.
 *
 * Returns `true` when the user has requested reduced motion in their operating system
 * or browser accessibility settings. Components should use this to conditionally
 * disable animations, transitions, and auto-playing content per WCAG 2.1 Success
 * Criterion 2.3.3 (Animation from Interactions).
 *
 * The hook listens for changes to the media query so the returned value updates
 * in real time if the user toggles the setting while the page is open.
 *
 * @returns `true` if the user prefers reduced motion, `false` otherwise.
 *
 * @example
 * ```tsx
 * const prefersReducedMotion = useReducedMotion();
 *
 * const transitionDuration = prefersReducedMotion ? '0ms' : '300ms';
 * const shouldAnimate = !prefersReducedMotion;
 * ```
 */
export function useReducedMotion(): boolean {
  const [prefersReducedMotion, setPrefersReducedMotion] = useState<boolean>(() => {
    // SSR-safe: default to false if window is not available
    if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
      return false;
    }
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  });

  useEffect(() => {
    if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
      return;
    }

    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');

    const handleChange = (event: MediaQueryListEvent) => {
      setPrefersReducedMotion(event.matches);
    };

    // Modern browsers
    mediaQuery.addEventListener('change', handleChange);

    // Sync initial state in case it changed between render and effect
    setPrefersReducedMotion(mediaQuery.matches);

    return () => {
      mediaQuery.removeEventListener('change', handleChange);
    };
  }, []);

  return prefersReducedMotion;
}
