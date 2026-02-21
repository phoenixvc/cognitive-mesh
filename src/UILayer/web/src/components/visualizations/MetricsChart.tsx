import React, { useEffect, useCallback, useMemo, useRef, useState } from 'react';
import * as d3 from 'd3';
import { MetricDataPoint, VisualizationTheme, ThresholdLine } from '../types/visualization';
import { useD3 } from '../hooks/useD3';
import { defaultTheme } from '../themes/defaultTheme';

/**
 * Props for the MetricsChart component.
 */
export interface MetricsChartProps {
  /** Array of time-series data points to plot. */
  data: MetricDataPoint[];
  /** Display name for the metric (shown as chart title and in the legend). */
  metricName: string;
  /** Width of the SVG canvas in pixels. */
  width: number;
  /** Height of the SVG canvas in pixels. */
  height: number;
  /** Optional horizontal threshold lines drawn as dashed lines. */
  thresholds?: ThresholdLine[];
  /** Visualization theme for light/dark mode support. */
  theme?: VisualizationTheme;
}

/** Padding around the chart area in pixels. */
const MARGIN = { top: 30, right: 30, bottom: 50, left: 60 };

/** Duration in milliseconds for line and area transition animations. */
const TRANSITION_DURATION = 750;

/** Radius for data point dots. */
const POINT_RADIUS = 3;

/** Radius for data point dots on hover. */
const POINT_HOVER_RADIUS = 6;

/**
 * Real-time metrics line chart powered by D3.js.
 *
 * Renders time-series data as a smooth interpolated line with optional
 * threshold indicator lines (horizontal dashed). Supports animated transitions
 * when data updates, hover tooltips showing exact values, and a legend
 * component displaying the metric name and any threshold labels.
 *
 * The X axis is time-based and the Y axis is value-based, both auto-scaled
 * from the data domain. Dark mode is supported via the theme prop.
 */
const MetricsChart: React.FC<MetricsChartProps> = ({
  data,
  metricName,
  width,
  height,
  thresholds = [],
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

  /** Screen reader summary of the metric data. */
  const screenReaderSummary = useMemo(() => {
    if (data.length === 0) {
      return `Metrics chart for ${metricName} with no data points.`;
    }
    const values = data.map((d) => d.value);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const avg = values.reduce((a, b) => a + b, 0) / values.length;
    return `Metrics chart for ${metricName}: ${data.length} data points, range ${min.toFixed(1)} to ${max.toFixed(1)}, average ${avg.toFixed(1)}.`;
  }, [data, metricName]);

  // Main D3 render effect
  useEffect(() => {
    const svg = getSelection();
    if (!svg || data.length === 0) {
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

    // Scales
    const xExtent = d3.extent(data, (d) => d.timestamp) as [Date, Date];
    const xScale = d3.scaleTime().domain(xExtent).range([0, innerWidth]).nice();

    const yMin = d3.min(data, (d) => d.value) ?? 0;
    const yMax = d3.max(data, (d) => d.value) ?? 100;
    const yPadding = (yMax - yMin) * 0.1 || 10;
    const yScale = d3
      .scaleLinear()
      .domain([Math.max(0, yMin - yPadding), yMax + yPadding])
      .range([innerHeight, 0])
      .nice();

    // Axes
    const xAxis = d3.axisBottom(xScale).ticks(6).tickFormat(d3.timeFormat('%b %d %H:%M') as (domainValue: Date | d3.NumberValue, index: number) => string);
    const xAxisGroup = g
      .append('g')
      .attr('transform', `translate(0,${innerHeight})`)
      .call(xAxis as any);

    xAxisGroup.selectAll('text').attr('fill', theme.text).style('font-size', '11px');
    xAxisGroup.selectAll('line').attr('stroke', theme.grid);
    xAxisGroup.select('.domain').attr('stroke', theme.grid);

    const yAxis = d3.axisLeft(yScale).ticks(6);
    const yAxisGroup = g.append('g').call(yAxis as any);

    yAxisGroup.selectAll('text').attr('fill', theme.text).style('font-size', '11px');
    yAxisGroup.selectAll('line').attr('stroke', theme.grid);
    yAxisGroup.select('.domain').attr('stroke', theme.grid);

    // Horizontal grid lines
    g.append('g')
      .selectAll('line')
      .data(yScale.ticks(6))
      .enter()
      .append('line')
      .attr('x1', 0)
      .attr('x2', innerWidth)
      .attr('y1', (d) => yScale(d))
      .attr('y2', (d) => yScale(d))
      .attr('stroke', theme.grid)
      .attr('stroke-opacity', 0.4)
      .attr('stroke-dasharray', '2,2');

    // Gradient area fill
    const areaGenerator = d3
      .area<MetricDataPoint>()
      .x((d) => xScale(d.timestamp))
      .y0(innerHeight)
      .y1((d) => yScale(d.value))
      .curve(d3.curveMonotoneX);

    const gradientId = 'metrics-area-gradient';
    const defs = svg.append('defs');
    const gradient = defs
      .append('linearGradient')
      .attr('id', gradientId)
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '0%')
      .attr('y2', '100%');

    gradient
      .append('stop')
      .attr('offset', '0%')
      .attr('stop-color', theme.primary)
      .attr('stop-opacity', 0.3);

    gradient
      .append('stop')
      .attr('offset', '100%')
      .attr('stop-color', theme.primary)
      .attr('stop-opacity', 0.02);

    const areaPath = g
      .append('path')
      .datum(data)
      .attr('fill', `url(#${gradientId})`)
      .attr('d', areaGenerator);

    // Animate area entrance
    areaPath
      .attr('opacity', 0)
      .transition()
      .duration(TRANSITION_DURATION)
      .attr('opacity', 1);

    // Line generator with curve interpolation
    const lineGenerator = d3
      .line<MetricDataPoint>()
      .x((d) => xScale(d.timestamp))
      .y((d) => yScale(d.value))
      .curve(d3.curveMonotoneX);

    const linePath = g
      .append('path')
      .datum(data)
      .attr('fill', 'none')
      .attr('stroke', theme.primary)
      .attr('stroke-width', 2.5)
      .attr('d', lineGenerator);

    // Animate line drawing
    const totalLength = (linePath.node() as SVGPathElement)?.getTotalLength() ?? 0;
    linePath
      .attr('stroke-dasharray', `${totalLength} ${totalLength}`)
      .attr('stroke-dashoffset', totalLength)
      .transition()
      .duration(TRANSITION_DURATION)
      .ease(d3.easeLinear)
      .attr('stroke-dashoffset', 0);

    // Data point dots
    const dots = g
      .selectAll<SVGCircleElement, MetricDataPoint>('circle.data-point')
      .data(data)
      .enter()
      .append('circle')
      .attr('class', 'data-point')
      .attr('cx', (d) => xScale(d.timestamp))
      .attr('cy', (d) => yScale(d.value))
      .attr('r', POINT_RADIUS)
      .attr('fill', theme.primary)
      .attr('stroke', theme.background)
      .attr('stroke-width', 1.5)
      .style('cursor', 'pointer');

    // Threshold lines
    thresholds.forEach((threshold) => {
      if (yScale(threshold.value) >= 0 && yScale(threshold.value) <= innerHeight) {
        g.append('line')
          .attr('x1', 0)
          .attr('x2', innerWidth)
          .attr('y1', yScale(threshold.value))
          .attr('y2', yScale(threshold.value))
          .attr('stroke', threshold.color)
          .attr('stroke-width', 1.5)
          .attr('stroke-dasharray', '6,4')
          .attr('opacity', 0.8);

        g.append('text')
          .attr('x', innerWidth - 5)
          .attr('y', yScale(threshold.value) - 6)
          .attr('text-anchor', 'end')
          .attr('fill', threshold.color)
          .style('font-size', '10px')
          .style('font-weight', 'bold')
          .text(threshold.label);
      }
    });

    // Tooltip
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
      .style('z-index', '1000');

    // Hover interactions
    dots
      .on('mouseenter', function (mouseEvent: MouseEvent, d: MetricDataPoint) {
        d3.select(this)
          .transition()
          .duration(150)
          .attr('r', POINT_HOVER_RADIUS);

        tooltip
          .html(
            `<strong>${metricName}</strong><br/>` +
            `Value: <strong>${d.value.toFixed(2)}</strong><br/>` +
            `<small>${d.timestamp.toLocaleString()}</small>` +
            (d.label ? `<br/><em>${d.label}</em>` : '')
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
          .attr('r', POINT_RADIUS);

        tooltip.transition().duration(200).style('opacity', 0);
      });

    // Chart title
    svg
      .append('text')
      .attr('x', w / 2)
      .attr('y', 18)
      .attr('text-anchor', 'middle')
      .attr('fill', theme.text)
      .style('font-size', '14px')
      .style('font-weight', 'bold')
      .text(metricName);

    // Legend
    const legendGroup = g
      .append('g')
      .attr('transform', `translate(0,${innerHeight + 35})`);

    // Metric line legend entry
    legendGroup
      .append('line')
      .attr('x1', 0)
      .attr('x2', 20)
      .attr('y1', 0)
      .attr('y2', 0)
      .attr('stroke', theme.primary)
      .attr('stroke-width', 2.5);

    legendGroup
      .append('text')
      .attr('x', 25)
      .attr('y', 4)
      .attr('fill', theme.text)
      .style('font-size', '11px')
      .text(metricName);

    // Threshold legend entries
    let legendOffset = 25 + metricName.length * 6 + 20;
    thresholds.forEach((threshold) => {
      legendGroup
        .append('line')
        .attr('x1', legendOffset)
        .attr('x2', legendOffset + 20)
        .attr('y1', 0)
        .attr('y2', 0)
        .attr('stroke', threshold.color)
        .attr('stroke-width', 1.5)
        .attr('stroke-dasharray', '6,4');

      legendGroup
        .append('text')
        .attr('x', legendOffset + 25)
        .attr('y', 4)
        .attr('fill', theme.text)
        .style('font-size', '11px')
        .text(threshold.label);

      legendOffset += 25 + threshold.label.length * 6 + 20;
    });
  }, [data, dimensions, theme, metricName, thresholds, getSelection]);

  return (
    <div style={{ position: 'relative', display: 'inline-block' }}>
      <svg
        ref={svgRef}
        width={dimensions.width}
        height={dimensions.height}
        role="img"
        aria-label={`Line chart showing ${metricName} over time.`}
        style={{ display: 'block' }}
      >
        <title>{metricName} Metrics Chart</title>
        <desc>{screenReaderSummary}</desc>
      </svg>
      <div ref={tooltipRef} aria-hidden="true" />
    </div>
  );
};

export default MetricsChart;
