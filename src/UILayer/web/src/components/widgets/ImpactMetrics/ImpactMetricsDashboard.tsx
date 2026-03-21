'use client';

import React, { useState, useEffect, useCallback } from 'react';
import SafetyGauge from './SafetyGauge';
import ImpactRadar from './ImpactRadar';
import ImpactTimeline from './ImpactTimeline';
import { getImpactReport, getResistancePatterns } from '../api';
import type { ImpactReport, ResistanceIndicator } from '../types';

interface ImpactMetricsDashboardProps {
  tenantId?: string;
}

export default function ImpactMetricsDashboard({ tenantId = 'default-tenant' }: ImpactMetricsDashboardProps) {
  const [report, setReport] = useState<ImpactReport | null>(null);
  const [resistance, setResistance] = useState<ResistanceIndicator[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [rep, res] = await Promise.all([
        getImpactReport(tenantId),
        getResistancePatterns(tenantId),
      ]);
      setReport(rep);
      setResistance(res);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load impact metrics.');
    } finally {
      setLoading(false);
    }
  }, [tenantId]);

  useEffect(() => { void fetchData(); }, [fetchData]);

  if (loading) {
    return (
      <div className="space-y-4" aria-busy="true" aria-label="Loading Impact Metrics Dashboard">
        <div className="h-8 w-56 animate-pulse rounded bg-white/10" />
        <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
          {[1, 2, 3].map((k) => <div key={k} className="h-40 animate-pulse rounded-lg bg-white/5" />)}
        </div>
        <div className="h-64 animate-pulse rounded-lg bg-white/5" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-6" role="alert">
        <h2 className="text-lg font-semibold text-red-400">Error loading impact metrics</h2>
        <p className="mt-1 text-sm text-red-300">{error}</p>
        <button onClick={() => void fetchData()} className="mt-3 rounded bg-red-500/20 px-4 py-1.5 text-sm font-medium text-red-300 hover:bg-red-500/30">Retry</button>
      </div>
    );
  }

  const radarLabels: string[] = [];
  const radarValues: number[] = [];
  if (report) {
    radarLabels.push('Safety', 'Alignment', 'Adoption', 'Overall');
    radarValues.push(report.safetyScore, report.alignmentScore * 100, report.adoptionRate * 100, report.overallImpactScore);
  }

  return (
    <div className="space-y-6" role="region" aria-label="Impact Metrics Dashboard">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-white">Impact Metrics</h1>
          {report && <p className="text-xs text-gray-400">Report generated {new Date(report.generatedAt).toLocaleDateString()}</p>}
        </div>
        <button onClick={() => void fetchData()} className="rounded bg-white/10 px-3 py-1.5 text-xs font-medium text-gray-300 hover:bg-white/15">Refresh</button>
      </div>

      {report && (
        <>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
            <div className="rounded-lg border border-white/10 bg-white/5 p-4">
              <SafetyGauge score={report.safetyScore} label="Safety" />
            </div>
            <div className="rounded-lg border border-white/10 bg-white/5 p-4 text-center">
              <p className="text-xs text-gray-400">Alignment</p>
              <p className="text-3xl font-bold text-white">{(report.alignmentScore * 100).toFixed(0)}%</p>
            </div>
            <div className="rounded-lg border border-white/10 bg-white/5 p-4 text-center">
              <p className="text-xs text-gray-400">Adoption Rate</p>
              <p className="text-3xl font-bold text-white">{(report.adoptionRate * 100).toFixed(0)}%</p>
            </div>
            <div className="rounded-lg border border-white/10 bg-white/5 p-4 text-center">
              <p className="text-xs text-gray-400">Overall Impact</p>
              <p className="text-3xl font-bold text-white">{report.overallImpactScore.toFixed(0)}</p>
            </div>
          </div>

          <div className="rounded-lg border border-white/10 bg-white/5 p-4">
            <h2 className="mb-3 text-sm font-semibold text-gray-300">Impact Dimensions</h2>
            <ImpactRadar labels={radarLabels} values={radarValues} />
          </div>

          {report.recommendations.length > 0 && (
            <div className="rounded-lg border border-white/10 bg-white/5 p-4">
              <h2 className="mb-2 text-sm font-semibold text-gray-300">Recommendations</h2>
              <ul className="space-y-1">
                {report.recommendations.map((r, i) => <li key={i} className="text-xs text-gray-300">{r}</li>)}
              </ul>
            </div>
          )}
        </>
      )}

      <div className="rounded-lg border border-white/10 bg-white/5 p-4">
        <h2 className="mb-3 text-sm font-semibold text-gray-300">Resistance Patterns</h2>
        <ImpactTimeline indicators={resistance} />
      </div>
    </div>
  );
}
