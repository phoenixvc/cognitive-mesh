"use client"

import { useEffect, useRef } from "react"
import { useAuth } from "@/contexts/AuthContext"
import { useToast } from "@/components/Toast"
import { servicesApi, agenticApi } from "@/lib/api/client"
import { configureInterceptors, errorInterceptor } from "@/lib/api/interceptors"

/**
 * Registers API error interceptors once providers are available.
 * Render this inside both AuthProvider and ToastProvider.
 */
export function ApiBootstrap() {
  const { logout } = useAuth()
  const { toast } = useToast()
  const registeredRef = useRef(false)

  useEffect(() => {
    configureInterceptors(toast, logout)
    if (!registeredRef.current) {
      servicesApi.use(errorInterceptor)
      agenticApi.use(errorInterceptor)
      registeredRef.current = true
    }
  }, [toast, logout])

  return null
}
