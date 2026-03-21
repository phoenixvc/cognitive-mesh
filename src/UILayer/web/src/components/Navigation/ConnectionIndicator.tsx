"use client"

import { Wifi, WifiOff } from "lucide-react"
import { useSignalR } from "@/hooks/useSignalR"

const statusColors = {
  connected: "bg-green-500",
  connecting: "bg-yellow-500 animate-pulse",
  reconnecting: "bg-yellow-500 animate-pulse",
  disconnected: "bg-red-500",
}

const statusLabels = {
  connected: "Connected",
  connecting: "Connecting...",
  reconnecting: "Reconnecting...",
  disconnected: "Disconnected",
}

export function ConnectionIndicator() {
  const { status } = useSignalR({ enabled: false })
  const Icon = status === "disconnected" ? WifiOff : Wifi

  return (
    <div
      className="flex items-center gap-1.5 text-xs text-gray-400"
      title={statusLabels[status]}
    >
      <span className={`h-2 w-2 rounded-full ${statusColors[status]}`} />
      <Icon size={14} />
    </div>
  )
}
