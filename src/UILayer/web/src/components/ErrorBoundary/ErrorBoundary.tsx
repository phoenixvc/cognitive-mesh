"use client"

import React, { Component, ErrorInfo } from "react"

interface Props {
  children: React.ReactNode
  fallback?: React.ReactNode
}

interface State {
  hasError: boolean
  error: Error | null
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false, error: null }
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("[ErrorBoundary]", error, info.componentStack)
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) return this.props.fallback

      return (
        <div className="flex min-h-[400px] items-center justify-center p-8">
          <div className="max-w-md rounded-xl border border-red-800 bg-red-950/30 p-6 text-center">
            <h2 className="text-lg font-semibold text-red-300">Something went wrong</h2>
            <p className="mt-2 text-sm text-gray-400">
              {this.state.error?.message ?? "An unexpected error occurred."}
            </p>
            <button
              onClick={() => this.setState({ hasError: false, error: null })}
              className="mt-4 rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500"
            >
              Try again
            </button>
          </div>
        </div>
      )
    }

    return this.props.children
  }
}
