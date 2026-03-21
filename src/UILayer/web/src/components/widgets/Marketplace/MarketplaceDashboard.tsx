'use client';

import React, { useState } from 'react';
import AgentCard from './AgentCard';

interface MarketplaceAgent {
  agentId: string;
  name: string;
  description: string;
  category: string;
  version: string;
  installed: boolean;
}

// Placeholder catalog — will be replaced by AgentRegistry API once available
const PLACEHOLDER_AGENTS: MarketplaceAgent[] = [
  {
    agentId: 'agent-debate-reasoning',
    name: 'Debate Reasoning',
    description: 'Multi-agent adversarial reasoning engine using ConclAIve debate protocol for balanced decision-making.',
    category: 'Reasoning',
    version: '1.2.0',
    installed: true,
  },
  {
    agentId: 'agent-nist-compliance',
    name: 'NIST Compliance',
    description: 'Automated assessment and monitoring of AI systems against the NIST AI Risk Management Framework.',
    category: 'Compliance',
    version: '1.0.3',
    installed: true,
  },
  {
    agentId: 'agent-threat-intel',
    name: 'Threat Intelligence',
    description: 'Real-time threat detection and intelligence gathering with automated response recommendations.',
    category: 'Security',
    version: '0.9.1',
    installed: false,
  },
  {
    agentId: 'agent-value-diagnostic',
    name: 'Value Diagnostic',
    description: 'Organizational value assessment and blindness detection for AI deployment readiness.',
    category: 'Analytics',
    version: '1.1.0',
    installed: true,
  },
  {
    agentId: 'agent-ethical-reasoning',
    name: 'Ethical Reasoning',
    description: 'Brandom-Floridi ethical framework evaluation for responsible AI decision-making.',
    category: 'Reasoning',
    version: '1.0.0',
    installed: false,
  },
  {
    agentId: 'agent-cognitive-sandwich',
    name: 'Cognitive Sandwich',
    description: 'Human-AI-Human workflow orchestration with cognitive debt monitoring and phase management.',
    category: 'Compliance',
    version: '1.3.0',
    installed: true,
  },
];

const CATEGORIES = ['All', 'Reasoning', 'Analytics', 'Security', 'Compliance'] as const;

/**
 * FE-019: Marketplace Dashboard widget.
 *
 * Provides a browsable catalog of agents and capabilities for the Cognitive Mesh platform.
 * Uses placeholder data until the AgentRegistry API is available.
 */
export default function MarketplaceDashboard() {
  const [filter, setFilter] = useState<string>('All');
  const [search, setSearch] = useState('');

  const filtered = PLACEHOLDER_AGENTS.filter((agent) => {
    const matchesCategory = filter === 'All' || agent.category === filter;
    const matchesSearch =
      search === '' ||
      agent.name.toLowerCase().includes(search.toLowerCase()) ||
      agent.description.toLowerCase().includes(search.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  return (
    <div className="space-y-6" role="region" aria-label="Marketplace Dashboard">
      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-white">Marketplace</h1>
        <p className="text-xs text-gray-400">
          Browse, install, and manage agents and integrations
        </p>
      </div>

      {/* Search and filter bar */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <input
          type="text"
          placeholder="Search agents..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="rounded-md border border-white/10 bg-white/5 px-3 py-2 text-sm text-gray-200 placeholder-gray-500 focus:border-cyan-500/40 focus:outline-none focus:ring-1 focus:ring-cyan-500/40"
          aria-label="Search agents"
        />
        <div className="flex gap-1">
          {CATEGORIES.map((cat) => (
            <button
              key={cat}
              onClick={() => setFilter(cat)}
              className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
                filter === cat
                  ? 'bg-cyan-500/20 text-cyan-400'
                  : 'bg-white/5 text-gray-400 hover:bg-white/10'
              }`}
            >
              {cat}
            </button>
          ))}
        </div>
      </div>

      {/* Agent grid */}
      {filtered.length === 0 ? (
        <div className="rounded-lg border border-white/10 bg-white/5 p-8 text-center">
          <p className="text-gray-400">No agents match your search criteria.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          {filtered.map((agent) => (
            <AgentCard
              key={agent.agentId}
              agentId={agent.agentId}
              name={agent.name}
              description={agent.description}
              category={agent.category}
              version={agent.version}
              installed={agent.installed}
            />
          ))}
        </div>
      )}

      {/* Summary */}
      <div className="flex items-center justify-between text-xs text-gray-500">
        <span>
          Showing {filtered.length} of {PLACEHOLDER_AGENTS.length} agents
        </span>
        <span className="text-gray-600">
          Agent Registry API integration pending
        </span>
      </div>
    </div>
  );
}
