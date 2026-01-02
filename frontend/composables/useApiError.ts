import type { ToastType } from './useNotificationToast'

export interface ApiError {
  statusCode?: number
  message?: string
  data?: {
    message?: string
    errors?: Record<string, string[]>
    title?: string
  }
}

export interface FormattedError {
  title: string
  message: string
  statusCode?: number
}

const HTTP_STATUS_MESSAGES: Record<number, string> = {
  400: 'Nieprawidlowe dane wejsciowe',
  401: 'Sesja wygasla. Zaloguj sie ponownie',
  403: 'Brak uprawnien do wykonania tej operacji',
  404: 'Nie znaleziono zasobu',
  409: 'Konflikt danych. Sprobuj ponownie',
  422: 'Dane nie przeszly walidacji',
  429: 'Zbyt wiele prob. Poczekaj chwile',
  500: 'Blad serwera. Sprobuj pozniej',
  502: 'Serwer chwilowo niedostepny',
  503: 'Usluga chwilowo niedostepna',
  504: 'Przekroczono czas oczekiwania'
}

export function useApiError() {
  const toast = useNotificationToast()

  /**
   * Formats an API error into a user-friendly message
   */
  function formatErrorMessage(error: unknown): FormattedError {
    const apiError = error as ApiError

    // Handle network errors
    if (error instanceof TypeError && error.message === 'Failed to fetch') {
      return {
        title: 'Blad polaczenia',
        message: 'Nie udalo sie polaczyc z serwerem. Sprawdz polaczenie internetowe.'
      }
    }

    // Get status code
    const statusCode = apiError?.statusCode

    // Try to extract message from various error formats
    let message = ''

    // Check for validation errors
    if (apiError?.data?.errors) {
      const errorMessages = Object.values(apiError.data.errors).flat()
      message = errorMessages.join('. ')
    }

    // Check for direct message in data
    if (!message && apiError?.data?.message) {
      message = apiError.data.message
    }

    // Check for title in data
    if (!message && apiError?.data?.title) {
      message = apiError.data.title
    }

    // Check for direct message
    if (!message && apiError?.message) {
      message = apiError.message
    }

    // Use HTTP status message as fallback
    if (!message && statusCode && HTTP_STATUS_MESSAGES[statusCode]) {
      message = HTTP_STATUS_MESSAGES[statusCode]
    }

    // Final fallback
    if (!message) {
      message = 'Wystapil nieoczekiwany blad. Sprobuj ponownie.'
    }

    // Determine title based on status code
    let title = 'Blad'
    if (statusCode) {
      if (statusCode >= 400 && statusCode < 500) {
        title = 'Blad zadania'
      } else if (statusCode >= 500) {
        title = 'Blad serwera'
      }
    }

    return {
      title,
      message,
      statusCode
    }
  }

  /**
   * Shows an error toast notification
   */
  function showErrorToast(
    message: string,
    title = 'Blad',
    duration?: number
  ): string {
    return toast.error(title, message, duration)
  }

  /**
   * Shows a success toast notification
   */
  function showSuccessToast(
    message: string,
    title = 'Sukces',
    duration?: number
  ): string {
    return toast.success(title, message, duration)
  }

  /**
   * Shows a warning toast notification
   */
  function showWarningToast(
    message: string,
    title = 'Uwaga',
    duration?: number
  ): string {
    return toast.warning(title, message, duration)
  }

  /**
   * Shows an info toast notification
   */
  function showInfoToast(
    message: string,
    title = 'Informacja',
    duration?: number
  ): string {
    return toast.info(title, message, duration)
  }

  /**
   * Shows a toast notification with specified type
   */
  function showToast(
    type: ToastType,
    title: string,
    message?: string,
    duration?: number
  ): string {
    return toast.show({ type, title, message, duration })
  }

  /**
   * Handles an API error by formatting it and showing a toast
   * Returns the formatted error for further processing
   */
  function handleError(
    error: unknown,
    options?: {
      showToast?: boolean
      customMessage?: string
      duration?: number
    }
  ): FormattedError {
    const { showToast: shouldShowToast = true, customMessage, duration } = options || {}

    const formattedError = formatErrorMessage(error)

    if (shouldShowToast) {
      const message = customMessage || formattedError.message
      showErrorToast(message, formattedError.title, duration)
    }

    // Log error for debugging in development
    if (process.env.NODE_ENV === 'development') {
      console.error('[API Error]', error)
    }

    return formattedError
  }

  /**
   * Creates an error handler function for use in catch blocks
   * Useful for creating consistent error handling across the app
   */
  function createErrorHandler(
    context: string,
    options?: {
      showToast?: boolean
      rethrow?: boolean
    }
  ) {
    return (error: unknown): FormattedError => {
      const { showToast: shouldShowToast = true, rethrow = false } = options || {}

      console.error(`[${context}]`, error)

      const formattedError = handleError(error, { showToast: shouldShowToast })

      if (rethrow) {
        throw error
      }

      return formattedError
    }
  }

  /**
   * Checks if an error is a specific HTTP status code
   */
  function isHttpError(error: unknown, statusCode: number): boolean {
    const apiError = error as ApiError
    return apiError?.statusCode === statusCode
  }

  /**
   * Checks if an error is an authentication error (401)
   */
  function isAuthError(error: unknown): boolean {
    return isHttpError(error, 401)
  }

  /**
   * Checks if an error is a forbidden error (403)
   */
  function isForbiddenError(error: unknown): boolean {
    return isHttpError(error, 403)
  }

  /**
   * Checks if an error is a not found error (404)
   */
  function isNotFoundError(error: unknown): boolean {
    return isHttpError(error, 404)
  }

  /**
   * Checks if an error is a validation error (400 or 422)
   */
  function isValidationError(error: unknown): boolean {
    return isHttpError(error, 400) || isHttpError(error, 422)
  }

  /**
   * Checks if an error is a server error (5xx)
   */
  function isServerError(error: unknown): boolean {
    const apiError = error as ApiError
    const statusCode = apiError?.statusCode
    return statusCode !== undefined && statusCode >= 500 && statusCode < 600
  }

  return {
    formatErrorMessage,
    showErrorToast,
    showSuccessToast,
    showWarningToast,
    showInfoToast,
    showToast,
    handleError,
    createErrorHandler,
    isHttpError,
    isAuthError,
    isForbiddenError,
    isNotFoundError,
    isValidationError,
    isServerError
  }
}
