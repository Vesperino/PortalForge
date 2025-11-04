import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useVacations } from '~/composables/useVacations'
import { useAuthStore } from '~/stores/auth'
import type { VacationSummary, UpdateVacationAllowanceRequest } from '~/composables/useVacations'

describe('useVacations', () => {
  beforeEach(() => {
    // Create a fresh pinia instance for each test
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup auth store with mock token
    const authStore = useAuthStore()
    authStore.accessToken = 'mock-token'
  })

  describe('getUserVacationSummary', () => {
    it('should fetch vacation summary for a user', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174000'
      const mockVacationSummary: VacationSummary = {
        annualVacationDays: 26,
        vacationDaysUsed: 10,
        vacationDaysRemaining: 16,
        onDemandVacationDaysUsed: 2,
        onDemandVacationDaysRemaining: 2,
        circumstantialLeaveDaysUsed: 0,
        carriedOverVacationDays: 5,
        carriedOverExpiryDate: '2025-03-31T00:00:00Z',
        totalAvailableVacationDays: 21
      }

      global.$fetch = vi.fn().mockResolvedValue(mockVacationSummary)

      // Act
      const { getUserVacationSummary } = useVacations()
      const result = await getUserVacationSummary(userId)

      // Assert
      expect(result).toEqual(mockVacationSummary)
      expect(global.$fetch).toHaveBeenCalledWith(
        `http://localhost:5155/api/admin/users/${userId}/vacation-summary`,
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: 'Bearer mock-token'
          })
        })
      )
    })

    it('should handle user with no carried-over days', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174001'
      const mockVacationSummary: VacationSummary = {
        annualVacationDays: 26,
        vacationDaysUsed: 5,
        vacationDaysRemaining: 21,
        onDemandVacationDaysUsed: 0,
        onDemandVacationDaysRemaining: 4,
        circumstantialLeaveDaysUsed: 0,
        carriedOverVacationDays: 0,
        carriedOverExpiryDate: null,
        totalAvailableVacationDays: 21
      }

      global.$fetch = vi.fn().mockResolvedValue(mockVacationSummary)

      // Act
      const { getUserVacationSummary } = useVacations()
      const result = await getUserVacationSummary(userId)

      // Assert
      expect(result).toEqual(mockVacationSummary)
      expect(result.carriedOverVacationDays).toBe(0)
      expect(result.carriedOverExpiryDate).toBeNull()
    })

    it('should handle user with all on-demand vacation used', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174002'
      const mockVacationSummary: VacationSummary = {
        annualVacationDays: 26,
        vacationDaysUsed: 15,
        vacationDaysRemaining: 11,
        onDemandVacationDaysUsed: 4,
        onDemandVacationDaysRemaining: 0,
        circumstantialLeaveDaysUsed: 2,
        carriedOverVacationDays: 0,
        carriedOverExpiryDate: null,
        totalAvailableVacationDays: 11
      }

      global.$fetch = vi.fn().mockResolvedValue(mockVacationSummary)

      // Act
      const { getUserVacationSummary } = useVacations()
      const result = await getUserVacationSummary(userId)

      // Assert
      expect(result.onDemandVacationDaysUsed).toBe(4)
      expect(result.onDemandVacationDaysRemaining).toBe(0)
      expect(result.circumstantialLeaveDaysUsed).toBe(2)
    })

    it('should handle API error when fetching vacation summary', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174003'
      const error = new Error('User not found')

      global.$fetch = vi.fn().mockRejectedValue(error)

      // Act
      const { getUserVacationSummary } = useVacations()

      // Assert
      await expect(getUserVacationSummary(userId)).rejects.toThrow('User not found')
    })

    it('should use authorization token from auth store', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174004'
      const authStore = useAuthStore()
      authStore.accessToken = 'test-token-12345'

      const mockVacationSummary: VacationSummary = {
        annualVacationDays: 26,
        vacationDaysUsed: 0,
        vacationDaysRemaining: 26,
        onDemandVacationDaysUsed: 0,
        onDemandVacationDaysRemaining: 4,
        circumstantialLeaveDaysUsed: 0,
        carriedOverVacationDays: 0,
        carriedOverExpiryDate: null,
        totalAvailableVacationDays: 26
      }

      global.$fetch = vi.fn().mockResolvedValue(mockVacationSummary)

      // Act
      const { getUserVacationSummary } = useVacations()
      await getUserVacationSummary(userId)

      // Assert
      expect(global.$fetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: 'Bearer test-token-12345'
          })
        })
      )
    })
  })

  describe('updateVacationAllowance', () => {
    it('should update vacation allowance for a user', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174005'
      const request: UpdateVacationAllowanceRequest = {
        newAnnualDays: 30,
        reason: 'Zmiana umowy - awans na stanowisko kierownicze'
      }

      global.$fetch = vi.fn().mockResolvedValue(undefined)

      // Act
      const { updateVacationAllowance } = useVacations()
      await updateVacationAllowance(userId, request)

      // Assert
      expect(global.$fetch).toHaveBeenCalledWith(
        `http://localhost:5155/api/admin/users/${userId}/vacation-allowance`,
        expect.objectContaining({
          method: 'PUT',
          headers: expect.objectContaining({
            Authorization: 'Bearer mock-token'
          }),
          body: expect.objectContaining({
            newAnnualDays: 30,
            reason: 'Zmiana umowy - awans na stanowisko kierownicze'
          })
        })
      )
    })

    it('should update vacation allowance with decreased days', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174006'
      const request: UpdateVacationAllowanceRequest = {
        newAnnualDays: 20,
        reason: 'Zmiana na część etatu'
      }

      global.$fetch = vi.fn().mockResolvedValue(undefined)

      // Act
      const { updateVacationAllowance } = useVacations()
      await updateVacationAllowance(userId, request)

      // Assert
      expect(global.$fetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          method: 'PUT',
          body: expect.objectContaining({
            newAnnualDays: 20,
            reason: 'Zmiana na część etatu'
          })
        })
      )
    })

    it('should handle API error when updating vacation allowance', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174007'
      const request: UpdateVacationAllowanceRequest = {
        newAnnualDays: 30,
        reason: 'Test reason'
      }
      const error = new Error('Forbidden: Admin or HR role required')

      global.$fetch = vi.fn().mockRejectedValue(error)

      // Act
      const { updateVacationAllowance } = useVacations()

      // Assert
      await expect(updateVacationAllowance(userId, request)).rejects.toThrow(
        'Forbidden: Admin or HR role required'
      )
    })

    it('should use authorization token when updating vacation allowance', async () => {
      // Arrange
      const userId = '123e4567-e89b-12d3-a456-426614174008'
      const authStore = useAuthStore()
      authStore.accessToken = 'admin-token-67890'

      const request: UpdateVacationAllowanceRequest = {
        newAnnualDays: 28,
        reason: 'Wzrost stażu pracy'
      }

      global.$fetch = vi.fn().mockResolvedValue(undefined)

      // Act
      const { updateVacationAllowance } = useVacations()
      await updateVacationAllowance(userId, request)

      // Assert
      expect(global.$fetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: 'Bearer admin-token-67890'
          })
        })
      )
    })
  })
})
