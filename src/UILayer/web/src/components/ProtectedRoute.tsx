"use client"

import { useAuth } from "@/contexts/AuthContext"
import { usePathname, useRouter } from "next/navigation"
import { useEffect } from "react"

interface ProtectedRouteProps {
  children: React.ReactNode
  requiredRoles?: string[]
}

export function ProtectedRoute({ children, requiredRoles }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, user } = useAuth()
  const router = useRouter()
  const pathname = usePathname()

  useEffect(() => {
    if (isLoading) return
    if (!isAuthenticated) {
      router.replace(`/login?returnTo=${encodeURIComponent(pathname)}`)
      return
    }
    if (requiredRoles?.length && user) {
      const hasRole = requiredRoles.some((r) => user.roles.includes(r))
      if (!hasRole) {
        router.replace("/forbidden")
      }
    }
  }, [isAuthenticated, isLoading, user, requiredRoles, router, pathname])

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-cyan-500 border-t-transparent" />
      </div>
    )
  }

  if (!isAuthenticated) return null

  if (requiredRoles?.length && user && !requiredRoles.some((r) => user.roles.includes(r))) {
    return null
  }

  return <>{children}</>
}
