"use client"
import type { DragState } from "@/types/nexus"
import { useEffect, useState } from "react"

export interface DragPreviewProps {
  isVisible: boolean
  draggedItem: DragState["draggedItem"]
  onDragEnd: () => void
  onDropZoneEnter: (zoneId: string) => void
  onDropZoneLeave: () => void
  className?: string
}

export default function DragPreview({
  isVisible,
  draggedItem,
  onDragEnd,
  onDropZoneEnter,
  onDropZoneLeave,
  className = "",
}: DragPreviewProps) {
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 })
  const [isDragging, setIsDragging] = useState(false)

  // Track mouse position during drag
  useEffect(() => {
    if (!isVisible) return

    const handleMouseMove = (e: MouseEvent) => {
      setMousePosition({ x: e.clientX, y: e.clientY })
    }

    const handleMouseUp = () => {
      setIsDragging(false)
      onDragEnd()
    }

    document.addEventListener("mousemove", handleMouseMove)
    document.addEventListener("mouseup", handleMouseUp)

    setIsDragging(true)

    return () => {
      document.removeEventListener("mousemove", handleMouseMove)
      document.removeEventListener("mouseup", handleMouseUp)
    }
  }, [isVisible, onDragEnd])

  // Don't render if not visible
  if (!isVisible || !draggedItem) return null

  return (
    <>
      {/* Drag preview element */}
      <div
        className={`
          fixed pointer-events-none z-[9999] transition-all duration-200
          ${isDragging ? "opacity-80" : "opacity-60"}
          ${className}
        `}
        style={{
          left: mousePosition.x - 25,
          top: mousePosition.y - 25,
          transform: "translate(-50%, -50%)",
        }}
      >
        {/* Preview content based on dragged item type */}
        {draggedItem.type === "nexus" && (
          <div className="w-12 h-12 bg-gradient-to-br from-cyan-500/80 to-blue-600/80 rounded-xl backdrop-blur-md border border-cyan-400/50 shadow-lg shadow-cyan-500/30 flex items-center justify-center">
            <div className="w-6 h-6 bg-white/20 rounded-lg flex items-center justify-center">
              <div className="w-3 h-3 bg-white rounded-full" />
            </div>
          </div>
        )}

        {draggedItem.type === "icon" && (
          <div className="w-10 h-10 bg-gradient-to-br from-purple-500/80 to-pink-600/80 rounded-lg backdrop-blur-md border border-purple-400/50 shadow-lg shadow-purple-500/30 flex items-center justify-center">
            <div className="w-4 h-4 bg-white/20 rounded flex items-center justify-center">
              <div className="w-2 h-2 bg-white rounded" />
            </div>
          </div>
        )}

        {/* Drag trail effect */}
        <div className="absolute inset-0 rounded-xl opacity-30 animate-pulse">
          <div className="w-full h-full rounded-xl bg-gradient-to-r from-transparent via-white/20 to-transparent" />
        </div>
      </div>

      {/* Drop zone indicators */}
      <div className="fixed inset-0 pointer-events-none z-[9998]">
        {/* Grid overlay for drop zones */}
        <div className="absolute inset-0 grid grid-cols-3 grid-rows-3 gap-4 p-8">
          {Array.from({ length: 9 }, (_, index) => (
            <div
              key={index}
              className={`
                border-2 border-dashed rounded-lg transition-all duration-300
                ${isDragging 
                  ? "border-cyan-400/50 bg-cyan-500/10" 
                  : "border-transparent"
                }
                hover:border-cyan-400/30 hover:bg-cyan-500/5
              `}
              onMouseEnter={() => onDropZoneEnter(`zone-${index}`)}
              onMouseLeave={onDropZoneLeave}
            />
          ))}
        </div>

        {/* Center drop zone */}
        <div
          className={`
            absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2
            w-32 h-32 border-2 border-dashed rounded-full transition-all duration-300
            flex items-center justify-center
            ${isDragging 
              ? "border-green-400/50 bg-green-500/10" 
              : "border-transparent"
            }
            hover:border-green-400/30 hover:bg-green-500/5
          `}
          onMouseEnter={() => onDropZoneEnter("center-zone")}
          onMouseLeave={onDropZoneLeave}
        >
          <div className="text-xs text-slate-400 opacity-0 hover:opacity-100 transition-opacity">
            Drop Here
          </div>
        </div>
      </div>

      {/* Backdrop */}
      <div
        className={`
          fixed inset-0 bg-slate-950/20 backdrop-blur-sm pointer-events-none z-[9997]
          transition-opacity duration-300
          ${isDragging ? "opacity-100" : "opacity-0"}
        `}
      />
    </>
  )
} 