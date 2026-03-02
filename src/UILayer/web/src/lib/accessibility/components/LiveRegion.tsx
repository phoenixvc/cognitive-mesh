import React, { useState, useEffect, useRef } from 'react';

/**
 * Props for the LiveRegion component.
 */
export interface LiveRegionProps {
  /** The message to announce to screen readers. */
  message: string;
  /**
   * ARIA live politeness level.
   * - 'polite': Waits until the user is idle before announcing (default).
   * - 'assertive': Interrupts the user's current task to announce immediately.
   */
  politeness?: 'polite' | 'assertive';
  /**
   * Debounce delay in milliseconds to prevent rapid-fire announcements.
   * Defaults to 300ms.
   */
  debounceMs?: number;
  /**
   * If true, clears the region after announcing to prevent re-reading
   * on focus. Defaults to true.
   */
  clearAfterAnnounce?: boolean;
  /**
   * Delay in milliseconds before clearing the region after announcement.
   * Defaults to 5000ms.
   */
  clearDelayMs?: number;
}

/**
 * ARIA live region component for announcing dynamic content changes to screen readers.
 *
 * Implements WCAG 2.1 Success Criterion 4.1.3 (Status Messages) by providing
 * a programmatic way to announce status updates, form errors, and other dynamic
 * content changes without moving focus.
 *
 * The component debounces rapid updates to avoid announcement spam and optionally
 * clears the region after a delay to prevent stale content from being re-read.
 *
 * @example
 * ```tsx
 * const [statusMessage, setStatusMessage] = useState('');
 *
 * // After a form submission:
 * setStatusMessage('Form submitted successfully.');
 *
 * <LiveRegion message={statusMessage} politeness="polite" />
 * ```
 */
const LiveRegion: React.FC<LiveRegionProps> = ({
  message,
  politeness = 'polite',
  debounceMs = 300,
  clearAfterAnnounce = true,
  clearDelayMs = 5000,
}) => {
  const [displayedMessage, setDisplayedMessage] = useState('');
  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const clearTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    // Clear any existing debounce timer
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    // Clear any existing clear timer
    if (clearTimerRef.current) {
      clearTimeout(clearTimerRef.current);
    }

    if (!message) {
      setDisplayedMessage('');
      return;
    }

    // Debounce the message update
    debounceTimerRef.current = setTimeout(() => {
      // Force screen reader re-announcement by briefly clearing then setting
      setDisplayedMessage('');
      requestAnimationFrame(() => {
        setDisplayedMessage(message);
      });

      // Optionally clear after delay
      if (clearAfterAnnounce) {
        clearTimerRef.current = setTimeout(() => {
          setDisplayedMessage('');
        }, clearDelayMs);
      }
    }, debounceMs);

    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
      if (clearTimerRef.current) {
        clearTimeout(clearTimerRef.current);
      }
    };
  }, [message, debounceMs, clearAfterAnnounce, clearDelayMs]);

  return (
    <div
      role="status"
      aria-live={politeness}
      aria-atomic="true"
      style={{
        position: 'absolute',
        width: '1px',
        height: '1px',
        padding: 0,
        margin: '-1px',
        overflow: 'hidden',
        clip: 'rect(0, 0, 0, 0)',
        whiteSpace: 'nowrap',
        border: 0,
      }}
    >
      {displayedMessage}
    </div>
  );
};

export default LiveRegion;
