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
  { label: "Marketplace", href: "/marketplace", icon: "Store", section: "Governance" },
  { label: "Settings", href: "/settings", icon: "Settings", section: "System" },
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
  return groups
}
