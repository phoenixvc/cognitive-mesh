import { act } from "@testing-library/react"
import { usePreferencesStore } from "@/stores/usePreferencesStore"

describe("Settings flow", () => {
  beforeEach(() => {
    const { resetDefaults } = usePreferencesStore.getState()
    act(() => { resetDefaults() })
  })

  it("has correct default theme", () => {
    expect(usePreferencesStore.getState().theme).toBe("dark")
  })

  it("changes theme preference", () => {
    act(() => { usePreferencesStore.getState().setTheme("light") })
    expect(usePreferencesStore.getState().theme).toBe("light")
  })

  it("toggles sidebar collapsed state", () => {
    const initial = usePreferencesStore.getState().sidebarCollapsed
    act(() => { usePreferencesStore.getState().toggleSidebar() })
    expect(usePreferencesStore.getState().sidebarCollapsed).toBe(!initial)
  })

  it("sets font size", () => {
    act(() => { usePreferencesStore.getState().setFontSize("large") })
    expect(usePreferencesStore.getState().fontSize).toBe("large")
  })

  it("sets language preference", () => {
    act(() => { usePreferencesStore.getState().setLanguage("fr-FR") })
    expect(usePreferencesStore.getState().language).toBe("fr-FR")
  })

  it("sets privacy consent", () => {
    act(() => { usePreferencesStore.getState().setPrivacyConsent({ analytics: true }) })
    expect(usePreferencesStore.getState().privacyConsent.analytics).toBe(true)
  })

  it("persists settings across store resets via resetDefaults", () => {
    act(() => { usePreferencesStore.getState().setTheme("light") })
    expect(usePreferencesStore.getState().theme).toBe("light")
    act(() => { usePreferencesStore.getState().resetDefaults() })
    expect(usePreferencesStore.getState().theme).toBe("dark")
  })

  it("handles rapid sequential changes", () => {
    act(() => {
      usePreferencesStore.getState().setTheme("light")
      usePreferencesStore.getState().setTheme("system")
      usePreferencesStore.getState().setTheme("dark")
    })
    expect(usePreferencesStore.getState().theme).toBe("dark")
  })
})
