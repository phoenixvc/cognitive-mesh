"use client";

import React, { useCallback, useEffect, useState } from "react";
import {
  ChevronRight,
  ChevronLeft,
  X,
  Sparkles,
  Layers,
  Brain,
  BarChart3,
  Shield,
  Database,
  Cpu,
  Settings,
  MessageSquare,
  Move,
} from "lucide-react";
import { PreferencesService } from "@/services/preferences";

interface TourStep {
  id: string;
  title: string;
  description: string;
  targetSelector?: string;
  icon: React.ElementType;
  position: "center" | "top" | "bottom" | "left" | "right";
  spotlightPadding?: number;
}

const TOUR_STEPS: TourStep[] = [
  {
    id: "welcome",
    title: "Welcome to Cognitive Mesh",
    description:
      "Your enterprise AI transformation dashboard. This quick tour will show you the key areas of the interface. You can drag, dock, and customize everything.",
    icon: Sparkles,
    position: "center",
  },
  {
    id: "bridge-header",
    title: "Bridge Controls",
    description:
      "The bridge header provides system-level controls: particle effects, layout modes (radial/grid/freeform), dock handle styles, and grid snapping. Think of it as your command center.",
    targetSelector: "[data-tour='bridge-header']",
    icon: Cpu,
    position: "bottom",
    spotlightPadding: 8,
  },
  {
    id: "architecture-layers",
    title: "Architecture Layers",
    description:
      "The five-layer hexagonal architecture: Foundation, Reasoning, Metacognitive, Agency, and Business Applications. Click any layer to drill down into its subsystems.",
    targetSelector: "[data-tour='architecture']",
    icon: Layers,
    position: "right",
    spotlightPadding: 8,
  },
  {
    id: "agents",
    title: "Active Agents",
    description:
      "View and manage your AI agents. Each agent shows its status (active/idle), current task count, and energy level. Agents coordinate through the multi-agent orchestration engine.",
    targetSelector: "[data-tour='agents']",
    icon: Brain,
    position: "left",
    spotlightPadding: 8,
  },
  {
    id: "metrics",
    title: "System Metrics",
    description:
      "Real-time performance metrics including MAKER benchmark scores, reasoning transparency, memory utilization, and ethical compliance ratings.",
    targetSelector: "[data-tour='metrics']",
    icon: BarChart3,
    position: "bottom",
    spotlightPadding: 8,
  },
  {
    id: "security",
    title: "Security & Compliance",
    description:
      "GDPR, EU AI Act, and NIST compliance status at a glance. Security monitoring includes circuit breaker status, threat intelligence, and audit trails.",
    targetSelector: "[data-tour='security']",
    icon: Shield,
    position: "right",
    spotlightPadding: 8,
  },
  {
    id: "nexus",
    title: "Command Nexus",
    description:
      "Your AI command center. Click to expand the Nexus for voice commands, system queries, and direct agent interaction. It can be dragged and docked anywhere.",
    targetSelector: "[data-tour='nexus']",
    icon: MessageSquare,
    position: "top",
    spotlightPadding: 12,
  },
  {
    id: "drag-drop",
    title: "Drag & Dock System",
    description:
      "Every module is draggable. Grab the handle to move modules, snap them to the grid, or dock them in designated zones. Use the layout tools panel to switch between radial, grid, and freeform modes.",
    icon: Move,
    position: "center",
  },
  {
    id: "storage-config",
    title: "Storage Configuration",
    description:
      "Cognitive Mesh supports polyglot persistence with 8 database backends and 5 vector search providers. Configure your storage layer anytime from Settings. Your current setup was configured during the setup wizard.",
    icon: Database,
    position: "center",
  },
  {
    id: "settings",
    title: "Settings & Preferences",
    description:
      "Access the settings panel to reconfigure your database backends, adjust visual effects, change language, and re-run the setup wizard at any time.",
    icon: Settings,
    position: "center",
  },
];

const GuidedTour: React.FC = () => {
  const [isActive, setIsActive] = useState(false);
  const [currentStepIndex, setCurrentStepIndex] = useState(0);
  const [spotlightRect, setSpotlightRect] = useState<DOMRect | null>(null);

  // Check if tour should auto-show
  useEffect(() => {
    const service = PreferencesService.getInstance();
    if (service.isSetupCompleted() && !service.isTourCompleted()) {
      // Show tour after a short delay for setup wizard to close
      const timer = setTimeout(() => setIsActive(true), 800);
      return () => clearTimeout(timer);
    }
  }, []);

  const currentStep = TOUR_STEPS[currentStepIndex];

  // Position spotlight on target element
  useEffect(() => {
    if (!isActive || !currentStep.targetSelector) {
      setSpotlightRect(null);
      return;
    }

    const el = document.querySelector(currentStep.targetSelector);
    if (el) {
      const rect = el.getBoundingClientRect();
      const padding = currentStep.spotlightPadding ?? 0;
      setSpotlightRect(
        new DOMRect(
          rect.x - padding,
          rect.y - padding,
          rect.width + padding * 2,
          rect.height + padding * 2
        )
      );
    } else {
      setSpotlightRect(null);
    }
  }, [isActive, currentStepIndex, currentStep]);

  const goNext = useCallback(() => {
    if (currentStepIndex < TOUR_STEPS.length - 1) {
      setCurrentStepIndex((i) => i + 1);
    } else {
      completeTour();
    }
  }, [currentStepIndex]);

  const goBack = useCallback(() => {
    if (currentStepIndex > 0) {
      setCurrentStepIndex((i) => i - 1);
    }
  }, [currentStepIndex]);

  const completeTour = useCallback(() => {
    PreferencesService.getInstance().markTourCompleted();
    setIsActive(false);
  }, []);

  const dismiss = useCallback(() => {
    PreferencesService.getInstance().markTourCompleted();
    setIsActive(false);
  }, []);

  // Public API to start tour programmatically
  useEffect(() => {
    const handler = () => {
      setCurrentStepIndex(0);
      setIsActive(true);
    };
    window.addEventListener("cognitive-mesh:start-tour", handler);
    return () =>
      window.removeEventListener("cognitive-mesh:start-tour", handler);
  }, []);

  if (!isActive) return null;

  const Icon = currentStep.icon;
  const isLastStep = currentStepIndex === TOUR_STEPS.length - 1;

  // Calculate tooltip position
  const getTooltipStyle = (): React.CSSProperties => {
    if (!spotlightRect || currentStep.position === "center") {
      return {
        position: "fixed",
        top: "50%",
        left: "50%",
        transform: "translate(-50%, -50%)",
      };
    }

    const margin = 16;
    switch (currentStep.position) {
      case "bottom":
        return {
          position: "fixed",
          top: spotlightRect.bottom + margin,
          left: spotlightRect.left + spotlightRect.width / 2,
          transform: "translateX(-50%)",
        };
      case "top":
        return {
          position: "fixed",
          bottom: window.innerHeight - spotlightRect.top + margin,
          left: spotlightRect.left + spotlightRect.width / 2,
          transform: "translateX(-50%)",
        };
      case "left":
        return {
          position: "fixed",
          top: spotlightRect.top + spotlightRect.height / 2,
          right: window.innerWidth - spotlightRect.left + margin,
          transform: "translateY(-50%)",
        };
      case "right":
        return {
          position: "fixed",
          top: spotlightRect.top + spotlightRect.height / 2,
          left: spotlightRect.right + margin,
          transform: "translateY(-50%)",
        };
      default:
        return {
          position: "fixed",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
        };
    }
  };

  return (
    <div className="fixed inset-0 z-[200]">
      {/* Overlay with spotlight cutout */}
      <svg className="absolute inset-0 w-full h-full">
        <defs>
          <mask id="tour-spotlight-mask">
            <rect width="100%" height="100%" fill="white" />
            {spotlightRect && (
              <rect
                x={spotlightRect.x}
                y={spotlightRect.y}
                width={spotlightRect.width}
                height={spotlightRect.height}
                rx="12"
                fill="black"
              />
            )}
          </mask>
        </defs>
        <rect
          width="100%"
          height="100%"
          fill="rgba(0, 0, 0, 0.75)"
          mask="url(#tour-spotlight-mask)"
        />
        {spotlightRect && (
          <rect
            x={spotlightRect.x}
            y={spotlightRect.y}
            width={spotlightRect.width}
            height={spotlightRect.height}
            rx="12"
            fill="none"
            stroke="rgba(0, 212, 255, 0.4)"
            strokeWidth="2"
            className="animate-pulse"
          />
        )}
      </svg>

      {/* Tooltip card */}
      <div
        style={getTooltipStyle()}
        className="w-80 bg-slate-900/95 border border-slate-700/60 rounded-xl shadow-2xl shadow-cyan-500/10 p-5 z-10"
      >
        {/* Close button */}
        <button
          onClick={dismiss}
          className="absolute top-3 right-3 w-6 h-6 rounded-full bg-slate-800/80 flex items-center justify-center text-slate-400 hover:text-white transition-colors"
        >
          <X className="w-3.5 h-3.5" />
        </button>

        {/* Icon + title */}
        <div className="flex items-center gap-3 mb-3">
          <div className="w-9 h-9 rounded-lg bg-cyan-500/20 flex items-center justify-center">
            <Icon className="w-5 h-5 text-cyan-400" />
          </div>
          <h3 className="text-sm font-bold text-white">{currentStep.title}</h3>
        </div>

        {/* Description */}
        <p className="text-xs text-slate-400 leading-relaxed mb-4">
          {currentStep.description}
        </p>

        {/* Progress + navigation */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-1">
            {TOUR_STEPS.map((_, idx) => (
              <div
                key={idx}
                className={`w-1.5 h-1.5 rounded-full transition-all ${
                  idx === currentStepIndex
                    ? "bg-cyan-400 w-4"
                    : idx < currentStepIndex
                      ? "bg-cyan-500/40"
                      : "bg-slate-600"
                }`}
              />
            ))}
          </div>

          <div className="flex items-center gap-2">
            {currentStepIndex > 0 && (
              <button
                onClick={goBack}
                className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
            )}
            <button
              onClick={isLastStep ? completeTour : goNext}
              className="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-medium bg-cyan-500/20 text-cyan-300 border border-cyan-500/30 hover:bg-cyan-500/30 transition-colors"
            >
              {isLastStep ? "Finish Tour" : "Next"}
              {!isLastStep && <ChevronRight className="w-3.5 h-3.5" />}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GuidedTour;
