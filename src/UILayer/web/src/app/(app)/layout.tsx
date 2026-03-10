"use client"

import { Sidebar, TopBar } from "@/components/Navigation"
import { ProtectedRoute } from "@/components/ProtectedRoute"
import { usePreferencesStore } from "@/stores"

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const collapsed = usePreferencesStore((s) => s.sidebarCollapsed)

  return (
    <ProtectedRoute>
      <div className="flex min-h-screen">
        <Sidebar />
        <div
          className={`flex flex-1 flex-col transition-all duration-300 ${
            collapsed ? "md:ml-16" : "md:ml-56"
          }`}
        >
          <TopBar />
          <main className="flex-1 p-6">{children}</main>
        </div>
      </div>
    </ProtectedRoute>
  )
}
