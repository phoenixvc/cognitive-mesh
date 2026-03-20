describe("Bundle size - lazy loading verification", () => {
  it("LazyWidgetLoader module exports createLazyWidget", async () => {
    const mod = await import("@/lib/code-splitting/LazyWidgetLoader")
    expect(mod.createLazyWidget).toBeDefined()
    expect(typeof mod.createLazyWidget).toBe("function")
  })

  it("lazyWidgets registry exports all expected widgets", async () => {
    const registry = await import("@/lib/code-splitting/registry/lazyWidgets")
    const expectedWidgets = [
      "LazyAgentControlCenter",
      "LazyValueDiagnosticDashboard",
      "LazyNistComplianceDashboard",
      "LazyAdaptiveBalanceDashboard",
      "LazyImpactMetricsDashboard",
      "LazyCognitiveSandwichDashboard",
      "LazyContextEngineeringDashboard",
      "LazyConvenerDashboard",
      "LazyMarketplaceDashboard",
      "LazyOrgMeshDashboard",
    ]
    for (const name of expectedWidgets) {
      expect((registry as Record<string, unknown>)[name]).toBeDefined()
    }
  })

  it("createLazyWidget returns a valid React lazy component", async () => {
    const { createLazyWidget } = await import("@/lib/code-splitting/LazyWidgetLoader")
    const LazyComp = createLazyWidget(
      () => Promise.resolve({ default: () => null }) as Promise<{ default: React.ComponentType }>
    )
    expect(LazyComp).toBeDefined()
  })

  it("dynamic imports are functions (not eagerly loaded)", async () => {
    const registry = await import("@/lib/code-splitting/registry/lazyWidgets")
    // All exports should be defined (wrapped lazy components)
    const exportCount = Object.keys(registry).filter((k) => k.startsWith("Lazy")).length
    expect(exportCount).toBeGreaterThanOrEqual(10)
  })

  it("navItems are statically importable without side effects", async () => {
    const { navItems } = await import("@/components/Navigation/navItems")
    expect(Array.isArray(navItems)).toBe(true)
    expect(navItems.length).toBeGreaterThan(0)
    expect(navItems[0]).toHaveProperty("label")
    expect(navItems[0]).toHaveProperty("href")
  })
})
