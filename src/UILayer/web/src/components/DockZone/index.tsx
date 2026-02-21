"use client"
import DraggableComponent from "@/components/DraggableComponent"
import { useDragDrop } from "@/contexts/DragDropContext"
import { motion } from "framer-motion"
import { GripVertical, Maximize2, Minimize2, Package } from "lucide-react"
import React, { useCallback, useEffect, useRef, useState } from "react"

interface DockZoneProps {
  id: string
  label: string
  maxItems?: number
  allowedSizes?: ("small" | "medium" | "large")[]
  className?: string
  children?: React.ReactNode
  isResizable?: boolean
  minWidth?: number
  minHeight?: number
  initialWidth?: number
  initialHeight?: number
  handleStyle?: "grip" | "anchor" | "titlebar" | "ring" | "invisible"
}

// Drag handle component supporting 5 styles
const DockZoneDragHandle: React.FC<{
  type: "grip" | "anchor" | "titlebar" | "ring" | "invisible"
  onDragStart: (e: React.MouseEvent) => void
  onDoubleClick?: (e: React.MouseEvent) => void
  isDragging?: boolean
}> = ({ type, onDragStart, onDoubleClick, isDragging }) => {
  switch (type) {
    case "grip":
      return (
        <div
          className="absolute left-4 top-2 flex flex-col items-center gap-0.5 cursor-move select-none z-50"
          onMouseDown={onDragStart}
          onDoubleClick={onDoubleClick}
          title="Drag to move panel. Double-click to dock."
        >
          <div className="w-8 h-1 rounded bg-cyan-400/80 shadow-cyan-400/40 shadow-md grip-bar animate-grip-glow" />
          <div className="w-8 h-1 rounded bg-cyan-400/60 shadow-cyan-400/30 shadow-md grip-bar animate-grip-glow delay-75" />
          <div className="w-8 h-1 rounded bg-cyan-400/40 shadow-cyan-400/20 shadow grip-bar animate-grip-glow delay-150" />
        </div>
      )
    case "anchor":
      return (
        <div
          className="absolute left-1/2 -translate-x-1/2 top-0 z-50 cursor-move select-none"
          style={{ marginTop: '-18px' }}
          onMouseDown={onDragStart}
          onDoubleClick={onDoubleClick}
          title="Drag to move panel. Double-click to dock."
        >
          <div className="w-7 h-7 rounded-full bg-cyan-400/30 border-2 border-cyan-400 shadow-cyan-400/40 shadow-lg flex items-center justify-center animate-anchor-glow anchor-node">
            <div className="w-3 h-3 rounded-full bg-cyan-300 animate-pulse" />
          </div>
          {isDragging && (
            <svg className="anchor-tether" width="40" height="20"><line x1="8" y1="8" x2="32" y2="16" stroke="#06b6d4" strokeWidth="2" strokeDasharray="4 2" /></svg>
          )}
        </div>
      )
    case "titlebar":
      return (
        <div
          className="absolute left-0 top-0 w-full h-6 flex items-center justify-center cursor-move select-none z-50"
          onMouseDown={onDragStart}
          onDoubleClick={onDoubleClick}
          title="Drag to move panel. Double-click to dock."
        >
          <div className="w-[90%] h-2 rounded-full bg-gradient-to-r from-cyan-400 via-blue-500 to-purple-500 shadow-lg titlebar-circuit animate-titlebar-glow" />
        </div>
      )
    case "ring":
      return (
        <div
          className="absolute right-4 top-2 z-50 flex flex-col items-end select-none"
          title="Drag to move panel. Double-click to dock."
        >
          <div className="relative w-14 h-14 flex items-center justify-center">
            {/* Radial ring */}
            <div className="absolute inset-0 rounded-full border-2 border-cyan-400/60 ring-glow animate-ring-glow" />
            {/* Center drag handle */}
            <div
              className="w-7 h-7 rounded-full bg-cyan-400/30 border-2 border-cyan-400 shadow-cyan-400/40 shadow-lg flex items-center justify-center cursor-move ring-center animate-ring-center-glow"
              onMouseDown={onDragStart}
              onDoubleClick={onDoubleClick}
            >
              <div className="w-3 h-3 rounded-full bg-cyan-300 animate-pulse" />
            </div>
            {/* Placeholder rim icons */}
            <div className="absolute left-1/2 top-0 -translate-x-1/2 -translate-y-1/2 w-4 h-4 bg-blue-500/40 rounded-full border border-blue-400 shadow" />
            <div className="absolute right-0 top-1/2 -translate-y-1/2 w-4 h-4 bg-purple-500/40 rounded-full border border-purple-400 shadow" />
            <div className="absolute left-1/2 bottom-0 -translate-x-1/2 translate-y-1/2 w-4 h-4 bg-green-500/40 rounded-full border border-green-400 shadow" />
            <div className="absolute left-0 top-1/2 -translate-y-1/2 w-4 h-4 bg-cyan-500/40 rounded-full border border-cyan-400 shadow" />
          </div>
        </div>
      )
    case "invisible":
      return (
        <div
          className="absolute left-0 top-0 w-full h-5 z-50 cursor-move select-none invisible-handle"
          onMouseDown={onDragStart}
          onDoubleClick={onDoubleClick}
          title="Drag to move panel. Double-click to dock."
        >
          <div className="w-full h-1 rounded-full bg-cyan-400/0 invisible-bar transition-all duration-200" />
        </div>
      )
    default:
      return null
  }
}

// Grid overlay for alignment
const GridOverlay: React.FC<{ visible: boolean }> = ({ visible }) => {
  if (!visible) return null;
  return (
    <div
      className="fixed inset-0 pointer-events-none z-40"
      style={{
        backgroundImage: `
          linear-gradient(rgba(6,182,212,0.08) 1px, transparent 1px),
          linear-gradient(90deg, rgba(6,182,212,0.08) 1px, transparent 1px)
        `,
        backgroundSize: "20px 20px",
        opacity: 0.7,
      }}
    />
  );
};

export const DockZone: React.FC<DockZoneProps> = ({
  id,
  label,
  maxItems,
  allowedSizes,
  className = "",
  children,
  isResizable = true,
  minWidth = 200,
  minHeight = 150,
  initialWidth = 400,
  initialHeight = 300,
  handleStyle = "grip",
}) => {
  const {
    registerDockZone,
    unregisterDockZone,
    updateDockZoneBounds,
    resizeDockZone,
    activeDockZone,
    isDragging,
    dockZones,
    getDockedItemsForZone,
    dockItem,
    draggedItem,
    dockZoneStack,
    insertDockZoneIntoStack,
  } = useDragDrop()

  const zoneRef = useRef<HTMLDivElement>(null)
  const [isResizing, setIsResizing] = useState(false)
  const [resizeHandle, setResizeHandle] = useState<string | null>(null)
  const [dimensions, setDimensions] = useState({ width: initialWidth, height: initialHeight })
  const boundsUpdateTimeoutRef = useRef<NodeJS.Timeout | null>(null)
  const resizeStartRef = useRef({ x: 0, y: 0, width: 0, height: 0 })
  const [isFloating, setIsFloating] = useState(false)
  const [floatPosition, setFloatPosition] = useState<{ x: number; y: number }>({ x: 200, y: 200 })
  const dragOffset = useRef<{ x: number; y: number }>({ x: 0, y: 0 })
  const [showGrid, setShowGrid] = useState(false)
  const GRID_SIZE = 20

  const isActive = activeDockZone === id
  const dockedItems = getDockedItemsForZone(id)

  // Determine if this DockZone will be affected by a stack insertion
  let isAffectedByStack = false;
  if (isDragging && draggedItem && dockZoneStack && dockZoneStack.length > 0 && activeDockZone && activeDockZone !== id) {
    const activeIdx = dockZoneStack.indexOf(activeDockZone);
    const thisIdx = dockZoneStack.indexOf(id);
    // If this DockZone comes after the activeDockZone in the stack, it will be pushed
    if (thisIdx > activeIdx && activeIdx !== -1) {
      isAffectedByStack = true;
    }
  }

  // Register zone once on mount
  useEffect(() => {
    registerDockZone({
      id,
      label,
      maxItems,
      allowedSizes,
      items: [],
      isResizable,
      minWidth,
      minHeight,
    })

    return () => unregisterDockZone(id)
  }, [id, label, maxItems, allowedSizes, isResizable, minWidth, minHeight, registerDockZone, unregisterDockZone])

  // Debounced bounds update function
  const debouncedUpdateBounds = useCallback(() => {
    if (boundsUpdateTimeoutRef.current) {
      clearTimeout(boundsUpdateTimeoutRef.current)
    }

    boundsUpdateTimeoutRef.current = setTimeout(() => {
      if (zoneRef.current) {
        const rect = zoneRef.current.getBoundingClientRect()
        updateDockZoneBounds(id, {
          x: rect.left,
          y: rect.top,
          width: rect.width,
          height: rect.height,
        })
      }
    }, 100)
  }, [id, updateDockZoneBounds])

  // Handle resize start
  const handleResizeStart = useCallback(
    (e: React.MouseEvent, handle: string) => {
      if (!isResizable) return

      e.preventDefault()
      e.stopPropagation()

      setIsResizing(true)
      setResizeHandle(handle)

      const rect = zoneRef.current?.getBoundingClientRect()
      if (rect) {
        resizeStartRef.current = {
          x: e.clientX,
          y: e.clientY,
          width: rect.width,
          height: rect.height,
        }
      }
    },
    [isResizable],
  )

  // Handle resize move
  useEffect(() => {
    if (!isResizing || !resizeHandle) return

    const handleMouseMove = (e: MouseEvent) => {
      const deltaX = e.clientX - resizeStartRef.current.x
      const deltaY = e.clientY - resizeStartRef.current.y

      let newWidth = resizeStartRef.current.width
      let newHeight = resizeStartRef.current.height

      if (resizeHandle.includes("right")) {
        newWidth = Math.max(minWidth, resizeStartRef.current.width + deltaX)
      }
      if (resizeHandle.includes("left")) {
        newWidth = Math.max(minWidth, resizeStartRef.current.width - deltaX)
      }
      if (resizeHandle.includes("bottom")) {
        newHeight = Math.max(minHeight, resizeStartRef.current.height + deltaY)
      }
      if (resizeHandle.includes("top")) {
        newHeight = Math.max(minHeight, resizeStartRef.current.height - deltaY)
      }

      setDimensions({ width: newWidth, height: newHeight })

      if (zoneRef.current) {
        const rect = zoneRef.current.getBoundingClientRect()
        resizeDockZone(id, {
          x: rect.left,
          y: rect.top,
          width: newWidth,
          height: newHeight,
        })
      }
    }

    const handleMouseUp = () => {
      setIsResizing(false)
      setResizeHandle(null)
      debouncedUpdateBounds()
    }

    document.addEventListener("mousemove", handleMouseMove)
    document.addEventListener("mouseup", handleMouseUp)

    return () => {
      document.removeEventListener("mousemove", handleMouseMove)
      document.removeEventListener("mouseup", handleMouseUp)
    }
  }, [isResizing, resizeHandle, minWidth, minHeight, id, resizeDockZone, debouncedUpdateBounds])

  // Update bounds with ResizeObserver and debouncing
  useEffect(() => {
    if (!zoneRef.current) return

    debouncedUpdateBounds()

    const resizeObserver = new ResizeObserver(debouncedUpdateBounds)
    resizeObserver.observe(zoneRef.current)

    const handleScroll = debouncedUpdateBounds
    window.addEventListener("scroll", handleScroll, { passive: true })

    return () => {
      resizeObserver.disconnect()
      window.removeEventListener("scroll", handleScroll)
      if (boundsUpdateTimeoutRef.current) {
        clearTimeout(boundsUpdateTimeoutRef.current)
      }
    }
  }, [debouncedUpdateBounds])

  // New: Mouse event handlers for floating drag
  const handleGripMouseDown = (e: React.MouseEvent) => {
    if (!isFloating) return
    e.preventDefault()
    dragOffset.current = {
      x: e.clientX - floatPosition.x,
      y: e.clientY - floatPosition.y,
    }
    setShowGrid(true)
    document.addEventListener("mousemove", handleGripMouseMove)
    document.addEventListener("mouseup", handleGripMouseUp)
  }
  const handleGripMouseMove = (e: MouseEvent) => {
    setFloatPosition({
      x: e.clientX - dragOffset.current.x,
      y: e.clientY - dragOffset.current.y,
    })
  }
  const handleGripMouseUp = () => {
    setFloatPosition((pos) => {
      const snappedX = Math.round(pos.x / GRID_SIZE) * GRID_SIZE
      const snappedY = Math.round(pos.y / GRID_SIZE) * GRID_SIZE
      const el = zoneRef.current
      if (el) {
        el.style.transition = 'left 0.25s cubic-bezier(0.22, 1, 0.36, 1), top 0.25s cubic-bezier(0.22, 1, 0.36, 1)'
        el.style.left = `${snappedX}px`
        el.style.top = `${snappedY}px`
        setTimeout(() => {
          if (el) el.style.transition = ''
        }, 300)
      }
      setTimeout(() => setShowGrid(false), 300)
      return { x: snappedX, y: snappedY }
    })
    document.removeEventListener("mousemove", handleGripMouseMove)
    document.removeEventListener("mouseup", handleGripMouseUp)
  }

  // Example: Toggle floating/undocked state (for demo, could be via context menu or button)
  // In real use, this would be controlled by docking logic
  // For now, double-click the grip to toggle floating
  const handleGripDoubleClick = () => {
    setIsFloating(false)
    setFloatPosition({ x: 200, y: 200 })
  }

  const zone = dockZones[id]

  // Helper function to safely format item titles
  const formatItemTitle = (itemId: string) => {
    if (!itemId || typeof itemId !== "string") {
      return "Unknown Item"
    }
    return itemId.replace(/[-_]/g, " ").replace(/\b\w/g, (l) => l.toUpperCase())
  }

  // Drop handler for docking components
  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    if (draggedItem && !draggedItem.isDocked) {
      dockItem(draggedItem.id, id);
    }
  };
  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  // Helper: Snap to nearest grid point
  const snapToGrid = (value: number) => Math.round(value / GRID_SIZE) * GRID_SIZE;

  // On drag end, snap to grid or stack
  const handleDragEnd = () => {
    if (isFloating && zoneRef.current) {
      // Get this zone's rect
      const rect = zoneRef.current.getBoundingClientRect();
      // Get all other zones' rects from context
      const otherZones = Object.entries(dockZones).filter(([zoneId]) => zoneId !== id);
      let overlappedZoneId: string | null = null;
      let overlapIndex = -1;
      for (let i = 0; i < otherZones.length; i++) {
        const [zoneId, zone] = otherZones[i];
        const zRect = zone.bounds;
        // Check for any intersection
        const overlap =
          rect.left < zRect.x + zRect.width &&
          rect.left + rect.width > zRect.x &&
          rect.top < zRect.y + zRect.height &&
          rect.top + rect.height > zRect.y;
        if (overlap) {
          overlappedZoneId = zoneId;
          overlapIndex = i;
          break;
        }
      }
      if (overlappedZoneId) {
        // Snap into stack at the overlapped position
        // (for simplicity, insert after the overlapped zone)
        insertDockZoneIntoStack(id, overlapIndex + 1);
        setIsFloating(false);
      } else {
        // Snap to grid
        const snappedX = snapToGrid(floatPosition.x);
        const snappedY = snapToGrid(floatPosition.y);
        setFloatPosition({ x: snappedX, y: snappedY });
      }
    }
  };

  return (
    <>
      {/* Grid overlay only when dragging this DockZone */}
      <GridOverlay visible={isFloating && isDragging} />
      <motion.div
        ref={zoneRef}
        className={`
          relative transition-all duration-300 rounded-xl border-2 border-dashed
          ${isActive && isDragging ? "border-green-500/70 bg-green-500/10" : "border-slate-600/30"}
          ${isDragging ? "border-opacity-100" : "border-opacity-50"}
          ${isResizing ? "select-none" : ""}
          ${isFloating ? "z-50 shadow-cyan-500/40" : ""}
          ${isAffectedByStack ? "ring-4 ring-cyan-400/60 animate-pulse" : ""}
          ${className}
        `}
        style={{
          width: isResizable ? dimensions.width : undefined,
          height: isResizable ? dimensions.height : undefined,
          minWidth: minWidth,
          minHeight: minHeight,
          position: isFloating ? "fixed" : undefined,
          left: isFloating ? floatPosition.x : undefined,
          top: isFloating ? floatPosition.y : undefined,
          cursor: isFloating ? "move" : undefined,
        }}
        animate={isFloating ? { x: floatPosition.x, y: floatPosition.y } : { x: 0, y: 0 }}
        transition={{ type: 'spring', stiffness: 180, damping: 14, mass: 1.2 }}
        drag={isFloating}
        dragMomentum={false}
        onDrag={(e, info) => {
          if (isFloating) {
            setFloatPosition({ x: info.point.x, y: info.point.y })
          }
        }}
        onDragEnd={handleDragEnd}
        onDrop={handleDrop}
        onDragOver={handleDragOver}
      >
        {/* Real-time glowing stack slot highlight */}
        {isActive && isDragging && (
          <div className="absolute inset-0 z-[10001] pointer-events-none animate-pulse border-4 border-cyan-400/80 rounded-xl shadow-cyan-400/40 shadow-xl stack-slot-glow" />
        )}
        
        {/* Animated slot bar showing insertion point */}
        {isActive && isDragging && (
          <div className="absolute inset-0 z-[10002] pointer-events-none">
            {/* Top slot indicator */}
            <div className="absolute top-0 left-1/2 -translate-x-1/2 w-16 h-1 bg-gradient-to-r from-transparent via-cyan-400 to-transparent rounded-full animate-pulse shadow-cyan-400/60 shadow-lg" />
            {/* Bottom slot indicator */}
            <div className="absolute bottom-0 left-1/2 -translate-x-1/2 w-16 h-1 bg-gradient-to-r from-transparent via-cyan-400 to-transparent rounded-full animate-pulse shadow-cyan-400/60 shadow-lg" />
            {/* Left slot indicator */}
            <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-16 bg-gradient-to-b from-transparent via-cyan-400 to-transparent rounded-full animate-pulse shadow-cyan-400/60 shadow-lg" />
            {/* Right slot indicator */}
            <div className="absolute right-0 top-1/2 -translate-y-1/2 w-1 h-16 bg-gradient-to-b from-transparent via-cyan-400 to-transparent rounded-full animate-pulse shadow-cyan-400/60 shadow-lg" />
            
            {/* Corner slot indicators */}
            <div className="absolute top-2 left-2 w-3 h-3 bg-cyan-400/80 rounded-full animate-ping" />
            <div className="absolute top-2 right-2 w-3 h-3 bg-cyan-400/80 rounded-full animate-ping delay-75" />
            <div className="absolute bottom-2 left-2 w-3 h-3 bg-cyan-400/80 rounded-full animate-ping delay-150" />
            <div className="absolute bottom-2 right-2 w-3 h-3 bg-cyan-400/80 rounded-full animate-ping delay-200" />
          </div>
        )}
        
        {/* Granular stack slot preview with custom colors */}
        {isActive && isDragging && (
          <div className="absolute inset-0 z-[10003] pointer-events-none">
            {/* Slot preview grid */}
            <div className="absolute inset-2 border-2 border-dashed border-purple-400/60 rounded-lg animate-pulse" />
            <div className="absolute inset-4 border-2 border-dashed border-blue-400/60 rounded-lg animate-pulse delay-100" />
            <div className="absolute inset-6 border-2 border-dashed border-green-400/60 rounded-lg animate-pulse delay-200" />
            
            {/* Center insertion point */}
            <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-8 h-8 bg-gradient-to-br from-cyan-400 via-purple-400 to-blue-400 rounded-full animate-spin shadow-cyan-400/80 shadow-xl" />
          </div>
        )}
        {/* Gridlines overlay for floating drag */}
        {isFloating && showGrid && (
          <div className="fixed inset-0 pointer-events-none z-[9998]">
            <svg width="100vw" height="100vh" className="absolute inset-0 w-full h-full">
              {[...Array(Math.ceil(window.innerWidth / GRID_SIZE)).keys()].map(i => (
                <line
                  key={`v-${i}`}
                  x1={i * GRID_SIZE}
                  y1={0}
                  x2={i * GRID_SIZE}
                  y2={window.innerHeight}
                  stroke="#0ea5e9"
                  strokeWidth={0.5}
                  opacity={0.12}
                />
              ))}
              {[...Array(Math.ceil(window.innerHeight / GRID_SIZE)).keys()].map(i => (
                <line
                  key={`h-${i}`}
                  x1={0}
                  y1={i * GRID_SIZE}
                  x2={window.innerWidth}
                  y2={i * GRID_SIZE}
                  stroke="#0ea5e9"
                  strokeWidth={0.5}
                  opacity={0.12}
                />
              ))}
            </svg>
          </div>
        )}
        {/* Drag Handle Styles */}
        {isFloating && (
          <div className="absolute inset-0 pointer-events-none z-50">
            <div className="pointer-events-auto">
              <DockZoneDragHandle 
                type={handleStyle} 
                onDragStart={handleGripMouseDown} 
                onDoubleClick={handleGripDoubleClick} 
                isDragging={isDragging} 
              />
            </div>
          </div>
        )}
        {/* Zone Header */}
        <div className="flex items-center justify-between p-3 border-b border-slate-700/30 bg-slate-800/20 rounded-t-xl z-[10000]" style={{ pointerEvents: 'auto' }}>
          <div className="flex items-center space-x-2">
            <GripVertical size={14} className="text-slate-500" />
            <Package size={14} className="text-cyan-400" />
            <h3 className="text-sm font-semibold text-slate-300">{label}</h3>
            {maxItems && (
              <span className="text-xs text-slate-500 bg-slate-700/50 px-2 py-1 rounded">
                {zone?.items.length || 0}/{maxItems}
              </span>
            )}
          </div>

          {isResizable && (
            <div className="flex items-center space-x-1">
              <button
                onClick={() => setDimensions({ width: minWidth, height: minHeight })}
                className="p-1 rounded hover:bg-slate-700/50 transition-colors"
                title="Minimize"
              >
                <Minimize2 size={12} className="text-slate-400 hover:text-cyan-400" />
              </button>
              <button
                onClick={() => setDimensions({ width: 600, height: 400 })}
                className="p-1 rounded hover:bg-slate-700/50 transition-colors"
                title="Maximize"
              >
                <Maximize2 size={12} className="text-slate-400 hover:text-cyan-400" />
              </button>
            </div>
          )}
        </div>

        {/* Zone Content */}
        <div className="p-4 overflow-auto scrollbar-custom" style={{ height: `calc(100% - 60px)` }}>
          {/* Render docked items with proper error handling */}
          <div className="grid gap-4 auto-fit-grid">
            {dockedItems
              .filter((item) => item && item.id) // Filter out invalid items
              .map((item) => {
                // Special handling for command nexus
                if (item.id === "command-nexus") {
                  return (
                    <div key={item.id} className="w-full">
                      {/* Render the nexus content directly when docked */}
                      <div className="backdrop-blur-md bg-slate-900/90 border-2 border-slate-500/30 rounded-xl shadow-xl p-4">
                        <div className="text-center mb-4">
                          <div className="text-xl font-bold bg-gradient-to-r from-cyan-400 via-blue-400 to-purple-400 bg-clip-text text-transparent mb-2">
                            COMMAND NEXUS
                          </div>
                          <div className="text-sm text-slate-400 mb-2">Central Command Interface</div>
                          <div className="flex items-center justify-center space-x-2">
                            <div className="w-2 h-2 bg-cyan-400 rounded-full animate-pulse" />
                            <span className="text-xs text-cyan-400">Neural Link Active</span>
                          </div>
                        </div>
                        <div className="text-center">
                          <button className="px-4 py-2 bg-slate-700/50 text-slate-300 hover:text-cyan-400 hover:bg-slate-600/50 rounded-lg transition-all duration-200 text-sm">
                            Click to Expand
                          </button>
                        </div>
                      </div>
                    </div>
                  )
                }
                
                return (
                  <DraggableComponent
                    key={item.id}
                    id={item.id}
                    title={formatItemTitle(item.id)}
                    type={item.type || "unknown"}
                    isDocked={true}
                  >
                    <div>Docked content for {formatItemTitle(item.id)}</div>
                  </DraggableComponent>
                )
              })}
          </div>

          {children}

          {/* Empty State */}
          {dockedItems.length === 0 && !children && (
            <div className="flex items-center justify-center h-32 text-slate-500 text-sm">
              <div className="text-center">
                <div className="mb-2 text-2xl">📦</div>
                <div>Drop components here</div>
                <div className="text-xs mt-1 text-slate-600">
                  {allowedSizes ? `Accepts: ${allowedSizes.join(", ")}` : "Accepts all sizes"}
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Resize Handles */}
        {isResizable && (
          <>
            {/* Corner handles */}
            <div
              className="absolute top-0 right-0 w-3 h-3 cursor-ne-resize opacity-0 hover:opacity-100 transition-opacity"
              onMouseDown={(e) => handleResizeStart(e, "top-right")}
            >
              <div className="w-full h-full bg-cyan-400/50 rounded-bl-lg" />
            </div>
            <div
              className="absolute bottom-0 right-0 w-3 h-3 cursor-se-resize opacity-0 hover:opacity-100 transition-opacity"
              onMouseDown={(e) => handleResizeStart(e, "bottom-right")}
            >
              <div className="w-full h-full bg-cyan-400/50 rounded-tl-lg" />
            </div>
            <div
              className="absolute bottom-0 left-0 w-3 h-3 cursor-sw-resize opacity-0 hover:opacity-100 transition-opacity"
              onMouseDown={(e) => handleResizeStart(e, "bottom-left")}
            >
              <div className="w-full h-full bg-cyan-400/50 rounded-tr-lg" />
            </div>
            <div
              className="absolute top-0 left-0 w-3 h-3 cursor-nw-resize opacity-0 hover:opacity-100 transition-opacity"
              onMouseDown={(e) => handleResizeStart(e, "top-left")}
            >
              <div className="w-full h-full bg-cyan-400/50 rounded-br-lg" />
            </div>

            {/* Edge handles */}
            <div
              className="absolute top-0 left-3 right-3 h-1 cursor-n-resize opacity-0 hover:opacity-100 transition-opacity"
              onMouseDown={(e) => handleResizeStart(e, "top")}
            >
              <div className="w-full h-full bg-cyan-400/30 rounded-b" />
            </div>
            <div
              className="absolute bottom-0 left-3 right-3 h-1 cursor-s-resize opacity-0 hover:opacity-100 transition-opacity"
              onMouseDown={(e) => handleResizeStart(e, "bottom")}
            >
              <div className="w-full h-full bg-cyan-400/30 rounded-t" />
            </div>
            <div
              className="absolute left-0 top-3 bottom-3 w-1 cursor-w-resize opacity-0 hover:opacity-100 transition-opacity"
              onMouseDown={(e) => handleResizeStart(e, "left")}
            >
              <div className="w-full h-full bg-cyan-400/30 rounded-r" />
            </div>
            <div
              className="absolute right-0 top-3 bottom-3 w-1 cursor-e-resize opacity-0 hover:opacity-100 transition-opacity"
              onMouseDown={(e) => handleResizeStart(e, "right")}
            >
              <div className="w-full h-full bg-cyan-400/30 rounded-l" />
            </div>
          </>
        )}

        {/* Active Drop Indicator */}
        {isActive && isDragging && (
          <div className="absolute inset-0 bg-gradient-to-br from-green-500/20 to-cyan-500/20 rounded-xl pointer-events-none animate-pulse border-2 border-green-400/50" />
        )}

        {/* Resize indicator */}
        {isResizing && (
          <div className="absolute top-2 left-2 bg-cyan-500/90 text-white px-2 py-1 rounded text-xs font-mono">
            {dimensions.width} × {dimensions.height}
          </div>
        )}
      </motion.div>
    </>
  )
}

export default DockZone
