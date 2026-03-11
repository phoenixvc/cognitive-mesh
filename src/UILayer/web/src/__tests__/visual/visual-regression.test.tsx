import React from "react"
import { render } from "@testing-library/react"
import {
  Skeleton,
  SkeletonCard,
  SkeletonTable,
  SkeletonMetric,
  SkeletonDashboard,
} from "@/components/Skeleton/Skeleton"
import { ConnectionIndicator } from "@/components/Navigation/ConnectionIndicator"

// Mock useSignalR for ConnectionIndicator
let mockStatus = "connected"
jest.mock("@/hooks/useSignalR", () => ({
  useSignalR: () => ({ status: mockStatus }),
}))

describe("Visual regression snapshots", () => {
  describe("Skeleton components", () => {
    it("Skeleton base matches snapshot", () => {
      const { container } = render(<Skeleton className="h-4 w-32" />)
      expect(container.firstChild).toMatchSnapshot()
    })

    it("SkeletonCard matches snapshot", () => {
      const { container } = render(<SkeletonCard />)
      expect(container.firstChild).toMatchSnapshot()
    })

    it("SkeletonTable matches snapshot with default rows", () => {
      const { container } = render(<SkeletonTable />)
      expect(container.firstChild).toMatchSnapshot()
    })

    it("SkeletonTable matches snapshot with 3 rows", () => {
      const { container } = render(<SkeletonTable rows={3} />)
      expect(container.firstChild).toMatchSnapshot()
    })

    it("SkeletonMetric matches snapshot", () => {
      const { container } = render(<SkeletonMetric />)
      expect(container.firstChild).toMatchSnapshot()
    })

    it("SkeletonDashboard matches snapshot", () => {
      const { container } = render(<SkeletonDashboard />)
      expect(container.firstChild).toMatchSnapshot()
    })
  })

  describe("ConnectionIndicator", () => {
    it("connected state matches snapshot", () => {
      mockStatus = "connected"
      const { container } = render(<ConnectionIndicator />)
      expect(container.firstChild).toMatchSnapshot()
    })

    it("connecting state matches snapshot", () => {
      mockStatus = "connecting"
      const { container } = render(<ConnectionIndicator />)
      expect(container.firstChild).toMatchSnapshot()
    })

    it("reconnecting state matches snapshot", () => {
      mockStatus = "reconnecting"
      const { container } = render(<ConnectionIndicator />)
      expect(container.firstChild).toMatchSnapshot()
    })

    it("disconnected state matches snapshot", () => {
      mockStatus = "disconnected"
      const { container } = render(<ConnectionIndicator />)
      expect(container.firstChild).toMatchSnapshot()
    })
  })
})
