"use client"

import { useEffect } from "react"
import { useAgentStore } from "@/stores"
import { SkeletonTable } from "@/components/Skeleton"

export default function AgentsPage() {
  const { agents, loading, error, fetchAgents, selectAgent, selectedAgentId } =
    useAgentStore()

  useEffect(() => {
    fetchAgents()
  }, [fetchAgents])

  if (loading && agents.length === 0) return <SkeletonTable rows={6} />

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-white">Agents</h1>
        <span className="text-xs text-gray-400">
          {agents.length} registered
        </span>
      </div>

      {error && (
        <div className="rounded border border-red-500/30 bg-red-500/10 p-3 text-sm text-red-400">
          {error}
        </div>
      )}

      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-white/10 text-left text-xs text-gray-400">
              <th className="px-4 py-3">Name</th>
              <th className="px-4 py-3">Type</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3">Tasks</th>
              <th className="px-4 py-3">Registered</th>
            </tr>
          </thead>
          <tbody>
            {agents.map((agent) => (
              <tr
                key={agent.agentId}
                onClick={() => selectAgent(agent.agentId)}
                className={`cursor-pointer border-b border-white/5 transition-colors hover:bg-white/5 ${
                  selectedAgentId === agent.agentId ? "bg-cyan-500/10" : ""
                }`}
              >
                <td className="px-4 py-3 font-medium text-white">
                  {agent.name}
                </td>
                <td className="px-4 py-3 text-gray-400">{agent.agentType}</td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs ${
                      agent.status === "active"
                        ? "bg-green-500/15 text-green-400"
                        : agent.status === "error"
                          ? "bg-red-500/15 text-red-400"
                          : "bg-gray-500/15 text-gray-400"
                    }`}
                  >
                    <span
                      className={`h-1.5 w-1.5 rounded-full ${
                        agent.status === "active"
                          ? "bg-green-400"
                          : agent.status === "error"
                            ? "bg-red-400"
                            : "bg-gray-400"
                      }`}
                    />
                    {agent.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-400">
                  {agent.currentTasks}
                </td>
                <td className="px-4 py-3 text-gray-400">
                  {new Date(agent.registeredAt).toLocaleDateString()}
                </td>
              </tr>
            ))}
            {agents.length === 0 && !loading && (
              <tr>
                <td
                  colSpan={5}
                  className="px-4 py-8 text-center text-gray-500"
                >
                  No agents registered
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
