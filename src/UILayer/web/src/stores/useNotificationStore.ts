/**
 * Notification store — manages in-app notifications + toast queue.
 *
 * SignalR pushes new notifications via addNotification().
 * Toast items auto-dismiss via the Toast component.
 */
import { create } from "zustand"

type NotificationType = "info" | "success" | "warning" | "error"

interface Notification {
  id: string
  type: NotificationType
  title: string
  message: string
  timestamp: number
  read: boolean
  action?: { label: string; href: string }
}

interface NotificationStoreState {
  notifications: Notification[]
  unreadCount: number
}

interface NotificationStoreActions {
  addNotification: (
    notification: Omit<Notification, "id" | "timestamp" | "read">
  ) => void
  markRead: (id: string) => void
  markAllRead: () => void
  dismiss: (id: string) => void
  clearAll: () => void
}

let nextId = 1

export const useNotificationStore = create<
  NotificationStoreState & NotificationStoreActions
>((set) => ({
  notifications: [],
  unreadCount: 0,

  addNotification: (notification) =>
    set((state) => {
      const newNotification: Notification = {
        ...notification,
        id: `notif-${nextId++}`,
        timestamp: Date.now(),
        read: false,
      }
      const notifications = [newNotification, ...state.notifications].slice(
        0,
        100
      )
      return {
        notifications,
        unreadCount: notifications.filter((n) => !n.read).length,
      }
    }),

  markRead: (id) =>
    set((state) => {
      const notifications = state.notifications.map((n) =>
        n.id === id ? { ...n, read: true } : n
      )
      return {
        notifications,
        unreadCount: notifications.filter((n) => !n.read).length,
      }
    }),

  markAllRead: () =>
    set((state) => ({
      notifications: state.notifications.map((n) => ({ ...n, read: true })),
      unreadCount: 0,
    })),

  dismiss: (id) =>
    set((state) => {
      const notifications = state.notifications.filter((n) => n.id !== id)
      return {
        notifications,
        unreadCount: notifications.filter((n) => !n.read).length,
      }
    }),

  clearAll: () => set({ notifications: [], unreadCount: 0 }),
}))
