"use client"

import { useEffect, useState, useCallback } from "react"
import { Activity, X } from "lucide-react"
import { useSignalR } from "@/hooks/useSignalR"

interface ActivityItem {
  id: string
  user: string
  action: string
  target: string
  timestamp: string
}

function timeAgo(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime()
  const mins = Math.floor(diff / 60_000)
  if (mins < 1) return "just now"
  if (mins < 60) return `${mins}m ago`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `${hours}h ago`
  return `${Math.floor(hours / 24)}d ago`
}

export function ActivityFeed() {
  const [open, setOpen] = useState(false)
  const [items, setItems] = useState<ActivityItem[]>([])
  const { subscribe, unsubscribe, status } = useSignalR()

  const handleActivity = useCallback((...args: unknown[]) => {
    const data = args[0] as Record<string, unknown> | undefined
    if (!data) return
    const item: ActivityItem = {
      id: String(data.id ?? crypto.randomUUID()),
      user: String(data.user ?? data.userName ?? "System"),
      action: String(data.action ?? "updated"),
      target: String(data.target ?? data.resource ?? ""),
      timestamp: String(data.timestamp ?? new Date().toISOString()),
    }
    setItems((prev) => [item, ...prev].slice(0, 50))
  }, [])

  useEffect(() => {
    if (status !== "connected") return
    subscribe("ActivityEvent", handleActivity)
    return () => unsubscribe("ActivityEvent", handleActivity)
  }, [status, subscribe, unsubscribe, handleActivity])

  return (
    <>
      <button
        onClick={() => setOpen(!open)}
        className="relative rounded p-2 text-gray-400 hover:bg-white/10 hover:text-white"
        aria-label="Activity feed"
      >
        <Activity size={18} />
        {items.length > 0 && (
          <span className="absolute -right-0.5 -top-0.5 h-2 w-2 rounded-full bg-emerald-500" />
        )}
      </button>

      {open && (
        <div className="fixed right-0 top-14 z-50 h-[calc(100vh-3.5rem)] w-80 border-l border-white/10 bg-gray-950 shadow-xl">
          <div className="flex items-center justify-between border-b border-white/10 px-4 py-3">
            <h3 className="text-sm font-semibold text-white">Activity</h3>
            <button
              onClick={() => setOpen(false)}
              className="rounded p-1 text-gray-400 hover:bg-white/10 hover:text-white"
              aria-label="Close activity feed"
            >
              <X size={16} />
            </button>
          </div>

          <div className="overflow-y-auto h-full pb-20">
            {items.length === 0 ? (
              <div className="px-4 py-8 text-center text-sm text-gray-500">
                No recent activity
              </div>
            ) : (
              items.map((item) => (
                <div
                  key={item.id}
                  className="border-b border-white/5 px-4 py-3 hover:bg-white/5"
                >
                  <div className="text-sm text-gray-300">
                    <span className="font-medium text-white">{item.user}</span>{" "}
                    {item.action}{" "}
                    <span className="text-cyan-400">{item.target}</span>
                  </div>
                  <div className="mt-1 text-xs text-gray-500">
                    {timeAgo(item.timestamp)}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </>
  )
}
