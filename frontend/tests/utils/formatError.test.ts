import { describe, it, expect } from 'vitest'
import {
  HTTP_ERROR_MESSAGES,
  getHttpErrorMessage,
  extractErrorMessage,
  formatError,
  formatValidationErrors,
  getFieldErrors,
  hasValidationErrors,
  formatNetworkError,
  isNetworkError,
  hasStatusCode,
  isApiError
} from '~/utils/formatError'

describe('formatError utilities', () => {
  describe('HTTP_ERROR_MESSAGES', () => {
    it('should contain common HTTP status codes', () => {
      expect(HTTP_ERROR_MESSAGES[400]).toBeDefined()
      expect(HTTP_ERROR_MESSAGES[401]).toBeDefined()
      expect(HTTP_ERROR_MESSAGES[403]).toBeDefined()
      expect(HTTP_ERROR_MESSAGES[404]).toBeDefined()
      expect(HTTP_ERROR_MESSAGES[500]).toBeDefined()
    })
  })

  describe('getHttpErrorMessage', () => {
    it('should return message for known status code', () => {
      expect(getHttpErrorMessage(400)).toBe('Nieprawidlowe dane wejsciowe')
      expect(getHttpErrorMessage(401)).toBe('Sesja wygasla. Zaloguj sie ponownie')
      expect(getHttpErrorMessage(403)).toBe('Brak uprawnien do wykonania tej operacji')
      expect(getHttpErrorMessage(404)).toBe('Nie znaleziono zasobu')
      expect(getHttpErrorMessage(500)).toBe('Blad serwera. Sprobuj pozniej')
    })

    it('should return default message for unknown status code', () => {
      expect(getHttpErrorMessage(999)).toBe('Wystapil nieoczekiwany blad')
    })
  })

  describe('extractErrorMessage', () => {
    it('should return empty string for null/undefined', () => {
      expect(extractErrorMessage(null)).toBe('')
      expect(extractErrorMessage(undefined)).toBe('')
    })

    it('should return string errors directly', () => {
      expect(extractErrorMessage('Simple error')).toBe('Simple error')
    })

    it('should extract message from Error object', () => {
      const error = new Error('Error message')
      expect(extractErrorMessage(error)).toBe('Error message')
    })

    it('should extract validation errors', () => {
      const error = {
        data: {
          errors: {
            email: ['Email is required'],
            password: ['Password is too short']
          }
        }
      }

      const result = extractErrorMessage(error)

      expect(result).toContain('Email is required')
      expect(result).toContain('Password is too short')
    })

    it('should extract message from data.message', () => {
      const error = { data: { message: 'Data message' } }
      expect(extractErrorMessage(error)).toBe('Data message')
    })

    it('should extract message from data.title', () => {
      const error = { data: { title: 'Data title' } }
      expect(extractErrorMessage(error)).toBe('Data title')
    })

    it('should extract direct message', () => {
      const error = { message: 'Direct message' }
      expect(extractErrorMessage(error)).toBe('Direct message')
    })

    it('should use HTTP status message as fallback', () => {
      const error = { statusCode: 500 }
      expect(extractErrorMessage(error)).toBe('Blad serwera. Sprobuj pozniej')
    })

    it('should return default message when no information available', () => {
      const error = {}
      expect(extractErrorMessage(error)).toBe('Wystapil nieoczekiwany blad')
    })
  })

  describe('formatError', () => {
    it('should format error with client error status (4xx)', () => {
      const error = { statusCode: 400, message: 'Bad request' }
      const result = formatError(error)

      expect(result.title).toBe('Blad zadania')
      expect(result.message).toBe('Bad request')
      expect(result.statusCode).toBe(400)
    })

    it('should format error with server error status (5xx)', () => {
      const error = { statusCode: 500, message: 'Server error' }
      const result = formatError(error)

      expect(result.title).toBe('Blad serwera')
      expect(result.message).toBe('Server error')
      expect(result.statusCode).toBe(500)
    })

    it('should use default title when no status code', () => {
      const error = { message: 'Unknown error' }
      const result = formatError(error)

      expect(result.title).toBe('Blad')
    })

    it('should include validation errors when present', () => {
      const error = {
        statusCode: 422,
        data: {
          errors: {
            field1: ['Error 1'],
            field2: ['Error 2']
          }
        }
      }

      const result = formatError(error)

      expect(result.validationErrors).toEqual({
        field1: ['Error 1'],
        field2: ['Error 2']
      })
    })
  })

  describe('formatValidationErrors', () => {
    it('should return empty string for null/undefined', () => {
      expect(formatValidationErrors(null)).toBe('')
      expect(formatValidationErrors(undefined)).toBe('')
    })

    it('should format validation errors into single string', () => {
      const errors = {
        email: ['Email is required', 'Invalid format'],
        password: ['Too short']
      }

      const result = formatValidationErrors(errors)

      expect(result).toContain('Email is required')
      expect(result).toContain('Invalid format')
      expect(result).toContain('Too short')
    })

    it('should handle empty errors object', () => {
      expect(formatValidationErrors({})).toBe('')
    })

    it('should handle empty arrays', () => {
      const errors = { field: [] }
      expect(formatValidationErrors(errors)).toBe('')
    })
  })

  describe('getFieldErrors', () => {
    it('should return empty array for null/undefined errors', () => {
      expect(getFieldErrors(null, 'field')).toEqual([])
      expect(getFieldErrors(undefined, 'field')).toEqual([])
    })

    it('should return errors for specific field', () => {
      const errors = {
        email: ['Error 1', 'Error 2'],
        password: ['Error 3']
      }

      expect(getFieldErrors(errors, 'email')).toEqual(['Error 1', 'Error 2'])
      expect(getFieldErrors(errors, 'password')).toEqual(['Error 3'])
    })

    it('should return empty array for non-existent field', () => {
      const errors = { email: ['Error'] }
      expect(getFieldErrors(errors, 'password')).toEqual([])
    })
  })

  describe('hasValidationErrors', () => {
    it('should return false for null/undefined', () => {
      expect(hasValidationErrors(null)).toBe(false)
      expect(hasValidationErrors(undefined)).toBe(false)
    })

    it('should return true when errors exist', () => {
      const errors = { field: ['Error'] }
      expect(hasValidationErrors(errors)).toBe(true)
    })

    it('should return false for empty errors object', () => {
      expect(hasValidationErrors({})).toBe(false)
    })

    it('should return false when all arrays are empty', () => {
      const errors = { field1: [], field2: [] }
      expect(hasValidationErrors(errors)).toBe(false)
    })
  })

  describe('formatNetworkError', () => {
    it('should return network error object', () => {
      const result = formatNetworkError()

      expect(result.title).toBe('Blad polaczenia')
      expect(result.message).toContain('Nie udalo sie polaczyc')
    })
  })

  describe('isNetworkError', () => {
    it('should return true for TypeError with "Failed to fetch" message', () => {
      const error = new TypeError('Failed to fetch')
      expect(isNetworkError(error)).toBe(true)
    })

    it('should return false for other TypeError messages', () => {
      const error = new TypeError('Cannot read property')
      expect(isNetworkError(error)).toBe(false)
    })

    it('should return false for non-TypeError', () => {
      const error = new Error('Failed to fetch')
      expect(isNetworkError(error)).toBe(false)
    })
  })

  describe('hasStatusCode', () => {
    it('should return true for matching status code', () => {
      const error = { statusCode: 404 }
      expect(hasStatusCode(error, 404)).toBe(true)
    })

    it('should return false for non-matching status code', () => {
      const error = { statusCode: 500 }
      expect(hasStatusCode(error, 404)).toBe(false)
    })

    it('should return false for error without statusCode', () => {
      const error = { message: 'Error' }
      expect(hasStatusCode(error, 404)).toBe(false)
    })
  })

  describe('isApiError', () => {
    it('should return true for object with statusCode', () => {
      expect(isApiError({ statusCode: 400 })).toBe(true)
    })

    it('should return true for object with message', () => {
      expect(isApiError({ message: 'Error' })).toBe(true)
    })

    it('should return true for object with data', () => {
      expect(isApiError({ data: {} })).toBe(true)
    })

    it('should return false for null', () => {
      expect(isApiError(null)).toBe(false)
    })

    it('should return false for undefined', () => {
      expect(isApiError(undefined)).toBe(false)
    })

    it('should return false for primitive values', () => {
      expect(isApiError('string')).toBe(false)
      expect(isApiError(123)).toBe(false)
      expect(isApiError(true)).toBe(false)
    })

    it('should return false for empty object', () => {
      expect(isApiError({})).toBe(false)
    })
  })
})
