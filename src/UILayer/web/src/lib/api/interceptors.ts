/**
 * API Error Interceptors
 *
 * Middleware for openapi-fetch clients that handles:
 * - 401 → redirect to login
 * - 403 → redirect to forbidden
 * - 5xx → toast error notification
 * - Network errors → toast with retry hint
 */

import type { Middleware } from "openapi-fetch"

type ToastFn = (message: string, type?: "error" | "warning") => void
type LogoutFn = () => void

let toastFn: ToastFn | null = null
let logoutFn: LogoutFn | null = null

/**
 * Wire up the interceptor to the toast and logout functions.
 * Call this once from a client component after providers are mounted.
 */
export function configureInterceptors(toast: ToastFn, logout: LogoutFn) {
  toastFn = toast
  logoutFn = logout
}

export const errorInterceptor: Middleware = {
  async onResponse({ response }) {
    if (response.ok) return response

    switch (response.status) {
      case 401:
        logoutFn?.()
        if (typeof window !== "undefined" && !window.location.pathname.startsWith("/login")) {
          window.location.href = `/login?returnTo=${encodeURIComponent(window.location.pathname)}`
        }
        break

      case 403:
        toastFn?.("You don't have permission to perform this action.", "error")
        break

      case 429:
        toastFn?.("Too many requests. Please wait a moment and try again.", "warning")
        break

      default:
        if (response.status >= 500) {
          toastFn?.("A server error occurred. Please try again later.", "error")
        } else if (response.status >= 400) {
          try {
            const body = await response.clone().json()
            toastFn?.(body.message ?? `Request failed (${response.status})`, "error")
          } catch {
            toastFn?.(`Request failed (${response.status})`, "error")
          }
        }
    }

    return response
  },
}
