import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { useApiError, type ApiError } from '~/composables/useApiError'
import { useNotificationToast, type Toast } from '~/composables/useNotificationToast'

describe('useApiError', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Clear all toasts before each test
    const { toasts, remove } = useNotificationToast()
    const toastIds = toasts.value.map((t: Toast) => t.id)
    toastIds.forEach((id: string) => remove(id))
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  describe('formatErrorMessage', () => {
    it('should format network error correctly', () => {
      const { formatErrorMessage } = useApiError()
      const error = new TypeError('Failed to fetch')

      const result = formatErrorMessage(error)

      expect(result.title).toBe('Blad polaczenia')
      expect(result.message).toContain('Nie udalo sie polaczyc z serwerem')
    })

    it('should format error with validation errors', () => {
      const { formatErrorMessage } = useApiError()
      const error: ApiError = {
        statusCode: 400,
        data: {
          errors: {
            email: ['Email is required', 'Email format is invalid'],
            password: ['Password is too short']
          }
        }
      }

      const result = formatErrorMessage(error)

      expect(result.message).toContain('Email is required')
      expect(result.message).toContain('Email format is invalid')
      expect(result.message).toContain('Password is too short')
      expect(result.statusCode).toBe(400)
    })

    it('should format error with data.message', () => {
      const { formatErrorMessage } = useApiError()
      const error: ApiError = {
        statusCode: 401,
        data: {
          message: 'Invalid credentials'
        }
      }

      const result = formatErrorMessage(error)

      expect(result.message).toBe('Invalid credentials')
      expect(result.statusCode).toBe(401)
    })

    it('should format error with data.title', () => {
      const { formatErrorMessage } = useApiError()
      const error: ApiError = {
        statusCode: 404,
        data: {
          title: 'Resource not found'
        }
      }

      const result = formatErrorMessage(error)

      expect(result.message).toBe('Resource not found')
    })

    it('should format error with direct message', () => {
      const { formatErrorMessage } = useApiError()
      const error: ApiError = {
        message: 'Something went wrong'
      }

      const result = formatErrorMessage(error)

      expect(result.message).toBe('Something went wrong')
    })

    it('should use HTTP status message as fallback', () => {
      const { formatErrorMessage } = useApiError()
      const error: ApiError = {
        statusCode: 500
      }

      const result = formatErrorMessage(error)

      expect(result.message).toBe('Blad serwera. Sprobuj pozniej')
      expect(result.title).toBe('Blad serwera')
    })

    it('should use default message when no information available', () => {
      const { formatErrorMessage } = useApiError()
      const error = {}

      const result = formatErrorMessage(error)

      expect(result.message).toBe('Wystapil nieoczekiwany blad. Sprobuj ponownie.')
    })

    it('should set title based on client error status code (4xx)', () => {
      const { formatErrorMessage } = useApiError()
      const error: ApiError = {
        statusCode: 404,
        message: 'Not found'
      }

      const result = formatErrorMessage(error)

      expect(result.title).toBe('Blad zadania')
    })

    it('should set title based on server error status code (5xx)', () => {
      const { formatErrorMessage } = useApiError()
      const error: ApiError = {
        statusCode: 503,
        message: 'Service unavailable'
      }

      const result = formatErrorMessage(error)

      expect(result.title).toBe('Blad serwera')
    })
  })

  describe('showErrorToast', () => {
    it('should show error toast with message', () => {
      const { showErrorToast } = useApiError()
      const toast = useNotificationToast()

      showErrorToast('Test error message')

      expect(toast.toasts.value).toHaveLength(1)
      expect(toast.toasts.value[0].type).toBe('error')
      expect(toast.toasts.value[0].message).toBe('Test error message')
    })

    it('should show error toast with custom title', () => {
      const { showErrorToast } = useApiError()
      const toast = useNotificationToast()

      showErrorToast('Test error', 'Custom Title')

      expect(toast.toasts.value).toHaveLength(1)
      expect(toast.toasts.value[0].title).toBe('Custom Title')
    })

    it('should return toast ID', () => {
      const { showErrorToast } = useApiError()

      const id = showErrorToast('Test error')

      expect(id).toContain('toast-')
    })
  })

  describe('showSuccessToast', () => {
    it('should show success toast with message', () => {
      const { showSuccessToast } = useApiError()
      const toast = useNotificationToast()

      showSuccessToast('Operation successful')

      expect(toast.toasts.value).toHaveLength(1)
      expect(toast.toasts.value[0].type).toBe('success')
      expect(toast.toasts.value[0].message).toBe('Operation successful')
    })

    it('should use default title "Sukces"', () => {
      const { showSuccessToast } = useApiError()
      const toast = useNotificationToast()

      showSuccessToast('Done')

      expect(toast.toasts.value[0].title).toBe('Sukces')
    })
  })

  describe('showWarningToast', () => {
    it('should show warning toast with message', () => {
      const { showWarningToast } = useApiError()
      const toast = useNotificationToast()

      showWarningToast('Warning message')

      expect(toast.toasts.value).toHaveLength(1)
      expect(toast.toasts.value[0].type).toBe('warning')
      expect(toast.toasts.value[0].message).toBe('Warning message')
    })

    it('should use default title "Uwaga"', () => {
      const { showWarningToast } = useApiError()
      const toast = useNotificationToast()

      showWarningToast('Warning')

      expect(toast.toasts.value[0].title).toBe('Uwaga')
    })
  })

  describe('showInfoToast', () => {
    it('should show info toast with message', () => {
      const { showInfoToast } = useApiError()
      const toast = useNotificationToast()

      showInfoToast('Info message')

      expect(toast.toasts.value).toHaveLength(1)
      expect(toast.toasts.value[0].type).toBe('info')
      expect(toast.toasts.value[0].message).toBe('Info message')
    })

    it('should use default title "Informacja"', () => {
      const { showInfoToast } = useApiError()
      const toast = useNotificationToast()

      showInfoToast('Info')

      expect(toast.toasts.value[0].title).toBe('Informacja')
    })
  })

  describe('showToast', () => {
    it('should show toast with specified type', () => {
      const { showToast } = useApiError()
      const toast = useNotificationToast()

      showToast('success', 'Title', 'Message')

      expect(toast.toasts.value).toHaveLength(1)
      expect(toast.toasts.value[0].type).toBe('success')
      expect(toast.toasts.value[0].title).toBe('Title')
      expect(toast.toasts.value[0].message).toBe('Message')
    })
  })

  describe('handleError', () => {
    it('should format error and show toast by default', () => {
      const { handleError } = useApiError()
      const toast = useNotificationToast()
      const error: ApiError = {
        statusCode: 400,
        message: 'Bad request'
      }

      const result = handleError(error)

      expect(result.message).toBe('Bad request')
      expect(toast.toasts.value).toHaveLength(1)
    })

    it('should not show toast when showToast is false', () => {
      const { handleError } = useApiError()
      const toast = useNotificationToast()
      const error: ApiError = {
        statusCode: 400,
        message: 'Bad request'
      }

      handleError(error, { showToast: false })

      expect(toast.toasts.value).toHaveLength(0)
    })

    it('should use custom message when provided', () => {
      const { handleError } = useApiError()
      const toast = useNotificationToast()
      const error: ApiError = {
        statusCode: 500,
        message: 'Server error'
      }

      handleError(error, { customMessage: 'Custom error message' })

      expect(toast.toasts.value[0].message).toBe('Custom error message')
    })

    it('should return formatted error object', () => {
      const { handleError } = useApiError()
      const error: ApiError = {
        statusCode: 403,
        message: 'Forbidden'
      }

      const result = handleError(error, { showToast: false })

      expect(result.statusCode).toBe(403)
      expect(result.message).toBe('Forbidden')
      expect(result.title).toBe('Blad zadania')
    })
  })

  describe('createErrorHandler', () => {
    it('should create a reusable error handler', () => {
      const { createErrorHandler } = useApiError()
      const toast = useNotificationToast()
      const errorHandler = createErrorHandler('TestContext')
      const error: ApiError = {
        statusCode: 500,
        message: 'Server error'
      }

      const result = errorHandler(error)

      expect(result.message).toBe('Server error')
      expect(toast.toasts.value).toHaveLength(1)
    })

    it('should not show toast when disabled', () => {
      const { createErrorHandler } = useApiError()
      const toast = useNotificationToast()
      const errorHandler = createErrorHandler('TestContext', { showToast: false })
      const error: ApiError = {
        statusCode: 500,
        message: 'Server error'
      }

      errorHandler(error)

      expect(toast.toasts.value).toHaveLength(0)
    })

    it('should rethrow error when configured', () => {
      const { createErrorHandler } = useApiError()
      const errorHandler = createErrorHandler('TestContext', { rethrow: true })
      const error = new Error('Test error')

      expect(() => errorHandler(error)).toThrow('Test error')
    })
  })

  describe('isHttpError', () => {
    it('should return true for matching status code', () => {
      const { isHttpError } = useApiError()
      const error: ApiError = { statusCode: 404 }

      expect(isHttpError(error, 404)).toBe(true)
    })

    it('should return false for non-matching status code', () => {
      const { isHttpError } = useApiError()
      const error: ApiError = { statusCode: 500 }

      expect(isHttpError(error, 404)).toBe(false)
    })

    it('should return false for error without status code', () => {
      const { isHttpError } = useApiError()
      const error = { message: 'Error' }

      expect(isHttpError(error, 404)).toBe(false)
    })
  })

  describe('isAuthError', () => {
    it('should return true for 401 status code', () => {
      const { isAuthError } = useApiError()
      const error: ApiError = { statusCode: 401 }

      expect(isAuthError(error)).toBe(true)
    })

    it('should return false for other status codes', () => {
      const { isAuthError } = useApiError()
      const error: ApiError = { statusCode: 403 }

      expect(isAuthError(error)).toBe(false)
    })
  })

  describe('isForbiddenError', () => {
    it('should return true for 403 status code', () => {
      const { isForbiddenError } = useApiError()
      const error: ApiError = { statusCode: 403 }

      expect(isForbiddenError(error)).toBe(true)
    })

    it('should return false for other status codes', () => {
      const { isForbiddenError } = useApiError()
      const error: ApiError = { statusCode: 401 }

      expect(isForbiddenError(error)).toBe(false)
    })
  })

  describe('isNotFoundError', () => {
    it('should return true for 404 status code', () => {
      const { isNotFoundError } = useApiError()
      const error: ApiError = { statusCode: 404 }

      expect(isNotFoundError(error)).toBe(true)
    })

    it('should return false for other status codes', () => {
      const { isNotFoundError } = useApiError()
      const error: ApiError = { statusCode: 500 }

      expect(isNotFoundError(error)).toBe(false)
    })
  })

  describe('isValidationError', () => {
    it('should return true for 400 status code', () => {
      const { isValidationError } = useApiError()
      const error: ApiError = { statusCode: 400 }

      expect(isValidationError(error)).toBe(true)
    })

    it('should return true for 422 status code', () => {
      const { isValidationError } = useApiError()
      const error: ApiError = { statusCode: 422 }

      expect(isValidationError(error)).toBe(true)
    })

    it('should return false for other status codes', () => {
      const { isValidationError } = useApiError()
      const error: ApiError = { statusCode: 500 }

      expect(isValidationError(error)).toBe(false)
    })
  })

  describe('isServerError', () => {
    it('should return true for 500 status code', () => {
      const { isServerError } = useApiError()
      const error: ApiError = { statusCode: 500 }

      expect(isServerError(error)).toBe(true)
    })

    it('should return true for 502 status code', () => {
      const { isServerError } = useApiError()
      const error: ApiError = { statusCode: 502 }

      expect(isServerError(error)).toBe(true)
    })

    it('should return true for 503 status code', () => {
      const { isServerError } = useApiError()
      const error: ApiError = { statusCode: 503 }

      expect(isServerError(error)).toBe(true)
    })

    it('should return false for 4xx status codes', () => {
      const { isServerError } = useApiError()
      const error: ApiError = { statusCode: 400 }

      expect(isServerError(error)).toBe(false)
    })

    it('should return false for error without status code', () => {
      const { isServerError } = useApiError()
      const error = { message: 'Error' }

      expect(isServerError(error)).toBe(false)
    })
  })
})
