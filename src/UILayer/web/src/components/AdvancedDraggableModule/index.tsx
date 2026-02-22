"use client"

import { Maximize2, MoreVertical, Move, Pin } from "lucide-react"
import React, { useEffect, useRef, useState } from "react"

interface DropZone {
  id: string
  x: number
  y: number
  width: number
  height: number
  label: string
}

export interface AdvancedDraggableModuleProps {
  title: string
  children: React.ReactNode
  className?: string
  onDock?: () => void
  onFloat?: () => void
  isFloating?: boolean
  energyLevel?: number
  id: string
  onPositionChange?: (id: string, position: { x: number; y: number }) => void
}

export const AdvancedDraggableModule: React.FC<AdvancedDraggableModuleProps> = ({
  title,
  children,
  className = "",
  onDock,
  onFloat,
  isFloating = false,
  energyLevel = 0.5,
  id,
  onPositionChange,
}) => {
  const [isDragging, setIsDragging] = useState(false)
  const [isHovered, setIsHovered] = useState(false)
  const [position, setPosition] = useState({ x: 0, y: 0 })
  const [showDropZones, setShowDropZones] = useState(false)
  const [activeDropZone, setActiveDropZone] = useState<string | null>(null)
  const [showContextMenu, setShowContextMenu] = useState(false)
  const [isReducedMotion, setIsReducedMotion] = useState(false)

  const dragRef = useRef<HTMLDivElement>(null)
  const startPos = useRef({ x: 0, y: 0 })
  const contextMenuRef = useRef<HTMLDivElement>(null)

  // Drop zones for docking
  const dropZones: DropZone[] = [
    { id: "top", x: 0, y: 0, width: 100, height: 10, label: "Dock to Top" },
    { id: "bottom", x: 0, y: 90, width: 100, height: 10, label: "Dock to Bottom" },
    { id: "left", x: 0, y: 0, width: 10, height: 100, label: "Dock to Left" },
    { id: "right", x: 90, y: 0, width: 10, height: 100, label: "Dock to Right" },
    { id: "center", x: 40, y: 40, width: 20, height: 20, label: "Center Position" },
  ]

  // Check for reduced motion preference
  useEffect(() => {
    const mediaQuery = window.matchMedia("(prefers-reduced-motion: reduce)")
    setIsReducedMotion(mediaQuery.matches)

    const handleChange = (e: MediaQueryListEvent) => setIsReducedMotion(e.matches)
    mediaQuery.addEventListener("change", handleChange)
    return () => mediaQuery.removeEventListener("change", handleChange)
  }, [])

  const handleMouseDown = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget || (e.target as HTMLElement).closest(".drag-handle")) {
      setIsDragging(true)
      setShowDropZones(true)
      startPos.current = { x: e.clientX - position.x, y: e.clientY - position.y }

      // Add ARIA attributes for accessibility
      if (dragRef.current) {
        dragRef.current.setAttribute("aria-grabbed", "true")
      }
    }
  }

  const handleMouseMove = React.useCallback((e: MouseEvent) => {
    if (isDragging) {
      const newPosition = {
        x: e.clientX - startPos.current.x,
        y: e.clientY - startPos.current.y,
      }
      setPosition(newPosition)
      onPositionChange?.(id, newPosition)

      // Check for drop zone collision
      const rect = dragRef.current?.getBoundingClientRect()
      if (rect) {
        const centerX = (e.clientX / window.innerWidth) * 100
        const centerY = (e.clientY / window.innerHeight) * 100

        const hoveredZone = dropZones.find(
          (zone) =>
            centerX >= zone.x && centerX <= zone.x + zone.width && centerY >= zone.y && centerY <= zone.y + zone.height,
        )

        setActiveDropZone(hoveredZone?.id || null)
      }
    }
  }, [isDragging, onPositionChange, id, dropZones])

  const handleMouseUp = React.useCallback(() => {
    if (isDragging) {
      setIsDragging(false)
      setShowDropZones(false)
      setActiveDropZone(null)

      // Remove ARIA attributes
      if (dragRef.current) {
        dragRef.current.setAttribute("aria-grabbed", "false")
      }
    }
  }, [isDragging])

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.altKey) {
      switch (e.key) {
        case "ArrowUp":
          e.preventDefault()
          setPosition((prev) => ({ ...prev, y: prev.y - 10 }))
          break
        case "ArrowDown":
          e.preventDefault()
          setPosition((prev) => ({ ...prev, y: prev.y + 10 }))
          break
        case "ArrowLeft":
          e.preventDefault()
          setPosition((prev) => ({ ...prev, x: prev.x - 10 }))
          break
        case "ArrowRight":
          e.preventDefault()
          setPosition((prev) => ({ ...prev, x: prev.x + 10 }))
          break
      }
    }
  }

  useEffect(() => {
    if (isDragging) {
      document.addEventListener("mousemove", handleMouseMove)
      document.addEventListener("mouseup", handleMouseUp)
      return () => {
        document.removeEventListener("mousemove", handleMouseMove)
        document.removeEventListener("mouseup", handleMouseUp)
      }
    }
  }, [isDragging, handleMouseMove, handleMouseUp])

  // Close context menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (contextMenuRef.current && !contextMenuRef.current.contains(e.target as Node)) {
        setShowContextMenu(false)
      }
    }

    if (showContextMenu) {
      document.addEventListener("mousedown", handleClickOutside)
      return () => document.removeEventListener("mousedown", handleClickOutside)
    }
  }, [showContextMenu])

  const energyColor = energyLevel > 0.7 ? "cyan" : energyLevel > 0.4 ? "blue" : "purple"

  const getBlurRadius = () => {
    if (isDragging) return "backdrop-blur-lg"
    if (isHovered) return "backdrop-blur-md"
    return "backdrop-blur-sm"
  }

  return (
    <>
      {/* Drop Zone Overlay */}
      {showDropZones && (
        <div className="fixed inset-0 z-40 pointer-events-none">
          {dropZones.map((zone) => (
            <div
              key={zone.id}
              className={`absolute transition-all duration-200 ${
                activeDropZone === zone.id
                  ? "bg-cyan-500/30 border-2 border-cyan-400"
                  : "bg-slate-700/20 border border-slate-600/50"
              }`}
              style={{
                left: `${zone.x}%`,
                top: `${zone.y}%`,
                width: `${zone.width}%`,
                height: `${zone.height}%`,
              }}
            >
              {activeDropZone === zone.id && (
                <div className="absolute inset-0 flex items-center justify-center">
                  <span className="text-cyan-400 text-sm font-medium bg-slate-900/80 px-2 py-1 rounded">
                    {zone.label}
                  </span>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Main Module */}
      <div
        ref={dragRef}
        className={`
          relative ${getBlurRadius()} bg-slate-900/60 border rounded-xl shadow-2xl
          transition-all duration-300 cursor-move group
          ${isHovered || isDragging ? "shadow-cyan-500/30 border-cyan-500/50" : "border-slate-700/50"}
          ${isDragging ? "z-50 scale-105 rotate-2" : "z-10"}
          ${isFloating ? "fixed" : "relative"}
          ${className}
        `}
        style={
          isFloating
            ? {
                left: position.x,
                top: position.y,
                filter: isDragging ? "drop-shadow(0 25px 25px rgb(0 0 0 / 0.5))" : "none",
              }
            : {}
        }
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
        onKeyDown={handleKeyDown}
        tabIndex={0}
        role="dialog"
        aria-label={`${title} module`}
        aria-grabbed={isDragging}
        aria-dropeffect={showDropZones ? "move" : "none"}
      >
        {/* Layered Frosted Glass Effect */}
        <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-slate-800/40 via-slate-900/60 to-slate-950/80" />
        <div className="absolute inset-0 rounded-xl bg-gradient-to-t from-transparent via-slate-700/10 to-slate-600/20" />

        {/* Animated Energy Border with Pulsing */}
        <div
          className={`absolute inset-0 rounded-xl pointer-events-none transition-opacity duration-300 ${
            isReducedMotion ? "opacity-50" : ""
          }`}
          style={{
            background: `linear-gradient(90deg, 
              transparent 0%, 
              ${
                energyColor === "cyan"
                  ? "rgba(34, 211, 238, 0.4)"
                  : energyColor === "blue"
                    ? "rgba(59, 130, 246, 0.4)"
                    : "rgba(147, 51, 234, 0.4)"
              } 50%, 
              transparent 100%)`,
            animation: isReducedMotion
              ? "none"
              : `pulse-energy-${energyColor} ${2 + energyLevel * 2}s ease-in-out infinite`,
          }}
        />

        {/* Neon Gradient Glowing Strokes */}
        <div
          className={`absolute inset-0 rounded-xl border-2 pointer-events-none ${
            isReducedMotion ? "" : "animate-pulse"
          }`}
          style={{
            borderImage: `linear-gradient(45deg, 
            rgba(34, 211, 238, 0.6), 
            rgba(59, 130, 246, 0.4), 
            rgba(99, 102, 241, 0.6)
          ) 1`,
            animation: isReducedMotion ? "none" : `border-glow 3s ease-in-out infinite`,
          }}
        />

        {/* Module Header */}
        <div
          className="flex items-center justify-between p-4 border-b border-slate-700/50 cursor-grab active:cursor-grabbing drag-handle relative z-10"
          onMouseDown={handleMouseDown}
        >
          <div className="flex items-center space-x-3">
            {/* Six-dot drag handle */}
            <div className="grid grid-cols-2 gap-1 p-1 rounded hover:bg-slate-700/50 transition-all duration-200 group-hover:scale-110">
              {[...Array(6)].map((_, i) => (
                <div
                  key={i}
                  className={`w-1.5 h-1.5 rounded-full transition-all duration-200 ${
                    isHovered ? "bg-cyan-400 shadow-cyan-400/50 shadow-sm" : "bg-slate-500"
                  }`}
                />
              ))}
            </div>
            <h3 className="font-semibold text-white">{title}</h3>
            <div className={`w-2 h-2 rounded-full ${isReducedMotion ? "" : "animate-pulse"} bg-${energyColor}-400`} />
          </div>

          <div className="flex items-center space-x-2">
            <button
              onClick={() => setShowContextMenu(!showContextMenu)}
              className="p-1 rounded hover:bg-slate-700/50 transition-colors"
              aria-label="Module options"
            >
              <MoreVertical size={14} className="text-slate-400 hover:text-cyan-400" />
            </button>
            <button
              onClick={onFloat}
              className="p-1 rounded hover:bg-slate-700/50 transition-colors"
              aria-label="Float module"
            >
              <Maximize2 size={14} className="text-slate-400 hover:text-cyan-400" />
            </button>
            <button
              onClick={onDock}
              className="p-1 rounded hover:bg-slate-700/50 transition-colors"
              aria-label="Dock module"
            >
              <Pin size={14} className="text-slate-400 hover:text-cyan-400" />
            </button>
          </div>
        </div>

        {/* Context Menu */}
        {showContextMenu && (
          <div
            ref={contextMenuRef}
            className="absolute top-16 right-4 z-50 backdrop-blur-md bg-slate-900/90 
              border border-slate-700/50 rounded-lg shadow-xl min-w-48"
          >
            <div className="p-2 space-y-1">
              <button
                onClick={() => {
                  onFloat?.()
                  setShowContextMenu(false)
                }}
                className="w-full flex items-center space-x-2 p-2 hover:bg-slate-700/50 
                  rounded text-sm text-slate-300 hover:text-white transition-colors"
              >
                <Maximize2 size={16} />
                <span>Float Module</span>
              </button>
              <button
                onClick={() => {
                  onDock?.()
                  setShowContextMenu(false)
                }}
                className="w-full flex items-center space-x-2 p-2 hover:bg-slate-700/50 
                  rounded text-sm text-slate-300 hover:text-white transition-colors"
              >
                <Pin size={16} />
                <span>Dock Module</span>
              </button>
              <button
                className="w-full flex items-center space-x-2 p-2 hover:bg-slate-700/50 
                  rounded text-sm text-slate-300 hover:text-white transition-colors"
              >
                <Move size={16} />
                <span>Move (Alt+Arrows)</span>
              </button>
            </div>
          </div>
        )}

        {/* Module Content */}
        <div className="p-4 relative z-10">{children}</div>

        {/* Holographic Glow Effect */}
        {(isHovered || isDragging) && !isReducedMotion && (
          <div className="absolute inset-0 rounded-xl bg-gradient-to-r from-cyan-500/10 via-transparent to-blue-500/10 pointer-events-none animate-pulse" />
        )}

        {/* Spotlight Effect when Dragging */}
        {isDragging && (
          <div className="absolute inset-0 rounded-xl bg-gradient-radial from-cyan-400/20 via-transparent to-transparent pointer-events-none" />
        )}
      </div>

      {/* Custom CSS for animations */}
      <style>{`
        @keyframes pulse-energy-cyan {
          0%, 100% { opacity: 0.3; }
          50% { opacity: 0.8; }
        }
        @keyframes pulse-energy-blue {
          0%, 100% { opacity: 0.3; }
          50% { opacity: 0.7; }
        }
        @keyframes pulse-energy-purple {
          0%, 100% { opacity: 0.3; }
          50% { opacity: 0.6; }
        }
        @keyframes border-glow {
          0%, 100% { filter: drop-shadow(0 0 5px rgba(34, 211, 238, 0.3)); }
          50% { filter: drop-shadow(0 0 15px rgba(34, 211, 238, 0.6)); }
        }
        .bg-gradient-radial {
          background: radial-gradient(circle at center, var(--tw-gradient-stops));
        }
      `}</style>
    </>
  )
}

export default AdvancedDraggableModule
