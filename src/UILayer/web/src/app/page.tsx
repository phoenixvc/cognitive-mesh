import { redirect } from "next/navigation"

/**
 * Root route — redirects to the authenticated dashboard.
 *
 * The (app) route group handles auth gating via ProtectedRoute in its layout.
 * The old "Bridge" drag-and-drop dashboard lived here; components are preserved
 * under src/components/ for potential reuse.
 */
export default function RootPage() {
  redirect("/dashboard")
}
