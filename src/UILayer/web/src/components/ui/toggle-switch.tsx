"use client"

import { useId } from "react"

export function ToggleRow({
  label,
  description,
  checked,
  onChange,
  disabled,
}: {
  label: string
  description?: string
  checked: boolean
  onChange: (v: boolean) => void
  disabled?: boolean
}) {
  const id = useId()

  return (
    <div className={`flex items-center justify-between gap-4 ${disabled ? "opacity-50" : ""}`}>
      <div>
        <span id={id} className="text-sm text-gray-400">
          {label}
        </span>
        {description && <p className="text-xs text-gray-600">{description}</p>}
      </div>
      <ToggleButton checked={checked} onChange={onChange} disabled={disabled} label={label} />
    </div>
  )
}

export function ToggleButton({
  checked,
  onChange,
  disabled,
  label,
}: {
  checked: boolean
  onChange: (v: boolean) => void
  disabled?: boolean
  label: string
}) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={checked ? "true" : "false"}
      aria-label={label}
      disabled={disabled}
      onClick={() => onChange(!checked)}
      className={`relative h-6 w-11 shrink-0 rounded-full transition-colors focus-visible:ring-2 focus-visible:ring-cyan-500 disabled:cursor-not-allowed ${
        checked ? "bg-cyan-600" : "bg-white/20"
      }`}
    >
      <span
        className={`absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white transition-transform ${
          checked ? "translate-x-5" : ""
        }`}
      />
    </button>
  )
}
