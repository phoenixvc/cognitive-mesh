"use client"

import { useId, useState } from "react"
import { useAuth } from "@/contexts/AuthContext"
import { usePreferencesStore } from "@/stores"

interface ConsentRecord {
  type: string
  label: string
  description: string
  granted: boolean
}

const GDPR_CONSENTS: ConsentRecord[] = [
  {
    type: "GDPRDataProcessing",
    label: "Data processing",
    description: "Allow processing of personal data for core platform functionality",
    granted: true,
  },
  {
    type: "GDPRAutomatedDecisionMaking",
    label: "Automated decision-making",
    description: "Allow AI agents to make decisions using your data",
    granted: false,
  },
  {
    type: "GDPRDataTransferOutsideEU",
    label: "Cross-border data transfer",
    description: "Allow data to be processed outside the EU/EEA",
    granted: false,
  },
  {
    type: "EUAIActHighRiskSystem",
    label: "High-risk AI system consent",
    description: "Acknowledge interaction with systems classified as high-risk under the EU AI Act",
    granted: false,
  },
]

export default function ProfilePage() {
  const { user } = useAuth()
  const { privacyConsent } = usePreferencesStore()
  const [consents, setConsents] = useState<ConsentRecord[]>(GDPR_CONSENTS)
  const [exportRequested, setExportRequested] = useState(false)

  function toggleConsent(type: string) {
    setConsents((prev) =>
      prev.map((c) => (c.type === type ? { ...c, granted: !c.granted } : c))
    )
  }

  function handleDataExport() {
    // In production this calls POST /api/v1/compliance/gdpr/data-export
    setExportRequested(true)
  }

  if (!user) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-cyan-500 border-t-transparent" />
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <h1 className="text-xl font-bold text-white">Profile</h1>

      {/* Account info */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Account</h2>
        <div className="space-y-3">
          <InfoRow label="Name" value={user.name || "Not set"} />
          <InfoRow label="Email" value={user.email} />
          <InfoRow label="User ID" value={user.id} mono />
          <InfoRow label="Tenant" value={user.tenantId || "Default"} mono />
        </div>
      </section>

      {/* Roles */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Roles</h2>
        {user.roles.length > 0 ? (
          <div className="flex flex-wrap gap-2">
            {user.roles.map((role) => (
              <RoleBadge key={role} role={role} />
            ))}
          </div>
        ) : (
          <p className="text-sm text-gray-500">No roles assigned</p>
        )}
      </section>

      {/* GDPR Consent */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-2 text-sm font-semibold text-white">GDPR &amp; AI Act Consent</h2>
        <p className="mb-4 text-xs text-gray-500">
          Manage your consent preferences. All changes are logged for compliance auditing.
        </p>
        <div className="space-y-3">
          {consents.map((consent) => (
            <ConsentRow
              key={consent.type}
              consent={consent}
              onToggle={() => toggleConsent(consent.type)}
            />
          ))}
        </div>
      </section>

      {/* Data & privacy summary */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Data Privacy</h2>
        <div className="space-y-3 text-sm">
          <div className="flex items-center justify-between">
            <span className="text-gray-400">Analytics consent</span>
            <StatusDot active={privacyConsent.analytics} />
          </div>
          <div className="flex items-center justify-between">
            <span className="text-gray-400">Telemetry consent</span>
            <StatusDot active={privacyConsent.telemetry} />
          </div>
          <div className="flex items-center justify-between">
            <span className="text-gray-400">Personalized content</span>
            <StatusDot active={privacyConsent.personalizedContent} />
          </div>
          <p className="text-xs text-gray-600">
            Manage these in{" "}
            <a href="/settings" className="text-cyan-500 hover:underline">
              Settings &gt; Data &amp; Privacy
            </a>
          </p>
        </div>
      </section>

      {/* Data export */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-2 text-sm font-semibold text-white">Data Export</h2>
        <p className="mb-4 text-xs text-gray-500">
          Request a copy of all personal data stored in the system (GDPR Article 20).
        </p>
        {exportRequested ? (
          <div className="rounded-md border border-cyan-800 bg-cyan-950/30 px-4 py-3 text-sm text-cyan-300">
            Export request submitted. You will receive a download link via email.
          </div>
        ) : (
          <button
            type="button"
            onClick={handleDataExport}
            className="rounded-md bg-cyan-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-cyan-500"
          >
            Request data export
          </button>
        )}
      </section>

      {/* Session info */}
      <section className="rounded-lg border border-white/10 bg-white/5 p-6">
        <h2 className="mb-4 text-sm font-semibold text-white">Session</h2>
        <div className="space-y-3">
          <InfoRow label="Status" value="Active" />
          <InfoRow
            label="Authenticated since"
            value={new Date().toLocaleString()}
          />
        </div>
      </section>
    </div>
  )
}

function InfoRow({
  label,
  value,
  mono,
}: {
  label: string
  value: string
  mono?: boolean
}) {
  return (
    <div className="flex items-center justify-between">
      <span className="text-sm text-gray-500">{label}</span>
      <span
        className={`text-sm text-gray-300 ${mono ? "font-mono text-xs" : ""}`}
      >
        {value}
      </span>
    </div>
  )
}

function RoleBadge({ role }: { role: string }) {
  const colors: Record<string, string> = {
    Admin: "border-red-700 bg-red-950/50 text-red-300",
    Analyst: "border-blue-700 bg-blue-950/50 text-blue-300",
    Viewer: "border-gray-700 bg-gray-950/50 text-gray-300",
  }
  const cls = colors[role] ?? "border-gray-700 bg-gray-950/50 text-gray-300"

  return (
    <span className={`rounded-full border px-3 py-1 text-xs font-medium ${cls}`}>
      {role}
    </span>
  )
}

function ConsentRow({
  consent,
  onToggle,
}: {
  consent: ConsentRecord
  onToggle: () => void
}) {
  const id = useId()

  return (
    <div className="flex items-center justify-between gap-4 rounded-md border border-white/5 bg-white/[0.02] px-4 py-3">
      <div>
        <span id={id} className="text-sm text-gray-300">
          {consent.label}
        </span>
        <p className="text-xs text-gray-600">{consent.description}</p>
      </div>
      <button
        type="button"
        role="switch"
        aria-checked={consent.granted ? "true" : "false"}
        aria-labelledby={id}
        onClick={onToggle}
        className={`relative h-6 w-11 shrink-0 rounded-full transition-colors focus-visible:ring-2 focus-visible:ring-cyan-500 ${
          consent.granted ? "bg-cyan-600" : "bg-white/20"
        }`}
      >
        <span
          className={`absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white transition-transform ${
            consent.granted ? "translate-x-5" : ""
          }`}
        />
      </button>
    </div>
  )
}

function StatusDot({ active }: { active: boolean }) {
  return (
    <span className="flex items-center gap-2 text-xs">
      <span
        className={`inline-block h-2 w-2 rounded-full ${
          active ? "bg-green-500" : "bg-gray-600"
        }`}
      />
      {active ? "Granted" : "Not granted"}
    </span>
  )
}
