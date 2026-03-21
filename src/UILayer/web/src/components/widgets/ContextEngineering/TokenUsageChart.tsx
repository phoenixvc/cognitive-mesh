'use client';

import React from 'react';

interface TokenUsageEntry {
  contextType: string;
  tokensUsed: number;
  maxTokens: number;
}

interface TokenUsageChartProps {
  /** Token usage data broken down by context type. */
  data: TokenUsageEntry[];
}

function getBarColor(ratio: number): string {
  if (ratio < 0.5) return 'bg-green-500';
  if (ratio < 0.75) return 'bg-yellow-500';
  if (ratio < 0.9) return 'bg-orange-500';
  return 'bg-red-500';
}

/**
 * Bar chart showing token consumption by context type.
 */
export default function TokenUsageChart({ data }: TokenUsageChartProps) {
  if (data.length === 0) {
    return <p className="text-sm text-gray-500">No token usage data available.</p>;
  }

  const maxTokens = Math.max(...data.map((d) => d.maxTokens), 1);

  return (
    <div className="space-y-3" role="img" aria-label="Token usage by context type">
      {data.map((entry) => {
        const ratio = entry.tokensUsed / Math.max(entry.maxTokens, 1);
        const widthPct = (entry.tokensUsed / maxTokens) * 100;
        return (
          <div key={entry.contextType} className="space-y-1">
            <div className="flex items-center justify-between text-xs">
              <span className="text-gray-300">{entry.contextType}</span>
              <span className="text-gray-400">
                {entry.tokensUsed.toLocaleString()} / {entry.maxTokens.toLocaleString()}
              </span>
            </div>
            <div className="h-3 w-full rounded-full bg-white/10">
              <div
                className={`h-3 rounded-full transition-all duration-500 ${getBarColor(ratio)}`}
                style={{ width: `${Math.min(widthPct, 100)}%` }}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}
