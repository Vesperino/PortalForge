import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import {
  isValidEmail,
  isValidPhone,
  isValidUrl,
  isNotEmpty,
  hasMinLength,
  hasMaxLength,
  hasLengthInRange,
  isValidPassword,
  getPasswordStrength,
  getPasswordStrengthLabel,
  valuesMatch,
  isInRange,
  isPositive,
  isNonNegative,
  isNotPastDate,
  isFutureDate,
  isEndDateAfterStart,
  isValidFileSize,
  isValidFileType,
  FILE_TYPE_GROUPS,
  sanitizeInput,
  isValidPostalCode,
  isValidNIP,
  validate
} from '~/utils/validation'

describe('validation utilities', () => {
  describe('isValidEmail', () => {
    it('should return true for valid emails', () => {
      expect(isValidEmail('test@example.com')).toBe(true)
      expect(isValidEmail('user.name@domain.co.uk')).toBe(true)
      expect(isValidEmail('user+tag@example.org')).toBe(true)
    })

    it('should return false for invalid emails', () => {
      expect(isValidEmail('invalid')).toBe(false)
      expect(isValidEmail('invalid@')).toBe(false)
      expect(isValidEmail('@domain.com')).toBe(false)
      expect(isValidEmail('user@.com')).toBe(false)
    })

    it('should return false for null/undefined/empty', () => {
      expect(isValidEmail(null)).toBe(false)
      expect(isValidEmail(undefined)).toBe(false)
      expect(isValidEmail('')).toBe(false)
    })

    it('should trim whitespace before validation', () => {
      expect(isValidEmail('  test@example.com  ')).toBe(true)
    })
  })

  describe('isValidPhone', () => {
    it('should return true for valid Polish phone numbers', () => {
      expect(isValidPhone('123456789')).toBe(true)
      expect(isValidPhone('123 456 789')).toBe(true)
      expect(isValidPhone('123-456-789')).toBe(true)
      expect(isValidPhone('+48123456789')).toBe(true)
      expect(isValidPhone('+48 123 456 789')).toBe(true)
    })

    it('should return false for invalid phone numbers', () => {
      expect(isValidPhone('12345')).toBe(false)
      expect(isValidPhone('abcdefghi')).toBe(false)
    })

    it('should return false for null/undefined/empty', () => {
      expect(isValidPhone(null)).toBe(false)
      expect(isValidPhone(undefined)).toBe(false)
      expect(isValidPhone('')).toBe(false)
    })
  })

  describe('isValidUrl', () => {
    it('should return true for valid URLs', () => {
      expect(isValidUrl('http://example.com')).toBe(true)
      expect(isValidUrl('https://example.com')).toBe(true)
      expect(isValidUrl('https://www.example.com/path')).toBe(true)
    })

    it('should return false for invalid URLs', () => {
      expect(isValidUrl('example.com')).toBe(false)
      expect(isValidUrl('ftp://example.com')).toBe(false)
      expect(isValidUrl('not-a-url')).toBe(false)
    })

    it('should return false for null/undefined/empty', () => {
      expect(isValidUrl(null)).toBe(false)
      expect(isValidUrl(undefined)).toBe(false)
      expect(isValidUrl('')).toBe(false)
    })
  })

  describe('isNotEmpty', () => {
    it('should return true for non-empty strings', () => {
      expect(isNotEmpty('hello')).toBe(true)
      expect(isNotEmpty('  hello  ')).toBe(true)
    })

    it('should return false for empty/whitespace strings', () => {
      expect(isNotEmpty('')).toBe(false)
      expect(isNotEmpty('   ')).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(isNotEmpty(null)).toBe(false)
      expect(isNotEmpty(undefined)).toBe(false)
    })
  })

  describe('hasMinLength', () => {
    it('should return true when value meets minimum length', () => {
      expect(hasMinLength('hello', 5)).toBe(true)
      expect(hasMinLength('hello world', 5)).toBe(true)
    })

    it('should return false when value is too short', () => {
      expect(hasMinLength('hi', 5)).toBe(false)
    })

    it('should trim whitespace before checking', () => {
      expect(hasMinLength('  hi  ', 5)).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(hasMinLength(null, 1)).toBe(false)
      expect(hasMinLength(undefined, 1)).toBe(false)
    })
  })

  describe('hasMaxLength', () => {
    it('should return true when value is within maximum length', () => {
      expect(hasMaxLength('hello', 10)).toBe(true)
      expect(hasMaxLength('hi', 5)).toBe(true)
    })

    it('should return false when value exceeds maximum length', () => {
      expect(hasMaxLength('hello world', 5)).toBe(false)
    })

    it('should return true for null/undefined', () => {
      expect(hasMaxLength(null, 10)).toBe(true)
      expect(hasMaxLength(undefined, 10)).toBe(true)
    })
  })

  describe('hasLengthInRange', () => {
    it('should return true when value is within range', () => {
      expect(hasLengthInRange('hello', 3, 10)).toBe(true)
    })

    it('should return false when value is outside range', () => {
      expect(hasLengthInRange('hi', 3, 10)).toBe(false)
      expect(hasLengthInRange('hello world!', 3, 10)).toBe(false)
    })
  })

  describe('isValidPassword', () => {
    it('should return true for password meeting default requirements', () => {
      expect(isValidPassword('Password1')).toBe(true)
      expect(isValidPassword('StrongPass123')).toBe(true)
    })

    it('should return false for password not meeting requirements', () => {
      expect(isValidPassword('short')).toBe(false)
      expect(isValidPassword('nouppercase1')).toBe(false)
      expect(isValidPassword('NOLOWERCASE1')).toBe(false)
      expect(isValidPassword('NoNumbers')).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(isValidPassword(null)).toBe(false)
      expect(isValidPassword(undefined)).toBe(false)
    })

    it('should respect custom options', () => {
      expect(isValidPassword('simple', { minLength: 4, requireUppercase: false, requireNumbers: false })).toBe(true)
      expect(isValidPassword('Password1!', { requireSpecialChars: true })).toBe(true)
      expect(isValidPassword('Password1', { requireSpecialChars: true })).toBe(false)
    })
  })

  describe('getPasswordStrength', () => {
    it('should return 0 for null/undefined', () => {
      expect(getPasswordStrength(null)).toBe(0)
      expect(getPasswordStrength(undefined)).toBe(0)
    })

    it('should return higher score for stronger passwords', () => {
      expect(getPasswordStrength('short')).toBeLessThan(getPasswordStrength('Password123!'))
    })

    it('should return max 4', () => {
      expect(getPasswordStrength('VeryStrongPassword123!')).toBe(4)
    })
  })

  describe('getPasswordStrengthLabel', () => {
    it('should return correct labels', () => {
      expect(getPasswordStrengthLabel(0)).toBe('Brak')
      expect(getPasswordStrengthLabel(1)).toBe('Slabe')
      expect(getPasswordStrengthLabel(2)).toBe('Srednie')
      expect(getPasswordStrengthLabel(3)).toBe('Dobre')
      expect(getPasswordStrengthLabel(4)).toBe('Silne')
    })

    it('should return Nieznane for invalid values', () => {
      expect(getPasswordStrengthLabel(5)).toBe('Nieznane')
      expect(getPasswordStrengthLabel(-1)).toBe('Nieznane')
    })
  })

  describe('valuesMatch', () => {
    it('should return true for matching values', () => {
      expect(valuesMatch('password', 'password')).toBe(true)
    })

    it('should return false for non-matching values', () => {
      expect(valuesMatch('password', 'different')).toBe(false)
    })

    it('should handle null/undefined', () => {
      expect(valuesMatch(null, null)).toBe(true)
      expect(valuesMatch(undefined, undefined)).toBe(true)
      expect(valuesMatch(null, 'value')).toBe(false)
    })
  })

  describe('isInRange', () => {
    it('should return true when value is within range', () => {
      expect(isInRange(5, 1, 10)).toBe(true)
      expect(isInRange(1, 1, 10)).toBe(true)
      expect(isInRange(10, 1, 10)).toBe(true)
    })

    it('should return false when value is outside range', () => {
      expect(isInRange(0, 1, 10)).toBe(false)
      expect(isInRange(11, 1, 10)).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(isInRange(null, 1, 10)).toBe(false)
      expect(isInRange(undefined, 1, 10)).toBe(false)
    })
  })

  describe('isPositive', () => {
    it('should return true for positive numbers', () => {
      expect(isPositive(1)).toBe(true)
      expect(isPositive(0.1)).toBe(true)
    })

    it('should return false for zero and negative numbers', () => {
      expect(isPositive(0)).toBe(false)
      expect(isPositive(-1)).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(isPositive(null)).toBe(false)
      expect(isPositive(undefined)).toBe(false)
    })
  })

  describe('isNonNegative', () => {
    it('should return true for zero and positive numbers', () => {
      expect(isNonNegative(0)).toBe(true)
      expect(isNonNegative(1)).toBe(true)
    })

    it('should return false for negative numbers', () => {
      expect(isNonNegative(-1)).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(isNonNegative(null)).toBe(false)
      expect(isNonNegative(undefined)).toBe(false)
    })
  })

  describe('isNotPastDate', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2026-01-15T12:00:00'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('should return true for today and future dates', () => {
      expect(isNotPastDate('2026-01-15')).toBe(true)
      expect(isNotPastDate('2026-01-16')).toBe(true)
    })

    it('should return false for past dates', () => {
      expect(isNotPastDate('2026-01-14')).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(isNotPastDate(null)).toBe(false)
      expect(isNotPastDate(undefined)).toBe(false)
    })

    it('should return false for invalid dates', () => {
      expect(isNotPastDate('invalid')).toBe(false)
    })
  })

  describe('isFutureDate', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2026-01-15T12:00:00'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('should return true for future dates', () => {
      expect(isFutureDate('2026-01-16')).toBe(true)
    })

    it('should return false for today and past dates', () => {
      expect(isFutureDate('2026-01-15T11:00:00')).toBe(false)
      expect(isFutureDate('2026-01-14')).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(isFutureDate(null)).toBe(false)
      expect(isFutureDate(undefined)).toBe(false)
    })
  })

  describe('isEndDateAfterStart', () => {
    it('should return true when end is after start', () => {
      expect(isEndDateAfterStart('2026-01-15', '2026-01-20')).toBe(true)
    })

    it('should return false when end is before or same as start', () => {
      expect(isEndDateAfterStart('2026-01-20', '2026-01-15')).toBe(false)
      expect(isEndDateAfterStart('2026-01-15', '2026-01-15')).toBe(false)
    })

    it('should return false for null/undefined', () => {
      expect(isEndDateAfterStart(null, '2026-01-20')).toBe(false)
      expect(isEndDateAfterStart('2026-01-15', null)).toBe(false)
    })

    it('should return false for invalid dates', () => {
      expect(isEndDateAfterStart('invalid', '2026-01-20')).toBe(false)
      expect(isEndDateAfterStart('2026-01-15', 'invalid')).toBe(false)
    })
  })

  describe('isValidFileSize', () => {
    it('should return true when file size is within limit', () => {
      const fiveMB = 5 * 1024 * 1024
      expect(isValidFileSize(fiveMB, 10)).toBe(true)
    })

    it('should return false when file size exceeds limit', () => {
      const fifteenMB = 15 * 1024 * 1024
      expect(isValidFileSize(fifteenMB, 10)).toBe(false)
    })
  })

  describe('isValidFileType', () => {
    it('should return true for allowed file types', () => {
      expect(isValidFileType('image/jpeg', FILE_TYPE_GROUPS.images)).toBe(true)
      expect(isValidFileType('application/pdf', FILE_TYPE_GROUPS.documents)).toBe(true)
    })

    it('should return false for disallowed file types', () => {
      expect(isValidFileType('application/exe', FILE_TYPE_GROUPS.images)).toBe(false)
    })

    it('should be case insensitive', () => {
      expect(isValidFileType('IMAGE/JPEG', ['image/jpeg'])).toBe(true)
    })
  })

  describe('FILE_TYPE_GROUPS', () => {
    it('should contain image types', () => {
      expect(FILE_TYPE_GROUPS.images).toContain('image/jpeg')
      expect(FILE_TYPE_GROUPS.images).toContain('image/png')
    })

    it('should contain document types', () => {
      expect(FILE_TYPE_GROUPS.documents).toContain('application/pdf')
    })

    it('should contain spreadsheet types', () => {
      expect(FILE_TYPE_GROUPS.spreadsheets).toContain('text/csv')
    })
  })

  describe('sanitizeInput', () => {
    it('should remove < and > characters', () => {
      expect(sanitizeInput('<script>alert("xss")</script>')).toBe('scriptalert("xss")/script')
    })

    it('should trim whitespace', () => {
      expect(sanitizeInput('  hello  ')).toBe('hello')
    })

    it('should handle normal input', () => {
      expect(sanitizeInput('normal text')).toBe('normal text')
    })
  })

  describe('isValidPostalCode', () => {
    it('should return true for valid Polish postal codes', () => {
      expect(isValidPostalCode('00-001')).toBe(true)
      expect(isValidPostalCode('12-345')).toBe(true)
    })

    it('should return false for invalid postal codes', () => {
      expect(isValidPostalCode('12345')).toBe(false)
      expect(isValidPostalCode('123-45')).toBe(false)
      expect(isValidPostalCode('ab-cde')).toBe(false)
    })

    it('should return false for null/undefined/empty', () => {
      expect(isValidPostalCode(null)).toBe(false)
      expect(isValidPostalCode(undefined)).toBe(false)
      expect(isValidPostalCode('')).toBe(false)
    })
  })

  describe('isValidNIP', () => {
    it('should return true for valid NIP numbers', () => {
      expect(isValidNIP('1234563218')).toBe(true)
      expect(isValidNIP('123-456-32-18')).toBe(true)
      expect(isValidNIP('123 456 32 18')).toBe(true)
    })

    it('should return false for invalid NIP numbers', () => {
      expect(isValidNIP('1234567890')).toBe(false)
      expect(isValidNIP('123456')).toBe(false)
      expect(isValidNIP('abcdefghij')).toBe(false)
    })

    it('should return false for null/undefined/empty', () => {
      expect(isValidNIP(null)).toBe(false)
      expect(isValidNIP(undefined)).toBe(false)
      expect(isValidNIP('')).toBe(false)
    })
  })

  describe('validate', () => {
    it('should return valid result when all rules pass', () => {
      const result = validate('test@example.com', [
        { validator: (v) => typeof v === 'string', message: 'Must be string' },
        { validator: (v) => (v as string).includes('@'), message: 'Must contain @' }
      ])

      expect(result.valid).toBe(true)
      expect(result.error).toBeUndefined()
    })

    it('should return first error when validation fails', () => {
      const result = validate('', [
        { validator: (v) => typeof v === 'string' && v.length > 0, message: 'Required' },
        { validator: (v) => (v as string).includes('@'), message: 'Must contain @' }
      ])

      expect(result.valid).toBe(false)
      expect(result.error).toBe('Required')
    })

    it('should return valid for empty rules array', () => {
      const result = validate('anything', [])

      expect(result.valid).toBe(true)
    })
  })
})
