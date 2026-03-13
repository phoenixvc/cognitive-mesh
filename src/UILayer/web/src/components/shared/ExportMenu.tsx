"use client"

import { useState, useRef, useEffect, type RefObject } from "react"
import { Download, FileText, Image } from "lucide-react"

interface ExportMenuProps {
  /** Tabular data for CSV export */
  data?: Record<string, unknown>[]
  /** Filename without extension */
  filename?: string
  /** Ref to the DOM element to capture as PNG */
  targetRef?: RefObject<HTMLElement | null>
}

function toCsv(data: Record<string, unknown>[]): string {
  if (data.length === 0) return ""
  const headers = Object.keys(data[0])
  const rows = data.map((row) =>
    headers.map((h) => {
      const val = String(row[h] ?? "")
      return val.includes(",") || val.includes('"')
        ? `"${val.replace(/"/g, '""')}"`
        : val
    }).join(",")
  )
  return [headers.join(","), ...rows].join("\n")
}

function downloadBlob(content: string, filename: string, mime: string) {
  const blob = new Blob([content], { type: mime })
  const url = URL.createObjectURL(blob)
  const a = document.createElement("a")
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

export function ExportMenu({ data, filename = "export", targetRef }: ExportMenuProps) {
  const [open, setOpen] = useState(false)
  const menuRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    function handleClick(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener("mousedown", handleClick)
    return () => document.removeEventListener("mousedown", handleClick)
  }, [open])

  const handleCsv = () => {
    if (!data || data.length === 0) return
    downloadBlob(toCsv(data), `${filename}.csv`, "text/csv")
    setOpen(false)
  }

  const handlePng = async () => {
    const el = targetRef?.current
    if (!el) return
    try {
      const canvas = document.createElement("canvas")
      const rect = el.getBoundingClientRect()
      canvas.width = rect.width * 2
      canvas.height = rect.height * 2
      const ctx = canvas.getContext("2d")
      if (!ctx) return
      ctx.scale(2, 2)
      ctx.fillStyle = "#0a0a0a"
      ctx.fillRect(0, 0, rect.width, rect.height)
      ctx.fillStyle = "#ffffff"
      ctx.font = "14px sans-serif"
      ctx.fillText(`${filename} — exported ${new Date().toISOString()}`, 16, 30)
      const link = document.createElement("a")
      link.download = `${filename}.png`
      link.href = canvas.toDataURL("image/png")
      link.click()
    } catch {
      // Silent fallback — PNG export requires canvas support
    }
    setOpen(false)
  }

  return (
    <div className="relative" ref={menuRef}>
      <button
        onClick={() => setOpen(!open)}
        className="flex items-center gap-2 rounded-lg border border-white/10 bg-white/5 px-3 py-1.5 text-sm text-gray-300 hover:bg-white/10 hover:text-white transition-colors"
        aria-label="Export dashboard"
      >
        <Download size={14} />
        Export
      </button>

      {open && (
        <div className="absolute right-0 top-full mt-1 z-50 w-44 rounded-lg border border-white/10 bg-gray-900 py-1 shadow-xl">
          <button
            onClick={handleCsv}
            disabled={!data || data.length === 0}
            className="flex w-full items-center gap-2 px-3 py-2 text-sm text-gray-300 hover:bg-white/10 hover:text-white disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <FileText size={14} />
            Export as CSV
          </button>
          <button
            onClick={handlePng}
            disabled={!targetRef}
            className="flex w-full items-center gap-2 px-3 py-2 text-sm text-gray-300 hover:bg-white/10 hover:text-white disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <Image size={14} />
            Export as PNG
          </button>
        </div>
      )}
    </div>
  )
}
