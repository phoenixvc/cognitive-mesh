import React from "react";
import { render, screen } from "@testing-library/react";
import {
  Skeleton,
  SkeletonCard,
  SkeletonTable,
  SkeletonMetric,
  SkeletonDashboard,
} from "./Skeleton";

describe("Skeleton", () => {
  it("should render with animate-pulse class", () => {
    const { container } = render(<Skeleton />);
    const el = container.firstChild as HTMLElement;
    expect(el.className).toContain("animate-pulse");
  });

  it("should render with rounded-md and bg-white/5 classes", () => {
    const { container } = render(<Skeleton />);
    const el = container.firstChild as HTMLElement;
    expect(el.className).toContain("rounded-md");
    expect(el.className).toContain("bg-white/5");
  });

  it("should apply custom className", () => {
    const { container } = render(<Skeleton className="h-10 w-full" />);
    const el = container.firstChild as HTMLElement;
    expect(el.className).toContain("h-10");
    expect(el.className).toContain("w-full");
  });

  it("should have aria-hidden=true for accessibility", () => {
    const { container } = render(<Skeleton />);
    const el = container.firstChild as HTMLElement;
    expect(el.getAttribute("aria-hidden")).toBe("true");
  });

  it("should render as a div element", () => {
    const { container } = render(<Skeleton />);
    expect(container.firstChild?.nodeName).toBe("DIV");
  });
});

describe("SkeletonCard", () => {
  it("should render multiple skeleton elements", () => {
    const { container } = render(<SkeletonCard />);
    const skeletons = container.querySelectorAll("[aria-hidden='true']");
    expect(skeletons.length).toBeGreaterThanOrEqual(3);
  });
});

describe("SkeletonTable", () => {
  it("should render a header row plus default 5 data rows", () => {
    const { container } = render(<SkeletonTable />);
    const skeletons = container.querySelectorAll("[aria-hidden='true']");
    // 1 header + 5 data rows = 6
    expect(skeletons.length).toBe(6);
  });

  it("should render custom number of rows", () => {
    const { container } = render(<SkeletonTable rows={3} />);
    const skeletons = container.querySelectorAll("[aria-hidden='true']");
    // 1 header + 3 data rows = 4
    expect(skeletons.length).toBe(4);
  });
});

describe("SkeletonMetric", () => {
  it("should render skeleton elements for label and value", () => {
    const { container } = render(<SkeletonMetric />);
    const skeletons = container.querySelectorAll("[aria-hidden='true']");
    expect(skeletons.length).toBe(2);
  });
});

describe("SkeletonDashboard", () => {
  it("should render multiple skeleton components", () => {
    const { container } = render(<SkeletonDashboard />);
    const skeletons = container.querySelectorAll("[aria-hidden='true']");
    // 4 metrics (2 each) + 2 cards (4 each) + 1 table (6) = 8 + 8 + 6 = 22
    expect(skeletons.length).toBeGreaterThanOrEqual(10);
  });
});
