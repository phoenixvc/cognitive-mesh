import type React from "react"
// Global type definitions for Cognitive Mesh
export interface Module {
  id: string
  title: string
  content: React.ReactNode
  position: { x: number; y: number }
  isDocked: boolean
  type?: "neural" | "quantum" | "data" | "mesh"
  priority?: "low" | "medium" | "high" | "critical"
}

export interface DropZone {
  id: string
  bounds: DOMRect
  element: HTMLElement
  type?: string
}

export interface CommandContext {
  timestamp: number
  user: string
  command: string
  result?: any
  status: "pending" | "success" | "error"
}

export interface SystemStatus {
  operational: boolean
  modules: number
  docked: number
  performance: "optimal" | "good" | "degraded" | "critical"
  network: "connected" | "disconnected" | "limited"
  security: "secured" | "warning" | "breach"
}
