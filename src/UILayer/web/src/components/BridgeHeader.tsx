import { EnergyFlow, FXModePanel, LayoutToolsPanel } from "@/components";
import { Server } from "lucide-react";
import React from "react";

interface BridgeHeaderProps {
  particleEffectsEnabled: boolean;
  snapToGrid: boolean;
  showGrid: boolean;
  layoutMode: "radial" | "grid" | "freeform";
  dockHandleStyle: "grip" | "anchor" | "titlebar" | "ring" | "invisible";
  nexusExpanded: boolean;
  getSizeIcon: () => React.ReactNode;
  getSizeLabel: () => string;
  cycleSizeDown: () => void;
  cycleSizeUp: () => void;
  setLayoutMode: (mode: "radial" | "grid" | "freeform") => void;
  setDockHandleStyle: (style: "grip" | "anchor" | "titlebar" | "ring" | "invisible") => void;
  toggleSnapToGrid: () => void;
  toggleShowGrid: () => void;
  toggleParticleEffects: () => void;
  handleNexusToggle: () => void;
}

const BridgeHeader: React.FC<BridgeHeaderProps> = ({
  particleEffectsEnabled,
  snapToGrid,
  showGrid,
  layoutMode,
  dockHandleStyle,
  nexusExpanded,
  getSizeIcon,
  getSizeLabel,
  cycleSizeDown,
  cycleSizeUp,
  setLayoutMode,
  setDockHandleStyle,
  toggleSnapToGrid,
  toggleShowGrid,
  toggleParticleEffects,
  handleNexusToggle,
}) => {
  return (
    <header className="mb-6">
      <div className="backdrop-blur-md bg-slate-900/70 border border-slate-700/50 rounded-xl shadow-2xl p-4 relative overflow-hidden header-glow flex flex-col gap-2">
        {/* Animated SVG Background Layers */}
        <div className="animated-svg-bg"></div>
        <div className="data-stream-bg"></div>
        <div className="scanning-line"></div>
        <div className="pulsing-border"></div>
        <div className="tech-corner top-left"></div>
        <div className="tech-corner top-right"></div>
        <div className="tech-corner bottom-left"></div>
        <div className="tech-corner bottom-right"></div>

        <EnergyFlow direction="horizontal" intensity="low" color="cyan" />

        {/* First Row - Title and System Status */}
        <div className="flex items-center justify-between relative z-10 mb-2 gap-4">
          {/* Left: App name, subtitle, status */}
          <div className="flex flex-col min-w-0">
            <div className="flex items-center gap-2">
              <h1 className="text-3xl font-extrabold bg-gradient-to-r from-cyan-400 via-blue-400 to-purple-400 bg-clip-text text-transparent tracking-wide whitespace-nowrap flex items-center gap-2">
                <Server size={28} className="text-cyan-400 drop-shadow" />
                COGNITIVE MESH BRIDGE
              </h1>
              <span className="ml-2 px-2 py-0.5 rounded bg-cyan-900/60 text-cyan-300 text-xs font-semibold tracking-wider border border-cyan-700/40">Command Center</span>
            </div>
            <p className="text-slate-400 mt-1 text-base truncate">Enterprise AI Transformation Framework</p>
            <div className="flex items-center space-x-4 mt-1">
              <div className="flex items-center space-x-2 text-green-400">
                <div className={`w-2 h-2 bg-green-400 rounded-full ${particleEffectsEnabled ? 'animate-pulse' : ''}`} />
                <span className="text-xs">Neural Network Online</span>
              </div>
              <div className="flex items-center space-x-2 text-cyan-400">
                <div className={`w-2 h-2 bg-cyan-400 rounded-full ${particleEffectsEnabled ? 'animate-pulse' : ''}`} />
                <span className="text-xs">Quantum Processing Active</span>
              </div>
            </div>
          </div>

          {/* Center: Animated Power/Load */}
          <div className="flex flex-col items-center justify-center flex-1">
            <div className="flex items-center gap-8">
              <div className="power-indicator" title="System Power Usage">
                <span className="power-ring"></span>
                <span className="text-xs text-yellow-300 font-bold">Power: 98%</span>
              </div>
              <div className="load-indicator" title="System Load">
                <div className="load-bar">
                  <div className="load-bar-fill" style={{ width: '67%' }}></div>
                </div>
                <span className="text-xs text-green-300 font-bold">Load: 67%</span>
              </div>
            </div>
          </div>

          {/* Right: Control Panels */}
          <div className="flex items-end gap-6">
            <LayoutToolsPanel
              snapToGrid={snapToGrid}
              showGrid={showGrid}
              layoutMode={layoutMode}
              dockHandleStyle={dockHandleStyle}
              getSizeIcon={getSizeIcon}
              getSizeLabel={getSizeLabel}
              cycleSizeDown={cycleSizeDown}
              cycleSizeUp={cycleSizeUp}
              setLayoutMode={setLayoutMode}
              setDockHandleStyle={setDockHandleStyle}
              toggleSnapToGrid={toggleSnapToGrid}
              toggleShowGrid={toggleShowGrid}
            />
            <FXModePanel
              particleEffectsEnabled={particleEffectsEnabled}
              nexusExpanded={nexusExpanded}
              toggleParticleEffects={toggleParticleEffects}
              handleNexusToggle={handleNexusToggle}
            />
          </div>
        </div>
      </div>
    </header>
  );
};

export default BridgeHeader; 