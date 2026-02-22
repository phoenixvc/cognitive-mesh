"use client"
import type { IconPosition, NexusModule } from "@/types/nexus"
import { useCallback, useEffect, useState } from "react"

export interface OrbitalIconsProps {
  modules: NexusModule[]
  isExpanded: boolean
  orbitMode: boolean
  iconPositions: IconPosition[]
  onIconPositionsChange: (positions: IconPosition[]) => void
  onModuleClick: (moduleId: string) => void
  activeModule: NexusModule | null
  orbitRadius?: number
  animationDuration?: number
  className?: string
}

export default function OrbitalIcons({
  modules,
  isExpanded,
  orbitMode,
  iconPositions,
  onIconPositionsChange,
  onModuleClick,
  activeModule,
  orbitRadius = 150,
  animationDuration = 500,
  className = "",
}: OrbitalIconsProps) {
  const [isTransitioning, setIsTransitioning] = useState(false)

  // Calculate orbital positions
  const calculateOrbitalPositions = useCallback((): IconPosition[] => {
    return modules.map((module, index) => {
      const angle = (index * 2 * Math.PI) / modules.length
      const x = Math.cos(angle) * orbitRadius
      const y = Math.sin(angle) * orbitRadius
      
      return {
        x,
        y,
        angle,
        moduleId: module.id,
      }
    })
  }, [modules, orbitRadius])

  // Calculate static positions
  const calculateStaticPositions = useCallback((): IconPosition[] => {
    return modules.map((module, index) => {
      const angle = (index * 2 * Math.PI) / modules.length
      const x = Math.cos(angle) * (orbitRadius * 0.7)
      const y = Math.sin(angle) * (orbitRadius * 0.7)
      
      return {
        x,
        y,
        angle,
        moduleId: module.id,
      }
    })
  }, [modules, orbitRadius])

  // Update positions when orbit mode changes
  useEffect(() => {
    setIsTransitioning(true)
    
    const newPositions = orbitMode ? calculateOrbitalPositions() : calculateStaticPositions()
    onIconPositionsChange(newPositions)

    const timeout = setTimeout(() => {
      setIsTransitioning(false)
    }, animationDuration)

    return () => clearTimeout(timeout)
  }, [orbitMode, calculateOrbitalPositions, calculateStaticPositions, onIconPositionsChange, animationDuration])

  // Initialize positions
  useEffect(() => {
    if (iconPositions.length === 0) {
      const initialPositions = orbitMode ? calculateOrbitalPositions() : calculateStaticPositions()
      onIconPositionsChange(initialPositions)
    }
  }, [iconPositions.length, orbitMode, calculateOrbitalPositions, calculateStaticPositions, onIconPositionsChange])

  // Handle module click with sound and animation
  const handleModuleClick = useCallback((moduleId: string, index: number) => {
    onModuleClick(moduleId)
    
    // Add click animation
    const iconElement = document.getElementById(`orbital-icon-${index}`)
    if (iconElement) {
      iconElement.classList.add("animate-ping")
      setTimeout(() => {
        iconElement.classList.remove("animate-ping")
      }, 300)
    }
  }, [onModuleClick])

  // Only render icons when not expanded or when in orbit mode
  const shouldRender = !isExpanded || orbitMode

  if (!shouldRender) return null

  return (
    <div className={`absolute inset-0 pointer-events-none ${className}`}>
      {modules.map((module, index) => {
        const position = iconPositions[index]
        if (!position) return null

        const Icon = module.icon
        const isActive = activeModule?.id === module.id

        return (
          <div
            key={module.id}
            id={`orbital-icon-${index}`}
            className={`
              absolute pointer-events-auto
              transition-all duration-${animationDuration}ms ease-in-out
              ${isTransitioning ? 'transition-transform' : ''}
              ${orbitMode ? 'z-50' : 'z-40'}
            `}
            style={{
              transform: `translate(-50%, -50%) translate(${position.x}px, ${position.y}px) rotate(${position.angle}rad)`,
              left: '50%',
              top: '50%',
            }}
          >
            <button
              onClick={() => handleModuleClick(module.id, index)}
              className={`
                w-14 h-14 backdrop-blur-md rounded-xl
                flex items-center justify-center cursor-pointer
                transition-all duration-300 hover:scale-110
                transform-gpu
                ${isActive 
                  ? `bg-${module.color}-500/30 border-2 border-${module.color}-400 shadow-lg shadow-${module.color}-500/50` 
                  : `bg-slate-900/80 border border-${module.color}-500/50 hover:border-${module.color}-400`
                }
                ${orbitMode ? 'animate-pulse' : ''}
                hover:shadow-${module.color}-500/30
              `}
              title={module.label}
            >
              <Icon 
                size={22} 
                className={`
                  transition-colors duration-300
                  ${isActive ? `text-${module.color}-200` : `text-${module.color}-400`}
                `} 
              />
            </button>

            {/* Active glow effect */}
            {isActive && (
              <div className="absolute -inset-1 rounded-xl opacity-50 animate-pulse">
                <div className={`w-full h-full rounded-xl bg-gradient-to-r from-${module.color}-500/30 to-${module.color}-600/30`} />
              </div>
            )}

            {/* Orbit trail effect */}
            {orbitMode && (
              <div 
                className={`
                  absolute inset-0 rounded-xl opacity-20
                  bg-gradient-to-r from-transparent via-${module.color}-500/20 to-transparent
                  animate-pulse
                `}
                style={{
                  animation: `orbit-trail 2s linear infinite ${index * 0.5}s`,
                }}
              />
            )}
          </div>
        )
      })}

      {/* Orbit mode indicator */}
      {orbitMode && (
        <div className="absolute inset-0 pointer-events-none">
          <div 
            className="absolute border border-cyan-500/20 rounded-full"
            style={{
              width: `${orbitRadius * 2}px`,
              height: `${orbitRadius * 2}px`,
              left: '50%',
              top: '50%',
              transform: 'translate(-50%, -50%)',
            }}
          />
        </div>
      )}

      {/* Custom CSS for orbit trail animation */}
      <style jsx>{`
        @keyframes orbit-trail {
          0% { opacity: 0.2; }
          50% { opacity: 0.6; }
          100% { opacity: 0.2; }
        }
      `}</style>
    </div>
  )
} 