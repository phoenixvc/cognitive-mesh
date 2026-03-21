/**
 * Tests for the toast reducer and dispatch logic.
 *
 * The use-toast module exports a pure `reducer` function that can be tested
 * directly, plus `toast` and `useToast` for integration.
 */

// Mock the toast component types since we only need the reducer
jest.mock("@/components/ui/toast", () => ({}));

import { reducer, toast } from "./use-toast";

describe("toast reducer", () => {
  it("should add a toast to an empty state", () => {
    const state = { toasts: [] };
    const result = reducer(state, {
      type: "ADD_TOAST",
      toast: { id: "1", open: true, title: "Hello" } as never,
    });
    expect(result.toasts).toHaveLength(1);
    expect(result.toasts[0].id).toBe("1");
  });

  it("should limit toasts to TOAST_LIMIT (1)", () => {
    const state = { toasts: [] };
    let result = reducer(state, {
      type: "ADD_TOAST",
      toast: { id: "1", open: true, title: "First" } as never,
    });
    result = reducer(result, {
      type: "ADD_TOAST",
      toast: { id: "2", open: true, title: "Second" } as never,
    });
    // Only 1 toast should remain (the newest)
    expect(result.toasts).toHaveLength(1);
    expect(result.toasts[0].id).toBe("2");
  });

  it("should update an existing toast by id", () => {
    const state = {
      toasts: [{ id: "1", open: true, title: "Original" } as never],
    };
    const result = reducer(state, {
      type: "UPDATE_TOAST",
      toast: { id: "1", title: "Updated" },
    });
    expect(result.toasts).toHaveLength(1);
    expect((result.toasts[0] as { title: string }).title).toBe("Updated");
  });

  it("should not affect other toasts when updating", () => {
    // Even though TOAST_LIMIT is 1, test update logic with a manually crafted state
    const state = {
      toasts: [{ id: "1", open: true, title: "Keep" } as never],
    };
    const result = reducer(state, {
      type: "UPDATE_TOAST",
      toast: { id: "999", title: "No match" },
    });
    expect((result.toasts[0] as { title: string }).title).toBe("Keep");
  });

  it("should set open to false when dismissing a specific toast", () => {
    const state = {
      toasts: [{ id: "1", open: true, title: "Test" } as never],
    };
    const result = reducer(state, {
      type: "DISMISS_TOAST",
      toastId: "1",
    });
    expect(result.toasts[0].open).toBe(false);
  });

  it("should dismiss all toasts when no toastId provided", () => {
    const state = {
      toasts: [{ id: "1", open: true, title: "Test" } as never],
    };
    const result = reducer(state, {
      type: "DISMISS_TOAST",
    });
    expect(result.toasts.every((t) => t.open === false)).toBe(true);
  });

  it("should remove a specific toast by id", () => {
    const state = {
      toasts: [{ id: "1", open: true } as never],
    };
    const result = reducer(state, {
      type: "REMOVE_TOAST",
      toastId: "1",
    });
    expect(result.toasts).toHaveLength(0);
  });

  it("should remove all toasts when REMOVE_TOAST has no toastId", () => {
    const state = {
      toasts: [
        { id: "1", open: true } as never,
      ],
    };
    const result = reducer(state, {
      type: "REMOVE_TOAST",
      toastId: undefined,
    });
    expect(result.toasts).toHaveLength(0);
  });
});

describe("toast function", () => {
  it("should return an id, dismiss, and update", () => {
    const result = toast({ title: "Test toast" } as never);
    expect(result.id).toBeDefined();
    expect(typeof result.dismiss).toBe("function");
    expect(typeof result.update).toBe("function");
  });

  it("should generate unique ids for each toast", () => {
    const a = toast({ title: "A" } as never);
    const b = toast({ title: "B" } as never);
    expect(a.id).not.toBe(b.id);
  });
});
