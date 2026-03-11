'use client';

import React, { useEffect, useCallback } from 'react';
import * as d3 from 'd3';
import { useD3 } from '@/lib/visualizations/useD3';
import type { PhaseAuditEntry } from '../types';

interface BurndownChartProps {
  totalPhases: number;
  auditEntries: PhaseAuditEntry[];
}

/**
 * D3 burndown chart showing Cognitive Sandwich process progress over time.
 * Plots completed phases against the timeline derived from audit entries.
 */
export default function BurndownChart({ totalPhases, auditEntries }: BurndownChartProps) {
  const render = useCallback(
    (width: number, height: number) => {
      const svg = d3.select(svgRef.current);
      if (!svg.node()) return;
      svg.selectAll('*').remove();

      if (auditEntries.length === 0 || totalPhases === 0) {
        svg.append('text').attr('x', width / 2).attr('y', height / 2).attr('text-anchor', 'middle').attr('fill', 'rgb(107,114,128)').attr('font-size', 12).text('No burndown data available.');
        return;
      }

      const margin = { top: 16, right: 16, bottom: 32, left: 40 };
      const innerW = width - margin.left - margin.right;
      const innerH = height - margin.top - margin.bottom;

      // Build burndown data: remaining phases over time
      const sorted = [...auditEntries].sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
      let remaining = totalPhases;
      const points = sorted.map((e) => {
        if (e.eventType.includes('Completed') || e.eventType.includes('Transition')) {
          remaining = Math.max(0, remaining - 1);
        }
        return { date: new Date(e.timestamp), remaining };
      });

      // Add start point
      points.unshift({ date: new Date(sorted[0].timestamp), remaining: totalPhases });

      const dates = points.map((p) => p.date);
      const x = d3.scaleTime().domain(d3.extent(dates) as [Date, Date]).range([0, innerW]);
      const y = d3.scaleLinear().domain([0, totalPhases]).range([innerH, 0]);

      const g = svg.append('g').attr('transform', `translate(${margin.left},${margin.top})`);

      // Axes
      g.append('g').attr('transform', `translate(0,${innerH})`).call(d3.axisBottom(x).ticks(5).tickFormat(d3.timeFormat('%b %d') as (d: Date | d3.NumberValue) => string)).selectAll('text').attr('fill', 'rgb(156,163,175)').attr('font-size', 10);
      g.append('g').call(d3.axisLeft(y).ticks(totalPhases)).selectAll('text').attr('fill', 'rgb(156,163,175)').attr('font-size', 10);
      g.selectAll('.domain, line').attr('stroke', 'rgba(255,255,255,0.15)');

      // Ideal line
      if (points.length >= 2) {
        g.append('line')
          .attr('x1', x(dates[0]))
          .attr('y1', y(totalPhases))
          .attr('x2', x(dates[dates.length - 1]))
          .attr('y2', y(0))
          .attr('stroke', 'rgba(255,255,255,0.15)')
          .attr('stroke-dasharray', '5,5');
      }

      // Actual burndown line
      const line = d3.line<{ date: Date; remaining: number }>().x((d) => x(d.date)).y((d) => y(d.remaining)).curve(d3.curveStepAfter);
      g.append('path').datum(points).attr('fill', 'none').attr('stroke', '#f59e0b').attr('stroke-width', 2).attr('d', line);

      g.selectAll('circle').data(points).join('circle').attr('cx', (d) => x(d.date)).attr('cy', (d) => y(d.remaining)).attr('r', 3).attr('fill', '#f59e0b');
    },
    [totalPhases, auditEntries],
  );

  const { svgRef } = useD3(render);

  useEffect(() => {
    const el = svgRef.current;
    if (el) {
      const { width, height } = el.getBoundingClientRect();
      if (width > 0 && height > 0) render(width, height);
    }
  }, [render, svgRef]);

  return <svg ref={svgRef} className="h-56 w-full" role="img" aria-label="Process burndown chart" />;
}
