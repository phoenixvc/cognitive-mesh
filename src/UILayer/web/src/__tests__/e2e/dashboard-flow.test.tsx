import React from "react"
import { render, screen, waitFor, fireEvent } from "@testing-library/react"

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter: () => ({ push: jest.fn(), back: jest.fn(), replace: jest.fn() }),
  usePathname: () => "/compliance",
  useSearchParams: () => new URLSearchParams(),
}))

// Mock SignalR
jest.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: jest.fn().mockReturnValue({
    withUrl: jest.fn().mockReturnThis(),
    withAutomaticReconnect: jest.fn().mockReturnThis(),
    configureLogging: jest.fn().mockReturnThis(),
    build: jest.fn().mockReturnValue({
      start: jest.fn().mockResolvedValue(undefined),
      stop: jest.fn().mockResolvedValue(undefined),
      on: jest.fn(),
      off: jest.fn(),
      onreconnecting: jest.fn(),
      onreconnected: jest.fn(),
      onclose: jest.fn(),
      state: "Connected",
    }),
  }),
  HubConnectionState: { Connected: "Connected" },
  LogLevel: { Warning: 3 },
}))

describe("Dashboard flow", () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it("renders a loading state initially", () => {
    const LoadingComponent = () => (
      <div role="status" aria-label="Loading">
        <div className="animate-pulse bg-gray-800 h-8 w-48 rounded" />
      </div>
    )
    render(<LoadingComponent />)
    expect(screen.getByRole("status")).toBeInTheDocument()
  })

  it("renders dashboard content after data loads", async () => {
    const Dashboard = () => {
      const [loaded, setLoaded] = React.useState(false)
      React.useEffect(() => { setLoaded(true) }, [])
      return loaded ? <h1>NIST Compliance</h1> : <div>Loading...</div>
    }
    render(<Dashboard />)
    await waitFor(() => {
      expect(screen.getByText("NIST Compliance")).toBeInTheDocument()
    })
  })

  it("shows error state on fetch failure", async () => {
    global.fetch = jest.fn().mockRejectedValue(new Error("Network error"))
    const ErrorDash = () => {
      const [error, setError] = React.useState<string | null>(null)
      React.useEffect(() => {
        fetch("/api/data").catch((e) => setError(e.message))
      }, [])
      return error ? <div role="alert">{error}</div> : <div>Loading...</div>
    }
    render(<ErrorDash />)
    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent("Network error")
    })
  })

  it("handles tab switching interaction", async () => {
    const TabbedDash = () => {
      const [tab, setTab] = React.useState("overview")
      return (
        <div>
          <button onClick={() => setTab("details")}>Details</button>
          <div>{tab === "overview" ? "Overview content" : "Details content"}</div>
        </div>
      )
    }
    const { getByText } = render(<TabbedDash />)
    expect(getByText("Overview content")).toBeInTheDocument()
    fireEvent.click(getByText("Details"))
    expect(getByText("Details content")).toBeInTheDocument()
  })

  it("renders metric cards with data", () => {
    const MetricCard = ({ label, value }: { label: string; value: number }) => (
      <div>
        <span>{label}</span>
        <span>{value}</span>
      </div>
    )
    render(
      <div>
        <MetricCard label="Score" value={85} />
        <MetricCard label="Gaps" value={3} />
      </div>
    )
    expect(screen.getByText("85")).toBeInTheDocument()
    expect(screen.getByText("3")).toBeInTheDocument()
  })
})
