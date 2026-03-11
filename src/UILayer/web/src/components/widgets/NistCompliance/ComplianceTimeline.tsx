'use client';

import React, { useEffect, useCallback } from 'react';
import * as d3 from 'd3';
import { useD3 } from '@/lib/visualizations/useD3';
import type { NISTAuditEntry } from '../types';

interface ComplianceTimelineProps {
  entries: NISTAuditEntry[];
}

/**
 * D3 timeline of NIST compliance audit events.
 */
export default function ComplianceTimeline({ entries }: ComplianceTimelineProps) {
  const render = useCallback(
    (width: number, height: number) => {
      const svg = d3.select(svgRef.current);
      if (!svg.node()) return;
      svg.selectAll('*').remove();

      if (entries.length === 0) {
        svg
          .append('text')
          .attr('x', width / 2)
          .attr('y', height / 2)
          .attr('text-anchor', 'middle')
          .attr('fill', 'rgb(107,114,128)')
          .attr('font-size', 12)
          .text('No audit events to display.');
        return;
      }

      const margin = { top: 20, right: 20, bottom: 40, left: 60 };
      const innerW = width - margin.left - margin.right;
      const innerH = height - margin.top - margin.bottom;

      const dates = entries.map((e) => new Date(e.performedAt));
      const xExtent = d3.extent(dates) as [Date, Date];
      const x = d3.scaleTime().domain(xExtent).range([0, innerW]).nice();

      const actions = [...new Set(entries.map((e) => e.action))];
      const y = d3.scaleBand<string>().domain(actions).range([0, innerH]).padding(0.4);

      const g = svg.append('g').attr('transform', `translate(${margin.left},${margin.top})`);

      // X axis
      g.append('g')
        .attr('transform', `translate(0,${innerH})`)
        .call(d3.axisBottom(x).ticks(5).tickFormat(d3.timeFormat('%b %d') as (d: Date | d3.NumberValue) => string))
        .selectAll('text')
        .attr('fill', 'rgb(156,163,175)')
        .attr('font-size', 10);
      g.selectAll('.domain, line').attr('stroke', 'rgba(255,255,255,0.15)');

      // Y axis
      g.append('g')
        .call(d3.axisLeft(y))
        .selectAll('text')
        .attr('fill', 'rgb(156,163,175)')
        .attr('font-size', 10);

      // Dots
      const color = d3.scaleOrdinal(d3.schemeTableau10).domain(actions);
      g.selectAll('circle')
        .data(entries)
        .join('circle')
        .attr('cx', (d) => x(new Date(d.performedAt)))
        .attr('cy', (d) => (y(d.action) ?? 0) + y.bandwidth() / 2)
        .attr('r', 5)
        .attr('fill', (d) => color(d.action))
        .attr('opacity', 0.85);
    },
    [entries],
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
      className="h-56 w-full"
      role="img"
      aria-label="Compliance audit event timeline"
    />
  );
}
