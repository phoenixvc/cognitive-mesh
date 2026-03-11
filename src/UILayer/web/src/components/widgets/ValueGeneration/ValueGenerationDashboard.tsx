'use client';

import React, { useState, useEffect, useCallback } from 'react';
import ValueRadarChart from './ValueRadarChart';
import BlindnessHeatmap from './BlindnessHeatmap';
import { runValueDiagnostic, detectOrgBlindness } from '../api';
import type { ValueDiagnosticResponse, OrgBlindnessDetectionResponse } from '../types';

interface ValueGenerationDashboardProps {
  targetId?: string;
  targetType?: string;
  organizationId?: string;
  tenantId?: string;
}

/**
 * FE-013: Value Generation Dashboard widget.
 *
 * Displays value diagnostic results with a radar chart of value dimensions,
 * and an organizational blindness heatmap sourced from the
 * ValueGenerationController API.
 */
export default function ValueGenerationDashboard({
  targetId = 'current-user',
  targetType = 'User',
  organizationId = 'default-org',
  tenantId = 'default-tenant',
}: ValueGenerationDashboardProps) {
  const [diagnostic, setDiagnostic] = useState<ValueDiagnosticResponse | null>(null);
  const [blindness, setBlindness] = useState<OrgBlindnessDetectionResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [diag, blind] = await Promise.all([
        runValueDiagnostic(targetId, targetType, tenantId),
        detectOrgBlindness(organizationId, tenantId),
      ]);
      setDiagnostic(diag);
      setBlindness(blind);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to load value generation data.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [targetId, targetType, organizationId, tenantId]);

  useEffect(() => {
    void fetchData();
  }, [fetchData]);

  if (loading) {
    return (
      <div className="space-y-4" aria-busy="true" aria-label="Loading Value Generation Dashboard">
        <div className="h-8 w-56 animate-pulse rounded bg-white/10" />
        <div className="h-64 animate-pulse rounded-lg bg-white/5" />
        <div className="h-48 animate-pulse rounded-lg bg-white/5" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-6" role="alert">
        <h2 className="text-lg font-semibold text-red-400">Error loading value data</h2>
        <p className="mt-1 text-sm text-red-300">{error}</p>
        <button
          onClick={() => void fetchData()}
          className="mt-3 rounded bg-red-500/20 px-4 py-1.5 text-sm font-medium text-red-300 hover:bg-red-500/30"
        >
          Retry
        </button>
      </div>
    );
  }

  // Build radar data from diagnostic
  const radarAxes: string[] = [];
  const radarValues: number[] = [];
  if (diagnostic) {
    radarAxes.push('Score');
    radarValues.push(Math.min(diagnostic.valueScore, 100));
    diagnostic.strengths.forEach((s) => {
      radarAxes.push(s);
      radarValues.push(80); // strengths are inherently high
    });
    diagnostic.developmentOpportunities.forEach((d) => {
      radarAxes.push(d);
      radarValues.push(35); // opportunities are inherently low
    });
  }

  return (
    <div className="space-y-6" role="region" aria-label="Value Generation Dashboard">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-white">Value Generation</h1>
        <button
          onClick={() => void fetchData()}
          className="rounded bg-white/10 px-3 py-1.5 text-xs font-medium text-gray-300 hover:bg-white/15"
        >
          Refresh
        </button>
      </div>

      {/* Value diagnostic summary */}
      {diagnostic && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-2 text-sm font-semibold text-gray-300">Diagnostic Summary</h2>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <div>
              <p className="text-xs text-gray-400">Value Score</p>
              <p className="text-2xl font-bold text-white">{diagnostic.valueScore}</p>
            </div>
            <div>
              <p className="text-xs text-gray-400">Profile</p>
              <p className="text-lg font-semibold text-blue-400">{diagnostic.valueProfile}</p>
            </div>
            <div>
              <p className="text-xs text-gray-400">Strengths</p>
              <p className="text-lg font-semibold text-green-400">{diagnostic.strengths.length}</p>
            </div>
            <div>
              <p className="text-xs text-gray-400">Opportunities</p>
              <p className="text-lg font-semibold text-yellow-400">
                {diagnostic.developmentOpportunities.length}
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Radar chart */}
      {radarAxes.length > 0 && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-3 text-sm font-semibold text-gray-300">Value Dimensions</h2>
          <ValueRadarChart axes={radarAxes} values={radarValues} />
        </div>
      )}

      {/* Strengths & opportunities */}
      {diagnostic && (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <div className="rounded-lg border border-white/10 bg-white/5 p-4">
            <h2 className="mb-2 text-sm font-semibold text-green-400">Strengths</h2>
            <ul className="space-y-1">
              {diagnostic.strengths.map((s, i) => (
                <li key={i} className="text-xs text-gray-300">
                  {s}
                </li>
              ))}
            </ul>
          </div>
          <div className="rounded-lg border border-white/10 bg-white/5 p-4">
            <h2 className="mb-2 text-sm font-semibold text-yellow-400">Development Opportunities</h2>
            <ul className="space-y-1">
              {diagnostic.developmentOpportunities.map((d, i) => (
                <li key={i} className="text-xs text-gray-300">
                  {d}
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}

      {/* Organizational blindness */}
      {blindness && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-3 text-sm font-semibold text-gray-300">Organizational Blindness</h2>
          <BlindnessHeatmap
            riskScore={blindness.blindnessRiskScore}
            blindSpots={blindness.identifiedBlindSpots}
          />
        </div>
      )}
    </div>
  );
}
