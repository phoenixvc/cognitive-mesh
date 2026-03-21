"use client"

/**
 * SignalR hook — establishes and manages the real-time connection to CognitiveMeshHub.
 *
 * Provides connection state, automatic reconnection with exponential backoff,
 * and methods to subscribe/unsubscribe from groups.
 */
import { useEffect, useRef, useState, useCallback } from "react"
import {
  HubConnectionBuilder,
  HubConnection,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr"
import { useAuthStore } from "@/stores"

type ConnectionStatus = "disconnected" | "connecting" | "connected" | "reconnecting"

interface UseSignalROptions {
  hubUrl?: string
  enabled?: boolean
}

interface UseSignalRReturn {
  status: ConnectionStatus
  subscribe: (method: string, handler: (...args: unknown[]) => void) => void
  unsubscribe: (method: string, handler: (...args: unknown[]) => void) => void
  invoke: <T = void>(method: string, ...args: unknown[]) => Promise<T>
  joinGroup: (group: string) => Promise<void>
  leaveGroup: (group: string) => Promise<void>
}

function getHubUrl(): string {
  const base = process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5000"
  return `${base}/hubs/cognitive-mesh`
}

export function useSignalR(options?: UseSignalROptions): UseSignalRReturn {
  const { hubUrl, enabled = true } = options ?? {}
  const [status, setStatus] = useState<ConnectionStatus>("disconnected")
  const connectionRef = useRef<HubConnection | null>(null)
  const accessToken = useAuthStore((s) => s.accessToken)

  useEffect(() => {
    if (!enabled || !accessToken) return

    const url = hubUrl ?? getHubUrl()

    const connection = new HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 1s, 2s, 4s, 8s, 16s, max 30s
          const delay = Math.min(
            1000 * Math.pow(2, retryContext.previousRetryCount),
            30_000
          )
          return delay
        },
      })
      .configureLogging(LogLevel.Warning)
      .build()

    let mounted = true

    connection.onreconnecting(() => { if (mounted) setStatus("reconnecting") })
    connection.onreconnected(() => { if (mounted) setStatus("connected") })
    connection.onclose(() => { if (mounted) setStatus("disconnected") })

    connectionRef.current = connection
    setStatus("connecting")

    connection
      .start()
      .then(() => { if (mounted) setStatus("connected") })
      .catch((err) => {
        console.error("SignalR connection failed:", err)
        if (mounted) setStatus("disconnected")
      })

    return () => {
      mounted = false
      connection.stop()
      connectionRef.current = null
    }
  }, [accessToken, enabled, hubUrl])

  const subscribe = useCallback(
    (method: string, handler: (...args: unknown[]) => void) => {
      connectionRef.current?.on(method, handler)
    },
    []
  )

  const unsubscribe = useCallback(
    (method: string, handler: (...args: unknown[]) => void) => {
      connectionRef.current?.off(method, handler)
    },
    []
  )

  const invoke = useCallback(
    async <T = void>(method: string, ...args: unknown[]): Promise<T> => {
      const conn = connectionRef.current
      if (!conn || conn.state !== HubConnectionState.Connected) {
        throw new Error("SignalR not connected")
      }
      return conn.invoke<T>(method, ...args)
    },
    []
  )

  const joinGroup = useCallback(
    async (group: string) => {
      await invoke("JoinGroup", group)
    },
    [invoke]
  )

  const leaveGroup = useCallback(
    async (group: string) => {
      await invoke("LeaveGroup", group)
    },
    [invoke]
  )

  return { status, subscribe, unsubscribe, invoke, joinGroup, leaveGroup }
}
