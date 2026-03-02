"use client"

import React from 'react';
import styles from './energy-flow.module.css';

interface EnergyFlowProps {
  direction?: "horizontal" | "vertical" | "diagonal"
  intensity?: "low" | "medium" | "high"
  color?: "cyan" | "blue" | "purple" | "green"
  className?: string
}

export default function EnergyFlow({
  direction = "horizontal",
  intensity = "medium",
  color = "cyan",
  className = "",
}: EnergyFlowProps) {
  const getFlowPath = () => {
    switch (direction) {
      case "vertical":
        return "M 0 0 L 0 100"
      case "diagonal":
        return "M 0 0 L 100 100"
      default:
        return "M 0 0 L 100 0"
    }
  }

  const getAnimationDuration = () => {
    // Get effect speed from CSS custom property, default to 1
    let effectSpeed = 1
    if (typeof window !== "undefined") {
      effectSpeed = Number.parseFloat(
        getComputedStyle(document.documentElement).getPropertyValue("--effect-speed") || "1",
      )
    }

    switch (intensity) {
      case "low":
        return `${3 / effectSpeed}s`
      case "high":
        return `${1 / effectSpeed}s`
      default:
        return `${2 / effectSpeed}s`
    }
  }

  const colorMap = {
    cyan: "#06b6d4",
    blue: "#3b82f6",
    purple: "#8b5cf6",
    green: "#10b981",
  }

  return (
    <div className={`${styles["energy-flow"]} ${className}`}>
      <svg className="w-full h-full" viewBox="0 0 100 100" preserveAspectRatio="none">
        <defs>
          <linearGradient id={`energy-gradient-${color}`} gradientUnits="userSpaceOnUse">
            <stop offset="0%" stopColor="transparent" />
            <stop offset="50%" stopColor={colorMap[color]} stopOpacity="0.8" />
            <stop offset="100%" stopColor="transparent" />
          </linearGradient>

          <filter id={`energy-glow-${color}`}>
            <feGaussianBlur stdDeviation="2" result="coloredBlur" />
            <feMerge>
              <feMergeNode in="coloredBlur" />
              <feMergeNode in="SourceGraphic" />
            </feMerge>
          </filter>
        </defs>

        {/* Main energy line */}
        <path
          d={getFlowPath()}
          stroke={`url(#energy-gradient-${color})`}
          strokeWidth="2"
          fill="none"
          filter={`url(#energy-glow-${color})`}
        />

        {/* Animated energy pulse */}
        <circle r="3" fill={colorMap[color]} filter={`url(#energy-glow-${color})`}>
          <animateMotion dur={getAnimationDuration()} repeatCount="indefinite" path={getFlowPath()} />
          <animate attributeName="opacity" values="0;1;0" dur={getAnimationDuration()} repeatCount="indefinite" />
        </circle>

        {/* Secondary pulse for more dynamic effect */}
        <circle r="2" fill={colorMap[color]} opacity="0.6">
          <animateMotion dur={getAnimationDuration()} repeatCount="indefinite" path={getFlowPath()} begin="0.5s" />
          <animate
            attributeName="opacity"
            values="0;0.6;0"
            dur={getAnimationDuration()}
            repeatCount="indefinite"
            begin="0.5s"
          />
        </circle>
      </svg>
    </div>
  )
}

export { EnergyFlow };
export type { EnergyFlowProps };

