/**
 * Preferences store — user settings persisted to localStorage.
 *
 * Uses Zustand's persist middleware to survive page reloads.
 * TODO: Sync to backend user preferences API when authenticated.
 */
import { create } from "zustand"
import { persist } from "zustand/middleware"
import type { SupportedLanguage } from "@/lib/i18n/i18nConfig"

type Theme = "dark" | "light" | "system"
type FontSize = "small" | "medium" | "large"

export interface PrivacyConsent {
  analytics: boolean
  telemetry: boolean
  personalizedContent: boolean
  thirdPartySharing: boolean
  updatedAt: number
}

export interface NotificationPreferences {
  channels: {
    email: boolean
    push: boolean
    sms: boolean
    inApp: boolean
  }
  categories: NotificationCategoryPreference[]
  quietHours: {
    enabled: boolean
    startTime: string
    endTime: string
    timezone: string
  }
}

export interface NotificationCategoryPreference {
  category: string
  enabled: boolean
  channels: { email: boolean; push: boolean; sms: boolean; inApp: boolean }
}

export interface GdprConsentRecord {
  type: string
  granted: boolean
  updatedAt: number
}

interface PreferencesState {
  theme: Theme
  sidebarCollapsed: boolean
  reducedMotion: boolean
  highContrast: boolean
  fontSize: FontSize
  soundEnabled: boolean
  notificationsEnabled: boolean
  language: SupportedLanguage
  privacyConsent: PrivacyConsent
  notificationPreferences: NotificationPreferences
  gdprConsents: GdprConsentRecord[]
}

interface PreferencesActions {
  setTheme: (theme: Theme) => void
  toggleSidebar: () => void
  setSidebarCollapsed: (collapsed: boolean) => void
  setReducedMotion: (enabled: boolean) => void
  setHighContrast: (enabled: boolean) => void
  setFontSize: (size: FontSize) => void
  setSoundEnabled: (enabled: boolean) => void
  setNotificationsEnabled: (enabled: boolean) => void
  setLanguage: (language: SupportedLanguage) => void
  setPrivacyConsent: (consent: Partial<Omit<PrivacyConsent, "updatedAt">>) => void
  setNotificationPreferences: (prefs: Partial<NotificationPreferences>) => void
  setNotificationChannel: (channel: keyof NotificationPreferences["channels"], enabled: boolean) => void
  setQuietHours: (quietHours: Partial<NotificationPreferences["quietHours"]>) => void
  setGdprConsent: (type: string, granted: boolean) => void
  resetDefaults: () => void
}

const defaultPrivacyConsent: PrivacyConsent = {
  analytics: false,
  telemetry: false,
  personalizedContent: false,
  thirdPartySharing: false,
  updatedAt: 0,
}

const defaultNotificationPreferences: NotificationPreferences = {
  channels: { email: true, push: true, sms: false, inApp: true },
  categories: [],
  quietHours: {
    enabled: false,
    startTime: "22:00",
    endTime: "08:00",
    timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
  },
}

const defaults: PreferencesState = {
  theme: "dark",
  sidebarCollapsed: false,
  reducedMotion: false,
  highContrast: false,
  fontSize: "medium",
  soundEnabled: true,
  notificationsEnabled: true,
  language: "en-US",
  privacyConsent: defaultPrivacyConsent,
  notificationPreferences: defaultNotificationPreferences,
  gdprConsents: [],
}

export const usePreferencesStore = create<PreferencesState & PreferencesActions>()(
  persist(
    (set) => ({
      ...defaults,

      setTheme: (theme) => set({ theme }),
      toggleSidebar: () =>
        set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),
      setSidebarCollapsed: (collapsed) => set({ sidebarCollapsed: collapsed }),
      setReducedMotion: (enabled) => set({ reducedMotion: enabled }),
      setHighContrast: (enabled) => set({ highContrast: enabled }),
      setFontSize: (size) => set({ fontSize: size }),
      setSoundEnabled: (enabled) => set({ soundEnabled: enabled }),
      setNotificationsEnabled: (enabled) =>
        set({ notificationsEnabled: enabled }),
      setLanguage: (language) => set({ language }),
      setPrivacyConsent: (consent) =>
        set((state) => ({
          privacyConsent: {
            ...state.privacyConsent,
            ...consent,
            updatedAt: Date.now(),
          },
        })),
      setNotificationPreferences: (prefs) =>
        set((state) => ({
          notificationPreferences: {
            ...state.notificationPreferences,
            ...prefs,
          },
        })),
      setNotificationChannel: (channel, enabled) =>
        set((state) => ({
          notificationPreferences: {
            ...state.notificationPreferences,
            channels: {
              ...state.notificationPreferences.channels,
              [channel]: enabled,
            },
          },
        })),
      setQuietHours: (quietHours) =>
        set((state) => ({
          notificationPreferences: {
            ...state.notificationPreferences,
            quietHours: {
              ...state.notificationPreferences.quietHours,
              ...quietHours,
            },
          },
        })),
      setGdprConsent: (type, granted) =>
        set((state) => {
          const filtered = state.gdprConsents.filter((c) => c.type !== type)
          return {
            gdprConsents: [
              ...filtered,
              { type, granted, updatedAt: Date.now() },
            ],
          }
        }),
      resetDefaults: () => set(defaults),
    }),
    {
      name: "cm-preferences",
    }
  )
)
