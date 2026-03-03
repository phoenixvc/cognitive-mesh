import { EnergyFlow } from "@/components";
import React from "react";

interface BackgroundEffectsProps {
  particleEffectsEnabled: boolean;
  effectSpeed: number;
}

const BackgroundEffects: React.FC<BackgroundEffectsProps> = ({
  particleEffectsEnabled,
  effectSpeed,
}) => {
  if (!particleEffectsEnabled) {
    return null;
  }

  return (
    <>
      {/* Enhanced Animated Starfield Background */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-cyan-900/20 via-transparent to-transparent" />
        {[...Array(100)].map((_, i) => (
          <div
            key={i}
            className="absolute w-1 h-1 bg-white rounded-full animate-pulse"
            style={{
              left: `${Math.random() * 100}%`,
              top: `${Math.random() * 100}%`,
              animationDelay: `${Math.random() * 3}s`,
              animationDuration: `${(2 + Math.random() * 3) / effectSpeed}s`,
            }}
          />
        ))}
        {/* Add some brighter stars */}
        {[...Array(20)].map((_, i) => (
          <div
            key={`bright-${i}`}
            className="absolute w-2 h-2 bg-cyan-400 rounded-full animate-pulse"
            style={{
              left: `${Math.random() * 100}%`,
              top: `${Math.random() * 100}%`,
              animationDelay: `${Math.random() * 5}s`,
              animationDuration: `${(3 + Math.random() * 4) / effectSpeed}s`,
              opacity: 0.6,
            }}
          />
        ))}
      </div>

      {/* Enhanced Energy Flow Network */}
      <div className="absolute inset-0 pointer-events-none">
        <EnergyFlow direction="horizontal" intensity="medium" color="cyan" className="top-1/4 left-0 w-full h-px" />
        <EnergyFlow direction="vertical" intensity="low" color="blue" className="left-1/4 top-0 w-px h-full" />
        <EnergyFlow direction="diagonal" intensity="high" color="purple" className="top-1/2 left-1/2 w-1/2 h-1/2" />
        <EnergyFlow direction="horizontal" intensity="low" color="green" className="bottom-1/4 left-0 w-full h-px" />
        <EnergyFlow direction="vertical" intensity="medium" color="purple" className="right-1/3 top-0 w-px h-full" />
      </div>
    </>
  );
};

export default BackgroundEffects; 