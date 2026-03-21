import React from "react";
import { render, screen } from "@testing-library/react";
import { ConnectionIndicator } from "./ConnectionIndicator";

// Mock useSignalR hook
const mockUseSignalR = jest.fn();
jest.mock("@/hooks/useSignalR", () => ({
  useSignalR: (...args: unknown[]) => mockUseSignalR(...args),
}));

// Mock lucide-react icons to simple elements
jest.mock("lucide-react", () => ({
  Wifi: (props: React.SVGAttributes<SVGElement>) => (
    <svg data-testid="wifi-icon" {...props} />
  ),
  WifiOff: (props: React.SVGAttributes<SVGElement>) => (
    <svg data-testid="wifi-off-icon" {...props} />
  ),
}));

beforeEach(() => {
  mockUseSignalR.mockReset();
});

describe("ConnectionIndicator", () => {
  it("should render a status dot element", () => {
    mockUseSignalR.mockReturnValue({ status: "connected" });
    const { container } = render(<ConnectionIndicator />);
    const dot = container.querySelector("span.rounded-full");
    expect(dot).toBeInTheDocument();
  });

  it("should show 'Connected' in the title when connected", () => {
    mockUseSignalR.mockReturnValue({ status: "connected" });
    const { container } = render(<ConnectionIndicator />);
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper.getAttribute("title")).toBe("Connected");
  });

  it("should show 'Disconnected' in the title when disconnected", () => {
    mockUseSignalR.mockReturnValue({ status: "disconnected" });
    const { container } = render(<ConnectionIndicator />);
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper.getAttribute("title")).toBe("Disconnected");
  });

  it("should show 'Connecting...' in the title when connecting", () => {
    mockUseSignalR.mockReturnValue({ status: "connecting" });
    const { container } = render(<ConnectionIndicator />);
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper.getAttribute("title")).toBe("Connecting...");
  });

  it("should show 'Reconnecting...' in the title when reconnecting", () => {
    mockUseSignalR.mockReturnValue({ status: "reconnecting" });
    const { container } = render(<ConnectionIndicator />);
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper.getAttribute("title")).toBe("Reconnecting...");
  });

  it("should render Wifi icon when connected", () => {
    mockUseSignalR.mockReturnValue({ status: "connected" });
    render(<ConnectionIndicator />);
    expect(screen.getByTestId("wifi-icon")).toBeInTheDocument();
  });

  it("should render WifiOff icon when disconnected", () => {
    mockUseSignalR.mockReturnValue({ status: "disconnected" });
    render(<ConnectionIndicator />);
    expect(screen.getByTestId("wifi-off-icon")).toBeInTheDocument();
  });

  it("should apply green color class when connected", () => {
    mockUseSignalR.mockReturnValue({ status: "connected" });
    const { container } = render(<ConnectionIndicator />);
    const dot = container.querySelector("span.rounded-full");
    expect(dot?.className).toContain("bg-green-500");
  });

  it("should apply red color class when disconnected", () => {
    mockUseSignalR.mockReturnValue({ status: "disconnected" });
    const { container } = render(<ConnectionIndicator />);
    const dot = container.querySelector("span.rounded-full");
    expect(dot?.className).toContain("bg-red-500");
  });
});
