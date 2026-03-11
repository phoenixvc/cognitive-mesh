"use client"

import { useEffect, useState, useRef, useCallback } from "react"
import { useRouter } from "next/navigation"
import { Search, ArrowRight } from "lucide-react"
import { navItems } from "@/components/Navigation/navItems"

interface PaletteItem {
  label: string
  href: string
  section: string
}

function fuzzyMatch(query: string, text: string): boolean {
  const q = query.toLowerCase()
  const t = text.toLowerCase()
  let qi = 0
  for (let ti = 0; ti < t.length && qi < q.length; ti++) {
    if (t[ti] === q[qi]) qi++
  }
  return qi === q.length
}

export function CommandPalette() {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState("")
  const [selectedIndex, setSelectedIndex] = useState(0)
  const inputRef = useRef<HTMLInputElement>(null)
  const router = useRouter()

  const items: PaletteItem[] = navItems.map((n) => ({
    label: n.label,
    href: n.href,
    section: n.section,
  }))

  const filtered = query
    ? items.filter((item) => fuzzyMatch(query, item.label))
    : items

  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if ((e.metaKey || e.ctrlKey) && e.key === "k") {
        e.preventDefault()
        setOpen((prev) => !prev)
      }
    }
    document.addEventListener("keydown", handleKeyDown)
    return () => document.removeEventListener("keydown", handleKeyDown)
  }, [])

  useEffect(() => {
    if (open) {
      setQuery("")
      setSelectedIndex(0)
      requestAnimationFrame(() => inputRef.current?.focus())
    }
  }, [open])

  useEffect(() => {
    setSelectedIndex(0)
  }, [query])

  const navigate = useCallback(
    (href: string) => {
      setOpen(false)
      router.push(href)
    },
    [router]
  )

  const handleKeyDown = (e: React.KeyboardEvent) => {
    switch (e.key) {
      case "ArrowDown":
        e.preventDefault()
        setSelectedIndex((i) => Math.min(i + 1, filtered.length - 1))
        break
      case "ArrowUp":
        e.preventDefault()
        setSelectedIndex((i) => Math.max(i - 1, 0))
        break
      case "Enter":
        e.preventDefault()
        if (filtered[selectedIndex]) {
          navigate(filtered[selectedIndex].href)
        }
        break
      case "Escape":
        e.preventDefault()
        setOpen(false)
        break
    }
  }

  if (!open) return null

  return (
    <div
      className="fixed inset-0 z-[100] flex items-start justify-center pt-[20vh]"
      onClick={() => setOpen(false)}
      role="dialog"
      aria-label="Command palette"
    >
      <div className="fixed inset-0 bg-black/60 backdrop-blur-sm" />

      <div
        className="relative z-10 w-full max-w-lg rounded-xl border border-white/10 bg-gray-900 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center gap-3 border-b border-white/10 px-4 py-3">
          <Search size={18} className="text-gray-500" />
          <input
            ref={inputRef}
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Search pages..."
            className="flex-1 bg-transparent text-sm text-white placeholder-gray-500 outline-none"
            aria-label="Search pages"
          />
          <kbd className="rounded border border-white/10 bg-white/5 px-1.5 py-0.5 text-[10px] text-gray-500">
            ESC
          </kbd>
        </div>

        <div className="max-h-72 overflow-y-auto py-2" role="listbox">
          {filtered.length === 0 && (
            <div className="px-4 py-6 text-center text-sm text-gray-500">
              No matching pages
            </div>
          )}
          {filtered.map((item, i) => (
            <button
              key={item.href}
              role="option"
              aria-selected={i === selectedIndex}
              onClick={() => navigate(item.href)}
              className={`flex w-full items-center justify-between px-4 py-2.5 text-sm transition-colors ${
                i === selectedIndex
                  ? "bg-white/10 text-white"
                  : "text-gray-400 hover:bg-white/5 hover:text-gray-200"
              }`}
            >
              <div className="flex items-center gap-3">
                <span>{item.label}</span>
                <span className="text-[10px] text-gray-600">{item.section}</span>
              </div>
              {i === selectedIndex && <ArrowRight size={14} className="text-gray-500" />}
            </button>
          ))}
        </div>
      </div>
    </div>
  )
}
