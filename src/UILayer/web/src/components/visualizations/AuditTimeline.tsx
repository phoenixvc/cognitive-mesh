import React, { useEffect, useCallback, useMemo, useRef, useState } from 'react';
import * as d3 from 'd3';
import { AuditEvent, VisualizationTheme, SeverityColorMap } from '../types/visualization';
import { useD3 } from '../hooks/useD3';
import { defaultTheme } from '../themes/defaultTheme';

/**
 * Props for the AuditTimeline component.
 */
export interface AuditTimelineProps {
  /** Array of audit events to plot on the timeline. */
  events: AuditEvent[];
  /** Width of the SVG canvas in pixels. */
  width: number;
  /** Height of the SVG canvas in pixels. */
  height: number;
  /** Callback fired when a user clicks on an event dot. */
  onEventClick?: (event: AuditEvent) => void;
  /** Visualization theme for light/dark mode support. */
  theme?: VisualizationTheme;
}

/** Mapping from severity level to dot color. */
const SEVERITY_COLORS: SeverityColorMap = {
  info: '#3b82f6',
  warning: '#f59e0b',
  error: '#ef4444',
  critical: '#8b5cf6',
};

/** Padding around the chart area in pixels. */
const MARGIN = { top: 20, right: 30, bottom: 40, left: 50 };

/** Radius of event dots. */
const DOT_RADIUS = 6;

/** Radius of event dots on hover. */
const DOT_HOVER_RADIUS = 9;

/**
 * D3.js-powered audit timeline visualization.
 *
 * Renders audit events along a linear time scale on the X axis. Events are
 * represented as colored dots positioned by timestamp, with color encoding
 * derived from the event severity level:
 * - info = blue
 * - warning = amber
 * - error = red
 * - critical = purple
 *
 * Supports zoom and pan via D3 zoom behavior, hover tooltips with event
 * details, responsive resizing via ResizeObserver, and dark mode theming.
 *
 * Accessibility: The SVG element carries `role="img"` and an `aria-label`
 * summarizing the timeline. A hidden text block provides a screen-reader
 * description of all events.
 */
const AuditTimeline: React.FC<AuditTimelineProps> = ({
  events,
  width,
  height,
  onEventClick,
  theme = defaultTheme,
}) => {
  const [dimensions, setDimensions] = useState({ width, height });
  const tooltipRef = useRef<HTMLDivElement | null>(null);

  const handleResize = useCallback((newWidth: number, newHeight: number) => {
    setDimensions({ width: newWidth, height: newHeight });
  }, []);

  const { svgRef, getSelection } = useD3(handleResize);

  // Update dimensions when props change
  useEffect(() => {
    setDimensions({ width, height });
  }, [width, height]);

  /** Screen reader summary of events by severity. */
  const screenReaderSummary = useMemo(() => {
    const counts = events.reduce(
      (acc, e) => {
        acc[e.severity] = (acc[e.severity] || 0) + 1;
        return acc;
      },
      {} as Record<string, number>
    );
    const parts = Object.entries(counts)
      .map(([severity, count]) => `${count} ${severity}`)
      .join(', ');
    return `Audit timeline with ${events.length} events: ${parts}.`;
  }, [events]);

  // Main D3 render effect
  useEffect(() => {
    const svg = getSelection();
    if (!svg || events.length === 0) {
      return;
    }

    const { width: w, height: h } = dimensions;
    const innerWidth = w - MARGIN.left - MARGIN.right;
    const innerHeight = h - MARGIN.top - MARGIN.bottom;

    if (innerWidth <= 0 || innerHeight <= 0) {
      return;
    }

    // Clear previous render
    svg.selectAll('*').remove();

    // Background
    svg
      .append('rect')
      .attr('width', w)
      .attr('height', h)
      .attr('fill', theme.background);

    // Chart group
    const g = svg
      .append('g')
      .attr('transform', `translate(${MARGIN.left},${MARGIN.top})`);

    // Clip path to contain zoomed content
    svg
      .append('defs')
      .append('clipPath')
      .attr('id', 'audit-timeline-clip')
      .append('rect')
      .attr('width', innerWidth)
      .attr('height', innerHeight);

    // Scales
    const xExtent = d3.extent(events, (d) => d.timestamp) as [Date, Date];
    const xScale = d3.scaleTime().domain(xExtent).range([0, innerWidth]).nice();

    // Y axis: spread events by severity for visual separation
    const severityOrder: AuditEvent['severity'][] = ['info', 'warning', 'error', 'critical'];
    const yScale = d3
      .scaleBand<string>()
      .domain(severityOrder)
      .range([innerHeight, 0])
      .padding(0.3);

    // Axes
    const xAxis = d3.axisBottom(xScale).ticks(6).tickFormat(d3.timeFormat('%b %d %H:%M') as (domainValue: Date | d3.NumberValue, index: number) => string);
    const xAxisGroup = g
      .append('g')
      .attr('transform', `translate(0,${innerHeight})`)
      .call(xAxis as any);

    xAxisGroup.selectAll('text').attr('fill', theme.text).style('font-size', '11px');
    xAxisGroup.selectAll('line').attr('stroke', theme.grid);
    xAxisGroup.select('.domain').attr('stroke', theme.grid);

    const yAxis = d3.axisLeft(yScale);
    const yAxisGroup = g.append('g').call(yAxis as any);

    yAxisGroup.selectAll('text').attr('fill', theme.text).style('font-size', '11px').style('text-transform', 'capitalize');
    yAxisGroup.selectAll('line').attr('stroke', theme.grid);
    yAxisGroup.select('.domain').attr('stroke', theme.grid);

    // Grid lines
    g.append('g')
      .attr('class', 'grid')
      .selectAll('line')
      .data(xScale.ticks(6))
      .enter()
      .append('line')
      .attr('x1', (d) => xScale(d))
      .attr('x2', (d) => xScale(d))
      .attr('y1', 0)
      .attr('y2', innerHeight)
      .attr('stroke', theme.grid)
      .attr('stroke-opacity', 0.5)
      .attr('stroke-dasharray', '3,3');

    // Event dots container (clipped)
    const dotsGroup = g
      .append('g')
      .attr('clip-path', 'url(#audit-timeline-clip)');

    // Event dots
    const dots = dotsGroup
      .selectAll<SVGCircleElement, AuditEvent>('circle.event-dot')
      .data(events, (d) => d.id)
      .enter()
      .append('circle')
      .attr('class', 'event-dot')
      .attr('cx', (d) => xScale(d.timestamp))
      .attr('cy', (d) => (yScale(d.severity) ?? 0) + yScale.bandwidth() / 2)
      .attr('r', DOT_RADIUS)
      .attr('fill', (d) => SEVERITY_COLORS[d.severity])
      .attr('stroke', theme.background)
      .attr('stroke-width', 1.5)
      .style('cursor', 'pointer')
      .attr('tabindex', '0')
      .attr('role', 'button')
      .attr('aria-label', (d) => `${d.severity} event: ${d.title} at ${d.timestamp.toLocaleString()}`);

    // Tooltip container
    const tooltip = d3
      .select(tooltipRef.current)
      .style('position', 'absolute')
      .style('pointer-events', 'none')
      .style('opacity', 0)
      .style('background', theme.background)
      .style('color', theme.text)
      .style('border', `1px solid ${theme.grid}`)
      .style('border-radius', '6px')
      .style('padding', '8px 12px')
      .style('font-size', '12px')
      .style('box-shadow', '0 2px 8px rgba(0,0,0,0.15)')
      .style('z-index', '1000')
      .style('max-width', '280px');

    // Hover interactions
    dots
      .on('mouseenter', function (mouseEvent: MouseEvent, d: AuditEvent) {
        d3.select(this)
          .transition()
          .duration(150)
          .attr('r', DOT_HOVER_RADIUS);

        tooltip
          .html(
            `<strong>${d.title}</strong><br/>` +
            `<span style="color:${SEVERITY_COLORS[d.severity]}; text-transform:capitalize;">${d.severity}</span><br/>` +
            `<small>${d.timestamp.toLocaleString()}</small><br/>` +
            `<span>${d.description}</span>` +
            (d.agentId ? `<br/><small>Agent: ${d.agentId}</small>` : '')
          )
          .style('left', `${mouseEvent.offsetX + 15}px`)
          .style('top', `${mouseEvent.offsetY - 10}px`)
          .transition()
          .duration(200)
          .style('opacity', 1);
      })
      .on('mouseleave', function () {
        d3.select(this)
          .transition()
          .duration(150)
          .attr('r', DOT_RADIUS);

        tooltip.transition().duration(200).style('opacity', 0);
      })
      .on('click', function (_mouseEvent: MouseEvent, d: AuditEvent) {
        if (onEventClick) {
          onEventClick(d);
        }
      });

    // Zoom behavior
    const zoom = d3
      .zoom<SVGSVGElement, unknown>()
      .scaleExtent([1, 10])
      .translateExtent([
        [0, 0],
        [w, h],
      ])
      .extent([
        [0, 0],
        [w, h],
      ])
      .on('zoom', (zoomEvent) => {
        const newXScale = zoomEvent.transform.rescaleX(xScale);

        // Update X axis
        xAxisGroup.call(
          d3.axisBottom(newXScale).ticks(6).tickFormat(d3.timeFormat('%b %d %H:%M') as any) as any
        );
        xAxisGroup.selectAll('text').attr('fill', theme.text);
        xAxisGroup.selectAll('line').attr('stroke', theme.grid);
        xAxisGroup.select('.domain').attr('stroke', theme.grid);

        // Update dots
        dots.attr('cx', (d) => newXScale(d.timestamp));

        // Update grid lines
        g.selectAll('.grid line').remove();
        g.select('.grid')
          .selectAll('line')
          .data(newXScale.ticks(6))
          .enter()
          .append('line')
          .attr('x1', (d) => newXScale(d))
          .attr('x2', (d) => newXScale(d))
          .attr('y1', 0)
          .attr('y2', innerHeight)
          .attr('stroke', theme.grid)
          .attr('stroke-opacity', 0.5)
          .attr('stroke-dasharray', '3,3');
      });

    svg.call(zoom);
  }, [events, dimensions, theme, onEventClick, getSelection]);

  return (
    <div style={{ position: 'relative', display: 'inline-block' }}>
      <svg
        ref={svgRef}
        width={dimensions.width}
        height={dimensions.height}
        role="img"
        aria-label={`Audit timeline visualization showing ${events.length} events over time, color-coded by severity.`}
        style={{ display: 'block' }}
      >
        {/* Screen reader text */}
        <title>Audit Event Timeline</title>
        <desc>{screenReaderSummary}</desc>
      </svg>
      <div ref={tooltipRef} aria-hidden="true" />
    </div>
  );
};

export default AuditTimeline;
