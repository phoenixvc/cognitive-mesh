'use client';

import React, { useEffect, useCallback } from 'react';
import * as d3 from 'd3';
import { useD3 } from '@/lib/visualizations/useD3';

interface ImpactRadarProps {
  /** Dimension labels. */
  labels: string[];
  /** Corresponding values (0-100 scale). */
  values: number[];
}

/**
 * D3 radar chart for impact metric dimensions (safety, alignment, adoption, etc.).
 */
export default function ImpactRadar({ labels, values }: ImpactRadarProps) {
  const render = useCallback(
    (width: number, height: number) => {
      const svg = d3.select(svgRef.current);
      if (!svg.node()) return;
      svg.selectAll('*').remove();

      if (labels.length === 0) {
        svg
          .append('text')
          .attr('x', width / 2)
          .attr('y', height / 2)
          .attr('text-anchor', 'middle')
          .attr('fill', 'rgb(107,114,128)')
          .attr('font-size', 12)
          .text('No impact data available.');
        return;
      }

      const cx = width / 2;
      const cy = height / 2;
      const maxRadius = Math.min(cx, cy) - 36;
      const numAxes = labels.length;
      const angleSlice = (Math.PI * 2) / numAxes;
      const rScale = d3.scaleLinear().domain([0, 100]).range([0, maxRadius]);

      const g = svg.append('g').attr('transform', `translate(${cx},${cy})`);

      // Grid
      const levels = 4;
      for (let lvl = 1; lvl <= levels; lvl++) {
        const r = (maxRadius / levels) * lvl;
        g.append('circle')
          .attr('r', r)
          .attr('fill', 'none')
          .attr('stroke', 'rgba(255,255,255,0.08)')
          .attr('stroke-dasharray', '2,3');
      }

      // Axes + labels
      labels.forEach((label, i) => {
        const angle = angleSlice * i - Math.PI / 2;
        g.append('line')
          .attr('x2', maxRadius * Math.cos(angle))
          .attr('y2', maxRadius * Math.sin(angle))
          .attr('stroke', 'rgba(255,255,255,0.08)');

        const labelR = maxRadius + 18;
        g.append('text')
          .attr('x', labelR * Math.cos(angle))
          .attr('y', labelR * Math.sin(angle))
          .attr('text-anchor', 'middle')
          .attr('dominant-baseline', 'central')
          .attr('fill', 'rgb(156,163,175)')
          .attr('font-size', 9)
          .text(label);
      });

      // Polygon
      const lineGen = d3
        .lineRadial<number>()
        .angle((_, i) => angleSlice * i)
        .radius((d) => rScale(d))
        .curve(d3.curveLinearClosed);

      g.append('path')
        .datum(values)
        .attr('d', lineGen)
        .attr('fill', 'rgba(34,197,94,0.2)')
        .attr('stroke', '#22c55e')
        .attr('stroke-width', 2);

      values.forEach((val, i) => {
        const angle = angleSlice * i - Math.PI / 2;
        g.append('circle')
          .attr('cx', rScale(val) * Math.cos(angle))
          .attr('cy', rScale(val) * Math.sin(angle))
          .attr('r', 3.5)
          .attr('fill', '#22c55e');
      });
    },
    [labels, values],
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
      aria-label="Impact metrics radar chart"
    />
  );
}
