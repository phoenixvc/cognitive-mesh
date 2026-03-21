'use client';

import React, { useState, useEffect, useCallback } from 'react';
import MaturityGauge from './MaturityGauge';
import GapAnalysisTable from './GapAnalysisTable';
import ComplianceTimeline from './ComplianceTimeline';
import { getNistScore, getNistRoadmap, getNistAuditLog } from '../api';
import type { NISTScoreResponse, NISTRoadmapResponse, NISTAuditEntry } from '../types';

interface NistComplianceDashboardProps {
  organizationId?: string;
}

/**
 * FE-011: Main NIST Compliance Dashboard widget.
 *
 * Displays maturity scores, pillar breakdown, gap analysis, and an audit
 * event timeline sourced from the NISTComplianceController API.
 */
export default function NistComplianceDashboard({
  organizationId = 'default-org',
}: NistComplianceDashboardProps) {
  const [scoreData, setScoreData] = useState<NISTScoreResponse | null>(null);
  const [roadmapData, setRoadmapData] = useState<NISTRoadmapResponse | null>(null);
  const [auditEntries, setAuditEntries] = useState<NISTAuditEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [score, roadmap, audit] = await Promise.all([
        getNistScore(organizationId),
        getNistRoadmap(organizationId),
        getNistAuditLog(organizationId, 50),
      ]);
      setScoreData(score);
      setRoadmapData(roadmap);
      setAuditEntries(audit.entries ?? []);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to load NIST compliance data.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [organizationId]);

  useEffect(() => {
    void fetchData();
  }, [fetchData]);

  // Loading skeleton
  if (loading) {
    return (
      <div className="space-y-4" aria-busy="true" aria-label="Loading NIST Compliance Dashboard">
        <div className="h-8 w-48 animate-pulse rounded bg-white/10" />
        <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
          {[1, 2, 3].map((k) => (
            <div key={k} className="h-48 animate-pulse rounded-lg bg-white/5" />
          ))}
        </div>
        <div className="h-64 animate-pulse rounded-lg bg-white/5" />
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-6" role="alert">
        <h2 className="text-lg font-semibold text-red-400">Error loading compliance data</h2>
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

  return (
    <div className="space-y-6" role="region" aria-label="NIST Compliance Dashboard">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-white">NIST AI RMF Compliance</h1>
          {scoreData && (
            <p className="text-xs text-gray-400">
              Assessed {new Date(scoreData.assessedAt).toLocaleDateString()}
            </p>
          )}
        </div>
        <button
          onClick={() => void fetchData()}
          className="rounded bg-white/10 px-3 py-1.5 text-xs font-medium text-gray-300 hover:bg-white/15"
        >
          Refresh
        </button>
      </div>

      {/* Gauges row */}
      {scoreData && (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
          {/* Overall maturity */}
          <div className="rounded-lg border border-white/10 bg-white/5 p-4">
            <MaturityGauge score={scoreData.overallScore} label="Overall Maturity" />
          </div>

          {/* Top 3 pillar scores */}
          {scoreData.pillarScores.slice(0, 3).map((ps) => (
            <div key={ps.pillarId} className="rounded-lg border border-white/10 bg-white/5 p-4">
              <MaturityGauge score={ps.averageScore} label={ps.pillarName} />
            </div>
          ))}
        </div>
      )}

      {/* Pillar breakdown table */}
      {scoreData && scoreData.pillarScores.length > 0 && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-3 text-sm font-semibold text-gray-300">Pillar Breakdown</h2>
          <div className="space-y-2">
            {scoreData.pillarScores.map((ps) => {
              const pct = (ps.averageScore / 5) * 100;
              return (
                <div key={ps.pillarId} className="flex items-center gap-3">
                  <span className="w-32 truncate text-xs text-gray-400">{ps.pillarName}</span>
                  <div className="flex-1">
                    <div className="h-2 rounded-full bg-white/10">
                      <div
                        className="h-2 rounded-full bg-blue-500"
                        style={{ width: `${pct}%` }}
                      />
                    </div>
                  </div>
                  <span className="w-10 text-right text-xs font-medium text-gray-300">
                    {ps.averageScore.toFixed(1)}
                  </span>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Gap analysis */}
      {roadmapData && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-3 text-sm font-semibold text-gray-300">Gap Analysis</h2>
          <GapAnalysisTable gaps={roadmapData.gaps} />
        </div>
      )}

      {/* Audit timeline */}
      <div className="rounded-lg border border-white/10 bg-white/5 p-4">
        <h2 className="mb-3 text-sm font-semibold text-gray-300">Compliance Timeline</h2>
        <ComplianceTimeline entries={auditEntries} />
      </div>
    </div>
  );
}
