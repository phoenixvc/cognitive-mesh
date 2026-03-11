'use client';

import React, { useEffect, useCallback } from 'react';
import * as d3 from 'd3';
import { useD3 } from '@/lib/visualizations/useD3';
import type { SpectrumHistoryEntry } from '../types';

interface BalanceHistoryProps {
  dimension: string;
  history: SpectrumHistoryEntry[];
}

/**
 * D3 line chart showing historical balance adjustments for a given dimension.
 */
export default function BalanceHistory({ dimension, history }: BalanceHistoryProps) {
  const render = useCallback(
    (width: number, height: number) => {
      const svg = d3.select(svgRef.current);
      if (!svg.node()) return;
      svg.selectAll('*').remove();

      if (history.length === 0) {
        svg
          .append('text')
          .attr('x', width / 2)
          .attr('y', height / 2)
          .attr('text-anchor', 'middle')
          .attr('fill', 'rgb(107,114,128)')
          .attr('font-size', 12)
          .text(`No history for ${dimension}`);
        return;
      }

      const margin = { top: 16, right: 16, bottom: 32, left: 40 };
      const innerW = width - margin.left - margin.right;
      const innerH = height - margin.top - margin.bottom;

      const dates = history.map((h) => new Date(h.timestamp));
      const x = d3
        .scaleTime()
        .domain(d3.extent(dates) as [Date, Date])
        .range([0, innerW]);
      const y = d3.scaleLinear().domain([0, 1]).range([innerH, 0]);

      const g = svg.append('g').attr('transform', `translate(${margin.left},${margin.top})`);

      // Axes
      g.append('g')
        .attr('transform', `translate(0,${innerH})`)
        .call(d3.axisBottom(x).ticks(5).tickFormat(d3.timeFormat('%b %d') as (d: Date | d3.NumberValue) => string))
        .selectAll('text')
        .attr('fill', 'rgb(156,163,175)')
        .attr('font-size', 10);
      g.append('g')
        .call(d3.axisLeft(y).ticks(5))
        .selectAll('text')
        .attr('fill', 'rgb(156,163,175)')
        .attr('font-size', 10);
      g.selectAll('.domain, line').attr('stroke', 'rgba(255,255,255,0.15)');

      // Line
      const line = d3
        .line<SpectrumHistoryEntry>()
        .x((d) => x(new Date(d.timestamp)))
        .y((d) => y(d.value))
        .curve(d3.curveMonotoneX);

      g.append('path')
        .datum(history)
        .attr('fill', 'none')
        .attr('stroke', '#3b82f6')
        .attr('stroke-width', 2)
        .attr('d', line);

      // Dots
      g.selectAll('circle')
        .data(history)
        .join('circle')
        .attr('cx', (d) => x(new Date(d.timestamp)))
        .attr('cy', (d) => y(d.value))
        .attr('r', 3)
        .attr('fill', '#3b82f6');
    },
    [dimension, history],
  );

  const { svgRef } = useD3(render);

  useEffect(() => {
    const el = svgRef.current;
    if (el) {
      const { width, height } = el.getBoundingClientRect();
      if (width > 0 && height > 0) render(width, height);
    }
  }, [render, svgRef]);

  return (
    <svg
      ref={svgRef}
      className="h-48 w-full"
      role="img"
      aria-label={`Balance history chart for ${dimension}`}
    />
  );
}
