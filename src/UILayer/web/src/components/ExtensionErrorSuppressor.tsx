"use client"

import { useEffect } from "react"

/**
 * Suppresses benign browser extension errors that pollute the console.
 * Registers error and unhandledrejection listeners with proper cleanup.
 */
export function ExtensionErrorSuppressor() {
  useEffect(() => {
    function handleError(e: ErrorEvent) {
      if (
        (typeof e.message === "string" &&
          (e.message.includes("Could not establish connection") ||
            e.message.includes("Receiving end does not exist") ||
            e.message.includes("requestStorageAccessFor"))) ||
        (e.filename && e.filename.includes("content.js"))
      ) {
        e.preventDefault()
      }
    }

    function handleRejection(e: PromiseRejectionEvent) {
      if (
        e.reason &&
        ((typeof e.reason.message === "string" &&
          (e.reason.message.includes("Could not establish connection") ||
            e.reason.message.includes("Receiving end does not exist"))) ||
          e.reason.toString().includes("content.js"))
      ) {
        e.preventDefault()
      }
    }

    window.addEventListener("error", handleError)
    window.addEventListener("unhandledrejection", handleRejection)

    return () => {
      window.removeEventListener("error", handleError)
      window.removeEventListener("unhandledrejection", handleRejection)
    }
  }, [])

  return null
}
