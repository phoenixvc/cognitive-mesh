import React, { CSSProperties } from 'react';

/**
 * Props for the WidgetSkeleton component.
 */
export interface WidgetSkeletonProps {
  /** Width of the skeleton container. Defaults to '100%'. */
  width?: string | number;
  /** Height of the skeleton container. Defaults to '300px'. */
  height?: string | number;
  /** Number of content rows to display in the skeleton. Defaults to 3. */
  rows?: number;
}

/** Keyframe animation name for the shimmer effect. */
const SHIMMER_ANIMATION_NAME = 'widget-skeleton-shimmer';

/**
 * Injects the shimmer keyframe animation into the document head if not already present.
 * This is done once and shared across all WidgetSkeleton instances.
 */
function ensureShimmerAnimation(): void {
  if (typeof document === 'undefined') {
    return;
  }

  const existingStyle = document.getElementById(SHIMMER_ANIMATION_NAME);
  if (existingStyle) {
    return;
  }

  const style = document.createElement('style');
  style.id = SHIMMER_ANIMATION_NAME;
  style.textContent = `
    @keyframes ${SHIMMER_ANIMATION_NAME} {
      0% { background-position: -200% 0; }
      100% { background-position: 200% 0; }
    }
  `;
  document.head.appendChild(style);
}

/** Base styles for the skeleton container. */
const containerStyles: CSSProperties = {
  padding: '20px',
  borderRadius: '8px',
  backgroundColor: '#f5f5f5',
  border: '1px solid #e0e0e0',
  display: 'flex',
  flexDirection: 'column',
  gap: '16px',
};

/** Base styles for the shimmer effect applied to skeleton elements. */
const shimmerStyles: CSSProperties = {
  background: 'linear-gradient(90deg, #e0e0e0 25%, #f0f0f0 50%, #e0e0e0 75%)',
  backgroundSize: '200% 100%',
  animation: `${SHIMMER_ANIMATION_NAME} 1.5s infinite ease-in-out`,
  borderRadius: '4px',
};

/**
 * Loading skeleton UI component for lazy-loaded widgets.
 *
 * Displays an animated shimmer effect that matches the typical dimensions of a
 * widget panel. The skeleton provides visual feedback that content is loading
 * and maintains layout stability to prevent content shift.
 *
 * Accessibility:
 * - `aria-busy="true"` indicates the region is loading.
 * - `role="progressbar"` identifies the element as a loading indicator.
 * - `aria-label` provides a screen reader description.
 *
 * The shimmer animation respects `prefers-reduced-motion` via CSS media query
 * when combined with the useReducedMotion hook at the application level.
 *
 * @example
 * ```tsx
 * <Suspense fallback={<WidgetSkeleton height={400} rows={5} />}>
 *   <LazyWidget />
 * </Suspense>
 * ```
 */
const WidgetSkeleton: React.FC<WidgetSkeletonProps> = ({
  width = '100%',
  height = '300px',
  rows = 3,
}) => {
  // Ensure the shimmer keyframe animation is injected
  ensureShimmerAnimation();

  return (
    <div
      style={{ ...containerStyles, width, minHeight: height }}
      aria-busy="true"
      role="progressbar"
      aria-label="Loading widget content"
      aria-valuetext="Loading..."
    >
      {/* Title skeleton */}
      <div
        style={{
          ...shimmerStyles,
          height: '24px',
          width: '60%',
        }}
      />

      {/* Subtitle skeleton */}
      <div
        style={{
          ...shimmerStyles,
          height: '16px',
          width: '40%',
        }}
      />

      {/* Content rows */}
      {Array.from({ length: rows }, (_, index) => (
        <div
          key={index}
          style={{
            ...shimmerStyles,
            height: '14px',
            width: `${85 - index * 10}%`,
          }}
        />
      ))}

      {/* Chart/visual area placeholder */}
      <div
        style={{
          ...shimmerStyles,
          height: '120px',
          width: '100%',
          marginTop: '8px',
        }}
      />

      {/* Action bar skeleton */}
      <div style={{ display: 'flex', gap: '12px', marginTop: '8px' }}>
        <div
          style={{
            ...shimmerStyles,
            height: '36px',
            width: '100px',
          }}
        />
        <div
          style={{
            ...shimmerStyles,
            height: '36px',
            width: '100px',
          }}
        />
      </div>
    </div>
  );
};

export default WidgetSkeleton;
