'use client';

import React from 'react';

interface MeshNode {
  nodeId: string;
  label: string;
  type: 'agent' | 'team' | 'department' | 'external';
}

interface MeshConnection {
  from: string;
  to: string;
  strength: number;
}

interface MeshTopologyProps {
  /** Nodes in the organizational mesh. */
  nodes: MeshNode[];
  /** Connections between nodes. */
  connections: MeshConnection[];
}

function getNodeColor(type: MeshNode['type']): string {
  switch (type) {
    case 'agent':
      return 'border-cyan-500 bg-cyan-500/20 text-cyan-400';
    case 'team':
      return 'border-blue-500 bg-blue-500/20 text-blue-400';
    case 'department':
      return 'border-purple-500 bg-purple-500/20 text-purple-400';
    case 'external':
      return 'border-orange-500 bg-orange-500/20 text-orange-400';
    default:
      return 'border-gray-500 bg-gray-500/20 text-gray-400';
  }
}

/**
 * Network graph visualization of organizational mesh connections.
 *
 * Renders a simplified CSS grid topology view. A full D3/WebGL network
 * graph will be implemented when the backend API provides real-time data.
 */
export default function MeshTopology({ nodes, connections }: MeshTopologyProps) {
  if (nodes.length === 0) {
    return <p className="text-sm text-gray-500">No mesh topology data available.</p>;
  }

  return (
    <div role="img" aria-label="Organization mesh topology">
      {/* Node grid */}
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
        {nodes.map((node) => {
          const nodeConnections = connections.filter(
            (c) => c.from === node.nodeId || c.to === node.nodeId
          );
          return (
            <div
              key={node.nodeId}
              className={`rounded-lg border p-3 text-center ${getNodeColor(node.type)}`}
            >
              <p className="text-xs font-semibold">{node.label}</p>
              <p className="mt-1 text-[10px] opacity-70">{node.type}</p>
              <p className="mt-0.5 text-[10px] opacity-50">
                {nodeConnections.length} connection{nodeConnections.length !== 1 ? 's' : ''}
              </p>
            </div>
          );
        })}
      </div>

      {/* Connection list */}
      {connections.length > 0 && (
        <div className="mt-4">
          <p className="mb-2 text-xs font-medium text-gray-400">Connections</p>
          <div className="space-y-1">
            {connections.slice(0, 10).map((conn, idx) => {
              const fromNode = nodes.find((n) => n.nodeId === conn.from);
              const toNode = nodes.find((n) => n.nodeId === conn.to);
              return (
                <div key={idx} className="flex items-center gap-2 text-xs text-gray-500">
                  <span className="text-gray-300">{fromNode?.label ?? conn.from}</span>
                  <span className="text-gray-600">&rarr;</span>
                  <span className="text-gray-300">{toNode?.label ?? conn.to}</span>
                  <div className="ml-auto h-1.5 w-12 rounded-full bg-white/10">
                    <div
                      className="h-1.5 rounded-full bg-cyan-500/60"
                      style={{ width: `${(conn.strength * 100)}%` }}
                    />
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
