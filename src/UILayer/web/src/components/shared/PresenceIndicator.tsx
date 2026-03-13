"use client"

import { useEffect, useState, useCallback } from "react"
import { useSignalR } from "@/hooks/useSignalR"

interface PresenceUser {
  id: string
  name: string
  color: string
}

const COLORS = [
  "bg-emerald-500",
  "bg-blue-500",
  "bg-purple-500",
  "bg-amber-500",
  "bg-rose-500",
  "bg-cyan-500",
]

function getInitials(name: string): string {
  return name
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase()
}

export function PresenceIndicator() {
  const [users, setUsers] = useState<PresenceUser[]>([])
  const { subscribe, unsubscribe, status } = useSignalR()

  const handlePresenceUpdate = useCallback((...args: unknown[]) => {
    const data = args[0]
    if (Array.isArray(data)) {
      setUsers(
        data.map((u: Record<string, unknown>, i: number) => ({
          id: String(u.id ?? u.userId ?? i),
          name: String(u.name ?? u.displayName ?? "User"),
          color: COLORS[i % COLORS.length],
        }))
      )
    }
  }, [])

  useEffect(() => {
    if (status !== "connected") return
    subscribe("PresenceUpdated", handlePresenceUpdate)
    return () => unsubscribe("PresenceUpdated", handlePresenceUpdate)
  }, [status, subscribe, unsubscribe, handlePresenceUpdate])

  if (users.length === 0) return null

  const visible = users.slice(0, 3)
  const overflow = users.length - 3

  return (
    <div className="flex items-center" aria-label={`${users.length} users online`}>
      <div className="flex -space-x-2">
        {visible.map((user) => (
          <div
            key={user.id}
            className={`flex h-7 w-7 items-center justify-center rounded-full border-2 border-black/60 text-[10px] font-semibold text-white ${user.color}`}
            title={user.name}
          >
            {getInitials(user.name)}
          </div>
        ))}
        {overflow > 0 && (
          <div
            className="flex h-7 w-7 items-center justify-center rounded-full border-2 border-black/60 bg-gray-700 text-[10px] font-semibold text-gray-300"
            title={`${overflow} more`}
          >
            +{overflow}
          </div>
        )}
      </div>
    </div>
  )
}
