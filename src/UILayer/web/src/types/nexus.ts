// types/nexus.ts

import type { LucideIcon } from "lucide-react"
import type React from "react"

// Represents an individual module in the Nexus
export interface NexusModule {
  /** Unique identifier */
  id: string
  /** Display label */
  label: string
  /** Icon component */
  icon: LucideIcon
  /** Tailwind color key, e.g. "cyan", "blue" */
  color: string
  /** Header text when the module is active */
  header: string
  /** Short description or tooltip */
  description: string
  /** Optional React component for detailed UI */
  component?: React.ComponentType<any>
  /** Any module-specific data */
  data?: any
}

// Defines the position and rotation of an icon in the Nexus
export interface IconPosition {
  /** Unique module ID (optional) */
  id?: string
  /** X offset or absolute X coordinate */
  x?: number
  /** Y offset or absolute Y coordinate */
  y?: number
  /** Rotation angle in radians */
  angle: number
  /** Radius from center (if orbital) */
  radius?: number
  /** Whether the icon is currently orbiting */
  isOrbiting?: boolean
  /** Original absolute coords before any transforms */
  originalPosition?: { x: number; y: number }
  /** Original module index (optional) */
  index?: number
}

// Audio settings for Nexus sounds
export interface AudioConfig {
  enabled: boolean
  volume: number
  sounds: {
    dock: string
    undock: string
    click: string
    snap?: string
    expand?: string
    collapse?: string
  }
}

// Audio state for the audio system
export interface AudioState {
  isEnabled: boolean
  volume: number
  sounds: Record<string, HTMLAudioElement | null>
}

// Configuration for drag-preview grid and snapping
export interface DragPreviewConfig {
  gridSize: number
  highlightColor: string
  snapThreshold: number
  showGrid: boolean
  animationDuration: number
}

// Configuration for the Nexus component
export interface NexusConfig {
  /** CSS transition duration (ms) */
  transitionDuration: number
  /** Orbit radius for icons */
  orbitRadius: number
  /** Orbit animation speed multiplier */
  orbitSpeed?: number
  /** Size presets for Nexus panel */
  sizes: {
    small: { width: number; height: number }
    medium: { width: number; height: number }
    large: { width: number; height: number }
  }
  /** Number of icons displayed */
  iconCount?: number
  /** Duration for orbit icon animations (ms) */
  orbitAnimationDuration: number
  /** Audio settings */
  audio: AudioConfig
  /** Drag preview settings */
  dragPreview: DragPreviewConfig
  /** Default modes */
  defaultOrbitMode: boolean
  defaultPinned: boolean
  defaultSizeMode: keyof NexusConfig["sizes"]
}

// Internal state representation for the Nexus panel
export interface NexusState {
  /** Whether panel is expanded */
  isExpanded: boolean
  /** Current size mode */
  expandSize: keyof NexusConfig["sizes"]
  /** Pinned (docked) state */
  isPinned: boolean
  /** Explicitly docked state */
  isDocked: boolean
  /** Floating (draggable) state */
  isFloating?: boolean
  /** Currently active module */
  activeModule: NexusModule | null
  /** Panel position */
  position?: { x: number; y: number }
  /** Panel dimensions */
  size?: { width: number; height: number }
  /** Orbit mode flag */
  orbitMode?: boolean
  /** Latest icon positions */
  iconPositions?: IconPosition[]
  /** Whether orbiting animation is underway */
  isOrbiting?: boolean
  /** Drag preview visibility */
  dragPreviewVisible?: boolean
  /** Currently highlighted drop zone */
  dropZoneHighlighted?: string | null
  /** Whether a drag is in progress */
  isDragging?: boolean
}

// Contextual actions for manipulating the Nexus
export interface NexusActions {
  setExpanded: (expanded: boolean) => void
  setExpandSize: (size: keyof NexusConfig["sizes"]) => void
  togglePin: () => void
  toggleDock: () => void
  setActiveModule: (module: NexusModule | null) => void
  loadModule: (moduleId: string) => void
  toggleOrbitMode: () => void
  setIconPositions: (positions: IconPosition[]) => void
  updateIconPosition: (index: number, position: IconPosition) => void
  cycleSizeUp: () => void
  cycleSizeDown: () => void
  startDrag: (target: "nexus" | "icon", id?: string) => void
  endDrag: () => void
  highlightDropZone: (zoneId: string | null) => void
}

// Drag state for enhanced mode
export interface DragState {
  isDragging: boolean
  draggedItem: {
    id: string
    type: "nexus" | "icon"
    data?: any
  } | null
  showPreviewGrid: boolean
  activeDropZone: string | null
}
