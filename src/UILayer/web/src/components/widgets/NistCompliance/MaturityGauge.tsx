'use client';

import React, { useEffect, useCallback } from 'react';
import * as d3 from 'd3';
import { useD3 } from '@/lib/visualizations/useD3';

interface MaturityGaugeProps {
  /** Overall maturity score (0-5 scale). */
  score: number;
  /** Label displayed below the gauge. */
  label?: string;
}

const MATURITY_LEVELS = ['Partial', 'Risk Informed', 'Repeatable', 'Adaptive', 'Optimal'] as const;

function getMaturityLabel(score: number): string {
  if (score < 1) return MATURITY_LEVELS[0];
  if (score < 2) return MATURITY_LEVELS[1];
  if (score < 3) return MATURITY_LEVELS[2];
  if (score < 4) return MATURITY_LEVELS[3];
  return MATURITY_LEVELS[4];
}

function getGaugeColor(score: number): string {
  if (score < 1.5) return '#ef4444';
  if (score < 2.5) return '#f59e0b';
  if (score < 3.5) return '#3b82f6';
  return '#22c55e';
}

/**
 * D3 radial gauge visualizing a NIST maturity score on a 0-5 scale.
 */
export default function MaturityGauge({ score, label }: MaturityGaugeProps) {
  const render = useCallback(
    (width: number, height: number) => {
      const svg = d3.select(svgRef.current);
      if (!svg.node()) return;
      svg.selectAll('*').remove();

      const size = Math.min(width, height);
      const cx = width / 2;
      const cy = height / 2;
      const outerRadius = size * 0.42;
      const innerRadius = outerRadius * 0.75;

      const startAngle = -Math.PI * 0.75;
      const endAngle = Math.PI * 0.75;
      const range = endAngle - startAngle;
      const valueAngle = startAngle + (score / 5) * range;

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
        .attr('fill', getGaugeColor(score));

      // Center text — score
      svg
        .append('text')
        .attr('x', cx)
        .attr('y', cy - 4)
        .attr('text-anchor', 'middle')
        .attr('dominant-baseline', 'auto')
        .attr('fill', 'white')
        .attr('font-size', size * 0.16)
        .attr('font-weight', '700')
        .text(score.toFixed(1));

      // Center text — maturity level
      svg
        .append('text')
        .attr('x', cx)
        .attr('y', cy + size * 0.1)
        .attr('text-anchor', 'middle')
        .attr('dominant-baseline', 'auto')
        .attr('fill', 'rgb(156,163,175)')
        .attr('font-size', size * 0.07)
        .text(getMaturityLabel(score));

      // Label below gauge
      if (label) {
        svg
          .append('text')
          .attr('x', cx)
          .attr('y', cy + size * 0.35)
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
      className="h-48 w-full"
      role="img"
      aria-label={`Maturity gauge: ${score.toFixed(1)} out of 5 — ${getMaturityLabel(score)}`}
    />
  );
}
