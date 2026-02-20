import type { Metadata } from "next"
import React from "react"
import { ThemeProvider } from "../../components/theme-provider"
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
        <script
          dangerouslySetInnerHTML={{
            __html: `
              // Suppress browser extension errors
              window.addEventListener('error', function(e) {
                if (e.message.includes('Could not establish connection') ||
                    e.message.includes('Receiving end does not exist') ||
                    e.message.includes('requestStorageAccessFor') ||
                    e.filename && e.filename.includes('content.js')) {
                  e.preventDefault();
                  return false;
                }
              });
              
              // Suppress unhandled promise rejections from extensions
              window.addEventListener('unhandledrejection', function(e) {
                if (e.reason && (
                    e.reason.message && (
                      e.reason.message.includes('Could not establish connection') ||
                      e.reason.message.includes('Receiving end does not exist')
                    ) ||
                    e.reason.toString().includes('content.js')
                  )) {
                  e.preventDefault();
                  return false;
                }
              });
            `
          }}
        />
        <ThemeProvider
          attribute="class"
          defaultTheme="dark"
          enableSystem={false}
          disableTransitionOnChange
        >
          {children}
        </ThemeProvider>
      </body>
    </html>
  )
}
