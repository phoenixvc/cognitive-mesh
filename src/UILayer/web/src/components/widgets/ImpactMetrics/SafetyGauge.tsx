'use client';

import React, { useEffect, useCallback } from 'react';
import * as d3 from 'd3';
import { useD3 } from '@/lib/visualizations/useD3';

interface SafetyGaugeProps {
  /** Safety score on a 0-100 scale. */
  score: number;
  /** Label displayed below the gauge. */
  label?: string;
}

function gaugeColor(score: number): string {
  if (score >= 75) return '#22c55e';
  if (score >= 50) return '#3b82f6';
  if (score >= 25) return '#f59e0b';
  return '#ef4444';
}

/**
 * D3 half-circle gauge for a psychological safety score (0-100).
 */
export default function SafetyGauge({ score, label }: SafetyGaugeProps) {
  const render = useCallback(
    (width: number, height: number) => {
      const svg = d3.select(svgRef.current);
      if (!svg.node()) return;
      svg.selectAll('*').remove();

      const size = Math.min(width, height);
      const cx = width / 2;
      const cy = height * 0.6;
      const outerRadius = size * 0.42;
      const innerRadius = outerRadius * 0.72;

      const startAngle = -Math.PI / 2;
      const endAngle = Math.PI / 2;
      const range = endAngle - startAngle;
      const clampedScore = Math.max(0, Math.min(100, score));
      const valueAngle = startAngle + (clampedScore / 100) * range;

      const arc = d3.arc<unknown>().innerRadius(innerRadius).outerRadius(outerRadius).cornerRadius(4);

      // Background arc
      svg
        .append('path')
        .attr('transform', `translate(${cx},${cy})`)
        .attr('d', arc({ startAngle, endAngle } as never) as string)
        .attr('fill', 'rgba(255,255,255,0.1)');

      // Value arc
      svg
        .append('path')
        .attr('transform', `translate(${cx},${cy})`)
        .attr('d', arc({ startAngle, endAngle: valueAngle } as never) as string)
        .attr('fill', gaugeColor(clampedScore));

      // Center text
      svg
        .append('text')
        .attr('x', cx)
        .attr('y', cy - 2)
        .attr('text-anchor', 'middle')
        .attr('dominant-baseline', 'auto')
        .attr('fill', 'white')
        .attr('font-size', size * 0.15)
        .attr('font-weight', '700')
        .text(Math.round(clampedScore));

      if (label) {
        svg
          .append('text')
          .attr('x', cx)
          .attr('y', cy + size * 0.12)
          .attr('text-anchor', 'middle')
          .attr('fill', 'rgb(156,163,175)')
          .attr('font-size', size * 0.06)
          .text(label);
      }
    },
    [score, label],
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
      className="h-40 w-full"
      role="img"
      aria-label={`Safety gauge: ${Math.round(score)} out of 100${label ? ` — ${label}` : ''}`}
    />
  );
}
