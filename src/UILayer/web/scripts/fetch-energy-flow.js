const fs = require("fs")
const https = require("https")
const path = require("path")

const url = "https://uxcanvas.ai/api/projects/300ccbb5-5157-4041-b7df-4cb9ecbd2fed/25/code/EnergyFlow"

console.log("Fetching EnergyFlow component from:", url)

https
  .get(url, (res) => {
    console.log("Response status:", res.statusCode)
    let data = ""

    res.on("data", (chunk) => {
      data += chunk
    })

    res.on("end", () => {
      console.log("Data received, length:", data.length)

      const filePath = "src/components/EnergyFlow/index.tsx"
      const dir = path.dirname(filePath)

      // Create directory if it doesn't exist
      if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true })
        console.log("Created directory:", dir)
      }

      try {
        fs.writeFileSync(filePath, data)
        console.log("✅ EnergyFlow component fetched and saved successfully to:", filePath)
        console.log("First 200 characters of fetched code:")
        console.log(data)
      } catch (error) {
        console.error("❌ Error writing file:", error.message)
      }
    })
  })
  .on("error", (err) => {
    console.error("❌ Error fetching EnergyFlow:", err.message)
    console.log("Using fallback component instead...")

    // Fallback component code
    const fallbackCode = `"use client"

import type React from "react"
import { useEffect, useRef } from "react"
import styles from "./EnergyFlow.module.css"

interface EnergyFlowProps {
  isActive?: boolean
  intensity?: number
  color?: string
  direction?: "horizontal" | "vertical" | "diagonal"
  className?: string
}

export const EnergyFlow: React.FC<EnergyFlowProps> = ({
  isActive = true,
  intensity = 1,
  color = "#00ffff",
  direction = "horizontal",
  className = "",
}) => {
  const canvasRef = useRef<HTMLCanvasElement>(null)

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext("2d")
    if (!ctx) return

    let animationId: number
    let time = 0

    const animate = () => {
      if (!isActive) return

      ctx.clearRect(0, 0, canvas.width, canvas.height)

      // Create energy flow effect
      const gradient = ctx.createLinearGradient(0, 0, canvas.width, 0)
      gradient.addColorStop(0, "transparent")
      gradient.addColorStop(0.5, color)
      gradient.addColorStop(1, "transparent")

      ctx.strokeStyle = gradient
      ctx.lineWidth = 2 * intensity
      ctx.shadowColor = color
      ctx.shadowBlur = 10 * intensity

      // Draw flowing energy lines
      for (let i = 0; i < 3; i++) {
        ctx.beginPath()
        const offset = (time + i * 100) % 300
        const y = canvas.height / 2 + Math.sin(time * 0.01 + i) * 5

        ctx.moveTo(-50 + offset, y)
        ctx.lineTo(50 + offset, y)
        ctx.stroke()
      }

      time += 2
      animationId = requestAnimationFrame(animate)
    }

    animate()

    return () => {
      if (animationId) {
        cancelAnimationFrame(animationId)
      }
    }
  }, [isActive, intensity, color])

  return (
    <div className={\`\${styles.energyFlow} \${className}\`}>
      <canvas ref={canvasRef} width={200} height={50} className={styles.canvas} />
    </div>
  )
}

export default EnergyFlow`

    const filePath = "src/components/EnergyFlow/index.tsx"
    const dir = path.dirname(filePath)

    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true })
    }

    fs.writeFileSync(filePath, fallbackCode)
    console.log("✅ Fallback EnergyFlow component created successfully")
  })
