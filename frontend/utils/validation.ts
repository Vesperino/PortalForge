/**
 * Common validation utilities for the PortalForge application
 */

/**
 * Email validation regex
 */
const EMAIL_REGEX = /^[\w!#$%&'*+./=?^`{|}~-]+@[\da-z](?:[\da-z-]{0,61}[\da-z])?(?:\.[\da-z](?:[\da-z-]{0,61}[\da-z])?)+$/i

/**
 * Phone number validation regex (Polish format)
 */
const PHONE_REGEX = /^(?:\+48)?[\s-]?(?:\d{3}[\s-]?){2}\d{3}$/

/**
 * URL validation regex
 */
const URL_REGEX = /^https?:\/\/.+\..+/

/**
 * Validates an email address
 */
export function isValidEmail(email: string | null | undefined): boolean {
  if (!email) return false
  return EMAIL_REGEX.test(email.trim())
}

/**
 * Validates a phone number (Polish format)
 */
export function isValidPhone(phone: string | null | undefined): boolean {
  if (!phone) return false
  return PHONE_REGEX.test(phone.trim())
}

/**
 * Validates a URL
 */
export function isValidUrl(url: string | null | undefined): boolean {
  if (!url) return false
  return URL_REGEX.test(url.trim())
}

/**
 * Checks if a string is not empty
 */
export function isNotEmpty(value: string | null | undefined): boolean {
  return value !== null && value !== undefined && value.trim().length > 0
}

/**
 * Checks if a string has minimum length
 */
export function hasMinLength(
  value: string | null | undefined,
  minLength: number
): boolean {
  if (!value) return false
  return value.trim().length >= minLength
}

/**
 * Checks if a string has maximum length
 */
export function hasMaxLength(
  value: string | null | undefined,
  maxLength: number
): boolean {
  if (!value) return true
  return value.trim().length <= maxLength
}

/**
 * Checks if a string length is within range
 */
export function hasLengthInRange(
  value: string | null | undefined,
  minLength: number,
  maxLength: number
): boolean {
  return hasMinLength(value, minLength) && hasMaxLength(value, maxLength)
}

/**
 * Validates password strength
 * Returns true if password meets minimum requirements
 */
export function isValidPassword(
  password: string | null | undefined,
  options?: {
    minLength?: number
    requireUppercase?: boolean
    requireLowercase?: boolean
    requireNumbers?: boolean
    requireSpecialChars?: boolean
  }
): boolean {
  if (!password) return false

  const {
    minLength = 8,
    requireUppercase = true,
    requireLowercase = true,
    requireNumbers = true,
    requireSpecialChars = false
  } = options || {}

  if (password.length < minLength) return false
  if (requireUppercase && !/[A-Z]/.test(password)) return false
  if (requireLowercase && !/[a-z]/.test(password)) return false
  if (requireNumbers && !/\d/.test(password)) return false
  if (requireSpecialChars && !/[!@#$%^&*(),.?":{}|<>]/.test(password)) return false

  return true
}

/**
 * Gets password strength score (0-4)
 */
export function getPasswordStrength(password: string | null | undefined): number {
  if (!password) return 0

  let score = 0

  if (password.length >= 8) score++
  if (password.length >= 12) score++
  if (/[A-Z]/.test(password) && /[a-z]/.test(password)) score++
  if (/\d/.test(password)) score++
  if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) score++

  return Math.min(score, 4)
}

/**
 * Gets password strength label
 */
export function getPasswordStrengthLabel(strength: number): string {
  const labels: Record<number, string> = {
    0: 'Brak',
    1: 'Slabe',
    2: 'Srednie',
    3: 'Dobre',
    4: 'Silne'
  }
  return labels[strength] || 'Nieznane'
}

/**
 * Validates that two values match (e.g., password confirmation)
 */
export function valuesMatch(
  value1: string | null | undefined,
  value2: string | null | undefined
): boolean {
  return value1 === value2
}

/**
 * Validates a number is within range
 */
export function isInRange(
  value: number | null | undefined,
  min: number,
  max: number
): boolean {
  if (value === null || value === undefined) return false
  return value >= min && value <= max
}

/**
 * Validates a number is positive
 */
export function isPositive(value: number | null | undefined): boolean {
  if (value === null || value === undefined) return false
  return value > 0
}

/**
 * Validates a number is non-negative
 */
export function isNonNegative(value: number | null | undefined): boolean {
  if (value === null || value === undefined) return false
  return value >= 0
}

/**
 * Validates a date is not in the past
 */
export function isNotPastDate(date: Date | string | null | undefined): boolean {
  if (!date) return false

  const dateObj = typeof date === 'string' ? new Date(date) : date
  if (Number.isNaN(dateObj.getTime())) return false

  const today = new Date()
  today.setHours(0, 0, 0, 0)

  return dateObj >= today
}

/**
 * Validates a date is in the future
 */
export function isFutureDate(date: Date | string | null | undefined): boolean {
  if (!date) return false

  const dateObj = typeof date === 'string' ? new Date(date) : date
  if (Number.isNaN(dateObj.getTime())) return false

  return dateObj > new Date()
}

/**
 * Validates that end date is after start date
 */
export function isEndDateAfterStart(
  startDate: Date | string | null | undefined,
  endDate: Date | string | null | undefined
): boolean {
  if (!startDate || !endDate) return false

  const start = typeof startDate === 'string' ? new Date(startDate) : startDate
  const end = typeof endDate === 'string' ? new Date(endDate) : endDate

  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return false

  return end > start
}

/**
 * Validates file size (in bytes)
 */
export function isValidFileSize(
  sizeInBytes: number,
  maxSizeInMB: number
): boolean {
  const maxSizeInBytes = maxSizeInMB * 1024 * 1024
  return sizeInBytes <= maxSizeInBytes
}

/**
 * Validates file type against allowed types
 */
export function isValidFileType(
  fileType: string,
  allowedTypes: string[]
): boolean {
  return allowedTypes.includes(fileType.toLowerCase())
}

/**
 * Common file type groups
 */
export const FILE_TYPE_GROUPS = {
  images: ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'image/svg+xml'],
  documents: [
    'application/pdf',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'application/vnd.ms-excel',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'text/plain'
  ],
  spreadsheets: [
    'application/vnd.ms-excel',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'text/csv'
  ]
} as const

/**
 * Sanitizes a string by removing dangerous characters
 */
export function sanitizeInput(value: string): string {
  return value
    .replace(/[<>]/g, '')
    .trim()
}

/**
 * Validates a Polish postal code (XX-XXX)
 */
export function isValidPostalCode(postalCode: string | null | undefined): boolean {
  if (!postalCode) return false
  return /^\d{2}-\d{3}$/.test(postalCode.trim())
}

/**
 * Validates a Polish NIP number
 */
export function isValidNIP(nip: string | null | undefined): boolean {
  if (!nip) return false

  const cleanNip = nip.replace(/[- ]/g, '')
  if (!/^\d{10}$/.test(cleanNip)) return false

  const weights = [6, 5, 7, 2, 3, 4, 5, 6, 7]
  let sum = 0

  for (let i = 0; i < 9; i++) {
    sum += Number.parseInt(cleanNip[i]) * weights[i]
  }

  const checkDigit = sum % 11
  return checkDigit === Number.parseInt(cleanNip[9])
}

/**
 * Creates a validation result object
 */
export interface ValidationResult {
  valid: boolean
  error?: string
}

/**
 * Validates a value against a set of rules
 */
export function validate(
  value: unknown,
  rules: Array<{
    validator: (val: unknown) => boolean
    message: string
  }>
): ValidationResult {
  for (const rule of rules) {
    if (!rule.validator(value)) {
      return { valid: false, error: rule.message }
    }
  }
  return { valid: true }
}
