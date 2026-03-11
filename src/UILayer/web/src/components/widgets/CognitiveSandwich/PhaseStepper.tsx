'use client';

import React from 'react';
import type { Phase } from '../types';

interface PhaseStepperProps {
  phases: Phase[];
  currentPhaseIndex: number;
}

function phaseTypeIcon(phaseType: string): string {
  const lower = phaseType.toLowerCase();
  if (lower.includes('human')) return 'H';
  if (lower.includes('ai') || lower.includes('machine')) return 'AI';
  return '?';
}

function statusColor(status: string, isCurrent: boolean): string {
  if (isCurrent) return 'border-blue-500 bg-blue-500/20 text-blue-400';
  if (status === 'Completed') return 'border-green-500 bg-green-500/20 text-green-400';
  if (status === 'Skipped') return 'border-gray-600 bg-gray-600/20 text-gray-500';
  return 'border-white/20 bg-white/5 text-gray-500';
}

/**
 * Visual phase progression stepper for the Cognitive Sandwich pattern
 * (Human -> AI -> Human).
 */
export default function PhaseStepper({ phases, currentPhaseIndex }: PhaseStepperProps) {
  if (phases.length === 0) {
    return <p className="text-sm text-gray-500">No phases defined.</p>;
  }

  return (
    <div className="flex items-center gap-1 overflow-x-auto pb-2" role="list" aria-label="Process phases">
      {phases.map((phase, i) => {
        const isCurrent = i === currentPhaseIndex;
        return (
          <React.Fragment key={phase.phaseId}>
            {i > 0 && (
              <div className={`h-0.5 w-6 shrink-0 ${i <= currentPhaseIndex ? 'bg-blue-500' : 'bg-white/10'}`} />
            )}
            <div
              className={`flex shrink-0 flex-col items-center gap-1 rounded-lg border p-3 ${statusColor(phase.status, isCurrent)}`}
              role="listitem"
              aria-current={isCurrent ? 'step' : undefined}
            >
              <span className="text-lg font-bold">{phaseTypeIcon(phase.phaseType)}</span>
              <span className="max-w-[80px] truncate text-[10px]">{phase.phaseName}</span>
              <span className="text-[9px] opacity-60">{phase.status}</span>
            </div>
          </React.Fragment>
        );
      })}
    </div>
  );
}
