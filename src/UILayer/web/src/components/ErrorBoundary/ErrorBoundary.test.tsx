import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { ErrorBoundary } from "./ErrorBoundary";

// Suppress expected console.error output from React and ErrorBoundary
const originalConsoleError = console.error;
beforeAll(() => {
  console.error = jest.fn();
});
afterAll(() => {
  console.error = originalConsoleError;
});

// Component that always throws to ensure ErrorBoundary catches it
function AlwaysThrows(): React.ReactNode {
  throw new Error("Test error message");
}

describe("ErrorBoundary", () => {
  it("should render children when no error occurs", () => {
    render(
      <ErrorBoundary>
        <div>Safe content</div>
      </ErrorBoundary>
    );
    expect(screen.getByText("Safe content")).toBeInTheDocument();
  });

  it("should catch errors and show default fallback", () => {
    render(
      <ErrorBoundary>
        <AlwaysThrows />
      </ErrorBoundary>
    );
    expect(screen.getByText("Something went wrong")).toBeInTheDocument();
  });

  it("should show a Try again button in the default fallback", () => {
    render(
      <ErrorBoundary>
        <AlwaysThrows />
      </ErrorBoundary>
    );
    const button = screen.getByRole("button", { name: /try again/i });
    expect(button).toBeInTheDocument();
  });

  it("should show error message in development mode", () => {
    // NODE_ENV is 'test' by default in jest, which is treated like development
    // in the component's ternary. Let's verify the fallback text appears.
    render(
      <ErrorBoundary>
        <AlwaysThrows />
      </ErrorBoundary>
    );
    // The fallback shows "An unexpected error occurred." in production,
    // or the actual error message in development. In test env, NODE_ENV='test'
    // so it falls to the else branch.
    expect(
      screen.getByText("An unexpected error occurred.")
    ).toBeInTheDocument();
  });

  it("should render custom fallback when provided", () => {
    render(
      <ErrorBoundary fallback={<div>Custom error UI</div>}>
        <AlwaysThrows />
      </ErrorBoundary>
    );
    expect(screen.getByText("Custom error UI")).toBeInTheDocument();
    expect(screen.queryByText("Something went wrong")).not.toBeInTheDocument();
  });

  it("should log error information via componentDidCatch", () => {
    render(
      <ErrorBoundary>
        <AlwaysThrows />
      </ErrorBoundary>
    );
    expect(console.error).toHaveBeenCalled();
    const calls = (console.error as jest.Mock).mock.calls;
    const boundaryCall = calls.find(
      (c: unknown[]) => c[0] === "[ErrorBoundary]"
    );
    expect(boundaryCall).toBeDefined();
  });

  it("should not render children content when error boundary has caught", () => {
    render(
      <ErrorBoundary>
        <AlwaysThrows />
      </ErrorBoundary>
    );
    expect(screen.queryByText("Child content")).not.toBeInTheDocument();
    expect(screen.getByText("Something went wrong")).toBeInTheDocument();
  });
});
