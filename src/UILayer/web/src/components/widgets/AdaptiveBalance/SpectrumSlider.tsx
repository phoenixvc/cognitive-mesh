'use client';

import React from 'react';
import type { SpectrumDimensionResult } from '../types';

interface SpectrumSliderProps {
  dimensions: SpectrumDimensionResult[];
}

function confidenceColor(value: number): string {
  if (value >= 0.7) return 'bg-emerald-500';
  if (value >= 0.4) return 'bg-yellow-500';
  return 'bg-red-500';
}

/**
 * Visual slider showing balance between autonomy and control for each
 * spectrum dimension. Each dimension is rendered as a horizontal slider
 * with confidence bounds.
 */
export default function SpectrumSlider({ dimensions }: SpectrumSliderProps) {
  if (dimensions.length === 0) {
    return <p className="text-sm text-gray-500">No spectrum dimensions available.</p>;
  }

  return (
    <div className="space-y-4" role="list" aria-label="Spectrum dimensions">
      {dimensions.map((dim) => {
        const pct = dim.value * 100;
        const lowerPct = dim.lowerBound * 100;
        const upperPct = dim.upperBound * 100;
        return (
          <div key={dim.dimension} className="space-y-1" role="listitem">
            <div className="flex items-center justify-between">
              <span className="text-xs font-medium text-gray-300">{dim.dimension}</span>
              <span className="text-xs text-gray-400">{(dim.value * 100).toFixed(0)}%</span>
            </div>
            <div className="relative h-3 rounded-full bg-white/10">
              {/* Confidence band */}
              <div
                className="absolute top-0 h-3 rounded-full bg-blue-500/20"
                style={{ left: `${lowerPct}%`, width: `${upperPct - lowerPct}%` }}
              />
              {/* Current value marker */}
              <div
                className={`absolute top-0 h-3 w-1 rounded-full ${confidenceColor(dim.value)}`}
                style={{ left: `${pct}%` }}
                aria-label={`${dim.dimension}: ${pct.toFixed(0)}%`}
              />
            </div>
            {dim.rationale && (
              <p className="text-xs text-gray-500 italic">{dim.rationale}</p>
            )}
          </div>
        );
      })}
    </div>
  );
}
