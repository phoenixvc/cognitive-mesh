"use client"
import React, { useCallback, useEffect, useRef, useState } from "react"

import { useDragDrop } from "@/contexts/DragDropContext"
import { useAudioSystem } from "@/hooks/useAudioSystem"
import { useNexusDrag } from "@/hooks/useNexusDrag"
import type { NexusModule, NexusState } from "@/types/nexus"
import {
    Activity,
    BarChart3,
    Brain,
    GripVertical,
    Maximize2,
    Mic,
    Minimize2,
    Move,
    Pin,
    PinOff,
    Send,
    Shield,
    Users,
    Zap
} from "lucide-react"
import DragPreview from "./DragPreview"
import OrbitalIcons from "./OrbitalIcons"

export type NexusMode = "command" | "draggable" | "enhanced"
export type NexusSize = "small" | "medium" | "large" | "x-large"

export interface NexusProps {
  // Core functionality
  onPromptSubmit?: (prompt: string) => void
  isVoiceActive?: boolean
  onVoiceToggle?: () => void
  
  // Mode and behavior
  mode?: NexusMode
  isDocked?: boolean
  isExpanded?: boolean
  isPinned?: boolean
  
  // Positioning and sizing
  initialPosition?: { x: number; y: number }
  initialSize?: NexusSize
  soundVolume?: number
  
  // Enhanced features
  showOrbitalIcons?: boolean
  showDragPreview?: boolean
  enableAudio?: boolean
  
  // Callbacks
  onDock?: () => void
  onUndock?: () => void
  onExpand?: () => void
  onCollapse?: () => void
  onPin?: () => void
  onUnpin?: () => void
  onDragStart?: () => void
  onDragEnd?: () => void
}

export default function Nexus({
  // Core functionality
  onPromptSubmit,
  isVoiceActive = false,
  onVoiceToggle,
  
  // Mode and behavior
  mode = "draggable",
  isDocked: propIsDocked = false,
  isExpanded: propIsExpanded = false,
  isPinned: propIsPinned = true,
  
  // Positioning and sizing
  initialPosition = { x: 400, y: 300 },
  initialSize = "large",
  soundVolume = 0.7,
  
  // Enhanced features
  showOrbitalIcons = true,
  showDragPreview = true,
  enableAudio = true,
  
  // Callbacks
  onDock,
  onUndock,
  onExpand,
  onCollapse,
  onPin,
  onUnpin,
  onDragStart,
  onDragEnd,
}: NexusProps) {
  // Core state
  const [prompt, setPrompt] = useState("")
  const [suggestions, setSuggestions] = useState<string[]>([])
  const [isHovered, setIsHovered] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)
  const nexusRef = useRef<HTMLDivElement>(null)

  // Audio system (only for enhanced mode)
  const { playSound, setVolume } = useAudioSystem(enableAudio ? soundVolume : 0)

  // Drag and drop context
  const {
    items,
    isDragging,
    draggedItem,
    startDrag,
    undockItem,
    bringToFront,
    activeDockZone,
    registerItem,
    dockZones,
    dockItem,
  } = useDragDrop()

  // Nexus state management
  const [nexusState, setNexusState] = useState<NexusState>({
    isExpanded: propIsExpanded,
    expandSize: "medium",
    isPinned: propIsPinned,
    isDocked: propIsDocked,
    isFloating: !propIsDocked,
    activeModule: null,
    position: initialPosition,
    size: { width: 400, height: 120 },
  })

  // Replace drag state with useNexusDrag
  const { start: handleDragStart, end: handleDragEnd } = useNexusDrag({ onDragStart, onDragEnd })

  // Nexus identification
  const nexusId = "command-nexus"
  const item = items[nexusId]
  const isBeingDragged = draggedItem?.id === nexusId
  const isInActiveDockZone = activeDockZone && isBeingDragged
  const isDocked = item?.isDocked || propIsDocked

  // Modules for enhanced mode
  const availableModules: NexusModule[] = [
    {
      id: "agent-control",
      label: "Agent Control",
      icon: Users,
      color: "cyan",
      header: "Agent Control Center",
      description: "Manage AI agents and their tasks"
    },
    {
      id: "reasoning-engine",
      label: "Reasoning Engine",
      icon: Brain,
      color: "blue",
      header: "Reasoning Engine",
      description: "Advanced cognitive processing"
    },
    {
      id: "analytics-hub",
      label: "Analytics Hub",
      icon: BarChart3,
      color: "purple",
      header: "Analytics Hub",
      description: "Data insights and metrics"
    },
    {
      id: "security-matrix",
      label: "Security Matrix",
      icon: Shield,
      color: "green",
      header: "Security Matrix",
      description: "Security monitoring and control"
    },
  ]

  // Context panels for command mode
  const contextPanels = [
    { icon: Brain, label: "Reasoning", status: "active", color: "cyan" },
    { icon: Shield, label: "Security", status: "monitoring", color: "green" },
    { icon: Activity, label: "Analytics", status: "processing", color: "purple" },
    { icon: Zap, label: "Agents", status: "ready", color: "blue" },
  ]

  // Prompt suggestions
  const promptSuggestions = [
    "Deploy Agent",
    "Security Scan", 
    "Performance Report",
    "System Status",
    "Analyze system performance trends",
    "Generate compliance report",
    "Optimize resource allocation",
    "Monitor threat intelligence",
    "Execute diagnostic scan",
  ]

  // Register nexus as draggable item (for draggable mode)
  useEffect(() => {
    if (mode === "draggable" && !item) {
      registerItem({
        id: nexusId,
        type: "nexus",
        size: initialSize,
        position: initialPosition,
        isDocked: propIsDocked,
        zIndex: 100,
      })
    }
  }, [mode, item, registerItem, initialPosition, propIsDocked, initialSize, nexusId])

  // Volume sync for enhanced mode
  useEffect(() => { 
    if (enableAudio) setVolume(soundVolume) 
  }, [soundVolume, setVolume, enableAudio])

  // Size calculation for enhanced mode
  const calculateSize = useCallback(() => {
    if (!nexusState.isExpanded) return { width: 400, height: 120 }
    const baseHeight = 320
    const heightMultiplier =
      nexusState.expandSize === "small" ? 1 :
      nexusState.expandSize === "medium" ? 1.4 : 1.8

    return {
      width: nexusState.expandSize === "small" ? 450 :
             nexusState.expandSize === "medium" ? 500 : 600,
      height: baseHeight * heightMultiplier
    }
  }, [nexusState.isExpanded, nexusState.expandSize])

  useEffect(() => {
    if (mode === "enhanced") {
      const newSize = calculateSize()
      setNexusState(prev => ({ ...prev, size: newSize }))
    }
  }, [calculateSize, mode])

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.ctrlKey && e.code === "Space") {
        e.preventDefault()
        if (mode === "enhanced") {
          setNexusState(prev => ({ ...prev, isExpanded: true }))
          if (onExpand) onExpand()
        } else {
          setNexusState(prev => ({ ...prev, isExpanded: true }))
        }
        setTimeout(() => inputRef.current?.focus(), 100)
        if (enableAudio) playSound("click")
      }
      if (e.key === "Escape") {
        setNexusState(prev => ({ ...prev, isExpanded: false }))
        setPrompt("")
        setSuggestions([])
        if (onCollapse) onCollapse()
      }
    }

    document.addEventListener("keydown", handleKeyDown)
    return () => document.removeEventListener("keydown", handleKeyDown)
  }, [mode, enableAudio, playSound])

  // Event handlers
  const handlePromptChange = (value: string) => {
    setPrompt(value)
    if (mode === "command" && value.length > 2) {
      const filtered = promptSuggestions.filter((s) => s.toLowerCase().includes(value.toLowerCase()))
      setSuggestions(filtered.slice(0, 4))
    } else {
      setSuggestions([])
    }
  }

  const handleSubmit = () => {
    if (prompt.trim()) {
      onPromptSubmit?.(prompt)
      setPrompt("")
      setSuggestions([])
      setNexusState(prev => ({ ...prev, isExpanded: false }))
      if (enableAudio) playSound("click")
    }
  }

  const handleNexusClick = () => {
    if (!isBeingDragged) {
      setNexusState(prev => ({ ...prev, isExpanded: !prev.isExpanded }))
      if (!nexusState.isExpanded) {
        setTimeout(() => inputRef.current?.focus(), 100)
      }
      if (enableAudio) playSound("click")
    }
  }

  const handleMouseDown = (e: React.MouseEvent) => {
    if (mode !== "draggable") return
    e.preventDefault()
    e.stopPropagation()
    if (item) {
      startDrag(item, e)
    }
  }

  const handleDockToggle = () => {
    if (mode !== "draggable") return
    
    if (item?.isDocked) {
      undockItem(nexusId)
      onUndock?.()
    } else {
      const availableZones = Object.entries(dockZones).filter(
        ([zoneId, zone]) =>
          (!zone.maxItems || zone.items.length < zone.maxItems) &&
          (!zone.allowedSizes || zone.allowedSizes.includes(initialSize)),
      )

      if (availableZones.length > 0) {
        dockItem(nexusId, availableZones[0][0])
        onDock?.()
      }
    }
    if (enableAudio) playSound("dock")
  }

  const handleBringToFront = () => {
    if (mode === "draggable") {
      bringToFront(nexusId)
    }
  }

  const handlePinToggle = () => {
    const newPinned = !nexusState.isPinned
    setNexusState(prev => ({
      ...prev,
      isPinned: newPinned,
      isDocked: newPinned ? prev.isDocked : false
    }))
    if (newPinned) {
      onPin?.()
    } else {
      onUnpin?.()
    }
    if (enableAudio) playSound(newPinned ? "dock" : "undock")
  }

  const handleExpandToggle = () => {
    if (mode === "enhanced") {
      const sizes: ("small" | "medium" | "large")[] = ["small", "medium", "large"]
      const currentIndex = sizes.indexOf(nexusState.expandSize)
      const nextIndex = (currentIndex + 1) % sizes.length
      setNexusState(prev => ({ ...prev, expandSize: sizes[nextIndex] }))
      if (enableAudio) playSound("click")
    }
  }

  const handleModuleClick = (moduleId: string) => {
    const moduleToActivate = availableModules.find(m => m.id === moduleId)
    if (moduleToActivate) {
      setNexusState(prev => ({ ...prev, activeModule: moduleToActivate }))
      if (enableAudio) playSound("click")
    }
  }

  // Enhanced mode drag handlers
  const handleDragStart = useCallback((type: "nexus" | "icon", data?: any) => {
    if (mode !== "enhanced") return
    startDrag({
      id: `${type}-${Date.now()}`,
      type: "nexus", // All modules dragged from Nexus are of type 'nexus'
      size: "small", // Default size for dragged Nexus modules
      position: { x: 0, y: 0 }, // Position will be updated by drag-and-drop context
      isDocked: false,
      zIndex: 100,
      data: data, // Pass relevant data for the module
    })
    if (enableAudio) playSound("click")
    if (onDragStart) onDragStart()
  }, [mode, startDrag, enableAudio, playSound, onDragStart])

  const handleDragEnd = useCallback(() => {
    if (mode !== "enhanced") return
    endDrag()
    if (enableAudio) playSound("snap")
  }, [mode, endDrag, enableAudio, playSound])

  // Early return for draggable mode if not registered
  if (mode === "draggable" && !item) return null

  // Transform calculations
  const dragTransform = isBeingDragged ? "scale(1.05) rotate(1deg)" : ""
  const hoverTransform = isHovered && !isBeingDragged ? "scale(1.02)" : ""

  // Render based on mode
  if (mode === "command") {
    return (
      <div className="fixed inset-0 flex items-center justify-center pointer-events-none z-40">
        {/* Backdrop when active */}
        {nexusState.isExpanded && (
          <div
            className="absolute inset-0 bg-slate-950/80 backdrop-blur-sm pointer-events-auto"
            onClick={() => setNexusState(prev => ({ ...prev, isExpanded: false }))}
          />
        )}

        {/* Central Command Interface */}
        <div
          className={`
            relative transition-all duration-500 pointer-events-auto
            ${nexusState.isExpanded ? "scale-110" : "scale-100"}
          `}
        >
          {/* Orbital Context Panels */}
          <div className="absolute inset-0 w-96 h-96">
            {contextPanels.map((panel, index) => {
              const Icon = panel.icon
              const angle = index * 90 - 45
              const radius = 180
              const x = Math.cos((angle * Math.PI) / 180) * radius
              const y = Math.sin((angle * Math.PI) / 180) * radius

              return (
                <div
                  key={panel.label}
                  className={`
                    absolute w-16 h-16 backdrop-blur-md bg-slate-900/80 
                    border border-slate-700/50 rounded-xl
                    flex items-center justify-center cursor-pointer
                    transition-all duration-300 hover:scale-110
                    hover:border-${panel.color}-500/50 hover:shadow-${panel.color}-500/20
                    ${nexusState.isExpanded ? "opacity-100" : "opacity-60"}
                  `}
                  style={{
                    transform: `translate(${x}px, ${y}px)`,
                    animation: nexusState.isExpanded ? `orbit-${index} 20s linear infinite` : "none",
                  }}
                >
                  <Icon size={24} className={`text-${panel.color}-400`} />
                  <div
                    className={`absolute -bottom-8 left-1/2 transform -translate-x-1/2 
                    text-xs text-${panel.color}-400 whitespace-nowrap`}
                  >
                    {panel.label}
                  </div>
                </div>
              )
            })}
          </div>

          {/* Central Command Input */}
          <div
            className={`
              relative w-96 backdrop-blur-md bg-slate-900/90 
              border-2 rounded-2xl shadow-2xl transition-all duration-300
              ${nexusState.isExpanded ? "border-cyan-500/50 shadow-cyan-500/30" : "border-slate-700/50 hover:border-cyan-500/30"}
            `}
          >
            {/* Energy Ring */}
            <div
              className={`
                absolute -inset-1 rounded-2xl opacity-50
                bg-gradient-to-r from-cyan-500/20 via-blue-500/20 to-purple-500/20
                ${nexusState.isExpanded ? "animate-spin-slow" : ""}
              `}
            />

            <div className="relative p-6">
              <div className="flex items-center space-x-4">
                <div className="flex-1 relative">
                  <input
                    ref={inputRef}
                    type="text"
                    value={prompt}
                    onChange={(e) => handlePromptChange(e.target.value)}
                    onFocus={() => setNexusState(prev => ({ ...prev, isExpanded: true }))}
                    onKeyDown={(e) => e.key === "Enter" && handleSubmit()}
                    placeholder="Enter AI command... (Ctrl+Space)"
                    className="w-full bg-transparent text-white placeholder-slate-400 
                      text-lg outline-none"
                  />

                  {/* Suggestions Dropdown */}
                  {suggestions.length > 0 && (
                    <div
                      className="absolute top-full left-0 right-0 mt-2 
                      backdrop-blur-md bg-slate-900/90 border border-slate-700/50 
                      rounded-lg shadow-xl z-50"
                    >
                      {suggestions.map((suggestion, index) => (
                        <div
                          key={index}
                          onClick={() => {
                            setPrompt(suggestion)
                            setSuggestions([])
                          }}
                          className="p-3 hover:bg-slate-700/50 cursor-pointer 
                            text-slate-300 hover:text-white transition-colors
                            border-b border-slate-700/30 last:border-b-0"
                        >
                          {suggestion}
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                <div className="flex items-center space-x-2">
                  <button
                    onClick={onVoiceToggle}
                    className={`p-2 rounded-lg transition-all duration-200 ${
                      isVoiceActive
                        ? "bg-red-500/20 text-red-400 animate-pulse"
                        : "bg-slate-700/50 text-slate-400 hover:text-cyan-400"
                    }`}
                    title={isVoiceActive ? "Voice recognition active" : "Activate voice recognition"}
                    aria-label={isVoiceActive ? "Voice recognition active" : "Activate voice recognition"}
                  >
                    <Mic size={20} />
                  </button>

                  <button
                    onClick={handleSubmit}
                    disabled={!prompt.trim()}
                    className="p-2 rounded-lg bg-cyan-500/20 text-cyan-400 
                      hover:bg-cyan-500/30 disabled:opacity-50 disabled:cursor-not-allowed
                      transition-all duration-200"
                    title="Execute command"
                    aria-label="Execute command"
                  >
                    <Send size={20} />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    )
  }

  // Draggable and Enhanced modes
  return (
    <>
      {/* Semi-opaque overlay when expanded and not docked */}
      {nexusState.isExpanded && !isDocked && (
        <div className="fixed inset-0 bg-slate-950/60 backdrop-blur-sm z-40" onClick={() => setNexusState(prev => ({ ...prev, isExpanded: false }))} />
      )}

      {/* Nexus Container */}
      <div
        ref={nexusRef}
        className={`
          backdrop-blur-md bg-slate-900/90 border-2 rounded-2xl shadow-2xl 
          transition-all duration-500 select-none
          ${isDocked ? "relative w-full border-slate-500/30" : "fixed border-slate-700/50"}
          ${isBeingDragged ? "shadow-cyan-500/50 border-cyan-500/70 z-50" : ""}
          ${isInActiveDockZone ? "border-green-500/70 shadow-green-500/30 bg-green-900/20" : ""}
          ${isDragging && !isBeingDragged ? "pointer-events-none opacity-50" : ""}
          ${!isDocked ? "cursor-move" : ""}
          ${nexusState.isExpanded ? "z-50" : "z-10"}
        `}
        style={{
          left: isDocked ? "auto" : item?.position.x || initialPosition.x,
          top: isDocked ? "auto" : item?.position.y || initialPosition.y,
          zIndex: item?.zIndex || 100,
          width: isDocked ? "auto" : nexusState.isExpanded ? 500 : 400,
          height: isDocked ? "auto" : nexusState.isExpanded ? 320 : 120,
          transform: `${dragTransform} ${hoverTransform}`,
        }}
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
        onClick={handleBringToFront}
      >
        {/* Draggable Header */}
        <div
          className={`
            flex items-center justify-between px-4 py-2 border-b border-slate-700/50
            ${isDocked ? "cursor-default" : "cursor-grab hover:cursor-grab active:cursor-grabbing"}
            bg-gradient-to-r from-slate-800/50 to-slate-700/30
            ${isBeingDragged ? "bg-gradient-to-r from-cyan-800/30 to-blue-800/30" : ""}
            transition-all duration-200
          `}
          onMouseDown={handleMouseDown}
          style={{ cursor: !isDocked ? "grab" : "default" }}
        >
          <div className="flex items-center space-x-2">
            {/* Drag Handle */}
            {mode === "draggable" && (
              <div className="flex items-center space-x-1 p-1 rounded transition-all duration-200 hover:bg-cyan-500/20">
                <GripVertical
                  size={14}
                  className={`transition-colors ${
                    isBeingDragged ? "text-cyan-400" : "text-slate-400 group-hover:text-cyan-400"
                  }`}
                />
              </div>
            )}

            {/* Pin Icon for Docked State */}
            {mode === "draggable" && (
              <button
                onClick={(e) => {
                  e.stopPropagation()
                  handleDockToggle()
                }}
                className={`
                  p-1.5 rounded-md transition-all duration-300 cursor-pointer
                  ${
                    isDocked
                      ? "bg-slate-700/50 text-cyan-400 hover:bg-slate-600/50 hover:text-cyan-300"
                      : "bg-slate-800/30 text-slate-500 hover:bg-slate-700/50 hover:text-slate-400"
                  }
                `}
                title={isDocked ? "Nexus is docked - Click to undock" : "Click to dock nexus"}
              >
                {isDocked ? <Pin size={12} className="text-cyan-400" /> : <PinOff size={12} />}
              </button>
            )}

            {/* Enhanced mode controls */}
            {mode === "enhanced" && (
              <>
                <button
                  onClick={(e) => {
                    e.stopPropagation()
                    handlePinToggle()
                  }}
                  className={`
                    p-1.5 rounded-md transition-all duration-300 cursor-pointer
                    ${
                      nexusState.isPinned
                        ? "bg-slate-700/50 text-cyan-400 hover:bg-slate-600/50 hover:text-cyan-300"
                        : "bg-slate-800/30 text-slate-500 hover:bg-slate-700/50 hover:text-slate-400"
                    }
                  `}
                  title={nexusState.isPinned ? "Unpin nexus" : "Pin nexus"}
                  aria-label={nexusState.isPinned ? "Unpin nexus" : "Pin nexus"}
                >
                  {nexusState.isPinned ? <Pin size={12} className="text-cyan-400" /> : <PinOff size={12} />}
                </button>

                <button
                  onClick={(e) => {
                    e.stopPropagation()
                    handleExpandToggle()
                  }}
                  className="p-1.5 rounded-md bg-slate-800/30 text-slate-400 hover:bg-slate-700/50 hover:text-cyan-400 transition-all duration-300"
                  title={`Expand size: ${nexusState.expandSize}`}
                  aria-label={`Expand size: ${nexusState.expandSize}`}
                >
                  {nexusState.expandSize === "small" ? <Minimize2 size={12} /> : 
                   nexusState.expandSize === "medium" ? <Move size={12} /> : <Maximize2 size={12} />}
                </button>
              </>
            )}

            <h3 className="font-semibold text-white text-sm">Command Nexus</h3>
            <div className="w-2 h-2 bg-cyan-400 rounded-full animate-pulse" />
          </div>

          <button
            onClick={(e) => {
              e.stopPropagation()
              handleNexusClick()
            }}
            className="px-3 py-1 bg-slate-700/50 hover:bg-slate-600/50 rounded-lg text-xs text-slate-300 hover:text-white transition-all duration-200"
          >
            {nexusState.isExpanded ? "Collapse" : "Expand"}
          </button>
        </div>

        {/* Energy Ring */}
        <div
          className={`
            absolute -inset-1 rounded-2xl opacity-50
            bg-gradient-to-r from-cyan-500/20 via-blue-500/20 to-purple-500/20
            ${nexusState.isExpanded ? "animate-spin-slow" : ""}
          `}
        />

        <div className="relative p-6 h-full flex flex-col">
          {/* Header */}
          <div className="text-center mb-4">
            <div className="text-2xl font-bold bg-gradient-to-r from-cyan-400 via-blue-400 to-purple-400 bg-clip-text text-transparent mb-2">
              COMMAND NEXUS
            </div>
            {!nexusState.isExpanded && (
              <div>
                <div className="text-sm text-slate-400 mb-2">Central Command Interface • Click expand to use</div>
                <div className="flex items-center justify-center space-x-2">
                  <div className="w-2 h-2 bg-cyan-400 rounded-full animate-pulse" />
                  <span className="text-xs text-cyan-400">Neural Link Active</span>
                </div>
              </div>
            )}
          </div>

          {/* Expanded Interface */}
          {nexusState.isExpanded && (
            <div className="flex-1 space-y-4">
              {/* Command Input */}
              <div className="relative">
                <input
                  ref={inputRef}
                  type="text"
                  value={prompt}
                  onChange={(e) => handlePromptChange(e.target.value)}
                  onKeyDown={(e) => e.key === "Enter" && handleSubmit()}
                  placeholder="Enter AI command... (Ctrl+Space)"
                  className="w-full bg-slate-800/50 text-white placeholder-slate-400 
                    text-lg outline-none p-4 rounded-xl border border-slate-700/50
                    focus:border-cyan-500/50 transition-all duration-300"
                />
              </div>

              {/* Action Buttons */}
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <button
                    onClick={onVoiceToggle}
                    className={`px-4 py-2 rounded-lg transition-all duration-200 flex items-center space-x-2 ${
                      isVoiceActive
                        ? "bg-red-500/20 text-red-400 animate-pulse"
                        : "bg-slate-700/50 text-slate-400 hover:text-cyan-400 hover:bg-slate-600/50"
                    }`}
                  >
                    <Mic size={16} />
                    <span className="text-sm">Hey Mesh</span>
                  </button>
                </div>

                <button
                  onClick={handleSubmit}
                  disabled={!prompt.trim()}
                  className="px-6 py-2 rounded-lg bg-cyan-500/20 text-cyan-400 
                    hover:bg-cyan-500/30 disabled:opacity-50 disabled:cursor-not-allowed
                    transition-all duration-200 flex items-center space-x-2"
                >
                  <Send size={16} />
                  <span>Execute</span>
                </button>
              </div>

              {/* Contextual Command Chips */}
              <div className="flex flex-wrap gap-2">
                {promptSuggestions.slice(0, 6).map((chip) => (
                  <button
                    key={chip}
                    onClick={() => setPrompt(chip.toLowerCase())}
                    className="px-3 py-1 text-xs bg-slate-700/50 text-slate-300 
                      rounded-full hover:bg-cyan-500/20 hover:text-cyan-400 
                      transition-all duration-200"
                  >
                    {chip}
                  </button>
                ))}
              </div>

              {/* Enhanced mode orbital icons */}
              {mode === "enhanced" && showOrbitalIcons && (
                <OrbitalIcons
                  modules={availableModules}
                  isExpanded={nexusState.isExpanded}
                  orbitMode={true}
                  iconPositions={[]}
                  onIconPositionsChange={() => {}}
                  onModuleClick={handleModuleClick}
                  activeModule={nexusState.activeModule}
                />
              )}
            </div>
          )}
        </div>
      </div>

      {/* Enhanced mode drag preview */}
      {mode === "enhanced" && showDragPreview && useNexusDragIsDragging && (
        <DragPreview
          isVisible={useNexusDragIsDragging}
          draggedItem={useNexusDragIsDragging.draggedItem}
          onDragEnd={handleDragEnd}
          onDropZoneEnter={() => {}}
          onDropZoneLeave={() => {}}
        />
      )}
    </>
  )
} 