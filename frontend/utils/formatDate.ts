/**
 * Date formatting utilities for the PortalForge application
 * All functions use Polish locale by default
 */

const DEFAULT_LOCALE = 'pl-PL'

/**
 * Formats a date to a full date and time string
 * Example: "2 stycznia 2026, 14:30"
 */
export function formatDateTime(
  date: Date | string | null | undefined,
  locale = DEFAULT_LOCALE
): string {
  if (!date) return ''

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return ''

  return new Intl.DateTimeFormat(locale, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(dateObj)
}

/**
 * Formats a date to a short date string
 * Example: "2 sty 2026"
 */
export function formatDateShort(
  date: Date | string | null | undefined,
  locale = DEFAULT_LOCALE
): string {
  if (!date) return ''

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return ''

  return new Intl.DateTimeFormat(locale, {
    day: 'numeric',
    month: 'short',
    year: 'numeric'
  }).format(dateObj)
}

/**
 * Formats a date to a long date string
 * Example: "2 stycznia 2026"
 */
export function formatDateLong(
  date: Date | string | null | undefined,
  locale = DEFAULT_LOCALE
): string {
  if (!date) return ''

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return ''

  return new Intl.DateTimeFormat(locale, {
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  }).format(dateObj)
}

/**
 * Formats a date to ISO date string (YYYY-MM-DD)
 * Useful for form inputs and API requests
 */
export function formatDateISO(
  date: Date | string | null | undefined
): string {
  if (!date) return ''

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return ''

  return dateObj.toISOString().split('T')[0]
}

/**
 * Formats time only
 * Example: "14:30"
 */
export function formatTime(
  date: Date | string | null | undefined,
  locale = DEFAULT_LOCALE
): string {
  if (!date) return ''

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return ''

  return new Intl.DateTimeFormat(locale, {
    hour: '2-digit',
    minute: '2-digit'
  }).format(dateObj)
}

/**
 * Formats a relative time string
 * Example: "2 dni temu", "za 3 godziny"
 */
export function formatRelativeTime(
  date: Date | string | null | undefined,
  locale = DEFAULT_LOCALE
): string {
  if (!date) return ''

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return ''

  const now = new Date()
  const diffInSeconds = Math.floor((dateObj.getTime() - now.getTime()) / 1000)
  const absDiff = Math.abs(diffInSeconds)

  const rtf = new Intl.RelativeTimeFormat(locale, { numeric: 'auto' })

  if (absDiff < 60) {
    return rtf.format(diffInSeconds, 'second')
  } else if (absDiff < 3600) {
    return rtf.format(Math.floor(diffInSeconds / 60), 'minute')
  } else if (absDiff < 86400) {
    return rtf.format(Math.floor(diffInSeconds / 3600), 'hour')
  } else if (absDiff < 2592000) {
    return rtf.format(Math.floor(diffInSeconds / 86400), 'day')
  } else if (absDiff < 31536000) {
    return rtf.format(Math.floor(diffInSeconds / 2592000), 'month')
  } else {
    return rtf.format(Math.floor(diffInSeconds / 31536000), 'year')
  }
}

/**
 * Formats a date range
 * Example: "2 - 5 stycznia 2026" or "28 grudnia 2025 - 2 stycznia 2026"
 */
export function formatDateRange(
  startDate: Date | string | null | undefined,
  endDate: Date | string | null | undefined,
  locale = DEFAULT_LOCALE
): string {
  if (!startDate || !endDate) return ''

  const start = typeof startDate === 'string' ? new Date(startDate) : startDate
  const end = typeof endDate === 'string' ? new Date(endDate) : endDate

  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return ''

  const sameYear = start.getFullYear() === end.getFullYear()
  const sameMonth = sameYear && start.getMonth() === end.getMonth()

  if (sameMonth) {
    const startDay = start.getDate()
    const endFormatted = formatDateLong(end, locale)
    return `${startDay} - ${endFormatted}`
  } else if (sameYear) {
    const startFormatted = new Intl.DateTimeFormat(locale, {
      day: 'numeric',
      month: 'long'
    }).format(start)
    const endFormatted = formatDateLong(end, locale)
    return `${startFormatted} - ${endFormatted}`
  } else {
    return `${formatDateLong(start, locale)} - ${formatDateLong(end, locale)}`
  }
}

/**
 * Checks if a date is today
 */
export function isToday(date: Date | string | null | undefined): boolean {
  if (!date) return false

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return false

  const today = new Date()
  return (
    dateObj.getDate() === today.getDate() &&
    dateObj.getMonth() === today.getMonth() &&
    dateObj.getFullYear() === today.getFullYear()
  )
}

/**
 * Checks if a date is in the past
 */
export function isPast(date: Date | string | null | undefined): boolean {
  if (!date) return false

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return false

  return dateObj.getTime() < Date.now()
}

/**
 * Checks if a date is in the future
 */
export function isFuture(date: Date | string | null | undefined): boolean {
  if (!date) return false

  const dateObj = typeof date === 'string' ? new Date(date) : date

  if (Number.isNaN(dateObj.getTime())) return false

  return dateObj.getTime() > Date.now()
}

/**
 * Gets the start of a day (00:00:00)
 */
export function startOfDay(date: Date | string): Date {
  const dateObj = typeof date === 'string' ? new Date(date) : new Date(date)
  dateObj.setHours(0, 0, 0, 0)
  return dateObj
}

/**
 * Gets the end of a day (23:59:59.999)
 */
export function endOfDay(date: Date | string): Date {
  const dateObj = typeof date === 'string' ? new Date(date) : new Date(date)
  dateObj.setHours(23, 59, 59, 999)
  return dateObj
}

/**
 * Adds days to a date
 */
export function addDays(date: Date | string, days: number): Date {
  const dateObj = typeof date === 'string' ? new Date(date) : new Date(date)
  dateObj.setDate(dateObj.getDate() + days)
  return dateObj
}

/**
 * Gets difference in days between two dates
 */
export function diffInDays(
  dateA: Date | string,
  dateB: Date | string
): number {
  const a = typeof dateA === 'string' ? new Date(dateA) : dateA
  const b = typeof dateB === 'string' ? new Date(dateB) : dateB

  const diffTime = Math.abs(a.getTime() - b.getTime())
  return Math.floor(diffTime / (1000 * 60 * 60 * 24))
}
