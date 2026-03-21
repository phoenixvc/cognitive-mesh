'use client';

import React, { useState, useEffect, useCallback } from 'react';
import PhaseStepper from './PhaseStepper';
import BurndownChart from './BurndownChart';
import { getSandwichProcess, getSandwichAuditTrail, getSandwichDebt } from '../api';
import type { SandwichProcess, PhaseAuditEntry, CognitiveDebtAssessment } from '../types';

interface CognitiveSandwichDashboardProps {
  processId?: string;
}

export default function CognitiveSandwichDashboard({ processId = 'default-process' }: CognitiveSandwichDashboardProps) {
  const [process, setProcess] = useState<SandwichProcess | null>(null);
  const [audit, setAudit] = useState<PhaseAuditEntry[]>([]);
  const [debt, setDebt] = useState<CognitiveDebtAssessment | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [proc, trail, debtData] = await Promise.all([
        getSandwichProcess(processId),
        getSandwichAuditTrail(processId),
        getSandwichDebt(processId),
      ]);
      setProcess(proc);
      setAudit(trail);
      setDebt(debtData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load Cognitive Sandwich data.');
    } finally {
      setLoading(false);
    }
  }, [processId]);

  useEffect(() => { void fetchData(); }, [fetchData]);

  if (loading) {
    return (
      <div className="space-y-4" aria-busy="true" aria-label="Loading Cognitive Sandwich Dashboard">
        <div className="h-8 w-56 animate-pulse rounded bg-white/10" />
        <div className="h-24 animate-pulse rounded-lg bg-white/5" />
        <div className="h-56 animate-pulse rounded-lg bg-white/5" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-6" role="alert">
        <h2 className="text-lg font-semibold text-red-400">Error loading sandwich data</h2>
        <p className="mt-1 text-sm text-red-300">{error}</p>
        <button onClick={() => void fetchData()} className="mt-3 rounded bg-red-500/20 px-4 py-1.5 text-sm font-medium text-red-300 hover:bg-red-500/30">Retry</button>
      </div>
    );
  }

  return (
    <div className="space-y-6" role="region" aria-label="Cognitive Sandwich Dashboard">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-white">Cognitive Sandwich</h1>
          {process && <p className="text-xs text-gray-400">{process.name} &middot; State: {process.state}</p>}
        </div>
        <button onClick={() => void fetchData()} className="rounded bg-white/10 px-3 py-1.5 text-xs font-medium text-gray-300 hover:bg-white/15">Refresh</button>
      </div>

      {process && (
        <>
          <div className="rounded-lg border border-white/10 bg-white/5 p-4">
            <h2 className="mb-3 text-sm font-semibold text-gray-300">Phase Progression</h2>
            <PhaseStepper phases={process.phases} currentPhaseIndex={process.currentPhaseIndex} />
          </div>

          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <div className="rounded-lg border border-white/10 bg-white/5 p-3 text-center">
              <p className="text-xs text-gray-400">Phases</p>
              <p className="text-lg font-bold text-white">{process.currentPhaseIndex + 1} / {process.phases.length}</p>
            </div>
            <div className="rounded-lg border border-white/10 bg-white/5 p-3 text-center">
              <p className="text-xs text-gray-400">Step-backs</p>
              <p className="text-lg font-bold text-white">{process.stepBackCount} / {process.maxStepBacks}</p>
            </div>
            <div className="rounded-lg border border-white/10 bg-white/5 p-3 text-center">
              <p className="text-xs text-gray-400">Debt Threshold</p>
              <p className="text-lg font-bold text-white">{process.cognitiveDebtThreshold}</p>
            </div>
            <div className="rounded-lg border border-white/10 bg-white/5 p-3 text-center">
              <p className="text-xs text-gray-400">Current Debt</p>
              <p className={`text-lg font-bold ${debt && debt.isBreached ? 'text-red-400' : 'text-white'}`}>
                {debt ? debt.debtScore.toFixed(1) : '-'}
              </p>
            </div>
          </div>
        </>
      )}

      {debt && debt.recommendations.length > 0 && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-2 text-sm font-semibold text-gray-300">Debt Reduction Recommendations</h2>
          <ul className="space-y-1">
            {debt.recommendations.map((r, i) => <li key={i} className="text-xs text-gray-300">{r}</li>)}
          </ul>
        </div>
      )}

      <div className="rounded-lg border border-white/10 bg-white/5 p-4">
        <h2 className="mb-3 text-sm font-semibold text-gray-300">Burndown</h2>
        <BurndownChart totalPhases={process?.phases.length ?? 0} auditEntries={audit} />
      </div>

      {audit.length > 0 && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-2 text-sm font-semibold text-gray-300">Audit Trail</h2>
          <div className="max-h-48 space-y-1 overflow-y-auto">
            {audit.map((e) => (
              <div key={e.entryId} className="flex items-center gap-2 text-xs">
                <span className="shrink-0 text-gray-500">{new Date(e.timestamp).toLocaleString()}</span>
                <span className="font-medium text-gray-400">{e.eventType}</span>
                <span className="truncate text-gray-500">{e.details}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
