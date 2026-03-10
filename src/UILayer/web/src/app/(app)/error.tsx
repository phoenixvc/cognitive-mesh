"use client"

export default function AppError({
  error,
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  return (
    <div className="flex min-h-[50vh] flex-col items-center justify-center gap-4 text-center">
      <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-6">
        <h2 className="mb-2 text-lg font-semibold text-red-400">
          Something went wrong
        </h2>
        <p className="mb-4 text-sm text-gray-400">
          {error.message || "An unexpected error occurred"}
        </p>
        <button
          onClick={reset}
          className="rounded-md bg-cyan-600 px-4 py-2 text-sm font-medium text-white hover:bg-cyan-500 transition-colors"
        >
          Try again
        </button>
      </div>
    </div>
  )
}
