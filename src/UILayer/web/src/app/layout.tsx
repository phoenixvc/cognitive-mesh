import type { Metadata } from "next"
import React from "react"
import { ThemeProvider } from "../../components/theme-provider"
import { AuthProvider } from "@/contexts/AuthContext"
import { ErrorBoundary } from "@/components/ErrorBoundary/ErrorBoundary"
import { ToastProvider } from "@/components/Toast"
import { ApiBootstrap } from "@/components/ApiBootstrap"
import { ExtensionErrorSuppressor } from "@/components/ExtensionErrorSuppressor"
import ParticleField from "../components/ParticleField"
import "./globals.css"

export const metadata: Metadata = {
  title: "Cognitive Mesh - Enterprise AI Transformation Framework",
  description:
    "A revolutionary spaceship-grade AI dashboard for enterprise AI transformation featuring draggable modules, neural processing, and quantum analysis capabilities.",
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="font-sans antialiased dark">
        <ParticleField />
        <ExtensionErrorSuppressor />
        <ThemeProvider
          attribute="class"
          defaultTheme="dark"
          enableSystem={false}
          disableTransitionOnChange
        >
          <AuthProvider>
            <ToastProvider>
              <ErrorBoundary>
                <ApiBootstrap />
                {children}
              </ErrorBoundary>
            </ToastProvider>
          </AuthProvider>
        </ThemeProvider>
      </body>
    </html>
  )
}
