import { defineStore } from 'pinia'
import type { Notification } from '~/types/requests'

export const useNotificationsStore = defineStore('notifications', {
  state: () => ({
    notifications: [] as Notification[],
    unreadCount: 0,
    loading: false,
    error: null as string | null
  }),

  getters: {
    unreadNotifications: (state) => state.notifications.filter(n => !n.isRead),
    hasUnread: (state) => state.unreadCount > 0
  },

  actions: {
    async fetchNotifications(unreadOnly = false) {
      this.loading = true
      this.error = null
      try {
        const { getNotifications } = useRequestsApi()
        this.notifications = await getNotifications(unreadOnly)
      } catch (error: any) {
        this.error = error.message || 'Failed to fetch notifications'
        console.error('Error fetching notifications:', error)
      } finally {
        this.loading = false
      }
    },

    async fetchUnreadCount() {
      try {
        const { getUnreadCount } = useRequestsApi()
        this.unreadCount = await getUnreadCount()
      } catch (error: any) {
        console.error('Error fetching unread count:', error)
      }
    },

    async markAsRead(notificationId: string) {
      try {
        const { markAsRead } = useRequestsApi()
        await markAsRead(notificationId)
        
        // Update local state
        const notification = this.notifications.find(n => n.id === notificationId)
        if (notification && !notification.isRead) {
          notification.isRead = true
          notification.readAt = new Date().toISOString()
          this.unreadCount = Math.max(0, this.unreadCount - 1)
        }
      } catch (error: any) {
        console.error('Error marking notification as read:', error)
        throw error
      }
    },

    async markAllAsRead() {
      try {
        const { markAllAsRead } = useRequestsApi()
        await markAllAsRead()
        
        // Update local state
        this.notifications.forEach(n => {
          if (!n.isRead) {
            n.isRead = true
            n.readAt = new Date().toISOString()
          }
        })
        this.unreadCount = 0
      } catch (error: any) {
        console.error('Error marking all notifications as read:', error)
        throw error
      }
    },

    // Poll for new notifications
    startPolling(intervalMs = 30000) {
      this.fetchUnreadCount()
      return setInterval(() => {
        this.fetchUnreadCount()
      }, intervalMs)
    }
  }
})

