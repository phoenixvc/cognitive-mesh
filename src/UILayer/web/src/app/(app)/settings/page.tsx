"use client"

import { useId, useState } from "react"
import Link from "next/link"
import { usePreferencesStore } from "@/stores"
import { ToggleRow } from "@/components/ui/toggle-switch"
import {
  SUPPORTED_LANGUAGES,
  LANGUAGE_LABELS,
  type SupportedLanguage,
} from "@/lib/i18n/i18nConfig"
import i18n from "@/lib/i18n/i18nConfig"

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
    language,
    setLanguage,
    privacyConsent,
    setPrivacyConsent,
    resetDefaults,
  } = usePreferencesStore()

  const [saved, setSaved] = useState(false)

  function handleLanguageChange(lang: SupportedLanguage) {
    setLanguage(lang)
    // Update i18next runtime language immediately
    i18n.changeLanguage(lang)
    // Also persist to localStorage key used by i18nConfig
    try {
      localStorage.setItem("cognitivemesh_language", lang)
    } catch {
      // localStorage may be unavailable
    }
  }

  function handleSave() {
    // Preferences are already persisted to localStorage via Zustand persist.
    // TODO: Sync to backend user preferences API.
    setSaved(true)
    setTimeout(() => setSaved(false), 2000)
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-white">Settings</h1>
        <button
          type="button"
          onClick={handleSave}
          className="rounded-md bg-cyan-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-cyan-500 disabled:opacity-50"
        >
          {saved ? "Saved" : "Save changes"}
        </button>
      </div>

      {/* Appearance */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Appearance</h2>
        <div className="space-y-4">
          <SelectRow
            label="Theme"
            value={theme}
            onChange={(v) => setTheme(v as "dark" | "light" | "system")}
            options={[
              { value: "dark", label: "Dark" },
              { value: "light", label: "Light" },
              { value: "system", label: "System" },
            ]}
          />
          <SelectRow
            label="Font size"
            value={fontSize}
            onChange={(v) => setFontSize(v as "small" | "medium" | "large")}
            options={[
              { value: "small", label: "Small" },
              { value: "medium", label: "Medium" },
              { value: "large", label: "Large" },
            ]}
          />
        </div>
      </section>

      {/* Language */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Language</h2>
        <SelectRow
          label="Display language"
          value={language}
          onChange={(v) => handleLanguageChange(v as SupportedLanguage)}
          options={SUPPORTED_LANGUAGES.map((lang) => ({
            value: lang,
            label: `${LANGUAGE_LABELS[lang].flag} ${LANGUAGE_LABELS[lang].nativeLabel}`,
          }))}
        />
      </section>

      {/* Accessibility */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Accessibility</h2>
        <div className="space-y-4">
          <ToggleRow
            label="Reduced motion"
            description="Minimize animations and transitions"
            checked={reducedMotion}
            onChange={setReducedMotion}
          />
          <ToggleRow
            label="High contrast"
            description="Increase contrast for better readability"
            checked={highContrast}
            onChange={setHighContrast}
          />
          <ToggleRow
            label="Sound effects"
            description="Play audio feedback for actions"
            checked={soundEnabled}
            onChange={setSoundEnabled}
          />
        </div>
      </section>

      {/* Data & Privacy */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Data &amp; Privacy</h2>
        <p className="mb-4 text-xs text-gray-500">
          Control how your data is collected and used. Changes are recorded for GDPR compliance.
        </p>
        <div className="space-y-4">
          <ToggleRow
            label="Analytics"
            description="Help improve the platform with anonymous usage data"
            checked={privacyConsent.analytics}
            onChange={(v) => setPrivacyConsent({ analytics: v })}
          />
          <ToggleRow
            label="Telemetry"
            description="Send performance and error data for diagnostics"
            checked={privacyConsent.telemetry}
            onChange={(v) => setPrivacyConsent({ telemetry: v })}
          />
          <ToggleRow
            label="Personalized content"
            description="Use your activity to personalize recommendations"
            checked={privacyConsent.personalizedContent}
            onChange={(v) => setPrivacyConsent({ personalizedContent: v })}
          />
          <ToggleRow
            label="Third-party sharing"
            description="Allow data sharing with approved partners"
            checked={privacyConsent.thirdPartySharing}
            onChange={(v) => setPrivacyConsent({ thirdPartySharing: v })}
          />
        </div>
        {privacyConsent.updatedAt > 0 && (
          <p className="mt-3 text-xs text-gray-600">
            Last updated: {new Date(privacyConsent.updatedAt).toLocaleString()}
          </p>
        )}
      </section>

      <div className="flex gap-3">
        <button
          type="button"
          onClick={resetDefaults}
          className="rounded-md border border-white/10 px-4 py-2 text-sm text-gray-400 transition-colors hover:bg-white/5 hover:text-white"
        >
          Reset to defaults
        </button>
        <Link
          href="/settings/notifications"
          className="rounded-md border border-white/10 px-4 py-2 text-sm text-gray-400 transition-colors hover:bg-white/5 hover:text-white"
        >
          Notification preferences
        </Link>
      </div>
    </div>
  )
}

function SelectRow({
  label,
  value,
  onChange,
  options,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  options: { value: string; label: string }[]
}) {
  const id = useId()
  return (
    <div className="flex items-center justify-between">
      <label htmlFor={id} className="text-sm text-gray-400">
        {label}
      </label>
      <select
        id={id}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="rounded bg-white/10 px-3 py-1.5 text-sm text-white outline-none focus-visible:ring-2 focus-visible:ring-cyan-500"
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
    </div>
  )
}
