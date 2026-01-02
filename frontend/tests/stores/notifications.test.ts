import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useNotificationsStore } from '~/stores/notifications'
import type { Notification, NotificationType } from '~/types/requests'

// Mock useRequestsApi
const mockGetNotifications = vi.fn()
const mockGetUnreadCount = vi.fn()
const mockMarkAsRead = vi.fn()
const mockMarkAllAsRead = vi.fn()

vi.mock('~/composables/useRequestsApi', () => ({
  useRequestsApi: () => ({
    getNotifications: mockGetNotifications,
    getUnreadCount: mockGetUnreadCount,
    markAsRead: mockMarkAsRead,
    markAllAsRead: mockMarkAllAsRead
  })
}))

// Mock global useRequestsApi
globalThis.useRequestsApi = () => ({
  getNotifications: mockGetNotifications,
  getUnreadCount: mockGetUnreadCount,
  markAsRead: mockMarkAsRead,
  markAllAsRead: mockMarkAllAsRead
})

function createMockNotification(overrides: Partial<Notification> = {}): Notification {
  return {
    id: 'notification-1',
    type: 'RequestPendingApproval' as NotificationType,
    title: 'Test Notification',
    message: 'Test notification message',
    isRead: false,
    createdAt: new Date().toISOString(),
    ...overrides
  }
}

describe('useNotificationsStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  describe('initial state', () => {
    it('should initialize with empty notifications', () => {
      const store = useNotificationsStore()

      expect(store.notifications).toEqual([])
      expect(store.unreadCount).toBe(0)
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('getters', () => {
    it('should return unread notifications', () => {
      const store = useNotificationsStore()
      store.notifications = [
        createMockNotification({ id: '1', isRead: false }),
        createMockNotification({ id: '2', isRead: true }),
        createMockNotification({ id: '3', isRead: false })
      ]

      const unread = store.unreadNotifications

      expect(unread).toHaveLength(2)
      expect(unread.every(n => !n.isRead)).toBe(true)
    })

    it('should return hasUnread as true when unreadCount > 0', () => {
      const store = useNotificationsStore()
      store.unreadCount = 5

      expect(store.hasUnread).toBe(true)
    })

    it('should return hasUnread as false when unreadCount is 0', () => {
      const store = useNotificationsStore()
      store.unreadCount = 0

      expect(store.hasUnread).toBe(false)
    })
  })

  describe('fetchNotifications', () => {
    it('should fetch notifications successfully', async () => {
      const store = useNotificationsStore()
      const mockNotifications = [
        createMockNotification({ id: '1' }),
        createMockNotification({ id: '2' })
      ]
      mockGetNotifications.mockResolvedValue(mockNotifications)

      await store.fetchNotifications()

      expect(store.notifications).toEqual(mockNotifications)
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('should set loading state during fetch', async () => {
      const store = useNotificationsStore()
      mockGetNotifications.mockImplementation(() => new Promise(resolve => setTimeout(() => resolve([]), 100)))

      const fetchPromise = store.fetchNotifications()

      expect(store.loading).toBe(true)

      await fetchPromise

      expect(store.loading).toBe(false)
    })

    it('should pass unreadOnly parameter', async () => {
      const store = useNotificationsStore()
      mockGetNotifications.mockResolvedValue([])

      await store.fetchNotifications(true)

      expect(mockGetNotifications).toHaveBeenCalledWith(true)
    })

    it('should handle fetch error', async () => {
      const store = useNotificationsStore()
      mockGetNotifications.mockRejectedValue(new Error('Network error'))

      await store.fetchNotifications()

      expect(store.error).toBe('Network error')
      expect(store.loading).toBe(false)
    })

    it('should set generic error message for non-Error objects', async () => {
      const store = useNotificationsStore()
      mockGetNotifications.mockRejectedValue({ statusCode: 500 })

      await store.fetchNotifications()

      expect(store.error).toBe('Failed to fetch notifications')
    })
  })

  describe('fetchUnreadCount', () => {
    it('should fetch unread count successfully', async () => {
      const store = useNotificationsStore()
      mockGetUnreadCount.mockResolvedValue(5)

      await store.fetchUnreadCount()

      expect(store.unreadCount).toBe(5)
    })

    it('should reset unread count to 0 on error', async () => {
      const store = useNotificationsStore()
      store.unreadCount = 10
      mockGetUnreadCount.mockRejectedValue(new Error('Network error'))

      await store.fetchUnreadCount()

      expect(store.unreadCount).toBe(0)
    })
  })

  describe('markAsRead', () => {
    it('should mark notification as read successfully', async () => {
      const store = useNotificationsStore()
      const notification = createMockNotification({ id: 'test-id', isRead: false })
      store.notifications = [notification]
      store.unreadCount = 1
      mockMarkAsRead.mockResolvedValue(undefined)

      await store.markAsRead('test-id')

      expect(mockMarkAsRead).toHaveBeenCalledWith('test-id')
      expect(store.notifications[0].isRead).toBe(true)
      expect(store.notifications[0].readAt).toBeDefined()
      expect(store.unreadCount).toBe(0)
    })

    it('should not decrement unreadCount if notification was already read', async () => {
      const store = useNotificationsStore()
      const notification = createMockNotification({ id: 'test-id', isRead: true })
      store.notifications = [notification]
      store.unreadCount = 0
      mockMarkAsRead.mockResolvedValue(undefined)

      await store.markAsRead('test-id')

      expect(store.unreadCount).toBe(0)
    })

    it('should not go below 0 for unreadCount', async () => {
      const store = useNotificationsStore()
      const notification = createMockNotification({ id: 'test-id', isRead: false })
      store.notifications = [notification]
      store.unreadCount = 0
      mockMarkAsRead.mockResolvedValue(undefined)

      await store.markAsRead('test-id')

      expect(store.unreadCount).toBe(0)
    })

    it('should throw error when markAsRead fails', async () => {
      const store = useNotificationsStore()
      store.notifications = [createMockNotification({ id: 'test-id' })]
      mockMarkAsRead.mockRejectedValue(new Error('API error'))

      await expect(store.markAsRead('test-id')).rejects.toThrow('API error')
    })
  })

  describe('markAllAsRead', () => {
    it('should mark all notifications as read', async () => {
      const store = useNotificationsStore()
      store.notifications = [
        createMockNotification({ id: '1', isRead: false }),
        createMockNotification({ id: '2', isRead: false }),
        createMockNotification({ id: '3', isRead: true })
      ]
      store.unreadCount = 2
      mockMarkAllAsRead.mockResolvedValue(undefined)

      await store.markAllAsRead()

      expect(mockMarkAllAsRead).toHaveBeenCalled()
      expect(store.notifications.every(n => n.isRead)).toBe(true)
      // Only previously unread notifications get readAt set
      expect(store.notifications.filter(n => n.readAt).length).toBeGreaterThanOrEqual(2)
      expect(store.unreadCount).toBe(0)
    })

    it('should throw error when markAllAsRead fails', async () => {
      const store = useNotificationsStore()
      store.notifications = [createMockNotification({ id: '1', isRead: false })]
      mockMarkAllAsRead.mockRejectedValue(new Error('API error'))

      await expect(store.markAllAsRead()).rejects.toThrow('API error')
    })
  })

  describe('startPolling', () => {
    beforeEach(() => {
      vi.useFakeTimers()
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('should fetch unread count immediately', () => {
      const store = useNotificationsStore()
      mockGetUnreadCount.mockResolvedValue(3)

      store.startPolling()

      expect(mockGetUnreadCount).toHaveBeenCalledTimes(1)
    })

    it('should fetch unread count at specified interval', async () => {
      const store = useNotificationsStore()
      mockGetUnreadCount.mockResolvedValue(3)

      const intervalId = store.startPolling(10000)

      expect(mockGetUnreadCount).toHaveBeenCalledTimes(1)

      vi.advanceTimersByTime(10000)
      expect(mockGetUnreadCount).toHaveBeenCalledTimes(2)

      vi.advanceTimersByTime(10000)
      expect(mockGetUnreadCount).toHaveBeenCalledTimes(3)

      clearInterval(intervalId)
    })

    it('should use default interval of 30000ms', () => {
      const store = useNotificationsStore()
      mockGetUnreadCount.mockResolvedValue(3)

      const intervalId = store.startPolling()

      expect(mockGetUnreadCount).toHaveBeenCalledTimes(1)

      vi.advanceTimersByTime(29999)
      expect(mockGetUnreadCount).toHaveBeenCalledTimes(1)

      vi.advanceTimersByTime(1)
      expect(mockGetUnreadCount).toHaveBeenCalledTimes(2)

      clearInterval(intervalId)
    })

    it('should return interval ID for cleanup', () => {
      const store = useNotificationsStore()
      mockGetUnreadCount.mockResolvedValue(0)

      const intervalId = store.startPolling()

      // In Node.js environment, setInterval returns a Timeout object, not a number
      expect(intervalId).toBeDefined()

      clearInterval(intervalId)
    })
  })
})
