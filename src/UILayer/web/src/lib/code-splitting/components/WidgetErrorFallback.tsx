import React, { CSSProperties, useCallback } from 'react';
import type { ErrorFallbackProps } from './ErrorBoundary';

/**
 * Styles for the error fallback container.
 */
const containerStyles: CSSProperties = {
  padding: '24px',
  borderRadius: '8px',
  backgroundColor: '#fef2f2',
  border: '1px solid #fecaca',
  textAlign: 'center',
  fontFamily: 'Arial, sans-serif',
};

/**
 * Styles for the error icon.
 */
const iconStyles: CSSProperties = {
  fontSize: '48px',
  marginBottom: '12px',
  display: 'block',
};

/**
 * Styles for the error heading.
 */
const headingStyles: CSSProperties = {
  color: '#991b1b',
  fontSize: '18px',
  fontWeight: 'bold',
  marginBottom: '8px',
};

/**
 * Styles for the error message text.
 */
const messageStyles: CSSProperties = {
  color: '#7f1d1d',
  fontSize: '14px',
  marginBottom: '16px',
  lineHeight: '1.5',
};

/**
 * Styles for the error details (collapsed by default).
 */
const detailsStyles: CSSProperties = {
  textAlign: 'left',
  backgroundColor: '#fee2e2',
  padding: '12px',
  borderRadius: '4px',
  fontSize: '12px',
  fontFamily: 'monospace',
  color: '#7f1d1d',
  marginBottom: '16px',
  maxHeight: '150px',
  overflow: 'auto',
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
};

/**
 * Styles for the retry button.
 */
const buttonStyles: CSSProperties = {
  padding: '10px 24px',
  borderRadius: '6px',
  border: 'none',
  backgroundColor: '#dc2626',
  color: '#ffffff',
  fontSize: '14px',
  fontWeight: 'bold',
  cursor: 'pointer',
  transition: 'background-color 0.2s ease',
};

/**
 * Error boundary fallback component rendered when a lazy-loaded widget crashes.
 *
 * Displays a user-friendly error message with:
 * - A visual error indicator
 * - A human-readable error description
 * - An expandable details section showing the technical error message
 * - A retry button that resets the error boundary
 *
 * The component also reports errors to telemetry by logging to console.
 * In production, this would integrate with the ITelemetryAdapter.
 *
 * @example
 * ```tsx
 * <ErrorBoundary FallbackComponent={WidgetErrorFallback}>
 *   <LazyWidget />
 * </ErrorBoundary>
 * ```
 */
const WidgetErrorFallback: React.FC<ErrorFallbackProps> = ({
  error,
  resetErrorBoundary,
}) => {
  const handleRetry = useCallback(() => {
    // Report retry attempt to telemetry
    console.info('[WidgetErrorFallback] User initiated retry after error:', error.message);
    resetErrorBoundary();
  }, [error, resetErrorBoundary]);

  // Report the error for telemetry on mount
  React.useEffect(() => {
    console.error('[WidgetErrorFallback] Widget failed to load:', {
      message: error.message,
      stack: error.stack,
      timestamp: new Date().toISOString(),
    });
  }, [error]);

  return (
    <div style={containerStyles} role="alert" aria-live="assertive">
      <span style={iconStyles} role="img" aria-hidden="true">
        &#x26A0;
      </span>
      <h3 style={headingStyles}>Widget Failed to Load</h3>
      <p style={messageStyles}>
        Something went wrong while loading this widget. This may be a temporary
        issue. Please try again, and if the problem persists, contact support.
      </p>

      <details>
        <summary
          style={{
            cursor: 'pointer',
            color: '#991b1b',
            fontSize: '13px',
            marginBottom: '8px',
          }}
        >
          Show technical details
        </summary>
        <div style={detailsStyles}>
          <strong>Error:</strong> {error.message}
          {error.stack && (
            <>
              {'\n\n'}
              <strong>Stack trace:</strong>
              {'\n'}
              {error.stack}
            </>
          )}
        </div>
      </details>

      <button
        style={buttonStyles}
        onClick={handleRetry}
        onMouseOver={(e) => {
          (e.currentTarget as HTMLButtonElement).style.backgroundColor = '#b91c1c';
        }}
        onMouseOut={(e) => {
          (e.currentTarget as HTMLButtonElement).style.backgroundColor = '#dc2626';
        }}
        aria-label="Retry loading the widget"
      >
        Retry
      </button>
    </div>
  );
};

export default WidgetErrorFallback;
