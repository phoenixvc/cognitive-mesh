import React, { Suspense } from 'react';
import WidgetSkeleton from './components/WidgetSkeleton';
import WidgetErrorFallback from './components/WidgetErrorFallback';
import ErrorBoundary from './components/ErrorBoundary';

/**
 * Creates a lazily-loaded widget component wrapped with Suspense and ErrorBoundary.
 *
 * This factory function takes a dynamic import function and returns a new component
 * that automatically handles:
 * - **Code splitting**: The widget chunk is only loaded when the component is first rendered.
 * - **Loading state**: A shimmer skeleton UI is shown via React Suspense while the chunk loads.
 * - **Error recovery**: An ErrorBoundary catches render errors and displays a retry-able fallback.
 *
 * Use this function to wrap any widget panel for lazy loading in the widget registry.
 *
 * @param importFn - A function returning a dynamic `import()` promise for the widget module.
 *                   The module must have a `default` export that is a React component.
 * @returns A wrapper component that renders the lazily-loaded widget with loading and error states.
 *
 * @example
 * ```tsx
 * // In registry/lazyWidgets.ts
 * export const LazyAgentControlCenter = createLazyWidget(
 *   () => import('../AgencyWidgets/Panels/AgentControlCenter')
 * );
 *
 * // Usage in a page or layout
 * <LazyAgentControlCenter userId="user-1" tenantId="tenant-1" />
 * ```
 */
export function createLazyWidget<P extends Record<string, unknown>>(
  importFn: () => Promise<{ default: React.ComponentType<P> }>
): React.FC<P> {
  const LazyComponent = React.lazy(importFn);

  /**
   * Wrapper component that renders the lazily-loaded widget within
   * Suspense and ErrorBoundary providers.
   */
  function LazyWidgetWrapper(props: P): React.JSX.Element {
    return (
      <ErrorBoundary
        FallbackComponent={WidgetErrorFallback}
        onError={(error, errorInfo) => {
          // Log to console; in production this would dispatch to telemetry
          console.error(
            `[LazyWidgetLoader] Error in lazy widget:`,
            error.message,
            errorInfo.componentStack
          );
        }}
      >
        <Suspense fallback={<WidgetSkeleton />}>
          <LazyComponent {...props} />
        </Suspense>
      </ErrorBoundary>
    );
  }

  // Set display name for React DevTools
  const importFnString = importFn.toString();
  const moduleMatch = importFnString.match(/import\(['"](.+?)['"]\)/);
  const moduleName = moduleMatch
    ? moduleMatch[1].split('/').pop()?.replace(/['"]/g, '')
    : 'Unknown';
  LazyWidgetWrapper.displayName = `LazyWidget(${moduleName})`;

  return LazyWidgetWrapper;
}
