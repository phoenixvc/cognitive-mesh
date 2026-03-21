'use client';

import React, { useEffect, useCallback } from 'react';
import * as d3 from 'd3';
import { useD3 } from '@/lib/visualizations/useD3';

interface ValueRadarChartProps {
  /** Labels for each axis of the radar. */
  axes: string[];
  /** Values for each axis (0-100 scale). */
  values: number[];
}

/**
 * D3 radar / spider chart rendering value dimensions.
 */
export default function ValueRadarChart({ axes, values }: ValueRadarChartProps) {
  const render = useCallback(
    (width: number, height: number) => {
      const svg = d3.select(svgRef.current);
      if (!svg.node()) return;
      svg.selectAll('*').remove();

      if (axes.length === 0) {
        svg
          .append('text')
          .attr('x', width / 2)
          .attr('y', height / 2)
          .attr('text-anchor', 'middle')
          .attr('fill', 'rgb(107,114,128)')
          .attr('font-size', 12)
          .text('No data for radar chart.');
        return;
      }

      const cx = width / 2;
      const cy = height / 2;
      const maxRadius = Math.min(cx, cy) - 40;
      const numAxes = axes.length;
      const angleSlice = (Math.PI * 2) / numAxes;

      const rScale = d3.scaleLinear().domain([0, 100]).range([0, maxRadius]);

      const g = svg.append('g').attr('transform', `translate(${cx},${cy})`);

      // Grid circles
      const levels = 4;
      for (let lvl = 1; lvl <= levels; lvl++) {
        const r = (maxRadius / levels) * lvl;
        g.append('circle')
          .attr('r', r)
          .attr('fill', 'none')
          .attr('stroke', 'rgba(255,255,255,0.1)')
          .attr('stroke-dasharray', '3,3');
      }

      // Axis lines + labels
      axes.forEach((label, i) => {
        const angle = angleSlice * i - Math.PI / 2;
        const xEnd = maxRadius * Math.cos(angle);
        const yEnd = maxRadius * Math.sin(angle);
        g.append('line')
          .attr('x1', 0)
          .attr('y1', 0)
          .attr('x2', xEnd)
          .attr('y2', yEnd)
          .attr('stroke', 'rgba(255,255,255,0.1)');

        const labelDist = maxRadius + 16;
        g.append('text')
          .attr('x', labelDist * Math.cos(angle))
          .attr('y', labelDist * Math.sin(angle))
          .attr('text-anchor', 'middle')
          .attr('dominant-baseline', 'central')
          .attr('fill', 'rgb(156,163,175)')
          .attr('font-size', 10)
          .text(label);
      });

      // Data polygon
      const lineGen = d3
        .lineRadial<number>()
        .angle((_, i) => angleSlice * i)
        .radius((d) => rScale(d))
        .curve(d3.curveLinearClosed);

      g.append('path')
        .datum(values)
        .attr('d', lineGen)
        .attr('fill', 'rgba(59,130,246,0.25)')
        .attr('stroke', '#3b82f6')
        .attr('stroke-width', 2);

      // Data dots
      values.forEach((val, i) => {
        const angle = angleSlice * i - Math.PI / 2;
        g.append('circle')
          .attr('cx', rScale(val) * Math.cos(angle))
          .attr('cy', rScale(val) * Math.sin(angle))
          .attr('r', 4)
          .attr('fill', '#3b82f6');
      });
    },
    [axes, values],
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
      className="h-64 w-full"
      role="img"
      aria-label="Value dimensions radar chart"
    />
  );
}
