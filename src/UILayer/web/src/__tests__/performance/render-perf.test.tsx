import React from "react"
import { render, act } from "@testing-library/react"
import { usePreferencesStore } from "@/stores/usePreferencesStore"

describe("Render performance", () => {
  beforeEach(() => {
    act(() => { usePreferencesStore.getState().resetDefaults() })
  })

  it("selector only re-renders on relevant state change", () => {
    let renderCount = 0

    const ThemeConsumer = () => {
      const theme = usePreferencesStore((s) => s.theme)
      renderCount++
      return <div>{theme}</div>
    }

    render(<ThemeConsumer />)
    expect(renderCount).toBe(1)

    // Changing unrelated state should NOT cause re-render
    act(() => { usePreferencesStore.getState().setFontSize("large") })
    expect(renderCount).toBe(1)

    // Changing theme SHOULD cause re-render
    act(() => { usePreferencesStore.getState().setTheme("light") })
    expect(renderCount).toBe(2)
  })

  it("sidebar selector is independent of theme changes", () => {
    let renderCount = 0

    const SidebarConsumer = () => {
      const collapsed = usePreferencesStore((s) => s.sidebarCollapsed)
      renderCount++
      return <div>{String(collapsed)}</div>
    }

    render(<SidebarConsumer />)
    expect(renderCount).toBe(1)

    act(() => { usePreferencesStore.getState().setTheme("light") })
    expect(renderCount).toBe(1)

    act(() => { usePreferencesStore.getState().toggleSidebar() })
    expect(renderCount).toBe(2)
  })

  it("multiple selectors render independently", () => {
    let themeRenders = 0
    let fontRenders = 0

    const ThemeComp = () => {
      usePreferencesStore((s) => s.theme)
      themeRenders++
      return null
    }

    const FontComp = () => {
      usePreferencesStore((s) => s.fontSize)
      fontRenders++
      return null
    }

    render(
      <>
        <ThemeComp />
        <FontComp />
      </>
    )
    expect(themeRenders).toBe(1)
    expect(fontRenders).toBe(1)

    act(() => { usePreferencesStore.getState().setTheme("light") })
    expect(themeRenders).toBe(2)
    expect(fontRenders).toBe(1) // font didn't change
  })

  it("resetDefaults triggers single re-render per selector", () => {
    let renderCount = 0

    const Consumer = () => {
      usePreferencesStore((s) => s.theme)
      renderCount++
      return null
    }

    render(<Consumer />)
    act(() => { usePreferencesStore.getState().setTheme("light") })
    const countBefore = renderCount
    act(() => { usePreferencesStore.getState().resetDefaults() })
    // Should trigger exactly one more render (theme changed back to "dark")
    expect(renderCount).toBe(countBefore + 1)
  })
})
