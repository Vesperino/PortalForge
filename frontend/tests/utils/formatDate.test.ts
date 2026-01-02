import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import {
  formatDateTime,
  formatDateShort,
  formatDateLong,
  formatDateISO,
  formatTime,
  formatRelativeTime,
  formatDateRange,
  isToday,
  isPast,
  isFuture,
  startOfDay,
  endOfDay,
  addDays,
  diffInDays
} from '~/utils/formatDate'

describe('formatDate utilities', () => {
  describe('formatDateTime', () => {
    it('should format Date object to full date and time string', () => {
      const date = new Date('2026-01-15T14:30:00')
      const result = formatDateTime(date)

      expect(result).toContain('2026')
      expect(result).toContain('14:30')
    })

    it('should format string date to full date and time string', () => {
      const result = formatDateTime('2026-01-15T14:30:00')

      expect(result).toContain('2026')
      expect(result).toContain('14:30')
    })

    it('should return empty string for null', () => {
      expect(formatDateTime(null)).toBe('')
    })

    it('should return empty string for undefined', () => {
      expect(formatDateTime(undefined)).toBe('')
    })

    it('should return empty string for invalid date', () => {
      expect(formatDateTime('invalid-date')).toBe('')
    })
  })

  describe('formatDateShort', () => {
    it('should format date to short format', () => {
      const date = new Date('2026-01-15')
      const result = formatDateShort(date)

      expect(result).toContain('15')
      expect(result).toContain('2026')
    })

    it('should return empty string for null', () => {
      expect(formatDateShort(null)).toBe('')
    })

    it('should return empty string for invalid date', () => {
      expect(formatDateShort('not-a-date')).toBe('')
    })
  })

  describe('formatDateLong', () => {
    it('should format date to long format', () => {
      const date = new Date('2026-01-15')
      const result = formatDateLong(date)

      expect(result).toContain('15')
      expect(result).toContain('2026')
    })

    it('should return empty string for null', () => {
      expect(formatDateLong(null)).toBe('')
    })

    it('should return empty string for invalid date', () => {
      expect(formatDateLong('not-a-date')).toBe('')
    })
  })

  describe('formatDateISO', () => {
    it('should format date to ISO format (YYYY-MM-DD)', () => {
      const date = new Date('2026-01-15T14:30:00Z')
      const result = formatDateISO(date)

      expect(result).toBe('2026-01-15')
    })

    it('should handle string dates', () => {
      const result = formatDateISO('2026-06-20T10:00:00Z')

      expect(result).toBe('2026-06-20')
    })

    it('should return empty string for null', () => {
      expect(formatDateISO(null)).toBe('')
    })

    it('should return empty string for invalid date', () => {
      expect(formatDateISO('invalid')).toBe('')
    })
  })

  describe('formatTime', () => {
    it('should format time only', () => {
      const date = new Date('2026-01-15T14:30:00')
      const result = formatTime(date)

      expect(result).toContain('14')
      expect(result).toContain('30')
    })

    it('should return empty string for null', () => {
      expect(formatTime(null)).toBe('')
    })

    it('should return empty string for invalid date', () => {
      expect(formatTime('invalid')).toBe('')
    })
  })

  describe('formatRelativeTime', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2026-01-15T12:00:00Z'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('should return empty string for null', () => {
      expect(formatRelativeTime(null)).toBe('')
    })

    it('should return empty string for invalid date', () => {
      expect(formatRelativeTime('invalid')).toBe('')
    })

    it('should format seconds ago', () => {
      const date = new Date('2026-01-15T11:59:30Z')
      const result = formatRelativeTime(date)

      expect(result).toBeTruthy()
    })

    it('should format minutes ago', () => {
      const date = new Date('2026-01-15T11:30:00Z')
      const result = formatRelativeTime(date)

      expect(result).toBeTruthy()
    })

    it('should format hours ago', () => {
      const date = new Date('2026-01-15T08:00:00Z')
      const result = formatRelativeTime(date)

      expect(result).toBeTruthy()
    })

    it('should format days ago', () => {
      const date = new Date('2026-01-10T12:00:00Z')
      const result = formatRelativeTime(date)

      expect(result).toBeTruthy()
    })

    it('should format months ago', () => {
      const date = new Date('2025-10-15T12:00:00Z')
      const result = formatRelativeTime(date)

      expect(result).toBeTruthy()
    })

    it('should format years ago', () => {
      const date = new Date('2024-01-15T12:00:00Z')
      const result = formatRelativeTime(date)

      expect(result).toBeTruthy()
    })
  })

  describe('formatDateRange', () => {
    it('should format date range within same month', () => {
      const result = formatDateRange('2026-01-15', '2026-01-20')

      expect(result).toContain('15')
      expect(result).toContain('20')
    })

    it('should format date range within same year', () => {
      const result = formatDateRange('2026-01-15', '2026-03-20')

      expect(result).toContain('15')
      expect(result).toContain('20')
      expect(result).toContain('2026')
    })

    it('should format date range across years', () => {
      const result = formatDateRange('2025-12-28', '2026-01-05')

      expect(result).toContain('28')
      expect(result).toContain('5')
      expect(result).toContain('2025')
      expect(result).toContain('2026')
    })

    it('should return empty string if start date is null', () => {
      expect(formatDateRange(null, '2026-01-20')).toBe('')
    })

    it('should return empty string if end date is null', () => {
      expect(formatDateRange('2026-01-15', null)).toBe('')
    })

    it('should return empty string for invalid dates', () => {
      expect(formatDateRange('invalid', '2026-01-20')).toBe('')
      expect(formatDateRange('2026-01-15', 'invalid')).toBe('')
    })
  })

  describe('isToday', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2026-01-15T12:00:00'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('should return true for today', () => {
      const today = new Date('2026-01-15T08:00:00')
      expect(isToday(today)).toBe(true)
    })

    it('should return false for yesterday', () => {
      const yesterday = new Date('2026-01-14T12:00:00')
      expect(isToday(yesterday)).toBe(false)
    })

    it('should return false for tomorrow', () => {
      const tomorrow = new Date('2026-01-16T12:00:00')
      expect(isToday(tomorrow)).toBe(false)
    })

    it('should return false for null', () => {
      expect(isToday(null)).toBe(false)
    })

    it('should return false for invalid date', () => {
      expect(isToday('invalid')).toBe(false)
    })
  })

  describe('isPast', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2026-01-15T12:00:00'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('should return true for past date', () => {
      const past = new Date('2026-01-14T12:00:00')
      expect(isPast(past)).toBe(true)
    })

    it('should return false for future date', () => {
      const future = new Date('2026-01-16T12:00:00')
      expect(isPast(future)).toBe(false)
    })

    it('should return false for null', () => {
      expect(isPast(null)).toBe(false)
    })

    it('should return false for invalid date', () => {
      expect(isPast('invalid')).toBe(false)
    })
  })

  describe('isFuture', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2026-01-15T12:00:00'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('should return true for future date', () => {
      const future = new Date('2026-01-16T12:00:00')
      expect(isFuture(future)).toBe(true)
    })

    it('should return false for past date', () => {
      const past = new Date('2026-01-14T12:00:00')
      expect(isFuture(past)).toBe(false)
    })

    it('should return false for null', () => {
      expect(isFuture(null)).toBe(false)
    })

    it('should return false for invalid date', () => {
      expect(isFuture('invalid')).toBe(false)
    })
  })

  describe('startOfDay', () => {
    it('should return start of day', () => {
      const date = new Date('2026-01-15T14:30:45')
      const result = startOfDay(date)

      expect(result.getHours()).toBe(0)
      expect(result.getMinutes()).toBe(0)
      expect(result.getSeconds()).toBe(0)
      expect(result.getMilliseconds()).toBe(0)
    })

    it('should handle string dates', () => {
      const result = startOfDay('2026-01-15T14:30:45')

      expect(result.getHours()).toBe(0)
      expect(result.getMinutes()).toBe(0)
    })
  })

  describe('endOfDay', () => {
    it('should return end of day', () => {
      const date = new Date('2026-01-15T14:30:45')
      const result = endOfDay(date)

      expect(result.getHours()).toBe(23)
      expect(result.getMinutes()).toBe(59)
      expect(result.getSeconds()).toBe(59)
      expect(result.getMilliseconds()).toBe(999)
    })

    it('should handle string dates', () => {
      const result = endOfDay('2026-01-15T14:30:45')

      expect(result.getHours()).toBe(23)
      expect(result.getMinutes()).toBe(59)
    })
  })

  describe('addDays', () => {
    it('should add positive days', () => {
      const date = new Date('2026-01-15')
      const result = addDays(date, 5)

      expect(result.getDate()).toBe(20)
    })

    it('should subtract days with negative value', () => {
      const date = new Date('2026-01-15')
      const result = addDays(date, -5)

      expect(result.getDate()).toBe(10)
    })

    it('should handle month boundaries', () => {
      const date = new Date('2026-01-30')
      const result = addDays(date, 5)

      expect(result.getMonth()).toBe(1) // February
      expect(result.getDate()).toBe(4)
    })

    it('should handle string dates', () => {
      const result = addDays('2026-01-15', 10)

      expect(result.getDate()).toBe(25)
    })
  })

  describe('diffInDays', () => {
    it('should calculate difference in days', () => {
      const dateA = new Date('2026-01-15')
      const dateB = new Date('2026-01-20')

      expect(diffInDays(dateA, dateB)).toBe(5)
    })

    it('should return absolute difference', () => {
      const dateA = new Date('2026-01-20')
      const dateB = new Date('2026-01-15')

      expect(diffInDays(dateA, dateB)).toBe(5)
    })

    it('should handle string dates', () => {
      const result = diffInDays('2026-01-15', '2026-01-25')

      expect(result).toBe(10)
    })

    it('should return 0 for same day', () => {
      const date = new Date('2026-01-15')

      expect(diffInDays(date, date)).toBe(0)
    })
  })
})
