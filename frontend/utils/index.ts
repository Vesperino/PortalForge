/**
 * Utility functions index for PortalForge
 * Re-exports all utility functions from individual modules
 */

// Date formatting utilities
export {
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
} from './formatDate'

// Error formatting utilities
export {
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
} from './formatError'
export type { ApiErrorData, FormattedErrorResult } from './formatError'

// Validation utilities
export {
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
} from './validation'
export type { ValidationResult } from './validation'
