import { EnergyFlow } from "@/components";
import { Activity, CheckCircle, Database, Lock, Shield } from "lucide-react";
import React from "react";

interface Layer {
  id: string;
  name: string;
  icon: LucideIcon;
  color: string;
  uptime: number;
  description: string;
}

interface Agent {
  name: string;
  status: "active" | "idle";
  tasks: number;
  energy: number;
}

interface Activity {
  time: string;
  event: string;
  type: string;
}

interface DraggableModuleContentProps {
  type: "architecture" | "security" | "resources" | "agents" | "activity";
  layers?: Layer[];
  agents?: Agent[];
  activities?: Activity[];
  activeLayer?: string;
  particleEffectsEnabled?: boolean;
  effectSpeed?: number;
  onLayerClick?: (layerId: string) => void;
}

const DraggableModuleContent: React.FC<DraggableModuleContentProps> = ({
  type,
  layers = [],
  agents = [],
  activities = [],
  activeLayer,
  particleEffectsEnabled = false,
  effectSpeed = 1.0,
  onLayerClick,
}) => {
  const renderArchitectureContent = () => (
    <div className="space-y-4">
      {layers.map((layer) => {
        const Icon = layer.icon;
        const isActive = activeLayer === layer.id;
        return (
          <div
            key={layer.id}
            onClick={() => onLayerClick?.(layer.id)}
            className={`p-4 rounded-lg border cursor-pointer transition-all duration-300 relative overflow-hidden ${
              isActive
                ? "bg-cyan-500/20 border-cyan-500/50 shadow-cyan-500/20"
                : "bg-slate-800/30 border-slate-700/50 hover:bg-slate-700/30"
            }`}
          >
            {isActive && (
              <EnergyFlow direction="horizontal" intensity="high" color="cyan" className="absolute inset-0" />
            )}

            <div className="flex items-center space-x-4 relative z-10">
              <div className={`p-3 rounded-lg ${isActive ? "bg-cyan-500/30" : "bg-slate-700/50"}`}>
                <Icon size={24} className={isActive ? "text-cyan-400" : "text-slate-400"} />
              </div>
              <div className="flex-1">
                <h4 className="font-semibold text-white">{layer.name}</h4>
                <p className="text-sm text-slate-400">{layer.description}</p>
              </div>
              <div className="text-right">
                <div className="text-lg font-bold text-white">{layer.uptime}%</div>
                <div className="text-xs text-slate-400">Uptime</div>
                <div
                  className={`w-2 h-2 rounded-full mt-1 ${
                    layer.uptime > 95
                      ? `bg-green-400 ${particleEffectsEnabled ? 'animate-pulse' : ''}`
                      : layer.uptime > 90
                        ? `bg-yellow-400 ${particleEffectsEnabled ? 'animate-pulse' : ''}`
                        : `bg-red-400 ${particleEffectsEnabled ? 'animate-pulse' : ''}`
                  }`}
                  style={{
                    animationDuration: particleEffectsEnabled ? `${2 / effectSpeed}s` : undefined,
                  }}
                />
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );

  const renderSecurityContent = () => (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <Lock size={16} className="text-green-400" />
          <span className="text-sm">Zero-Trust Protocol</span>
        </div>
        <div className="flex items-center space-x-2">
          <CheckCircle size={16} className="text-green-400" />
          <span className="text-xs text-green-400">ACTIVE</span>
        </div>
      </div>
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <Shield size={16} className="text-green-400" />
          <span className="text-sm">NIST AI RMF</span>
        </div>
        <div className="flex items-center space-x-2">
          <CheckCircle size={16} className="text-green-400" />
          <span className="text-xs text-green-400">COMPLIANT</span>
        </div>
      </div>
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <Database size={16} className="text-green-400" />
          <span className="text-sm">Quantum Encryption</span>
        </div>
        <div className="flex items-center space-x-2">
          <CheckCircle size={16} className="text-green-400" />
          <span className="text-xs text-green-400">SECURED</span>
        </div>
      </div>
    </div>
  );

  const renderResourcesContent = () => (
    <div className="space-y-4">
      <div>
        <div className="flex justify-between text-sm mb-2">
          <span>Neural Processing</span>
          <span>67%</span>
        </div>
        <div className="w-full bg-slate-700 rounded-full h-3 relative overflow-hidden">
          <div
            className="bg-gradient-to-r from-cyan-500 to-blue-500 h-3 rounded-full transition-all duration-1000"
            style={{
              width: "67%",
              transitionDuration: `${1000 / effectSpeed}ms`,
            }}
          />
          <div
            className={`absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent ${particleEffectsEnabled ? 'animate-pulse' : ''}`}
            style={{
              animationDuration: particleEffectsEnabled ? `${2 / effectSpeed}s` : undefined,
            }}
          />
        </div>
      </div>
      <div>
        <div className="flex justify-between text-sm mb-2">
          <span>Quantum Memory</span>
          <span>43%</span>
        </div>
        <div className="w-full bg-slate-700 rounded-full h-3 relative overflow-hidden">
          <div
            className="bg-gradient-to-r from-green-500 to-emerald-500 h-3 rounded-full transition-all duration-1000"
            style={{
              width: "43%",
              transitionDuration: `${1000 / effectSpeed}ms`,
            }}
          />
          <div
            className={`absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent ${particleEffectsEnabled ? 'animate-pulse' : ''}`}
            style={{
              animationDuration: particleEffectsEnabled ? `${2 / effectSpeed}s` : undefined,
            }}
          />
        </div>
      </div>
      <div>
        <div className="flex justify-between text-sm mb-2">
          <span>Data Streams</span>
          <span>82%</span>
        </div>
        <div className="w-full bg-slate-700 rounded-full h-3 relative overflow-hidden">
          <div
            className="bg-gradient-to-r from-purple-500 to-pink-500 h-3 rounded-full transition-all duration-1000"
            style={{
              width: "82%",
              transitionDuration: `${1000 / effectSpeed}ms`,
            }}
          />
          <div
            className={`absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent ${particleEffectsEnabled ? 'animate-pulse' : ''}`}
            style={{
              animationDuration: particleEffectsEnabled ? `${2 / effectSpeed}s` : undefined,
            }}
          />
        </div>
      </div>
    </div>
  );

  const renderAgentsContent = () => (
    <div className="space-y-3">
      {agents.map((agent, index) => (
        <div key={index} className="flex items-center justify-between p-3 bg-slate-800/30 rounded-lg">
          <div className="flex items-center space-x-3">
            <div
              className={`w-3 h-3 rounded-full ${
                agent.status === "active" ? `bg-green-400 ${particleEffectsEnabled ? 'animate-pulse' : ''}` : "bg-slate-500"
              }`}
              style={{
                animationDuration: particleEffectsEnabled ? `${2 / effectSpeed}s` : undefined,
              }}
            />
            <div>
              <div className="text-sm font-medium text-white">{agent.name}</div>
              <div className="text-xs text-slate-400">{agent.tasks} active tasks</div>
            </div>
          </div>
          <div className="text-right">
            <div className="text-xs text-slate-400">Energy</div>
            <div className="w-16 bg-slate-700 rounded-full h-2 mt-1">
              <div
                className="bg-gradient-to-r from-cyan-500 to-blue-500 h-2 rounded-full transition-all duration-1000"
                style={{
                  width: `${agent.energy * 100}%`,
                  transitionDuration: `${1000 / effectSpeed}ms`,
                }}
              />
            </div>
          </div>
        </div>
      ))}
    </div>
  );

  const renderActivityContent = () => (
    <div className="space-y-3">
      {activities.map((activity, index) => (
        <div
          key={index}
          className="flex items-center space-x-3 p-2 hover:bg-slate-800/30 rounded-lg transition-colors"
        >
          <div
            className={`w-2 h-2 rounded-full ${
              activity.type === "security"
                ? "bg-red-400"
                : activity.type === "deployment"
                  ? "bg-green-400"
                  : activity.type === "optimization"
                    ? "bg-blue-400"
                    : activity.type === "compliance"
                      ? "bg-yellow-400"
                      : "bg-purple-400"
            }`}
          />
          <div className="flex-1">
            <div className="text-sm text-white">{activity.event}</div>
            <div className="text-xs text-slate-400">{activity.time}</div>
          </div>
        </div>
      ))}
    </div>
  );

  switch (type) {
    case "architecture":
      return renderArchitectureContent();
    case "security":
      return renderSecurityContent();
    case "resources":
      return renderResourcesContent();
    case "agents":
      return renderAgentsContent();
    case "activity":
      return renderActivityContent();
    default:
      return null;
  }
};

export default DraggableModuleContent; 