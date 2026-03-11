/**
 * Preferences store — user settings persisted to localStorage.
 *
 * Uses Zustand's persist middleware to survive page reloads.
 * Preferences are local-only; no backend sync needed.
 */
import { create } from "zustand"
import { persist } from "zustand/middleware"

type Theme = "dark" | "light" | "system"
type FontSize = "small" | "medium" | "large"

interface PreferencesState {
  theme: Theme
  sidebarCollapsed: boolean
  reducedMotion: boolean
  highContrast: boolean
  fontSize: FontSize
  soundEnabled: boolean
  notificationsEnabled: boolean
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
  resetDefaults: () => void
}

const defaults: PreferencesState = {
  theme: "dark",
  sidebarCollapsed: false,
  reducedMotion: false,
  highContrast: false,
  fontSize: "medium",
  soundEnabled: true,
  notificationsEnabled: true,
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
      resetDefaults: () => set(defaults),
    }),
    {
      name: "cm-preferences",
    }
  )
)
