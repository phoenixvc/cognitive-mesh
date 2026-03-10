"use client"

import { useEffect } from "react"
import { useDashboardStore } from "@/stores"
import { SkeletonDashboard } from "@/components/Skeleton"

export default function DashboardPage() {
  const { layers, metrics, systemStatus, loading, error, fetchAll } =
    useDashboardStore()

  useEffect(() => {
    fetchAll()
  }, [fetchAll])

  if (loading && layers.length === 0) {
    return <SkeletonDashboard />
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-6 text-center">
        <p className="text-sm text-red-400">{error}</p>
        <button
          onClick={fetchAll}
          className="mt-3 rounded bg-cyan-600 px-4 py-1.5 text-sm text-white hover:bg-cyan-500"
        >
          Retry
        </button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <h1 className="text-xl font-bold text-white">Dashboard</h1>

      {/* Metrics row */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {metrics.map((m) => (
          <div
            key={m.id}
            className="rounded-lg border border-white/10 bg-white/5 p-4"
          >
            <p className="text-xs text-gray-400">{m.label}</p>
            <p className="mt-1 text-2xl font-bold text-white">{m.value}</p>
            <p
              className={`mt-1 text-xs ${
                m.status === "up"
                  ? "text-green-400"
                  : m.status === "down"
                    ? "text-red-400"
                    : "text-gray-400"
              }`}
            >
              {m.change}
            </p>
          </div>
        ))}
      </div>

      {/* Layers */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {layers.map((layer) => (
          <div
            key={layer.id}
            className="rounded-lg border border-white/10 bg-white/5 p-4"
          >
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-semibold text-white">{layer.name}</h3>
              <span className="text-xs text-cyan-400">
                {layer.uptime.toFixed(1)}% uptime
              </span>
            </div>
            <p className="mt-2 text-xs text-gray-400">{layer.description}</p>
          </div>
        ))}
      </div>

      {/* System status */}
      {systemStatus && (
        <div className="rounded-lg border border-white/10 bg-white/5 p-4">
          <h3 className="mb-3 text-sm font-semibold text-white">
            System Status
          </h3>
          <div className="grid grid-cols-2 gap-4 text-sm md:grid-cols-4">
            <div>
              <p className="text-gray-400">Power</p>
              <p className="text-white">{systemStatus.power}%</p>
            </div>
            <div>
              <p className="text-gray-400">Load</p>
              <p className="text-white">{systemStatus.load}%</p>
            </div>
            <div>
              <p className="text-gray-400">Neural Network</p>
              <p className={systemStatus.neuralNetwork ? "text-green-400" : "text-red-400"}>
                {systemStatus.neuralNetwork ? "Online" : "Offline"}
              </p>
            </div>
            <div>
              <p className="text-gray-400">Quantum Processing</p>
              <p className={systemStatus.quantumProcessing ? "text-green-400" : "text-red-400"}>
                {systemStatus.quantumProcessing ? "Active" : "Inactive"}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
