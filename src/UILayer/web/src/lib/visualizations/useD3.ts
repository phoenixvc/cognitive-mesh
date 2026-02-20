import { useRef, useEffect, useCallback } from 'react';
import * as d3 from 'd3';

/**
 * Return type for the useD3 hook, providing an SVG ref and D3 selection helper.
 */
interface UseD3Result {
  /** React ref to attach to the SVG element. */
  svgRef: React.RefObject<SVGSVGElement | null>;
  /**
   * Returns a D3 selection wrapping the SVG element, or null if the element
   * is not yet mounted in the DOM.
   */
  getSelection: () => d3.Selection<SVGSVGElement, unknown, null, undefined> | null;
}

/**
 * Custom hook for managing a D3-powered SVG container.
 *
 * Handles:
 * - Providing a stable ref for the SVG element
 * - Responsive resizing via ResizeObserver
 * - Automatic cleanup of D3 content on unmount
 * - Providing a typed D3 selection helper
 *
 * @param onResize - Optional callback invoked when the container's dimensions change.
 *                   Receives the new width and height as arguments.
 * @returns An object containing the SVG ref and a D3 selection accessor.
 *
 * @example
 * ```tsx
 * const { svgRef, getSelection } = useD3((width, height) => {
 *   // Re-render visualization with new dimensions
 * });
 *
 * return <svg ref={svgRef} />;
 * ```
 */
export function useD3(
  onResize?: (width: number, height: number) => void
): UseD3Result {
  const svgRef = useRef<SVGSVGElement | null>(null);

  // Set up ResizeObserver for responsive behavior
  useEffect(() => {
    const svgElement = svgRef.current;
    if (!svgElement || !onResize) {
      return;
    }

    const resizeObserver = new ResizeObserver((entries) => {
      for (const entry of entries) {
        const { width, height } = entry.contentRect;
        if (width > 0 && height > 0) {
          onResize(width, height);
        }
      }
    });

    resizeObserver.observe(svgElement);

    return () => {
      resizeObserver.disconnect();
    };
  }, [onResize]);

  // Cleanup D3 content on unmount
  useEffect(() => {
    return () => {
      const svgElement = svgRef.current;
      if (svgElement) {
        d3.select(svgElement).selectAll('*').remove();
      }
    };
  }, []);

  const getSelection = useCallback((): d3.Selection<SVGSVGElement, unknown, null, undefined> | null => {
    const svgElement = svgRef.current;
    if (!svgElement) {
      return null;
    }
    return d3.select(svgElement);
  }, []);

  return { svgRef, getSelection };
}
