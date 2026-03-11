"use client"

import { useId } from "react"
import { usePreferencesStore } from "@/stores"
import type { NotificationPreferences } from "@/stores/usePreferencesStore"

const NOTIFICATION_CATEGORIES = [
  { id: "approvals", label: "Approvals", description: "Approval requests and decisions" },
  { id: "security", label: "Security", description: "Security alerts and access changes" },
  { id: "system", label: "System", description: "System status and maintenance" },
  { id: "agents", label: "Agent activity", description: "Agent status changes and completions" },
  { id: "compliance", label: "Compliance", description: "Compliance deadlines and audit results" },
] as const

export default function NotificationPreferencesPage() {
  const {
    notificationsEnabled,
    setNotificationsEnabled,
    notificationPreferences,
    setNotificationChannel,
    setQuietHours,
  } = usePreferencesStore()

  const { channels, quietHours } = notificationPreferences

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h1 className="text-xl font-bold text-white">Notification Preferences</h1>
        <p className="mt-1 text-sm text-gray-500">
          Choose how and when you receive notifications.
        </p>
      </div>

      {/* Master toggle */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <ToggleRow
          label="Enable notifications"
          description="Turn off to mute all notifications"
          checked={notificationsEnabled}
          onChange={setNotificationsEnabled}
        />
      </section>

      {/* Channels */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Channels</h2>
        <div className="space-y-4">
          <ToggleRow
            label="In-app"
            description="Show notifications in the notification center"
            checked={channels.inApp}
            onChange={(v) => setNotificationChannel("inApp", v)}
            disabled={!notificationsEnabled}
          />
          <ToggleRow
            label="Email"
            description="Send notifications to your email address"
            checked={channels.email}
            onChange={(v) => setNotificationChannel("email", v)}
            disabled={!notificationsEnabled}
          />
          <ToggleRow
            label="Push"
            description="Browser push notifications"
            checked={channels.push}
            onChange={(v) => setNotificationChannel("push", v)}
            disabled={!notificationsEnabled}
          />
          <ToggleRow
            label="SMS"
            description="Text message notifications (urgent only)"
            checked={channels.sms}
            onChange={(v) => setNotificationChannel("sms", v)}
            disabled={!notificationsEnabled}
          />
        </div>
      </section>

      {/* Categories */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Categories</h2>
        <p className="mb-4 text-xs text-gray-500">
          Enable or disable notifications by category. Channel overrides can be configured per category.
        </p>
        <div className="space-y-3">
          {NOTIFICATION_CATEGORIES.map((cat) => (
            <CategoryRow
              key={cat.id}
              category={cat}
              preferences={notificationPreferences}
              disabled={!notificationsEnabled}
            />
          ))}
        </div>
      </section>

      {/* Quiet hours */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Quiet Hours</h2>
        <div className="space-y-4">
          <ToggleRow
            label="Enable quiet hours"
            description="Suppress non-urgent notifications during specified times"
            checked={quietHours.enabled}
            onChange={(v) => setQuietHours({ enabled: v })}
            disabled={!notificationsEnabled}
          />
          {quietHours.enabled && notificationsEnabled && (
            <div className="ml-1 grid grid-cols-2 gap-4">
              <TimeInput
                label="Start time"
                value={quietHours.startTime}
                onChange={(v) => setQuietHours({ startTime: v })}
              />
              <TimeInput
                label="End time"
                value={quietHours.endTime}
                onChange={(v) => setQuietHours({ endTime: v })}
              />
              <div className="col-span-2">
                <label className="mb-1 block text-xs text-gray-500">Timezone</label>
                <input
                  type="text"
                  value={quietHours.timezone}
                  onChange={(e) => setQuietHours({ timezone: e.target.value })}
                  className="w-full rounded bg-white/10 px-3 py-1.5 text-sm text-white outline-none focus-visible:ring-2 focus-visible:ring-cyan-500"
                  placeholder="America/New_York"
                />
              </div>
            </div>
          )}
        </div>
      </section>

      <a
        href="/settings"
        className="inline-block rounded-md border border-white/10 px-4 py-2 text-sm text-gray-400 transition-colors hover:bg-white/5 hover:text-white"
      >
        Back to settings
      </a>
    </div>
  )
}

function CategoryRow({
  category,
  preferences,
  disabled,
}: {
  category: { id: string; label: string; description: string }
  preferences: NotificationPreferences
  disabled: boolean
}) {
  const existing = preferences.categories.find((c) => c.category === category.id)
  const enabled = existing ? existing.enabled : true
  const store = usePreferencesStore()

  function toggleCategory(value: boolean) {
    const updated = preferences.categories.filter((c) => c.category !== category.id)
    updated.push({
      category: category.id,
      enabled: value,
      channels: existing?.channels ?? { ...preferences.channels },
    })
    store.setNotificationPreferences({ categories: updated })
  }

  return (
    <div className="flex items-center justify-between rounded-md border border-white/5 bg-white/[0.02] px-4 py-3">
      <div>
        <span className="text-sm text-gray-300">{category.label}</span>
        <p className="text-xs text-gray-600">{category.description}</p>
      </div>
      <ToggleButton checked={enabled} onChange={toggleCategory} disabled={disabled} label={category.label} />
    </div>
  )
}

function TimeInput({
  label,
  value,
  onChange,
}: {
  label: string
  value: string
  onChange: (v: string) => void
}) {
  const id = useId()
  return (
    <div>
      <label htmlFor={id} className="mb-1 block text-xs text-gray-500">
        {label}
      </label>
      <input
        id={id}
        type="time"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full rounded bg-white/10 px-3 py-1.5 text-sm text-white outline-none focus-visible:ring-2 focus-visible:ring-cyan-500"
      />
    </div>
  )
}

function ToggleRow({
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

function ToggleButton({
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
