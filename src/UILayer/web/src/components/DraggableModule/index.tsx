"use client"
import React, { useState, useRef } from "react"
import { GripVertical, Maximize2, Pin } from "lucide-react"

interface DraggableModuleProps {
  title: string
  children: React.ReactNode
  className?: string
  onDock?: () => void
  onFloat?: () => void
  isFloating?: boolean
  energyLevel?: number
}

export default function DraggableModule({
  title,
  children,
  className = "",
  onDock,
  onFloat,
  isFloating = false,
  energyLevel = 0.5,
}: DraggableModuleProps) {
  const [isDragging, setIsDragging] = useState(false)
  const [isHovered, setIsHovered] = useState(false)
  const [position, setPosition] = useState({ x: 0, y: 0 })
  const dragRef = useRef<HTMLDivElement>(null)
  const startPos = useRef({ x: 0, y: 0 })

  const handleMouseDown = (e: React.MouseEvent) => {
    setIsDragging(true)
    startPos.current = { x: e.clientX - position.x, y: e.clientY - position.y }
  }

  const handleMouseMove = React.useCallback((e: MouseEvent) => {
    if (isDragging) {
      setPosition({
        x: e.clientX - startPos.current.x,
        y: e.clientY - startPos.current.y,
      })
    }
  }, [isDragging])

  const handleMouseUp = React.useCallback(() => {
    setIsDragging(false)
  }, [])

  React.useEffect(() => {
    if (isDragging) {
      document.addEventListener("mousemove", handleMouseMove)
      document.addEventListener("mouseup", handleMouseUp)
      return () => {
        document.removeEventListener("mousemove", handleMouseMove)
        document.removeEventListener("mouseup", handleMouseUp)
      }
    }
  }, [isDragging, handleMouseMove, handleMouseUp])

  const energyColor = energyLevel > 0.7 ? "cyan" : energyLevel > 0.4 ? "blue" : "purple"
  const energyIntensity = Math.floor(energyLevel * 100)

  return (
    <div
      ref={dragRef}
      className={`
        relative backdrop-blur-md bg-slate-900/60 border rounded-xl shadow-2xl
        transition-all duration-300 cursor-move group
        ${isHovered || isDragging ? "shadow-cyan-500/30 border-cyan-500/50" : "border-slate-700/50"}
        ${isDragging ? "z-50 scale-105" : "z-10"}
        ${isFloating ? "fixed" : "relative"}
        ${className}
      `}
      style={
        isFloating
          ? {
              left: position.x,
              top: position.y,
              transform: isDragging ? "rotate(2deg)" : "rotate(0deg)",
            }
          : {}
      }
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Animated Energy Border */}
      <div
        className={`absolute inset-0 rounded-xl opacity-${energyIntensity} pointer-events-none`}
        style={{
          background: `linear-gradient(90deg, 
            transparent 0%, 
            ${
              energyColor === "cyan"
                ? "rgba(34, 211, 238, 0.3)"
                : energyColor === "blue"
                  ? "rgba(59, 130, 246, 0.3)"
                  : "rgba(147, 51, 234, 0.3)"
            } 50%, 
            transparent 100%)`,
          animation: `pulse-energy-${energyColor} 2s ease-in-out infinite`,
        }}
      />

      {/* Module Header */}
      <div
        className="flex items-center justify-between p-4 border-b border-slate-700/50 cursor-grab active:cursor-grabbing"
        onMouseDown={handleMouseDown}
      >
        <div className="flex items-center space-x-3">
          <GripVertical size={16} className="text-slate-400 group-hover:text-cyan-400 transition-colors" />
          <h3 className="font-semibold text-white">{title}</h3>
          <div className={`w-2 h-2 rounded-full animate-pulse bg-${energyColor}-400`} />
        </div>

        <div className="flex items-center space-x-2">
          <button onClick={onFloat} className="p-1 rounded hover:bg-slate-700/50 transition-colors">
            <Maximize2 size={14} className="text-slate-400 hover:text-cyan-400" />
          </button>
          <button onClick={onDock} className="p-1 rounded hover:bg-slate-700/50 transition-colors">
            <Pin size={14} className="text-slate-400 hover:text-cyan-400" />
          </button>
        </div>
      </div>

      {/* Module Content */}
      <div className="p-4">{children}</div>

      {/* Holographic Glow Effect */}
      {(isHovered || isDragging) && (
        <div className="absolute inset-0 rounded-xl bg-gradient-to-r from-cyan-500/10 via-transparent to-blue-500/10 pointer-events-none" />
      )}
    </div>
  )
}

export { DraggableModule }
export type { DraggableModuleProps }
