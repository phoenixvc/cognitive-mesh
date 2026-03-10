"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { ChevronRight } from "lucide-react"
import { navItems } from "./navItems"

const labelMap: Record<string, string> = Object.fromEntries(
  navItems.map((item) => [item.href.replace("/", ""), item.label])
)

export function Breadcrumbs() {
  const pathname = usePathname()
  const segments = pathname.split("/").filter(Boolean)
  if (segments.length === 0) return null

  return (
    <nav aria-label="Breadcrumb" className="flex items-center gap-1 text-xs text-gray-400">
      <Link href="/dashboard" className="hover:text-white transition-colors">
        Home
      </Link>
      {segments.map((segment, i) => {
        const href = "/" + segments.slice(0, i + 1).join("/")
        const isLast = i === segments.length - 1
        const label = labelMap[segment] ?? segment.replace(/-/g, " ")
        return (
          <span key={href} className="flex items-center gap-1">
            <ChevronRight size={12} className="text-gray-600" />
            {isLast ? (
              <span className="text-gray-200 capitalize">{label}</span>
            ) : (
              <Link href={href} className="hover:text-white transition-colors capitalize">
                {label}
              </Link>
            )}
          </span>
        )
      })}
    </nav>
  )
}
