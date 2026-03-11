"use client"

import { Bell } from "lucide-react"
import { useNotificationStore } from "@/stores"
import { Breadcrumbs } from "./Breadcrumbs"
import { MobileMenu } from "./MobileMenu"
import { ConnectionIndicator } from "./ConnectionIndicator"

export function TopBar() {
  const unreadCount = useNotificationStore((s) => s.unreadCount)

  return (
    <header className="sticky top-0 z-30 flex h-14 items-center justify-between border-b border-white/10 bg-black/60 px-4 backdrop-blur-xl">
      <div className="flex items-center gap-3">
        <MobileMenu />
        <Breadcrumbs />
      </div>

      <div className="flex items-center gap-3">
        <ConnectionIndicator />

        {/* TODO: implement notification panel onClick handler */}
        <button
          className="relative rounded p-2 text-gray-400 hover:bg-white/10 hover:text-white"
          aria-label={unreadCount > 0 ? `Notifications (${unreadCount} unread)` : "Notifications"}
        >
          <Bell size={18} />
          {unreadCount > 0 && (
            <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </button>
      </div>
    </header>
  )
}
