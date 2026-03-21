'use client';

import React, { useState, useEffect, useCallback } from 'react';
import SpectrumSlider from './SpectrumSlider';
import BalanceHistory from './BalanceHistory';
import { getAdaptiveBalance, getSpectrumHistory, getReflexionStatus } from '../api';
import type {
  BalanceResponse,
  SpectrumHistoryResponse,
  ReflexionStatusResponse,
} from '../types';

/**
 * FE-012: Adaptive Balance Dashboard widget.
 *
 * Visualizes the current balance state, spectrum positions, history of
 * balance adjustments, and reflexion system status sourced from the
 * AdaptiveBalanceController API.
 */
export default function AdaptiveBalanceDashboard() {
  const [balance, setBalance] = useState<BalanceResponse | null>(null);
  const [selectedDim, setSelectedDim] = useState<string | null>(null);
  const [dimHistory, setDimHistory] = useState<SpectrumHistoryResponse | null>(null);
  const [reflexion, setReflexion] = useState<ReflexionStatusResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [bal, ref] = await Promise.all([
        getAdaptiveBalance(),
        getReflexionStatus(),
      ]);
      setBalance(bal);
      setReflexion(ref);
      // Select first dimension by default
      if (bal.dimensions.length > 0 && !selectedDim) {
        setSelectedDim(bal.dimensions[0].dimension);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to load adaptive balance data.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [selectedDim]);

  // Fetch history when dimension changes
  useEffect(() => {
    if (!selectedDim) return;
    let cancelled = false;
    getSpectrumHistory(selectedDim)
      .then((h) => {
        if (!cancelled) setDimHistory(h);
      })
      .catch(() => {
        if (!cancelled) setDimHistory(null);
      });
    return () => {
      cancelled = true;
    };
  }, [selectedDim]);

  useEffect(() => {
    void fetchData();
  }, [fetchData]);

  // Loading skeleton
  if (loading) {
    return (
      <div className="space-y-4" aria-busy="true" aria-label="Loading Adaptive Balance Dashboard">
        <div className="h-8 w-56 animate-pulse rounded bg-white/10" />
        <div className="h-64 animate-pulse rounded-lg bg-white/5" />
        <div className="h-48 animate-pulse rounded-lg bg-white/5" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-6" role="alert">
        <h2 className="text-lg font-semibold text-red-400">Error loading balance data</h2>
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
    <div className="space-y-6" role="region" aria-label="Adaptive Balance Dashboard">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-white">Adaptive Balance</h1>
          {balance && (
            <p className="text-xs text-gray-400">
              Confidence: {(balance.overallConfidence * 100).toFixed(0)}% &middot;{' '}
              Generated {new Date(balance.generatedAt).toLocaleDateString()}
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

      {/* Spectrum sliders */}
      {balance && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-3 text-sm font-semibold text-gray-300">Spectrum Positions</h2>
          <SpectrumSlider dimensions={balance.dimensions} />
        </div>
      )}

      {/* Dimension selector + history */}
      {balance && balance.dimensions.length > 0 && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <div className="mb-3 flex items-center gap-3">
            <h2 className="text-sm font-semibold text-gray-300">History</h2>
            <select
              value={selectedDim ?? ''}
              onChange={(e) => setSelectedDim(e.target.value)}
              className="rounded border border-white/10 bg-slate-800 px-2 py-1 text-xs text-gray-300"
              aria-label="Select dimension for history"
            >
              {balance.dimensions.map((d) => (
                <option key={d.dimension} value={d.dimension}>
                  {d.dimension}
                </option>
              ))}
            </select>
          </div>
          {dimHistory ? (
            <BalanceHistory dimension={dimHistory.dimension} history={dimHistory.history} />
          ) : (
            <p className="text-sm text-gray-500">Select a dimension to view history.</p>
          )}
        </div>
      )}

      {/* Reflexion status */}
      {reflexion && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h2 className="mb-3 text-sm font-semibold text-gray-300">Reflexion System Status</h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-xs text-gray-400">Hallucination Rate</p>
              <p className="text-lg font-bold text-white">
                {(reflexion.hallucinationRate * 100).toFixed(1)}%
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-400">Average Confidence</p>
              <p className="text-lg font-bold text-white">
                {(reflexion.averageConfidence * 100).toFixed(1)}%
              </p>
            </div>
          </div>
          {reflexion.recentResults.length > 0 && (
            <div className="mt-3">
              <p className="text-xs font-medium text-gray-400">Recent Evaluations</p>
              <div className="mt-1 space-y-1">
                {reflexion.recentResults.slice(0, 5).map((r) => (
                  <div key={r.evaluationId} className="flex items-center justify-between text-xs">
                    <span className="text-gray-400">{r.result}</span>
                    <span className="text-gray-500">
                      {(r.confidence * 100).toFixed(0)}% &middot;{' '}
                      {new Date(r.timestamp).toLocaleDateString()}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
