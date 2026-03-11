'use client';

import React, { useState, useMemo } from 'react';
import type { NISTGapItem } from '../types';

interface GapAnalysisTableProps {
  gaps: NISTGapItem[];
}

type SortField = 'statementId' | 'currentScore' | 'targetScore' | 'priority';
type SortDir = 'asc' | 'desc';

const PRIORITY_ORDER: Record<string, number> = {
  Critical: 0,
  High: 1,
  Medium: 2,
  Low: 3,
};

function priorityColor(p: string): string {
  switch (p) {
    case 'Critical':
      return 'text-red-400';
    case 'High':
      return 'text-orange-400';
    case 'Medium':
      return 'text-yellow-400';
    default:
      return 'text-gray-400';
  }
}

/**
 * Sortable table displaying NIST compliance gap items.
 */
export default function GapAnalysisTable({ gaps }: GapAnalysisTableProps) {
  const [sortField, setSortField] = useState<SortField>('priority');
  const [sortDir, setSortDir] = useState<SortDir>('asc');

  const sorted = useMemo(() => {
    const copy = [...gaps];
    copy.sort((a, b) => {
      let cmp = 0;
      switch (sortField) {
        case 'statementId':
          cmp = a.statementId.localeCompare(b.statementId);
          break;
        case 'currentScore':
          cmp = a.currentScore - b.currentScore;
          break;
        case 'targetScore':
          cmp = a.targetScore - b.targetScore;
          break;
        case 'priority':
          cmp = (PRIORITY_ORDER[a.priority] ?? 99) - (PRIORITY_ORDER[b.priority] ?? 99);
          break;
      }
      return sortDir === 'asc' ? cmp : -cmp;
    });
    return copy;
  }, [gaps, sortField, sortDir]);

  function handleSort(field: SortField) {
    if (field === sortField) {
      setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortField(field);
      setSortDir('asc');
    }
  }

  function renderHeader(label: string, field: SortField) {
    const active = sortField === field;
    const arrow = active ? (sortDir === 'asc' ? ' \u25B2' : ' \u25BC') : '';
    return (
      <th
        scope="col"
        className="cursor-pointer select-none px-3 py-2 text-left text-xs font-semibold uppercase text-gray-400 hover:text-white"
        onClick={() => handleSort(field)}
      >
        {label}
        {arrow}
      </th>
    );
  }

  if (gaps.length === 0) {
    return <p className="text-sm text-gray-500">No compliance gaps identified.</p>;
  }

  return (
    <div className="overflow-x-auto rounded-lg border border-white/10">
      <table className="w-full text-sm" aria-label="NIST Gap Analysis">
        <thead className="border-b border-white/10 bg-white/5">
          <tr>
            {renderHeader('Statement', 'statementId')}
            {renderHeader('Current', 'currentScore')}
            {renderHeader('Target', 'targetScore')}
            {renderHeader('Priority', 'priority')}
            <th scope="col" className="px-3 py-2 text-left text-xs font-semibold uppercase text-gray-400">
              Actions
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-white/5">
          {sorted.map((gap) => (
            <tr key={gap.statementId} className="hover:bg-white/5">
              <td className="px-3 py-2 font-mono text-xs text-gray-300">{gap.statementId}</td>
              <td className="px-3 py-2 text-gray-300">{gap.currentScore}</td>
              <td className="px-3 py-2 text-gray-300">{gap.targetScore}</td>
              <td className={`px-3 py-2 font-medium ${priorityColor(gap.priority)}`}>{gap.priority}</td>
              <td className="px-3 py-2">
                <ul className="list-inside list-disc text-xs text-gray-400">
                  {gap.recommendedActions.map((action, i) => (
                    <li key={i}>{action}</li>
                  ))}
                </ul>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
