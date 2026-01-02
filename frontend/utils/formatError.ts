/**
 * Error formatting utilities for the PortalForge application
 */

export interface ApiErrorData {
  statusCode?: number
  message?: string
  data?: {
    message?: string
    errors?: Record<string, string[]>
    title?: string
  }
}

export interface FormattedErrorResult {
  title: string
  message: string
  statusCode?: number
  validationErrors?: Record<string, string[]>
}

/**
 * HTTP status code to Polish message mapping
 */
export const HTTP_ERROR_MESSAGES: Record<number, string> = {
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

/**
 * Gets a user-friendly message for an HTTP status code
 */
export function getHttpErrorMessage(statusCode: number): string {
  return HTTP_ERROR_MESSAGES[statusCode] || 'Wystapil nieoczekiwany blad'
}

/**
 * Extracts error message from various error object formats
 */
export function extractErrorMessage(error: unknown): string {
  if (!error) return ''

  // String error
  if (typeof error === 'string') {
    return error
  }

  // Error object
  if (error instanceof Error) {
    return error.message
  }

  const apiError = error as ApiErrorData

  // Check for validation errors first
  if (apiError?.data?.errors) {
    const errorMessages = Object.values(apiError.data.errors).flat()
    if (errorMessages.length > 0) {
      return errorMessages.join('. ')
    }
  }

  // Check for message in data
  if (apiError?.data?.message) {
    return apiError.data.message
  }

  // Check for title in data
  if (apiError?.data?.title) {
    return apiError.data.title
  }

  // Check for direct message
  if (apiError?.message) {
    return apiError.message
  }

  // Use HTTP status message as fallback
  if (apiError?.statusCode && HTTP_ERROR_MESSAGES[apiError.statusCode]) {
    return HTTP_ERROR_MESSAGES[apiError.statusCode]
  }

  return 'Wystapil nieoczekiwany blad'
}

/**
 * Formats an error object into a user-friendly result
 */
export function formatError(error: unknown): FormattedErrorResult {
  const apiError = error as ApiErrorData
  const statusCode = apiError?.statusCode
  const message = extractErrorMessage(error)

  let title = 'Blad'
  if (statusCode) {
    if (statusCode >= 400 && statusCode < 500) {
      title = 'Blad zadania'
    } else if (statusCode >= 500) {
      title = 'Blad serwera'
    }
  }

  const result: FormattedErrorResult = {
    title,
    message,
    statusCode
  }

  // Include validation errors if present
  if (apiError?.data?.errors) {
    result.validationErrors = apiError.data.errors
  }

  return result
}

/**
 * Formats validation errors into a single message string
 */
export function formatValidationErrors(
  errors: Record<string, string[]> | undefined | null
): string {
  if (!errors) return ''

  const messages: string[] = []

  for (const [_field, fieldErrors] of Object.entries(errors)) {
    if (Array.isArray(fieldErrors) && fieldErrors.length > 0) {
      messages.push(...fieldErrors)
    }
  }

  return messages.join('. ')
}

/**
 * Gets validation errors for a specific field
 */
export function getFieldErrors(
  errors: Record<string, string[]> | undefined | null,
  fieldName: string
): string[] {
  if (!errors || !errors[fieldName]) return []
  return errors[fieldName]
}

/**
 * Checks if there are any validation errors
 */
export function hasValidationErrors(
  errors: Record<string, string[]> | undefined | null
): boolean {
  if (!errors) return false

  return Object.values(errors).some(
    fieldErrors => Array.isArray(fieldErrors) && fieldErrors.length > 0
  )
}

/**
 * Creates a network error message
 */
export function formatNetworkError(): FormattedErrorResult {
  return {
    title: 'Blad polaczenia',
    message: 'Nie udalo sie polaczyc z serwerem. Sprawdz polaczenie internetowe.'
  }
}

/**
 * Checks if an error is a network error
 */
export function isNetworkError(error: unknown): boolean {
  return (
    error instanceof TypeError &&
    error.message === 'Failed to fetch'
  )
}

/**
 * Checks if an error has a specific status code
 */
export function hasStatusCode(error: unknown, statusCode: number): boolean {
  const apiError = error as ApiErrorData
  return apiError?.statusCode === statusCode
}

/**
 * Type guard to check if error is an API error with data
 */
export function isApiError(error: unknown): error is ApiErrorData {
  if (!error || typeof error !== 'object') return false
  const obj = error as Record<string, unknown>
  return 'statusCode' in obj || 'message' in obj || 'data' in obj
}
