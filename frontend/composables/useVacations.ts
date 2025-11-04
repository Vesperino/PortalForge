import { useAuthStore } from '~/stores/auth'

/**
 * User vacation summary matching backend VacationSummaryDto
 */
export interface VacationSummary {
  annualVacationDays: number
  vacationDaysUsed: number
  vacationDaysRemaining: number
  onDemandVacationDaysUsed: number
  onDemandVacationDaysRemaining: number
  circumstantialLeaveDaysUsed: number
  carriedOverVacationDays: number
  carriedOverExpiryDate: string | null
  totalAvailableVacationDays: number
}

/**
 * Request body for updating vacation allowance
 */
export interface UpdateVacationAllowanceRequest {
  newAnnualDays: number
  reason: string
}

/**
 * Leave type enum matching backend
 */
export enum LeaveType {
  Annual = 'Annual',
  OnDemand = 'OnDemand',
  Circumstantial = 'Circumstantial',
  Sick = 'Sick'
}

/**
 * Request body for validating vacation availability
 */
export interface ValidateVacationRequest {
  startDate: string
  endDate: string
  leaveType: LeaveType
}

/**
 * Response from vacation validation
 */
export interface ValidateVacationResponse {
  canTake: boolean
  errorMessage: string | null
  requestedDays: number
}

/**
 * Composable for vacation management API calls
 */
export function useVacations() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl || 'http://localhost:5155'

  const authStore = useAuthStore()

  const getAuthHeaders = (): Record<string, string> | undefined => {
    const token = authStore.accessToken

    if (token) {
      return { Authorization: `Bearer ${token}` }
    }
    return undefined
  }

  /**
   * Get vacation summary for a specific user
   * @param userId - The user's GUID
   * @returns VacationSummary with all vacation-related data
   */
  async function getUserVacationSummary(userId: string): Promise<VacationSummary> {
    const headers = getAuthHeaders()
    return await $fetch(`${apiUrl}/api/admin/users/${userId}/vacation-summary`, { headers }) as VacationSummary
  }

  /**
   * Update user's annual vacation allowance (Admin/HR only)
   * @param userId - The user's GUID
   * @param request - The update request with new allowance and reason
   */
  async function updateVacationAllowance(
    userId: string,
    request: UpdateVacationAllowanceRequest
  ): Promise<void> {
    const headers = getAuthHeaders()
    await $fetch(`${apiUrl}/api/admin/users/${userId}/vacation-allowance`, {
      method: 'PUT',
      headers,
      body: request
    })
  }

  /**
   * Validates if user can take vacation on specified dates
   * Used for real-time validation before submitting request
   * @param request - Vacation validation request with dates and leave type
   * @returns Validation result with error message if invalid
   */
  async function validateVacation(
    request: ValidateVacationRequest
  ): Promise<ValidateVacationResponse> {
    const headers = getAuthHeaders()
    return await $fetch(`${apiUrl}/api/vacation-schedules/validate`, {
      method: 'POST',
      headers,
      body: request
    }) as ValidateVacationResponse
  }

  return {
    getUserVacationSummary,
    updateVacationAllowance,
    validateVacation
  }
}
