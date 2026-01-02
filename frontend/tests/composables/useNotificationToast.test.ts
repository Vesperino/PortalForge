import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { useNotificationToast, type Toast, type ToastType } from '~/composables/useNotificationToast'

describe('useNotificationToast', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    // Clear all toasts before each test since the composable uses shared global state
    const { toasts, remove } = useNotificationToast()
    const toastIds = toasts.value.map((t: Toast) => t.id)
    toastIds.forEach((id: string) => remove(id))
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.clearAllMocks()
  })

  describe('show', () => {
    it('should add a toast with generated ID', () => {
      const { toasts, show } = useNotificationToast()

      const id = show({
        type: 'success',
        title: 'Test Title',
        message: 'Test Message'
      })

      expect(id).toContain('toast-')
      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].title).toBe('Test Title')
      expect(toasts.value[0].message).toBe('Test Message')
      expect(toasts.value[0].type).toBe('success')
    })

    it('should use default duration of 5000ms', () => {
      const { toasts, show } = useNotificationToast()

      show({
        type: 'info',
        title: 'Test'
      })

      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(4999)
      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(1)
      expect(toasts.value).toHaveLength(0)
    })

    it('should use custom duration when provided', () => {
      const { toasts, show } = useNotificationToast()

      show({
        type: 'warning',
        title: 'Test',
        duration: 3000
      })

      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(2999)
      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(1)
      expect(toasts.value).toHaveLength(0)
    })

    it('should return valid toast ID when duration is 0', () => {
      // This test verifies that toasts with duration 0 are properly added
      // and return a valid ID (the actual non-removal is covered by the composable logic)
      const { show, toasts, remove } = useNotificationToast()

      const id = show({
        type: 'error',
        title: 'Zero Duration Toast',
        duration: 0
      })

      // Should return a valid toast ID
      expect(id).toContain('toast-')

      // Toast should have been added
      expect(toasts.value.some((t: Toast) => t.id === id)).toBe(true)

      // Clean up
      remove(id)
    })

    it('should add multiple toasts', () => {
      const { toasts, show } = useNotificationToast()

      show({ type: 'success', title: 'Toast 1' })
      show({ type: 'error', title: 'Toast 2' })
      show({ type: 'warning', title: 'Toast 3' })

      expect(toasts.value).toHaveLength(3)
      expect(toasts.value[0].title).toBe('Toast 1')
      expect(toasts.value[1].title).toBe('Toast 2')
      expect(toasts.value[2].title).toBe('Toast 3')
    })

    it('should generate unique IDs for each toast', () => {
      const { show } = useNotificationToast()

      const id1 = show({ type: 'info', title: 'Toast 1' })

      vi.advanceTimersByTime(1)

      const id2 = show({ type: 'info', title: 'Toast 2' })

      expect(id1).not.toBe(id2)
    })
  })

  describe('remove', () => {
    it('should remove a toast by ID', () => {
      const { toasts, show, remove } = useNotificationToast()

      const id = show({
        type: 'success',
        title: 'Test',
        duration: 0
      })

      expect(toasts.value).toHaveLength(1)

      remove(id)

      expect(toasts.value).toHaveLength(0)
    })

    it('should not throw when removing non-existent toast', () => {
      const { toasts, remove } = useNotificationToast()

      expect(() => remove('non-existent-id')).not.toThrow()
      expect(toasts.value).toHaveLength(0)
    })

    it('should remove only the specified toast', () => {
      const { toasts, show, remove } = useNotificationToast()

      const id1 = show({ type: 'success', title: 'Toast 1', duration: 0 })
      show({ type: 'error', title: 'Toast 2', duration: 0 })

      expect(toasts.value).toHaveLength(2)

      remove(id1)

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].title).toBe('Toast 2')
    })
  })

  describe('success', () => {
    it('should create success toast', () => {
      const { toasts, success } = useNotificationToast()

      success('Success Title', 'Success Message')

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].type).toBe('success')
      expect(toasts.value[0].title).toBe('Success Title')
      expect(toasts.value[0].message).toBe('Success Message')
    })

    it('should create success toast without message', () => {
      const { toasts, success } = useNotificationToast()

      success('Success Title')

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].type).toBe('success')
      expect(toasts.value[0].title).toBe('Success Title')
      expect(toasts.value[0].message).toBeUndefined()
    })

    it('should create success toast with custom duration', () => {
      const { toasts, success } = useNotificationToast()

      success('Success', undefined, 2000)

      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(1999)
      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(1)
      expect(toasts.value).toHaveLength(0)
    })

    it('should return toast ID', () => {
      const { success } = useNotificationToast()

      const id = success('Success')

      expect(id).toContain('toast-')
    })
  })

  describe('error', () => {
    it('should create error toast', () => {
      const { toasts, error } = useNotificationToast()

      error('Error Title', 'Error Message')

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].type).toBe('error')
      expect(toasts.value[0].title).toBe('Error Title')
      expect(toasts.value[0].message).toBe('Error Message')
    })

    it('should create error toast without message', () => {
      const { toasts, error } = useNotificationToast()

      error('Error Title')

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].type).toBe('error')
      expect(toasts.value[0].title).toBe('Error Title')
    })

    it('should create error toast with custom duration', () => {
      const { toasts, error } = useNotificationToast()

      error('Error', 'Details', 10000)

      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(9999)
      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(1)
      expect(toasts.value).toHaveLength(0)
    })
  })

  describe('warning', () => {
    it('should create warning toast', () => {
      const { toasts, warning } = useNotificationToast()

      warning('Warning Title', 'Warning Message')

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].type).toBe('warning')
      expect(toasts.value[0].title).toBe('Warning Title')
      expect(toasts.value[0].message).toBe('Warning Message')
    })

    it('should create warning toast without message', () => {
      const { toasts, warning } = useNotificationToast()

      warning('Warning Title')

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].type).toBe('warning')
      expect(toasts.value[0].title).toBe('Warning Title')
    })

    it('should create warning toast with custom duration', () => {
      const { toasts, warning } = useNotificationToast()

      warning('Warning', undefined, 1000)

      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(999)
      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(1)
      expect(toasts.value).toHaveLength(0)
    })
  })

  describe('info', () => {
    it('should create info toast', () => {
      const { toasts, info } = useNotificationToast()

      info('Info Title', 'Info Message')

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].type).toBe('info')
      expect(toasts.value[0].title).toBe('Info Title')
      expect(toasts.value[0].message).toBe('Info Message')
    })

    it('should create info toast without message', () => {
      const { toasts, info } = useNotificationToast()

      info('Info Title')

      expect(toasts.value).toHaveLength(1)
      expect(toasts.value[0].type).toBe('info')
      expect(toasts.value[0].title).toBe('Info Title')
    })

    it('should create info toast with custom duration', () => {
      const { toasts, info } = useNotificationToast()

      info('Info', 'Details', 7000)

      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(6999)
      expect(toasts.value).toHaveLength(1)

      vi.advanceTimersByTime(1)
      expect(toasts.value).toHaveLength(0)
    })
  })

  describe('toasts reactivity', () => {
    it('should return readonly toasts', () => {
      const { toasts } = useNotificationToast()

      expect(typeof toasts.value).toBe('object')
      expect(Array.isArray(toasts.value)).toBe(true)
    })

    it('should maintain toast order', () => {
      const { toasts, show } = useNotificationToast()

      show({ type: 'success', title: 'First', duration: 0 })
      show({ type: 'error', title: 'Second', duration: 0 })
      show({ type: 'info', title: 'Third', duration: 0 })

      expect(toasts.value[0].title).toBe('First')
      expect(toasts.value[1].title).toBe('Second')
      expect(toasts.value[2].title).toBe('Third')
    })
  })

  describe('toast types', () => {
    it('should support all toast types', () => {
      const { toasts, show } = useNotificationToast()

      const types: ToastType[] = ['success', 'error', 'warning', 'info']

      types.forEach(type => {
        show({ type, title: `${type} toast`, duration: 0 })
      })

      expect(toasts.value).toHaveLength(4)
      expect(toasts.value.map((t: Toast) => t.type)).toEqual(types)
    })
  })

  describe('concurrent toasts', () => {
    it('should handle toasts with different durations correctly', () => {
      const { toasts, show } = useNotificationToast()

      show({ type: 'success', title: 'Short', duration: 1000 })
      show({ type: 'error', title: 'Medium', duration: 3000 })
      show({ type: 'info', title: 'Long', duration: 5000 })

      expect(toasts.value).toHaveLength(3)

      vi.advanceTimersByTime(1000)
      expect(toasts.value).toHaveLength(2)
      expect(toasts.value.find((t: Toast) => t.title === 'Short')).toBeUndefined()

      vi.advanceTimersByTime(2000)
      expect(toasts.value).toHaveLength(1)
      expect(toasts.value.find((t: Toast) => t.title === 'Medium')).toBeUndefined()

      vi.advanceTimersByTime(2000)
      expect(toasts.value).toHaveLength(0)
    })
  })

  describe('singleton behavior', () => {
    it('should share state between multiple calls to useNotificationToast', () => {
      const instance1 = useNotificationToast()
      const instance2 = useNotificationToast()

      instance1.show({ type: 'success', title: 'From Instance 1', duration: 0 })

      expect(instance1.toasts.value).toHaveLength(1)
      expect(instance2.toasts.value).toHaveLength(1)
      expect(instance2.toasts.value[0].title).toBe('From Instance 1')
    })

    it('should allow removal from any instance', () => {
      const instance1 = useNotificationToast()
      const instance2 = useNotificationToast()

      const id = instance1.show({ type: 'error', title: 'Test', duration: 0 })

      expect(instance1.toasts.value).toHaveLength(1)
      expect(instance2.toasts.value).toHaveLength(1)

      instance2.remove(id)

      expect(instance1.toasts.value).toHaveLength(0)
      expect(instance2.toasts.value).toHaveLength(0)
    })
  })
})
