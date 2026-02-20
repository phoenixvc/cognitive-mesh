"use client"
import React, { useEffect, useState } from "react"

import { BackgroundEffects, BridgeHeader, DashboardLayout, LoadingSpinner, Nexus } from "@/components"
import DraggableComponent from "@/components/DraggableComponent"
import VoiceFeedback from "@/components/VoiceFeedback"
import { DragDropProvider, useDragDrop } from "@/contexts/DragDropContext"
import { useDashboardData } from "@/hooks/useDashboardData"
import {
  Activity,
  BarChart3,
  Brain,
  CheckCircle,
  Cpu,
  Eye,
  Maximize,
  Minimize,
  Monitor,
  Shield,
  Square,
  TrendingUp,
  Users,
  LucideIcon
} from "lucide-react"

import DraggableModuleContent from "@/components/DraggableModuleContent"

function DashboardContent() {
  const { globalSize, setGlobalSize, snapToGrid, showGrid, toggleSnapToGrid, toggleShowGrid, dockItem, items, dockZones } = useDragDrop();

  // Fetch dashboard data using API service
  const { data, loading, error, refetch } = useDashboardData();

  const [activeLayer, setActiveLayer] = useState("foundation");
  const [isVoiceActive, setIsVoiceActive] = useState(false);
  const [layoutMode, setLayoutMode] = useState<"radial" | "grid" | "freeform">("radial");
  const [voiceFeedback, setVoiceFeedback] = useState("");
  const [nexusExpanded, setNexusExpanded] = useState(false);
  const [nexusPosition, setNexusPosition] = useState({ x: 400, y: 300 }); // Default fallback

  // New state for bridge controls
  const [effectSpeed] = useState(1.0); // 0.1 to 3.0
  const [soundVolume] = useState(0.7); // 0.0 to 1.0
  const [particleEffectsEnabled, setParticleEffectsEnabled] = useState(() => {
    if (typeof window !== 'undefined') {
      const saved = localStorage.getItem('cognitive-mesh-particle-effects');
      return saved ? JSON.parse(saved) : false;
    }
    return false;
  });

  // Drag handle style state
  const [dockHandleStyle, setDockHandleStyle] = useState<"grip" | "anchor" | "titlebar" | "ring" | "invisible">("grip");

  const [nexusAutoDockEnabled, setNexusAutoDockEnabled] = useState(true);

  // Icon mapping for API data
  const iconMap: Record<string, LucideIcon> = {
    Shield,
    Brain,
    Eye,
    Users,
    BarChart3,
    Cpu,
    CheckCircle,
  };

  // Transform API data to include icon components (only when data is available)
  const layers = data?.layers?.map(layer => ({
    ...layer,
    icon: iconMap[layer.icon] || Shield,
  })) || [];

  const metrics = data?.metrics?.map(metric => ({
    ...metric,
    icon: iconMap[metric.icon] || Activity,
  })) || [];

  // Calculate center position for Command Nexus
  useEffect(() => {
    const calculateCenterPosition = () => {
      const viewportWidth = window.innerWidth
      const viewportHeight = window.innerHeight

      // Command Nexus dimensions (400px wide, 120px tall when collapsed)
      const nexusWidth = 400
      const nexusHeight = 120

      const centerX = viewportWidth / 2 - nexusWidth / 2
      const centerY = viewportHeight / 2 - nexusHeight / 2

      setNexusPosition({
        x: Math.max(50, centerX), // Ensure it's not too close to edge
        y: Math.max(100, centerY), // Account for header space
      })
    }

    // Calculate on mount
    calculateCenterPosition()

    // Recalculate on window resize
    const handleResize = () => calculateCenterPosition()
    window.addEventListener("resize", handleResize)

    return () => window.removeEventListener("resize", handleResize)
  }, [])

  // Apply effect speed to CSS custom properties
  useEffect(() => {
    document.documentElement.style.setProperty("--effect-speed-multiplier", effectSpeed.toString())
    document.documentElement.style.setProperty("--animation-duration-base", `${2 / effectSpeed}s`)
  }, [effectSpeed])

  // Persist particle effects setting to localStorage
  useEffect(() => {
    if (typeof window !== 'undefined') {
      localStorage.setItem('cognitive-mesh-particle-effects', JSON.stringify(particleEffectsEnabled))
    }
  }, [particleEffectsEnabled])

  // Robustly dock all items (including Nexus) when both items and zones are registered
  useEffect(() => {
    if (!data) return; // Don't run if data is not available
    
    // Wait until all items and all dock zones are registered before attempting to dock
    const allZones = [
      "central-nexus-dock",
      "metrics-dock",
      "main-modules-dock",
      "sidebar-dock",
      "bottom-dock",
    ];
    const allZonesRegistered = allZones.every((zone) => dockZones && dockZones[zone]);
    const allMetricsRegistered = metrics.every((metric) => items[metric.id]);
    if (!allZonesRegistered || !items["command-nexus"] || !allMetricsRegistered) return;

    // Nexus
    if (
      nexusAutoDockEnabled &&
      !items["command-nexus"].isDocked
    ) {
      dockItem("command-nexus", "central-nexus-dock", 0);
    }
    // Metrics
    metrics.forEach((metric, index) => {
      if (!items[metric.id].isDocked) {
        dockItem(metric.id, "metrics-dock", index);
      }
    });
    // Architecture
    if (items["architecture"] && !items["architecture"].isDocked) {
      dockItem("architecture", "main-modules-dock", 0);
    }
    // Security
    if (items["security"] && !items["security"].isDocked) {
      dockItem("security", "sidebar-dock", 0);
    }
    // Resources
    if (items["resources"] && !items["resources"].isDocked) {
      dockItem("resources", "sidebar-dock", 1);
    }
    // Agents
    if (items["agents"] && !items["agents"].isDocked) {
      dockItem("agents", "bottom-dock", 0);
    }
    // Activity
    if (items["activity"] && !items["activity"].isDocked) {
      dockItem("activity", "bottom-dock", 1);
    }
  }, [nexusAutoDockEnabled, items, dockZones, dockItem, metrics, data]);

  // Show loading state
  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 text-white flex items-center justify-center">
        <div className="text-center">
          <LoadingSpinner size="large" />
          <p className="mt-4 text-cyan-400">Loading Cognitive Mesh Dashboard...</p>
        </div>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 text-white flex items-center justify-center">
        <div className="text-center max-w-md">
          <div className="text-red-400 text-6xl mb-4">⚠️</div>
          <h2 className="text-2xl font-bold mb-2">Connection Error</h2>
          <p className="text-slate-400 mb-4">{error}</p>
          <button
            onClick={refetch}
            className="px-4 py-2 bg-cyan-600 hover:bg-cyan-700 text-white rounded-lg transition-colors"
          >
            Retry Connection
          </button>
        </div>
      </div>
    );
  }

  // Ensure data is available
  if (!data) {
    return null;
  }

  const handlePromptSubmit = (prompt: string) => {
    console.log("AI Prompt submitted:", prompt)
  }

  const toggleParticleEffects = () => {
    setParticleEffectsEnabled(!particleEffectsEnabled)
  }

  const handleVoiceActivation = () => {
    setIsVoiceActive(!isVoiceActive)
    if (!isVoiceActive) {
      setVoiceFeedback('Voice activation enabled - Say "Hey Mesh" to begin')
      setTimeout(() => setVoiceFeedback(""), 3000)
    } else {
      setVoiceFeedback("Voice activation disabled")
      setTimeout(() => setVoiceFeedback(""), 2000)
    }
  }

  // Handle Nexus toggle (undock/dock)
  const handleNexusToggle = () => {
    if (nexusExpanded) {
      // User is undocking Nexus, disable auto-dock
      setNexusAutoDockEnabled(false);
    } else {
      // User is re-docking via toggle, allow auto-dock again
      setNexusAutoDockEnabled(true);
    }
    setNexusExpanded(!nexusExpanded);
  };

  // Size control functions
  const cycleSizeUp = () => {
    const sizes: ("small" | "medium" | "large" | "x-large")[] = ["small", "medium", "large", "x-large"]
    const currentIndex = sizes.indexOf(globalSize as "small" | "medium" | "large" | "x-large")
    const nextIndex = currentIndex < sizes.length - 1 ? currentIndex + 1 : 0
    setGlobalSize(sizes[nextIndex] as "small" | "medium" | "large" | "x-large")
  }

  const cycleSizeDown = () => {
    const sizes: ("small" | "medium" | "large" | "x-large")[] = ["small", "medium", "large", "x-large"]
    const currentIndex = sizes.indexOf(globalSize as "small" | "medium" | "large" | "x-large")
    const nextIndex = currentIndex > 0 ? currentIndex - 1 : sizes.length - 1
    setGlobalSize(sizes[nextIndex] as "small" | "medium" | "large" | "x-large")
  }

  const getSizeIcon = () => {
    switch (globalSize) {
      case "small":
        return <Minimize size={16} />
      case "medium":
        return <Square size={16} />
      case "large":
        return <Maximize size={16} />
      case "x-large":
        return <Monitor size={16} />
      default:
        return <Square size={16} />
    }
  }

  const getSizeLabel = () => {
    switch (globalSize) {
      case "small":
        return "Small"
      case "medium":
        return "Medium"
      case "large":
        return "Large"
      case "x-large":
        return "X-Large"
      default:
        return "Medium"
    }
  }

  return (
    <div
      className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 text-white overflow-hidden relative"
      style={
        {
          "--effect-speed": effectSpeed,
          "--sound-volume": soundVolume,
        } as React.CSSProperties
      }
    >
      {/* Enhanced Animated Starfield Background */}
      <BackgroundEffects particleEffectsEnabled={particleEffectsEnabled} effectSpeed={effectSpeed} />

      {/* Enhanced Central Command Nexus - Only show when expanded */}
      {nexusExpanded && (
        <div className="fixed inset-0 flex items-center justify-center z-50 pointer-events-none">
          <div className="pointer-events-auto">
            <Nexus
              mode="enhanced"
              onPromptSubmit={handlePromptSubmit}
              isVoiceActive={isVoiceActive}
              onVoiceToggle={() => setIsVoiceActive(!isVoiceActive)}
              onDock={() => setNexusExpanded(false)}
              soundVolume={soundVolume}
              enableAudio={true}
            />
          </div>
        </div>
      )}

      <div className="relative z-10 p-6">
        <BridgeHeader
          particleEffectsEnabled={particleEffectsEnabled}
          snapToGrid={snapToGrid}
          showGrid={showGrid}
          layoutMode={layoutMode}
          dockHandleStyle={dockHandleStyle}
          nexusExpanded={nexusExpanded}
          getSizeIcon={getSizeIcon}
          getSizeLabel={getSizeLabel}
          cycleSizeDown={cycleSizeDown}
          cycleSizeUp={cycleSizeUp}
          setLayoutMode={setLayoutMode}
          setDockHandleStyle={setDockHandleStyle}
          toggleSnapToGrid={toggleSnapToGrid}
          toggleShowGrid={toggleShowGrid}
          toggleParticleEffects={toggleParticleEffects}
          handleNexusToggle={handleNexusToggle}
        />

        <DashboardLayout dockHandleStyle={dockHandleStyle} />
      </div>

      {/* Draggable Components - Always visible but positioned off-screen initially */}
      <div className="fixed -top-full -left-full">
        {metrics.map((metric) => {
          const Icon = metric.icon
          return (
            <DraggableComponent
              key={metric.id}
              id={metric.id}
              title={metric.label}
              type="metric"
              initialSize="small"
              initialPosition={{ x: 50, y: 200 }}
            >
              <div className="text-center">
                <div className="flex items-center justify-center mb-3">
                  <Icon size={24} className="text-cyan-400" />
                </div>
                <div className="text-3xl font-bold text-white mb-2 bg-gradient-to-r from-cyan-400 to-blue-400 bg-clip-text text-transparent">
                  {metric.value}
                </div>
                <div
                  className={`text-sm flex items-center justify-center space-x-1 ${
                    metric.status === "up" ? "text-green-400" : "text-cyan-400"
                  }`}
                >
                  {metric.status === "up" && <TrendingUp size={14} />}
                  {metric.status === "stable" && <Activity size={14} />}
                  <span>{metric.change}</span>
                </div>
              </div>
            </DraggableComponent>
          )
        })}

        <DraggableComponent
          id="architecture"
          title="Cognitive Architecture Layers"
          type="module"
          initialSize="large"
          initialPosition={{ x: 100, y: 400 }}
        >
          <DraggableModuleContent
            type="architecture"
            layers={layers}
            activeLayer={activeLayer}
            particleEffectsEnabled={particleEffectsEnabled}
            effectSpeed={effectSpeed}
            onLayerClick={setActiveLayer}
          />
        </DraggableComponent>

        <DraggableComponent
          id="security"
          title="Security Matrix"
          type="module"
          initialSize="medium"
          initialPosition={{ x: 800, y: 400 }}
        >
          <DraggableModuleContent
            type="security"
            particleEffectsEnabled={particleEffectsEnabled}
            effectSpeed={effectSpeed}
          />
        </DraggableComponent>

        <DraggableComponent
          id="resources"
          title="System Resources"
          type="module"
          initialSize="medium"
          initialPosition={{ x: 800, y: 650 }}
        >
          <DraggableModuleContent
            type="resources"
            particleEffectsEnabled={particleEffectsEnabled}
            effectSpeed={effectSpeed}
          />
        </DraggableComponent>

        <DraggableComponent
          id="agents"
          title="Active Agents"
          type="module"
          initialSize="medium"
          initialPosition={{ x: 100, y: 800 }}
        >
          <DraggableModuleContent
            type="agents"
            agents={data.agents}
            particleEffectsEnabled={particleEffectsEnabled}
            effectSpeed={effectSpeed}
          />
        </DraggableComponent>

        <DraggableComponent
          id="activity"
          title="Recent Activity"
          type="module"
          initialSize="medium"
          initialPosition={{ x: 500, y: 800 }}
        >
          <DraggableModuleContent
            type="activity"
            activities={data.activities}
            particleEffectsEnabled={particleEffectsEnabled}
            effectSpeed={effectSpeed}
          />
        </DraggableComponent>

        {/* Design System Demo removed */}
      </div>

      {/* Command Nexus - Floating version (only when not docked) */}
      {!nexusExpanded && (() => {
        const nexusItem = items["command-nexus"];
        const isNexusDocked = nexusItem?.isDocked;
        // When user starts dragging, re-enable auto-dock
        const handleNexusDragStart = () => {
          setNexusAutoDockEnabled(true);
        };
        return !isNexusDocked ? (
          <Nexus
            mode="draggable"
            onPromptSubmit={handlePromptSubmit}
            isVoiceActive={isVoiceActive}
            onVoiceToggle={handleVoiceActivation}
            initialPosition={nexusPosition}
            isDocked={false}
            soundVolume={soundVolume}
            enableAudio={false}
            onDragStart={handleNexusDragStart}
          />
        ) : null;
      })()}

      {/* Voice Activation Feedback */}
      <VoiceFeedback
        isVoiceActive={isVoiceActive}
        voiceFeedback={voiceFeedback}
        particleEffectsEnabled={particleEffectsEnabled}
        effectSpeed={effectSpeed}
      />
    </div>
  )
}

export default function CognitiveMeshDashboard() {
  return (
    <DragDropProvider>
      <DashboardContent />
    </DragDropProvider>
  )
}
