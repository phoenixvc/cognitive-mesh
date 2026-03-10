"use client"

import { useState } from "react"
import { usePreferencesStore } from "@/stores"

export default function SettingsPage() {
  const {
    theme,
    setTheme,
    reducedMotion,
    setReducedMotion,
    highContrast,
    setHighContrast,
    fontSize,
    setFontSize,
    soundEnabled,
    setSoundEnabled,
    resetDefaults,
  } = usePreferencesStore()

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-xl font-bold text-white">Settings</h1>

      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Appearance</h2>
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <label htmlFor="theme-select" className="text-sm text-gray-400">Theme</label>
            <select
              id="theme-select"
              value={theme}
              onChange={(e) => setTheme(e.target.value as "dark" | "light" | "system")}
              className="rounded bg-white/10 px-3 py-1.5 text-sm text-white outline-none focus-visible:ring-2 focus-visible:ring-cyan-500"
            >
              <option value="dark">Dark</option>
              <option value="light">Light</option>
              <option value="system">System</option>
            </select>
          </div>

          <div className="flex items-center justify-between">
            <label htmlFor="font-size-select" className="text-sm text-gray-400">Font size</label>
            <select
              id="font-size-select"
              value={fontSize}
              onChange={(e) => setFontSize(e.target.value as "small" | "medium" | "large")}
              className="rounded bg-white/10 px-3 py-1.5 text-sm text-white outline-none focus-visible:ring-2 focus-visible:ring-cyan-500"
            >
              <option value="small">Small</option>
              <option value="medium">Medium</option>
              <option value="large">Large</option>
            </select>
          </div>
        </div>
      </section>

      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Accessibility</h2>
        <div className="space-y-4">
          <ToggleRow
            label="Reduced motion"
            checked={reducedMotion}
            onChange={setReducedMotion}
          />
          <ToggleRow
            label="High contrast"
            checked={highContrast}
            onChange={setHighContrast}
          />
          <ToggleRow
            label="Sound effects"
            checked={soundEnabled}
            onChange={setSoundEnabled}
          />
        </div>
      </section>

      <button
        onClick={resetDefaults}
        className="rounded-md border border-white/10 px-4 py-2 text-sm text-gray-400 hover:bg-white/5 hover:text-white transition-colors"
      >
        Reset to defaults
      </button>
    </div>
  )
}

let toggleId = 0

function ToggleRow({
  label,
  checked,
  onChange,
}: {
  label: string
  checked: boolean
  onChange: (v: boolean) => void
}) {
  const [labelId] = useState(() => `toggle-label-${++toggleId}`)

  return (
    <div className="flex items-center justify-between">
      <span id={labelId} className="text-sm text-gray-400">{label}</span>
      <button
        role="switch"
        aria-checked={checked}
        aria-labelledby={labelId}
        onClick={() => onChange(!checked)}
        className={`relative h-6 w-11 rounded-full transition-colors focus-visible:ring-2 focus-visible:ring-cyan-500 ${
          checked ? "bg-cyan-600" : "bg-white/20"
        }`}
      >
        <span
          className={`absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white transition-transform ${
            checked ? "translate-x-5" : ""
          }`}
        />
      </button>
    </div>
  )
}
