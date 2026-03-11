export interface NavItem {
  label: string
  href: string
  icon: string
  section: string
  badge?: string
}

export const navItems: NavItem[] = [
  { label: "Dashboard", href: "/dashboard", icon: "LayoutDashboard", section: "Core" },
  { label: "Agents", href: "/agents", icon: "Bot", section: "Core" },
  { label: "Analytics", href: "/analytics", icon: "BarChart3", section: "Core" },
  { label: "Compliance", href: "/compliance", icon: "ShieldCheck", section: "Governance" },
  { label: "Balance", href: "/balance", icon: "Scale", section: "Governance" },
  { label: "Value", href: "/value", icon: "TrendingUp", section: "Governance" },
  { label: "Impact", href: "/impact", icon: "Activity", section: "Governance" },
  { label: "Sandwich", href: "/sandwich", icon: "Layers", section: "Governance" },
  { label: "Marketplace", href: "/marketplace", icon: "Store", section: "Governance" },
  { label: "Settings", href: "/settings", icon: "Settings", section: "System" },
  { label: "Profile", href: "/profile", icon: "User", section: "System" },
]

export const sectionOrder = ["Core", "Governance", "System"]

export function groupBySections(items: NavItem[]): Map<string, NavItem[]> {
  const groups = new Map<string, NavItem[]>()
  for (const section of sectionOrder) {
    const sectionItems = items.filter((i) => i.section === section)
    if (sectionItems.length > 0) {
      groups.set(section, sectionItems)
    }
  }

  // Collect items with unknown sections into a fallback group
  const knownSections = new Set(sectionOrder)
  const unknownItems = items.filter((i) => !knownSections.has(i.section))
  if (unknownItems.length > 0) {
    if (process.env.NODE_ENV === "development") {
      const sections = [...new Set(unknownItems.map((i) => i.section))]
      console.warn(`[navItems] Unknown sections: ${sections.join(", ")}`)
    }
    groups.set("Other", unknownItems)
  }

  return groups
}
