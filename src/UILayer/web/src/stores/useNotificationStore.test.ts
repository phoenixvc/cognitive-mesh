import { useNotificationStore } from "./useNotificationStore";
import { act } from "@testing-library/react";

// Reset store between tests
beforeEach(() => {
  act(() => {
    useNotificationStore.setState({
      notifications: [],
      unreadCount: 0,
    });
  });
});

describe("useNotificationStore", () => {
  it("should start with empty notifications and zero unread count", () => {
    const state = useNotificationStore.getState();
    expect(state.notifications).toEqual([]);
    expect(state.unreadCount).toBe(0);
  });

  it("should add a notification and increment unread count", () => {
    act(() => {
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "Test",
        message: "Hello world",
      });
    });
    const state = useNotificationStore.getState();
    expect(state.notifications).toHaveLength(1);
    expect(state.notifications[0].title).toBe("Test");
    expect(state.notifications[0].message).toBe("Hello world");
    expect(state.notifications[0].type).toBe("info");
    expect(state.notifications[0].read).toBe(false);
    expect(state.notifications[0].id).toBeDefined();
    expect(state.notifications[0].timestamp).toBeDefined();
    expect(state.unreadCount).toBe(1);
  });

  it("should prepend new notifications (newest first)", () => {
    act(() => {
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "First",
        message: "first",
      });
    });
    act(() => {
      useNotificationStore.getState().addNotification({
        type: "success",
        title: "Second",
        message: "second",
      });
    });
    const state = useNotificationStore.getState();
    expect(state.notifications).toHaveLength(2);
    expect(state.notifications[0].title).toBe("Second");
    expect(state.notifications[1].title).toBe("First");
    expect(state.unreadCount).toBe(2);
  });

  it("should cap notifications at 100", () => {
    act(() => {
      for (let i = 0; i < 105; i++) {
        useNotificationStore.getState().addNotification({
          type: "info",
          title: `Notification ${i}`,
          message: `msg ${i}`,
        });
      }
    });
    expect(useNotificationStore.getState().notifications).toHaveLength(100);
  });

  it("should mark a single notification as read", () => {
    act(() => {
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "Test",
        message: "msg",
      });
    });
    const id = useNotificationStore.getState().notifications[0].id;
    act(() => {
      useNotificationStore.getState().markRead(id);
    });
    const state = useNotificationStore.getState();
    expect(state.notifications[0].read).toBe(true);
    expect(state.unreadCount).toBe(0);
  });

  it("should mark all notifications as read", () => {
    act(() => {
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "A",
        message: "a",
      });
      useNotificationStore.getState().addNotification({
        type: "warning",
        title: "B",
        message: "b",
      });
    });
    act(() => {
      useNotificationStore.getState().markAllRead();
    });
    const state = useNotificationStore.getState();
    expect(state.notifications.every((n) => n.read)).toBe(true);
    expect(state.unreadCount).toBe(0);
  });

  it("should dismiss (remove) a notification by id", () => {
    act(() => {
      useNotificationStore.getState().addNotification({
        type: "error",
        title: "Remove me",
        message: "bye",
      });
    });
    const id = useNotificationStore.getState().notifications[0].id;
    act(() => {
      useNotificationStore.getState().dismiss(id);
    });
    expect(useNotificationStore.getState().notifications).toHaveLength(0);
    expect(useNotificationStore.getState().unreadCount).toBe(0);
  });

  it("should clear all notifications", () => {
    act(() => {
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "A",
        message: "a",
      });
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "B",
        message: "b",
      });
    });
    act(() => {
      useNotificationStore.getState().clearAll();
    });
    const state = useNotificationStore.getState();
    expect(state.notifications).toEqual([]);
    expect(state.unreadCount).toBe(0);
  });

  it("should update unread count correctly after mixed operations", () => {
    act(() => {
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "A",
        message: "a",
      });
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "B",
        message: "b",
      });
      useNotificationStore.getState().addNotification({
        type: "info",
        title: "C",
        message: "c",
      });
    });
    expect(useNotificationStore.getState().unreadCount).toBe(3);

    // Mark one as read
    const idB = useNotificationStore.getState().notifications[1].id;
    act(() => {
      useNotificationStore.getState().markRead(idB);
    });
    expect(useNotificationStore.getState().unreadCount).toBe(2);

    // Dismiss another (unread)
    const idC = useNotificationStore.getState().notifications[0].id;
    act(() => {
      useNotificationStore.getState().dismiss(idC);
    });
    expect(useNotificationStore.getState().unreadCount).toBe(1);
  });
});
