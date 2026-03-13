"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { usePreferencesStore } from "@/stores"
import { useAuth } from "@/contexts/AuthContext"
import { navItems, groupBySections } from "./navItems"
import {
  LayoutDashboard,
  Bot,
  BarChart3,
  Braces,
  ShieldCheck,
  Store,
  Settings,
  User,
  Users,
  Network,
  Scale,
  TrendingUp,
  Activity,
  Layers,
  ChevronLeft,
  ChevronRight,
  type LucideIcon,
} from "lucide-react"

const iconMap: Record<string, LucideIcon> = {
  LayoutDashboard,
  Bot,
  BarChart3,
  Braces,
  ShieldCheck,
  Store,
  Settings,
  User,
  Users,
  Network,
  Scale,
  TrendingUp,
  Activity,
  Layers,
}

export function Sidebar() {
  const pathname = usePathname()
  const collapsed = usePreferencesStore((s) => s.sidebarCollapsed)
  const toggleSidebar = usePreferencesStore((s) => s.toggleSidebar)
  const { user } = useAuth()
  const sections = groupBySections(navItems)
  const primaryRole = user?.roles?.[0] ?? null

  return (
    <aside
      className={`fixed left-0 top-0 z-40 flex h-screen flex-col border-r border-white/10 bg-black/80 backdrop-blur-xl transition-all duration-300 ${
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
      <nav className="flex flex-1 flex-col gap-1 overflow-y-auto p-2" role="navigation">
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

      {/* Role indicator */}
      {user && (
        <div className="border-t border-white/10 px-3 py-3">
          {collapsed ? (
            <div
              className="flex items-center justify-center"
              title={primaryRole ? `Role: ${primaryRole}` : user.name}
            >
              <div className="flex h-7 w-7 items-center justify-center rounded-full bg-cyan-500/20 text-[10px] font-bold text-cyan-400">
                {user.name.charAt(0).toUpperCase()}
              </div>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-cyan-500/20 text-[10px] font-bold text-cyan-400">
                {user.name.charAt(0).toUpperCase()}
              </div>
              <div className="min-w-0">
                <p className="truncate text-xs font-medium text-gray-300">{user.name}</p>
                {primaryRole && (
                  <p className="truncate text-[10px] text-gray-500">{primaryRole}</p>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </aside>
  )
}
