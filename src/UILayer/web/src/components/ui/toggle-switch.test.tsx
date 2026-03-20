import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { ToggleRow, ToggleButton } from "./toggle-switch";

describe("ToggleButton", () => {
  it("should render with role switch and aria-checked false when unchecked", () => {
    render(
      <ToggleButton checked={false} onChange={jest.fn()} label="Test toggle" />
    );
    const btn = screen.getByRole("switch", { name: "Test toggle" });
    expect(btn).toBeInTheDocument();
    expect(btn).toHaveAttribute("aria-checked", "false");
  });

  it("should render with aria-checked true when checked", () => {
    render(
      <ToggleButton checked={true} onChange={jest.fn()} label="Test toggle" />
    );
    const btn = screen.getByRole("switch", { name: "Test toggle" });
    expect(btn).toHaveAttribute("aria-checked", "true");
  });

  it("should call onChange with toggled value when clicked", () => {
    const onChange = jest.fn();
    render(
      <ToggleButton checked={false} onChange={onChange} label="Test toggle" />
    );
    fireEvent.click(screen.getByRole("switch"));
    expect(onChange).toHaveBeenCalledWith(true);
  });

  it("should call onChange with false when checked is true and clicked", () => {
    const onChange = jest.fn();
    render(
      <ToggleButton checked={true} onChange={onChange} label="Test toggle" />
    );
    fireEvent.click(screen.getByRole("switch"));
    expect(onChange).toHaveBeenCalledWith(false);
  });

  it("should be disabled when disabled prop is true", () => {
    render(
      <ToggleButton
        checked={false}
        onChange={jest.fn()}
        disabled={true}
        label="Test toggle"
      />
    );
    expect(screen.getByRole("switch")).toBeDisabled();
  });

  it("should not call onChange when disabled and clicked", () => {
    const onChange = jest.fn();
    render(
      <ToggleButton
        checked={false}
        onChange={onChange}
        disabled={true}
        label="Test toggle"
      />
    );
    fireEvent.click(screen.getByRole("switch"));
    expect(onChange).not.toHaveBeenCalled();
  });
});

describe("ToggleRow", () => {
  it("should render label text", () => {
    render(
      <ToggleRow label="Dark mode" checked={false} onChange={jest.fn()} />
    );
    expect(screen.getByText("Dark mode")).toBeInTheDocument();
  });

  it("should render description when provided", () => {
    render(
      <ToggleRow
        label="Dark mode"
        description="Enable dark theme"
        checked={false}
        onChange={jest.fn()}
      />
    );
    expect(screen.getByText("Enable dark theme")).toBeInTheDocument();
  });

  it("should not render description when not provided", () => {
    const { container } = render(
      <ToggleRow label="Dark mode" checked={false} onChange={jest.fn()} />
    );
    expect(container.querySelector("p")).toBeNull();
  });

  it("should apply opacity class when disabled", () => {
    const { container } = render(
      <ToggleRow
        label="Dark mode"
        checked={false}
        onChange={jest.fn()}
        disabled={true}
      />
    );
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper.className).toContain("opacity-50");
  });

  it("should render the toggle button inside the row", () => {
    render(
      <ToggleRow label="Dark mode" checked={true} onChange={jest.fn()} />
    );
    const toggle = screen.getByRole("switch", { name: "Dark mode" });
    expect(toggle).toBeInTheDocument();
    expect(toggle).toHaveAttribute("aria-checked", "true");
  });
});
