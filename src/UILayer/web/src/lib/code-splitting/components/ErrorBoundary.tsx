import React, { Component, ErrorInfo } from 'react';

/**
 * Props for the ErrorBoundary component.
 */
export interface ErrorBoundaryProps {
  /** Fallback UI to render when an error is caught. Receives the error and a reset function. */
  FallbackComponent: React.ComponentType<ErrorFallbackProps>;
  /** Optional callback invoked when an error is caught, for telemetry/logging. */
  onError?: (error: Error, errorInfo: ErrorInfo) => void;
  /** Child components to render within the boundary. */
  children: React.ReactNode;
}

/**
 * Props passed to the fallback component when an error is caught.
 */
export interface ErrorFallbackProps {
  /** The error that was caught. */
  error: Error;
  /** Function to reset the error boundary and retry rendering. */
  resetErrorBoundary: () => void;
}

/**
 * Internal state for the ErrorBoundary class component.
 */
interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

/**
 * React error boundary class component for catching and recovering from render errors.
 *
 * Wraps child components and catches JavaScript errors during rendering, in lifecycle
 * methods, and in constructors of the whole tree below. When an error is caught, the
 * boundary renders the provided FallbackComponent instead of the crashed component tree.
 *
 * Features:
 * - Catches render-time errors and prevents them from crashing the entire application
 * - Renders a configurable fallback UI with error details
 * - Provides a reset mechanism to retry rendering the original children
 * - Logs errors to console and optionally to external telemetry via onError callback
 *
 * Note: Error boundaries do NOT catch errors in event handlers, async code,
 * server-side rendering, or errors thrown in the boundary itself.
 *
 * @example
 * ```tsx
 * <ErrorBoundary
 *   FallbackComponent={WidgetErrorFallback}
 *   onError={(error, info) => telemetry.logError(error, info)}
 * >
 *   <SomeWidget />
 * </ErrorBoundary>
 * ```
 */
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
    };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return {
      hasError: true,
      error,
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    // Log to console for development visibility
    console.error('[ErrorBoundary] Caught error:', error);
    console.error('[ErrorBoundary] Component stack:', errorInfo.componentStack);

    // Invoke external error handler for telemetry
    if (this.props.onError) {
      this.props.onError(error, errorInfo);
    }
  }

  /**
   * Resets the error boundary state, allowing the children to attempt rendering again.
   */
  resetErrorBoundary = (): void => {
    this.setState({
      hasError: false,
      error: null,
    });
  };

  render(): React.ReactNode {
    const { hasError, error } = this.state;
    const { FallbackComponent, children } = this.props;

    if (hasError && error) {
      return (
        <FallbackComponent
          error={error}
          resetErrorBoundary={this.resetErrorBoundary}
        />
      );
    }

    return children;
  }
}

export default ErrorBoundary;
