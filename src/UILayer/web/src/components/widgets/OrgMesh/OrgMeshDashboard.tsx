'use client';

import React from 'react';
import MeshTopology from './MeshTopology';

/**
 * FE-020: Org Mesh Dashboard widget.
 *
 * Visualizes the organizational mesh — relationships between agents, teams,
 * departments, and external connections within the Cognitive Mesh platform.
 * Currently renders a placeholder layout since the backend API is not yet deployed.
 */
export default function OrgMeshDashboard() {
  return (
    <div className="space-y-6" role="region" aria-label="Organization Mesh Dashboard">
      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-white">Organization Mesh</h1>
        <p className="text-xs text-gray-400">
          Organizational topology, agent relationships, and collaboration networks
        </p>
      </div>

      {/* Coming soon banner */}
      <div className="rounded-lg border border-cyan-500/20 bg-cyan-500/5 p-4">
        <p className="text-sm font-medium text-cyan-400">
          Organization Mesh data will be available when the backend API is deployed.
        </p>
        <p className="mt-1 text-xs text-gray-400">
          This dashboard will visualize the relationships between agents, teams,
          and departments, showing communication patterns and collaboration strength.
        </p>
      </div>

      {/* Metrics row */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {[
          { label: 'Total Nodes', value: '--' },
          { label: 'Active Connections', value: '--' },
          { label: 'Network Density', value: '--' },
          { label: 'Clusters', value: '--' },
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

      {/* Topology placeholder */}
      <div className="rounded-lg border border-white/10 bg-white/5 p-4">
        <h2 className="mb-3 text-sm font-semibold text-gray-300">Mesh Topology</h2>
        <MeshTopology nodes={[]} connections={[]} />
      </div>

      {/* Node types legend */}
      <div className="rounded-lg border border-white/10 bg-white/5 p-4">
        <h2 className="mb-3 text-sm font-semibold text-gray-300">Node Types</h2>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
          {[
            { type: 'Agent', color: 'bg-cyan-500', description: 'AI agents in the mesh' },
            { type: 'Team', color: 'bg-blue-500', description: 'Human teams' },
            { type: 'Department', color: 'bg-purple-500', description: 'Organization units' },
            { type: 'External', color: 'bg-orange-500', description: 'External integrations' },
          ].map((item) => (
            <div key={item.type} className="flex items-start gap-2">
              <div className={`mt-1 h-3 w-3 rounded-full ${item.color}`} />
              <div>
                <p className="text-xs font-medium text-gray-300">{item.type}</p>
                <p className="text-[10px] text-gray-600">{item.description}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
