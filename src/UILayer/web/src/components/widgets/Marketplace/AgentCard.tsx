'use client';

import React from 'react';

interface AgentCardProps {
  /** Unique agent identifier. */
  agentId: string;
  /** Display name of the agent or capability. */
  name: string;
  /** Short description. */
  description: string;
  /** Category tag (e.g. "Reasoning", "Analytics"). */
  category: string;
  /** Version string. */
  version: string;
  /** Whether the agent is currently installed/active. */
  installed: boolean;
}

function getCategoryColor(category: string): string {
  switch (category.toLowerCase()) {
    case 'reasoning':
      return 'bg-purple-500/20 text-purple-400';
    case 'analytics':
      return 'bg-blue-500/20 text-blue-400';
    case 'security':
      return 'bg-red-500/20 text-red-400';
    case 'compliance':
      return 'bg-green-500/20 text-green-400';
    default:
      return 'bg-gray-500/20 text-gray-400';
  }
}

/**
 * Card component for a marketplace agent/capability listing.
 */
export default function AgentCard({
  agentId,
  name,
  description,
  category,
  version,
  installed,
}: AgentCardProps) {
  return (
    <div
      className="flex flex-col rounded-lg border border-white/10 bg-white/5 p-4 transition-colors hover:border-white/20 hover:bg-white/[0.07]"
      data-agent-id={agentId}
    >
      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-md bg-cyan-500/20 text-sm font-bold text-cyan-400">
            {name.charAt(0).toUpperCase()}
          </div>
          <div>
            <h3 className="text-sm font-semibold text-white">{name}</h3>
            <span className="text-[10px] text-gray-500">v{version}</span>
          </div>
        </div>
        <span className={`rounded px-1.5 py-0.5 text-[10px] font-medium ${getCategoryColor(category)}`}>
          {category}
        </span>
      </div>

      {/* Description */}
      <p className="mt-3 flex-1 text-xs leading-relaxed text-gray-400">
        {description}
      </p>

      {/* Footer */}
      <div className="mt-4 flex items-center justify-between">
        {installed ? (
          <span className="inline-flex items-center gap-1 text-xs text-green-400">
            <span className="h-1.5 w-1.5 rounded-full bg-green-500" />
            Installed
          </span>
        ) : (
          <span className="text-xs text-gray-500">Not installed</span>
        )}
        <button
          className={`rounded px-3 py-1 text-xs font-medium transition-colors ${
            installed
              ? 'bg-white/10 text-gray-400 hover:bg-white/15'
              : 'bg-cyan-500/20 text-cyan-400 hover:bg-cyan-500/30'
          }`}
          disabled={installed}
        >
          {installed ? 'Installed' : 'Install'}
        </button>
      </div>
    </div>
  );
}
