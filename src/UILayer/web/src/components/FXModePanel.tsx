import { Sparkles } from "lucide-react";
import React from "react";

interface FXModePanelProps {
  particleEffectsEnabled: boolean;
  nexusExpanded: boolean;
  toggleParticleEffects: () => void;
  handleNexusToggle: () => void;
}

const FXModePanel: React.FC<FXModePanelProps> = ({
  particleEffectsEnabled,
  nexusExpanded,
  toggleParticleEffects,
  handleNexusToggle,
}) => {
  return (
    <div className="flex flex-col gap-2 items-end">
      {/* FX Toggle */}
      <div className="flex items-center gap-1" title="Toggle animated background and energy effects">
        <Sparkles size={18} className={particleEffectsEnabled ? "text-green-400" : "text-slate-500"} />
        <span className="text-xs text-slate-300 font-medium">FX</span>
        <button
          type="button"
          onClick={() => { 
            toggleParticleEffects(); 
            if (window.navigator.vibrate) window.navigator.vibrate(30); 
          }}
          className={`relative inline-flex h-5 w-9 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-cyan-400 focus:ring-offset-2 focus:ring-offset-slate-900 ${particleEffectsEnabled ? 'bg-green-500' : 'bg-slate-600'}`}
          role="switch"
          aria-checked={particleEffectsEnabled ? 'true' : 'false'}
          aria-label="Toggle particle effects"
          title={`Particle effects: ${particleEffectsEnabled ? 'ON' : 'OFF'}`}
        >
          <span
            aria-hidden="true"
            className={`pointer-events-none inline-block h-4 w-4 rounded-full bg-white shadow-lg ring-0 transition duration-200 ease-in-out ${particleEffectsEnabled ? 'translate-x-4' : 'translate-x-0'}`}
          />
        </button>
        <span className={`text-xs font-mono w-8 text-center ${particleEffectsEnabled ? 'text-green-400' : 'text-slate-500'}`}>
          {particleEffectsEnabled ? 'ON' : 'OFF'}
        </span>
      </div>
      {/* Mode Toggle Switch + Info */}
      <div className="flex items-center gap-1">
        <span className="text-xs font-medium">Mode:</span>
        <label className="relative inline-flex items-center cursor-pointer">
          <input 
            type="checkbox" 
            checked={nexusExpanded} 
            onChange={handleNexusToggle} 
            className="sr-only peer" 
          />
          <div className="w-11 h-6 bg-slate-600 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-cyan-400 rounded-full peer dark:bg-slate-700 peer-checked:bg-cyan-500 transition-all duration-200"></div>
          <span className="absolute left-1.5 top-0.5 text-xs font-bold text-slate-300 peer-checked:text-cyan-900 select-none">BASIC</span>
          <span className="absolute right-1.5 top-0.5 text-xs font-bold text-cyan-900 peer-checked:text-slate-300 select-none">ENHANCED</span>
        </label>
        {/* Info icon for Basic/Enhanced */}
        <span className="ml-1 text-cyan-400 cursor-pointer group relative">
          <svg width="14" height="14" fill="none" viewBox="0 0 24 24" stroke="currentColor" className="inline-block align-middle">
            <circle cx="12" cy="12" r="10" strokeWidth="2" />
            <text x="12" y="16" textAnchor="middle" fontSize="12" fill="currentColor">i</text>
          </svg>
          <span className="absolute left-1/2 -translate-x-1/2 top-6 w-56 bg-slate-900 text-xs text-slate-200 rounded-lg shadow-lg p-2 opacity-0 group-hover:opacity-100 pointer-events-none transition-opacity duration-200 z-50">
            <b>Basic:</b> Minimal interface for quick access.<br/><b>Enhanced:</b> Full-featured interface with advanced controls and visualizations.
          </span>
        </span>
      </div>
    </div>
  );
};

export default FXModePanel; 