'use client';

import React from 'react';
import SessionTimeline from './SessionTimeline';

/**
 * FE-018: Convener Dashboard widget.
 *
 * Displays meeting and session orchestration information.
 * Currently renders a placeholder layout since the backend Convener API is not yet deployed.
 */
export default function ConvenerDashboard() {
  return (
    <div className="space-y-6" role="region" aria-label="Convener Dashboard">
      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-white">Convener</h1>
        <p className="text-xs text-gray-400">
          Session orchestration, meeting management, and collaboration coordination
        </p>
      </div>

      {/* Coming soon banner */}
      <div className="rounded-lg border border-cyan-500/20 bg-cyan-500/5 p-4">
        <p className="text-sm font-medium text-cyan-400">
          Convener data will be available when the backend API is deployed.
        </p>
        <p className="mt-1 text-xs text-gray-400">
          This dashboard will orchestrate multi-agent sessions, coordinate
          collaborative reasoning, and manage decision-making workflows.
        </p>
      </div>

      {/* Metrics row */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {[
          { label: 'Active Sessions', value: '--' },
          { label: 'Total Sessions', value: '--' },
          { label: 'Avg. Duration', value: '--' },
          { label: 'Participants Today', value: '--' },
        ].map((metric) => (
          <div
            key={metric.label}
            className="rounded-lg border border-white/10 bg-white/5 p-4"
          >
            <p className="text-xs text-gray-400">{metric.label}</p>
            <p className="mt-1 text-2xl font-bold text-white/30">{metric.value}</p>
          </div>
        ))}
      </div>

      {/* Session timeline placeholder */}
      <div className="rounded-lg border border-white/10 bg-white/5 p-4">
        <h2 className="mb-3 text-sm font-semibold text-gray-300">Recent Sessions</h2>
        <SessionTimeline sessions={[]} />
      </div>

      {/* Orchestration modes */}
      <div className="rounded-lg border border-white/10 bg-white/5 p-4">
        <h2 className="mb-3 text-sm font-semibold text-gray-300">
          Orchestration Modes
        </h2>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
          {[
            {
              mode: 'Debate',
              description: 'Multi-agent adversarial reasoning sessions',
            },
            {
              mode: 'Sequential',
              description: 'Ordered step-by-step agent collaboration',
            },
            {
              mode: 'Strategic Simulation',
              description: 'Scenario-based strategy evaluation',
            },
          ].map((item) => (
            <div
              key={item.mode}
              className="rounded border border-white/5 bg-white/[0.02] p-3"
            >
              <p className="text-sm font-medium text-gray-400">{item.mode}</p>
              <p className="mt-1 text-xs text-gray-600">{item.description}</p>
              <span className="mt-2 inline-block text-[10px] text-gray-600">
                Pending
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
