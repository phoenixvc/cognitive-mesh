import { Grid3X3, Layers, Move, RotateCcw } from "lucide-react";
import React from "react";

interface LayoutToolsPanelProps {
  snapToGrid: boolean;
  showGrid: boolean;
  layoutMode: "radial" | "grid" | "freeform";
  dockHandleStyle: "grip" | "anchor" | "titlebar" | "ring" | "invisible";
  getSizeIcon: () => React.ReactNode;
  getSizeLabel: () => string;
  cycleSizeDown: () => void;
  cycleSizeUp: () => void;
  setLayoutMode: (mode: "radial" | "grid" | "freeform") => void;
  setDockHandleStyle: (style: "grip" | "anchor" | "titlebar" | "ring" | "invisible") => void;
  toggleSnapToGrid: () => void;
  toggleShowGrid: () => void;
}

const LayoutToolsPanel: React.FC<LayoutToolsPanelProps> = ({
  snapToGrid,
  showGrid,
  layoutMode,
  dockHandleStyle,
  getSizeIcon,
  getSizeLabel,
  cycleSizeDown,
  cycleSizeUp,
  setLayoutMode,
  setDockHandleStyle,
  toggleSnapToGrid,
  toggleShowGrid,
}) => {
  return (
    <div className="flex flex-col gap-2 items-end">
      <div className="flex flex-wrap items-center gap-2 bg-slate-800/40 rounded-lg px-2 py-1 border border-slate-700/40">
        {/* Snap Toggle + Info */}
        <div className="flex items-center gap-1">
          <button
            type="button"
            onClick={toggleSnapToGrid}
            className={`flex items-center space-x-2 px-2 py-1 rounded ghost-btn ${snapToGrid ? 'bg-cyan-500/20 text-cyan-400' : 'hover:bg-slate-600/50 text-slate-400 hover:text-cyan-400'}`}
            title="Snap to Grid: Aligns modules to a grid for precise placement."
            aria-label="Snap to Grid"
          >
            <Grid3X3 size={18} />
            <span className="text-xs font-medium">SNAP</span>
          </button>
          {/* Info icon for Snap */}
          <span className="ml-1 text-cyan-400 cursor-pointer group relative">
            <svg width="14" height="14" fill="none" viewBox="0 0 24 24" stroke="currentColor" className="inline-block align-middle">
              <circle cx="12" cy="12" r="10" strokeWidth="2" />
              <text x="12" y="16" textAnchor="middle" fontSize="12" fill="currentColor">i</text>
            </svg>
            <span className="absolute left-1/2 -translate-x-1/2 top-6 w-56 bg-slate-900 text-xs text-slate-200 rounded-lg shadow-lg p-2 opacity-0 group-hover:opacity-100 pointer-events-none transition-opacity duration-200 z-50">
              <b>Snap:</b> When enabled, modules will snap to the nearest grid cell when moved or resized. This keeps your layout tidy and aligned. <br/><b>Grid:</b> Only shows a visual grid overlay for alignment help; does not affect placement unless Snap is also enabled.
            </span>
          </span>
        </div>
        {/* Grid Toggle + Info */}
        <div className="flex items-center gap-1">
          <button
            type="button"
            onClick={toggleShowGrid}
            className={`flex items-center space-x-2 px-2 py-1 rounded ghost-btn ${showGrid ? 'bg-purple-500/20 text-purple-400' : 'hover:bg-slate-600/50 text-slate-400 hover:text-purple-400'}`}
            title="Show Grid Overlay: Displays a visual grid to help align modules."
            aria-label="Show Grid Overlay"
          >
            <Layers size={18} />
            <span className="text-xs font-medium">GRID</span>
          </button>
          {/* Info icon for Grid (same tooltip as Snap) */}
          <span className="ml-1 text-purple-400 cursor-pointer group relative">
            <svg width="14" height="14" fill="none" viewBox="0 0 24 24" stroke="currentColor" className="inline-block align-middle">
              <circle cx="12" cy="12" r="10" strokeWidth="2" />
              <text x="12" y="16" textAnchor="middle" fontSize="12" fill="currentColor">i</text>
            </svg>
            <span className="absolute left-1/2 -translate-x-1/2 top-6 w-56 bg-slate-900 text-xs text-slate-200 rounded-lg shadow-lg p-2 opacity-0 group-hover:opacity-100 pointer-events-none transition-opacity duration-200 z-50">
              <b>Grid:</b> Shows a visual grid overlay for alignment help. <br/><b>Snap:</b> Must be enabled for modules to snap to the grid when moved or resized.
            </span>
          </span>
        </div>
        {/* Layout Mode Toggle */}
        <div className="flex items-center gap-1">
          <button
            type="button"
            onClick={() => setLayoutMode(layoutMode === "radial" ? "grid" : layoutMode === "grid" ? "freeform" : "radial")}
            className="flex items-center space-x-2 px-2 py-1 rounded ghost-btn hover:bg-slate-600/50 text-slate-400 hover:text-cyan-400"
            title={`Switch to ${layoutMode === "radial" ? "grid" : layoutMode === "grid" ? "freeform" : "radial"} layout: Change how modules are arranged.`}
            aria-label="Change layout mode"
          >
            <Move size={18} />
            <span className="text-xs font-medium">LAYOUT</span>
          </button>
        </div>
        {/* Global Size Control */}
        <div className="flex items-center gap-1">
          <div className="flex items-center space-x-2 px-2 py-1 bg-slate-700/30 rounded">
            {getSizeIcon()}
            <span className="text-sm text-slate-300 font-medium">{getSizeLabel()}</span>
          </div>
          <div className="flex">
            <button
              type="button"
              onClick={cycleSizeDown}
              className="px-2 py-1 hover:bg-slate-600/50 transition-all duration-300 text-slate-400 hover:text-cyan-400 border-r border-slate-600/50 rounded-l"
              title="Decrease global size"
              aria-label="Decrease global size"
            >
              <RotateCcw size={16} />
            </button>
            <button
              type="button"
              onClick={cycleSizeUp}
              className="px-2 py-1 hover:bg-slate-600/50 transition-all duration-300 text-slate-400 hover:text-cyan-400 rounded-r"
              title="Increase global size"
              aria-label="Increase global size"
            >
              <RotateCcw size={16} className="rotate-180" />
            </button>
          </div>
        </div>
        {/* Dock Handle Style Switcher */}
        <div className="flex items-center gap-1">
          <span className="text-xs text-slate-400">Handle:</span>
          <div className="flex bg-slate-800/60 rounded-lg border border-slate-700/50 overflow-hidden">
            {[
              { key: "grip", label: "≡≡≡", tooltip: "Grip" },
              { key: "anchor", label: "●", tooltip: "Anchor" },
              { key: "titlebar", label: "━", tooltip: "Titlebar" },
              { key: "ring", label: "◎", tooltip: "Ring" },
              { key: "invisible", label: "☰", tooltip: "Invisible" },
            ].map(opt => (
              <button
                key={opt.key}
                type="button"
                onClick={() => setDockHandleStyle(opt.key as "grip" | "anchor" | "titlebar" | "ring" | "invisible")}
                className={`px-2 py-1 text-base font-mono transition-all duration-200 ${
                  dockHandleStyle === opt.key
                    ? "bg-cyan-500/20 text-cyan-300 font-bold"
                    : "text-slate-400 hover:bg-slate-700/40"
                }`}
                title={opt.tooltip}
                aria-label={`Switch to ${opt.tooltip} handle`}
              >
                {opt.label}
              </button>
            ))}
          </div>
        </div>
      </div>
      {/* Preset Mode Switcher & Compact Mode */}
      <div className="flex items-center gap-2 mt-2">
        <span className="text-xs text-slate-400">Preset:</span>
        <select 
          className="bg-slate-800/60 border border-slate-700/50 rounded px-2 py-1 text-xs text-cyan-300 focus:outline-none"
          aria-label="Select preset mode"
        >
          <option>Default</option>
          <option>Expert</option>
          <option>Quantum</option>
        </select>
        <button className="ml-2 px-2 py-1 rounded bg-slate-700/40 text-cyan-400 hover:bg-slate-600/60 transition-all duration-200" title="Compact Mode: Hide controls into a dropdown">
          <svg width="18" height="18" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 8h16M4 16h16" /></svg>
        </button>
      </div>
    </div>
  );
};

export default LayoutToolsPanel; 