import { NextRequest, NextResponse } from "next/server"

const PUBLIC_PATHS = ["/login", "/forbidden"]

function isJwtExpired(token: string): boolean {
  try {
    const parts = token.split(".")
    if (parts.length !== 3) return true
    const payload = JSON.parse(atob(parts[1].replace(/-/g, "+").replace(/_/g, "/")))
    if (typeof payload.exp !== "number") return true
    return Date.now() >= payload.exp * 1000
  } catch {
    return true
  }
}

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl

  // Allow public paths and Next.js internals
  if (
    PUBLIC_PATHS.some((p) => pathname.startsWith(p)) ||
    pathname.startsWith("/_next") ||
    pathname.startsWith("/api")
  ) {
    return NextResponse.next()
  }

  // Check for auth token in cookies (set by client-side after login)
  const token = request.cookies.get("cm_access_token")?.value
  if (!token || isJwtExpired(token)) {
    const loginUrl = new URL("/login", request.url)
    const returnTo = pathname + request.nextUrl.search
    loginUrl.searchParams.set("returnTo", returnTo)
    return NextResponse.redirect(loginUrl)
  }

  return NextResponse.next()
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
}
