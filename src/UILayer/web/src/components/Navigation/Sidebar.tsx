"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { usePreferencesStore } from "@/stores"
import { navItems, groupBySections } from "./navItems"
import {
  LayoutDashboard,
  Bot,
  BarChart3,
  ShieldCheck,
  Store,
  Settings,
  User,
  ChevronLeft,
  ChevronRight,
  type LucideIcon,
} from "lucide-react"

const iconMap: Record<string, LucideIcon> = {
  LayoutDashboard,
  Bot,
  BarChart3,
  ShieldCheck,
  Store,
  Settings,
  User,
}

export function Sidebar() {
  const pathname = usePathname()
  const collapsed = usePreferencesStore((s) => s.sidebarCollapsed)
  const toggleSidebar = usePreferencesStore((s) => s.toggleSidebar)
  const sections = groupBySections(navItems)

  return (
    <aside
      className={`fixed left-0 top-0 z-40 h-screen border-r border-white/10 bg-black/80 backdrop-blur-xl transition-all duration-300 ${
        collapsed ? "w-16" : "w-56"
      }`}
    >
      {/* Logo */}
      <div className="flex h-14 items-center justify-between border-b border-white/10 px-3">
        {!collapsed && (
          <span className="text-sm font-bold tracking-wider text-cyan-400">
            COGNITIVE MESH
          </span>
        )}
        <button
          onClick={toggleSidebar}
          className="rounded p-1 text-gray-400 hover:bg-white/10 hover:text-white"
          aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
        >
          {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
        </button>
      </div>

      {/* Nav sections */}
      <nav className="flex flex-col gap-1 p-2" role="navigation">
        {Array.from(sections.entries()).map(([section, items]) => (
          <div key={section}>
            {!collapsed && (
              <span className="mb-1 block px-2 pt-3 text-[10px] font-semibold uppercase tracking-widest text-gray-500">
                {section}
              </span>
            )}
            {items.map((item) => {
              const Icon = iconMap[item.icon] ?? LayoutDashboard
              const isActive =
                pathname === item.href || pathname.startsWith(`${item.href}/`)
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 rounded-md px-2 py-2 text-sm transition-colors ${
                    isActive
                      ? "bg-cyan-500/15 text-cyan-400"
                      : "text-gray-400 hover:bg-white/5 hover:text-white"
                  } ${collapsed ? "justify-center" : ""}`}
                  title={collapsed ? item.label : undefined}
                >
                  <Icon size={18} />
                  {!collapsed && <span>{item.label}</span>}
                </Link>
              )
            })}
          </div>
        ))}
      </nav>
    </aside>
  )
}
