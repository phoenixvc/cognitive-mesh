"use client"

import { useEffect } from "react"
import { useAuth } from "@/contexts/AuthContext"
import { useToast } from "@/components/Toast"
import { servicesApi, agenticApi } from "@/lib/api/client"
import { configureInterceptors, errorInterceptor } from "@/lib/api/interceptors"

let interceptorsRegistered = false

/**
 * Registers API error interceptors once providers are available.
 * Render this inside both AuthProvider and ToastProvider.
 */
export function ApiBootstrap() {
  const { logout } = useAuth()
  const { toast } = useToast()

  useEffect(() => {
    configureInterceptors(toast, logout)
    if (!interceptorsRegistered) {
      servicesApi.use(errorInterceptor)
      agenticApi.use(errorInterceptor)
      interceptorsRegistered = true
    }
  }, [toast, logout])

  return null
}
