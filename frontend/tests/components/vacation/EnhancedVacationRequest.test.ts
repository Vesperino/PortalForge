import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import EnhancedVacationRequest from '~/components/vacation/EnhancedVacationRequest.vue'
import { LeaveType } from '~/composables/useVacations'
import type { VacationSummary, ValidateVacationResponse } from '~/composables/useVacations'

// Mock composables
const mockValidateVacation = vi.fn()
const mockGetUserVacationSummary = vi.fn()
const mockGetAuthHeaders = vi.fn(() => ({ Authorization: 'Bearer token' }))
const mockUseAuth = vi.fn(() => ({ getAuthHeaders: mockGetAuthHeaders }))
const mockUseAuthStore = vi.fn(() => ({ user: { id: 'user1' } }))
const mockUseNotificationToast = vi.fn(() => ({
  success: vi.fn(),
  error: vi.fn()
}))
const mockUseRuntimeConfig = vi.fn(() => ({
  public: { apiUrl: 'http://localhost:5155' }
}))

// Mock global $fetch
global.$fetch = vi.fn()

// Mock composables
vi.mock('~/composables/useVacations', () => ({
  useVacations: () => ({
    validateVacation: mockValidateVacation,
    getUserVacationSummary: mockGetUserVacationSummary
  }),
  LeaveType: {
    Annual: 'Annual',
    OnDemand: 'OnDemand',
    Circumstantial: 'Circumstantial',
    Sick: 'Sick'
  }
}))

vi.mock('#app', () => ({
  useAuth: mockUseAuth,
  useAuthStore: mockUseAuthStore,
  useNotificationToast: mockUseNotificationToast,
  useRuntimeConfig: mockUseRuntimeConfig
}))

describe('EnhancedVacationRequest', () => {
  const mockVacationSummary: VacationSummary = {
    annualVacationDays: 26,
    vacationDaysUsed: 10,
    vacationDaysRemaining: 16,
    onDemandVacationDaysUsed: 2,
    onDemandVacationDaysRemaining: 2,
    circumstantialLeaveDaysUsed: 1,
    carriedOverVacationDays: 5,
    carriedOverExpiryDate: '2025-09-30T00:00:00Z',
    totalAvailableVacationDays: 21
  }

  const mockValidationResponse: ValidateVacationResponse = {
    canTake: true,
    requestedDays: 5,
    errorMessage: null
  }

  beforeEach(() => {
    vi.clearAllMocks()
    mockGetUserVacationSummary.mockResolvedValue(mockVacationSummary)
    mockValidateVacation.mockResolvedValue(mockValidationResponse)
    global.$fetch.mockResolvedValue([])
  })

  describe('Vacation Request Validations', () => {
    it('should display vacation quota tracking correctly', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Stan urlopów')
      expect(wrapper.text()).toContain('Urlop wypoczynkowy')
      expect(wrapper.text()).toContain('Urlop na żądanie')
      expect(wrapper.text()).toContain('Urlop okolicznościowy')
      
      // Check quota values
      expect(wrapper.text()).toContain('10/26') // Annual vacation used/total
      expect(wrapper.text()).toContain('2/4') // On-demand used/total
      expect(wrapper.text()).toContain('Pozostało: 16 dni') // Annual remaining
      expect(wrapper.text()).toContain('Pozostało: 2 dni') // On-demand remaining
    })

    it('should validate vacation request when dates are entered', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')

      // Wait for debounced validation
      await new Promise(resolve => setTimeout(resolve, 600))

      expect(mockValidateVacation).toHaveBeenCalledWith({
        startDate: '2024-07-01',
        endDate: '2024-07-05',
        leaveType: LeaveType.Annual
      })
    })

    it('should show validation success message', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Urlop można udzielić (5 dni)')
    })

    it('should show validation error message', async () => {
      mockValidateVacation.mockResolvedValue({
        canTake: false,
        requestedDays: 0,
        errorMessage: 'Niewystarczająca liczba dni urlopu'
      })

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-30')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Niewystarczająca liczba dni urlopu')
    })

    it('should show loading state during validation', async () => {
      // Make validation take longer
      mockValidateVacation.mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve(mockValidationResponse), 1000))
      )

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')

      await new Promise(resolve => setTimeout(resolve, 100))
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Sprawdzanie dostępności...')
    })

    it('should require documents for circumstantial leave', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const leaveTypeSelect = wrapper.find('select')
      await leaveTypeSelect.setValue(LeaveType.Circumstantial)

      expect(wrapper.text()).toContain('Załącz dokumenty potwierdzające')
      expect(wrapper.text()).toContain('Wymagane dla urlopu okolicznościowego')
      expect(wrapper.find('input[type="file"]').exists()).toBe(true)
    })

    it('should not require documents for annual leave', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const leaveTypeSelect = wrapper.find('select')
      await leaveTypeSelect.setValue(LeaveType.Annual)

      expect(wrapper.text()).not.toContain('Załącz dokumenty potwierdzające')
      expect(wrapper.find('input[type="file"]').exists()).toBe(false)
    })

    it('should validate form completeness', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Initially form should be invalid
      const submitButton = wrapper.find('button:has-text("Złóż wniosek")')
      expect(submitButton.attributes('disabled')).toBeDefined()

      // Fill required fields
      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)
      const reasonTextarea = wrapper.find('textarea')

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')
      await reasonTextarea.setValue('Summer vacation')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      // Form should now be valid
      expect(submitButton.attributes('disabled')).toBeUndefined()
    })

    it('should validate document requirement for circumstantial leave', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Set to circumstantial leave
      const leaveTypeSelect = wrapper.find('select')
      await leaveTypeSelect.setValue(LeaveType.Circumstantial)

      // Fill other required fields
      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)
      const reasonTextarea = wrapper.find('textarea')

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')
      await reasonTextarea.setValue('Family emergency')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      // Should still be invalid without documents
      const submitButton = wrapper.find('button:has-text("Złóż wniosek")')
      expect(submitButton.attributes('disabled')).toBeDefined()

      // Add document
      const fileInput = wrapper.find('input[type="file"]')
      const file = new File(['content'], 'document.pdf', { type: 'application/pdf' })
      
      Object.defineProperty(fileInput.element, 'files', {
        value: [file],
        writable: false
      })

      await fileInput.trigger('change')
      await wrapper.vm.$nextTick()

      // Should now be valid
      expect(submitButton.attributes('disabled')).toBeUndefined()
    })
  })

  describe('Vacation Conflict Visualization', () => {
    it('should load and display vacation conflicts', async () => {
      const mockConflicts = [
        {
          date: '2024-07-01',
          conflictingUsers: [
            { id: '1', name: 'Jan Kowalski', department: 'IT' },
            { id: '2', name: 'Anna Nowak', department: 'IT' }
          ],
          severity: 'medium' as const
        }
      ]

      global.$fetch.mockResolvedValue(mockConflicts)

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Konflikty urlopowe')
      expect(wrapper.text()).toContain('Wykryto 1 konflikt(ów)')
    })

    it('should show conflict details when expanded', async () => {
      const mockConflicts = [
        {
          date: '2024-07-01',
          conflictingUsers: [
            { id: '1', name: 'Jan Kowalski', department: 'IT' }
          ],
          severity: 'high' as const
        }
      ]

      global.$fetch.mockResolvedValue(mockConflicts)

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      // Expand conflict details
      const showDetailsButton = wrapper.find('button:has-text("Pokaż szczegóły")')
      await showDetailsButton.trigger('click')

      expect(wrapper.text()).toContain('Jan Kowalski')
      expect(wrapper.text()).toContain('(IT)')
      expect(wrapper.text()).toContain('Wysoki') // High severity
    })

    it('should display correct severity colors', async () => {
      const mockConflicts = [
        {
          date: '2024-07-01',
          conflictingUsers: [{ id: '1', name: 'Jan Kowalski', department: 'IT' }],
          severity: 'high' as const
        }
      ]

      global.$fetch.mockResolvedValue(mockConflicts)

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      // Should show red color for high severity
      expect(wrapper.find('.text-red-600').exists()).toBe(true)
    })

    it('should handle no conflicts gracefully', async () => {
      global.$fetch.mockResolvedValue([])

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).not.toContain('Konflikty urlopowe')
    })
  })

  describe('Vacation Analytics Dashboard', () => {
    it('should load and display analytics', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Analityka urlopowa')
      
      // Expand analytics
      const showAnalyticsButton = wrapper.find('button:has-text("Pokaż")')
      await showAnalyticsButton.trigger('click')

      expect(wrapper.text()).toContain('Wnioski w tym roku')
      expect(wrapper.text()).toContain('Średni czas realizacji')
      expect(wrapper.text()).toContain('Wskaźnik akceptacji')
      expect(wrapper.text()).toContain('Najczęściej używany typ')
      expect(wrapper.text()).toContain('Szczyty urlopowe')
      expect(wrapper.text()).toContain('Wpływ na zespół')
    })

    it('should display correct analytics values', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Expand analytics
      const showAnalyticsButton = wrapper.find('button:has-text("Pokaż")')
      await showAnalyticsButton.trigger('click')

      // Check mock values
      expect(wrapper.text()).toContain('3') // Total requests
      expect(wrapper.text()).toContain('2.5d') // Average processing time
      expect(wrapper.text()).toContain('95%') // Approval rate
      expect(wrapper.text()).toContain('Annual') // Most used type
      expect(wrapper.text()).toContain('Lipiec, Sierpień') // Peak months
      expect(wrapper.text()).toContain('15%') // Team impact
    })

    it('should toggle analytics visibility', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const toggleButton = wrapper.find('button:has-text("Pokaż")')
      
      // Show analytics
      await toggleButton.trigger('click')
      expect(wrapper.text()).toContain('Wnioski w tym roku')
      
      // Hide analytics
      const hideButton = wrapper.find('button:has-text("Ukryj")')
      await hideButton.trigger('click')
      expect(wrapper.text()).not.toContain('Wnioski w tym roku')
    })
  })

  describe('File Upload for Documents', () => {
    it('should handle file upload for circumstantial leave', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Set to circumstantial leave
      const leaveTypeSelect = wrapper.find('select')
      await leaveTypeSelect.setValue(LeaveType.Circumstantial)

      const fileInput = wrapper.find('input[type="file"]')
      const file = new File(['content'], 'document.pdf', { type: 'application/pdf' })
      
      Object.defineProperty(fileInput.element, 'files', {
        value: [file],
        writable: false
      })

      await fileInput.trigger('change')
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('document.pdf')
    })

    it('should allow removing uploaded documents', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Set to circumstantial leave and upload file
      const leaveTypeSelect = wrapper.find('select')
      await leaveTypeSelect.setValue(LeaveType.Circumstantial)

      const fileInput = wrapper.find('input[type="file"]')
      const file = new File(['content'], 'document.pdf', { type: 'application/pdf' })
      
      Object.defineProperty(fileInput.element, 'files', {
        value: [file],
        writable: false
      })

      await fileInput.trigger('change')
      await wrapper.vm.$nextTick()

      // Remove file
      const removeButton = wrapper.find('button:has-svg("X")')
      await removeButton.trigger('click')

      expect(wrapper.text()).not.toContain('document.pdf')
    })

    it('should support multiple file uploads', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Set to circumstantial leave
      const leaveTypeSelect = wrapper.find('select')
      await leaveTypeSelect.setValue(LeaveType.Circumstantial)

      const fileInput = wrapper.find('input[type="file"]')
      const files = [
        new File(['content1'], 'document1.pdf', { type: 'application/pdf' }),
        new File(['content2'], 'document2.jpg', { type: 'image/jpeg' })
      ]
      
      Object.defineProperty(fileInput.element, 'files', {
        value: files,
        writable: false
      })

      await fileInput.trigger('change')
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('document1.pdf')
      expect(wrapper.text()).toContain('document2.jpg')
    })
  })

  describe('Form Submission', () => {
    it('should submit vacation request with correct data', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Fill form
      const leaveTypeSelect = wrapper.find('select')
      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)
      const reasonTextarea = wrapper.find('textarea')

      await leaveTypeSelect.setValue(LeaveType.Annual)
      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')
      await reasonTextarea.setValue('Summer vacation')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      // Submit form
      const submitButton = wrapper.find('button:has-text("Złóż wniosek")')
      await submitButton.trigger('click')

      expect(wrapper.emitted('submit')).toBeTruthy()
      const submittedData = wrapper.emitted('submit')?.[0]?.[0]
      expect(submittedData).toEqual({
        leaveType: LeaveType.Annual,
        startDate: '2024-07-01',
        endDate: '2024-07-05',
        reason: 'Summer vacation',
        substituteUserId: null,
        requestedDays: 5,
        documents: []
      })
    })

    it('should show loading state during submission', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Fill form
      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)
      const reasonTextarea = wrapper.find('textarea')

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')
      await reasonTextarea.setValue('Summer vacation')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      // Submit form
      const submitButton = wrapper.find('button:has-text("Złóż wniosek")')
      await submitButton.trigger('click')

      expect(submitButton.text()).toContain('Składanie...')
      expect(submitButton.attributes('disabled')).toBeDefined()
    })

    it('should emit cancel event when cancel button is clicked', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const cancelButton = wrapper.find('button:has-text("Anuluj")')
      await cancelButton.trigger('click')

      expect(wrapper.emitted('cancel')).toBeTruthy()
    })
  })

  describe('Leave Type Specific Validations', () => {
    it('should validate on-demand vacation quota', async () => {
      // Mock validation to fail for on-demand when quota exceeded
      mockValidateVacation.mockResolvedValue({
        canTake: false,
        requestedDays: 0,
        errorMessage: 'Przekroczono limit urlopu na żądanie (4 dni rocznie)'
      })

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const leaveTypeSelect = wrapper.find('select')
      await leaveTypeSelect.setValue(LeaveType.OnDemand)

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-10') // 10 days, exceeds limit

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Przekroczono limit urlopu na żądanie')
    })

    it('should show correct quota for selected leave type', async () => {
      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      // Select on-demand vacation
      const leaveTypeSelect = wrapper.find('select')
      await leaveTypeSelect.setValue(LeaveType.OnDemand)

      // Should highlight on-demand quota
      expect(wrapper.vm.selectedQuota?.type).toBe('ondemand')
      expect(wrapper.vm.selectedQuota?.total).toBe(4)
    })
  })

  describe('Error Handling', () => {
    it('should handle validation API errors gracefully', async () => {
      mockValidateVacation.mockRejectedValue(new Error('API Error'))

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      const startDateInput = wrapper.find('input[type="date"]').at(0)
      const endDateInput = wrapper.find('input[type="date"]').at(1)

      await startDateInput.setValue('2024-07-01')
      await endDateInput.setValue('2024-07-05')

      await new Promise(resolve => setTimeout(resolve, 600))
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Błąd podczas walidacji wniosku')
    })

    it('should handle vacation summary loading errors', async () => {
      mockGetUserVacationSummary.mockRejectedValue(new Error('API Error'))
      const mockToast = mockUseNotificationToast()

      const wrapper = mount(EnhancedVacationRequest)
      
      await wrapper.vm.$nextTick()

      expect(mockToast.error).toHaveBeenCalledWith('Błąd podczas ładowania danych urlopowych')
    })
  })
})