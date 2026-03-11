"use client"

import Link from "next/link"

export default function ForbiddenPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-gray-950 via-gray-900 to-gray-950 px-4">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-red-500">403</h1>
        <p className="mt-4 text-xl text-gray-300">Access Denied</p>
        <p className="mt-2 text-sm text-gray-500">
          You don&apos;t have permission to access this resource.
        </p>
        <Link
          href="/"
          className="mt-6 inline-block rounded-lg bg-cyan-600 px-6 py-2.5 font-medium text-white transition hover:bg-cyan-500"
        >
          Go to Dashboard
        </Link>
      </div>
    </div>
  )
}
