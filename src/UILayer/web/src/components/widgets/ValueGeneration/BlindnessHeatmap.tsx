'use client';

import React from 'react';

interface BlindnessHeatmapProps {
  /** Risk score 0-1. */
  riskScore: number;
  /** Identified blind spots. */
  blindSpots: string[];
}

function severityClass(index: number, total: number): string {
  const pct = total > 0 ? index / total : 0;
  if (pct < 0.33) return 'bg-red-500/80';
  if (pct < 0.66) return 'bg-orange-500/70';
  return 'bg-yellow-500/60';
}

/**
 * Heatmap-style visualization of organizational blind spots.
 * Each blind spot is rendered as a tile whose color intensity maps to
 * its relative severity (position in the ordered list from the backend).
 */
export default function BlindnessHeatmap({ riskScore, blindSpots }: BlindnessHeatmapProps) {
  if (blindSpots.length === 0) {
    return <p className="text-sm text-gray-500">No blind spots detected.</p>;
  }

  return (
    <div>
      {/* Risk badge */}
      <div className="mb-3 flex items-center gap-2">
        <span className="text-xs font-medium text-gray-400">Blindness Risk Score</span>
        <span
          className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-bold ${
            riskScore > 0.6
              ? 'bg-red-500/20 text-red-400'
              : riskScore > 0.3
                ? 'bg-yellow-500/20 text-yellow-400'
                : 'bg-green-500/20 text-green-400'
          }`}
        >
          {(riskScore * 100).toFixed(0)}%
        </span>
      </div>

      {/* Blind spot tiles */}
      <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
        {blindSpots.map((spot, i) => (
          <div
            key={i}
            className={`rounded-lg p-3 text-xs text-white ${severityClass(i, blindSpots.length)}`}
          >
            {spot}
          </div>
        ))}
      </div>
    </div>
  );
}
