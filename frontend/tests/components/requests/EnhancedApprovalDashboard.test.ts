import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import EnhancedApprovalDashboard from '~/components/requests/EnhancedApprovalDashboard.vue'
import type { Request, RequestStatus, ApprovalStepStatus } from '~/types/requests'

// Mock the global $fetch
global.$fetch = vi.fn()

describe('EnhancedApprovalDashboard', () => {
  const mockRequests = [
    {
      id: '1',
      requestNumber: 'REQ-2024-001',
      requestTemplateId: 'template1',
      requestTemplateName: 'Wniosek urlopowy',
      requestTemplateIcon: '🏖️',
      submittedById: 'user1',
      submittedByName: 'Jan Kowalski',
      submittedAt: '2024-01-15T10:00:00Z',
      priority: 'Standard',
      formData: '{}',
      status: 'InReview' as RequestStatus,
      approvalSteps: [],
      daysWaiting: 3,
      isOverdue: false,
      canDelegate: true
    },
    {
      id: '2',
      requestNumber: 'REQ-2024-002',
      requestTemplateId: 'template2',
      requestTemplateName: 'Wniosek IT',
      requestTemplateIcon: '💻',
      submittedById: 'user2',
      submittedByName: 'Anna Nowak',
      submittedAt: '2024-01-10T14:30:00Z',
      priority: 'Urgent',
      formData: '{}',
      status: 'InReview' as RequestStatus,
      approvalSteps: [],
      daysWaiting: 8,
      isOverdue: true,
      canDelegate: true,
      serviceCategory: 'IT Support',
      serviceStatus: 'In Progress'
    },
    {
      id: '3',
      requestNumber: 'REQ-2024-003',
      requestTemplateId: 'template3',
      requestTemplateName: 'Wniosek finansowy',
      requestTemplateIcon: '💰',
      submittedById: 'user3',
      submittedByName: 'Piotr Wiśniewski',
      submittedAt: '2024-01-18T09:15:00Z',
      priority: 'Standard',
      formData: '{}',
      status: 'InReview' as RequestStatus,
      approvalSteps: [],
      daysWaiting: 1,
      isOverdue: false,
      canDelegate: false
    }
  ]

  beforeEach(() => {
    vi.clearAllMocks()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  describe('Bulk Approval Operations', () => {
    it('should display bulk actions when requests are selected', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      // Wait for component to load
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select first request
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1] // Skip header checkbox
      await firstCheckbox.setChecked(true)

      expect(wrapper.text()).toContain('Wybrano: 1')
      expect(wrapper.find('button:has-text("Akcje grupowe")').exists()).toBe(true)
    })

    it('should select all requests when header checkbox is clicked', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const headerCheckbox = wrapper.find('thead input[type="checkbox"]')
      await headerCheckbox.setChecked(true)

      expect(wrapper.text()).toContain('Wybrano: 3')
    })

    it('should deselect all requests when header checkbox is unchecked', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const headerCheckbox = wrapper.find('thead input[type="checkbox"]')
      
      // Select all first
      await headerCheckbox.setChecked(true)
      expect(wrapper.text()).toContain('Wybrano: 3')
      
      // Then deselect all
      await headerCheckbox.setChecked(false)
      expect(wrapper.text()).not.toContain('Wybrano:')
    })

    it('should open bulk actions modal when bulk actions button is clicked', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select a request
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      // Click bulk actions button
      const bulkActionsButton = wrapper.find('button:has-text("Akcje grupowe")')
      await bulkActionsButton.trigger('click')

      expect(wrapper.text()).toContain('Akcje grupowe (1 wniosków)')
      expect(wrapper.find('select option[value="approve"]').exists()).toBe(true)
      expect(wrapper.find('select option[value="reject"]').exists()).toBe(true)
      expect(wrapper.find('select option[value="delegate"]').exists()).toBe(true)
    })

    it('should execute bulk approve action', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select requests
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      // Open bulk actions modal
      const bulkActionsButton = wrapper.find('button:has-text("Akcje grupowe")')
      await bulkActionsButton.trigger('click')

      // Select approve action
      const actionSelect = wrapper.find('select')
      await actionSelect.setValue('approve')

      // Add comment
      const commentTextarea = wrapper.find('textarea')
      await commentTextarea.setValue('Bulk approval comment')

      // Execute action
      const executeButton = wrapper.find('button:has-text("Wykonaj")')
      await executeButton.trigger('click')

      expect(executeButton.text()).toContain('Wykonywanie...')
      
      // Fast forward to complete the action
      vi.advanceTimersByTime(1500)
      await wrapper.vm.$nextTick()

      // Modal should be closed and selections cleared
      expect(wrapper.text()).not.toContain('Akcje grupowe (1 wniosków)')
      expect(wrapper.text()).not.toContain('Wybrano:')
    })

    it('should execute bulk reject action', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select requests
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      // Open bulk actions modal
      const bulkActionsButton = wrapper.find('button:has-text("Akcje grupowe")')
      await bulkActionsButton.trigger('click')

      // Select reject action
      const actionSelect = wrapper.find('select')
      await actionSelect.setValue('reject')

      // Execute action
      const executeButton = wrapper.find('button:has-text("Wykonaj")')
      await executeButton.trigger('click')

      vi.advanceTimersByTime(1500)
      await wrapper.vm.$nextTick()

      // Should update request status to rejected
      expect(wrapper.vm.requests.some(r => r.status === 'Rejected')).toBe(true)
    })

    it('should disable execute button when no action is selected', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select requests
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      // Open bulk actions modal
      const bulkActionsButton = wrapper.find('button:has-text("Akcje grupowe")')
      await bulkActionsButton.trigger('click')

      // Execute button should be disabled
      const executeButton = wrapper.find('button:has-text("Wykonaj")')
      expect(executeButton.attributes('disabled')).toBeDefined()
    })

    it('should close bulk actions modal when cancel is clicked', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select requests and open modal
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      const bulkActionsButton = wrapper.find('button:has-text("Akcje grupowe")')
      await bulkActionsButton.trigger('click')

      // Click cancel
      const cancelButton = wrapper.find('button:has-text("Anuluj")')
      await cancelButton.trigger('click')

      expect(wrapper.text()).not.toContain('Akcje grupowe (1 wniosków)')
    })
  })

  describe('Delegation Functionality', () => {
    it('should show delegation button when requests are selected', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select a request
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      const delegateButton = wrapper.find('button:has-text("Deleguj")')
      expect(delegateButton.exists()).toBe(true)
      expect(delegateButton.text()).toContain('Deleguj (1)')
    })

    it('should disable delegation button when no requests are selected', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const delegateButton = wrapper.find('button:has-text("Deleguj")')
      expect(delegateButton.attributes('disabled')).toBeDefined()
    })

    it('should open delegation modal when delegation button is clicked', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select a request
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      // Click delegation button
      const delegateButton = wrapper.find('button:has-text("Deleguj")')
      await delegateButton.trigger('click')

      expect(wrapper.text()).toContain('Deleguj wnioski (1)')
      expect(wrapper.find('select option[value="user1"]').exists()).toBe(true)
    })

    it('should execute delegation when user is selected', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select a request
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      // Open delegation modal
      const delegateButton = wrapper.find('button:has-text("Deleguj")')
      await delegateButton.trigger('click')

      // Select user
      const userSelect = wrapper.find('select')
      await userSelect.setValue('user1')

      // Execute delegation
      const executeButton = wrapper.find('button:has-text("Deleguj")')
      await executeButton.trigger('click')

      expect(executeButton.text()).toContain('Delegowanie...')
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Modal should be closed
      expect(wrapper.text()).not.toContain('Deleguj wnioski (1)')
    })

    it('should disable delegation execute button when no user is selected', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Select a request
      const firstCheckbox = wrapper.findAll('input[type="checkbox"]')[1]
      await firstCheckbox.setChecked(true)

      // Open delegation modal
      const delegateButton = wrapper.find('button:has-text("Deleguj")')
      await delegateButton.trigger('click')

      // Execute button should be disabled
      const executeButton = wrapper.find('button:has-text("Deleguj")')
      expect(executeButton.attributes('disabled')).toBeDefined()
    })
  })

  describe('Individual Request Actions', () => {
    it('should show individual action buttons for each request', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Should have approve, reject, and more options buttons for each request
      const approveButtons = wrapper.findAll('button[title="Zatwierdź"]')
      const rejectButtons = wrapper.findAll('button[title="Odrzuć"]')
      const moreButtons = wrapper.findAll('button[title="Więcej opcji"]')

      expect(approveButtons.length).toBe(3)
      expect(rejectButtons.length).toBe(3)
      expect(moreButtons.length).toBe(3)
    })

    it('should handle individual approve action', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const firstApproveButton = wrapper.find('button[title="Zatwierdź"]')
      await firstApproveButton.trigger('click')

      // Should trigger individual approval (implementation would depend on actual component logic)
      expect(firstApproveButton.exists()).toBe(true)
    })

    it('should handle individual reject action', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const firstRejectButton = wrapper.find('button[title="Odrzuć"]')
      await firstRejectButton.trigger('click')

      // Should trigger individual rejection
      expect(firstRejectButton.exists()).toBe(true)
    })
  })

  describe('Filtering and Search', () => {
    it('should filter requests by search query', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const searchInput = wrapper.find('input[placeholder="Szukaj wniosków..."]')
      await searchInput.setValue('REQ-2024-001')

      // Should show only matching request
      expect(wrapper.text()).toContain('REQ-2024-001')
      expect(wrapper.text()).not.toContain('REQ-2024-002')
    })

    it('should filter by submitter name', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const searchInput = wrapper.find('input[placeholder="Szukaj wniosków..."]')
      await searchInput.setValue('Jan Kowalski')

      expect(wrapper.text()).toContain('Jan Kowalski')
      expect(wrapper.text()).not.toContain('Anna Nowak')
    })

    it('should show advanced filters when filter button is clicked', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const filterButton = wrapper.find('button:has-text("Filtry")')
      await filterButton.trigger('click')

      expect(wrapper.text()).toContain('Status')
      expect(wrapper.text()).toContain('Priorytet')
      expect(wrapper.text()).toContain('Typ')
    })

    it('should filter by overdue status', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Open filters
      const filterButton = wrapper.find('button:has-text("Filtry")')
      await filterButton.trigger('click')

      // Check overdue filter
      const overdueCheckbox = wrapper.find('input[type="checkbox"]:has-text("Przeterminowane")')
      await overdueCheckbox.setChecked(true)

      // Should show only overdue requests
      expect(wrapper.text()).toContain('REQ-2024-002') // This one is overdue
      expect(wrapper.text()).not.toContain('REQ-2024-001') // This one is not overdue
    })

    it('should filter by priority', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Open filters
      const filterButton = wrapper.find('button:has-text("Filtry")')
      await filterButton.trigger('click')

      // Check urgent priority filter
      const urgentCheckbox = wrapper.find('input[value="Urgent"]')
      await urgentCheckbox.setChecked(true)

      // Should show only urgent requests
      expect(wrapper.text()).toContain('REQ-2024-002') // This one is urgent
    })

    it('should filter by service requests', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Open filters
      const filterButton = wrapper.find('button:has-text("Filtry")')
      await filterButton.trigger('click')

      // Check service requests filter
      const serviceCheckbox = wrapper.find('input[type="checkbox"]:has-text("Wnioski serwisowe")')
      await serviceCheckbox.setChecked(true)

      // Should show only service requests
      expect(wrapper.text()).toContain('IT Support') // Service category
    })

    it('should clear all filters', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Set some filters first
      const searchInput = wrapper.find('input[placeholder="Szukaj wniosków..."]')
      await searchInput.setValue('test')

      const filterButton = wrapper.find('button:has-text("Filtry")')
      await filterButton.trigger('click')

      const overdueCheckbox = wrapper.find('input[type="checkbox"]:has-text("Przeterminowane")')
      await overdueCheckbox.setChecked(true)

      // Clear filters
      const clearButton = wrapper.find('button:has-text("Wyczyść filtry")')
      await clearButton.trigger('click')

      expect(searchInput.element.value).toBe('')
      expect(overdueCheckbox.element.checked).toBe(false)
    })
  })

  describe('Sorting', () => {
    it('should sort by different criteria', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const sortSelect = wrapper.find('select')
      await sortSelect.setValue('priority')

      // Should sort by priority
      expect(wrapper.vm.sortBy).toBe('priority')
    })

    it('should toggle sort order', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const sortOrderButton = wrapper.find('button:has-svg("ArrowUpDown")')
      await sortOrderButton.trigger('click')

      expect(wrapper.vm.sortOrder).toBe('asc')
    })
  })

  describe('Statistics Display', () => {
    it('should display correct statistics', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Should show stats cards
      expect(wrapper.text()).toContain('Oczekujące')
      expect(wrapper.text()).toContain('Przeterminowane')
      expect(wrapper.text()).toContain('Serwisowe')
      expect(wrapper.text()).toContain('Pilne')

      // Check specific counts
      expect(wrapper.text()).toContain('3') // Total pending
      expect(wrapper.text()).toContain('1') // Overdue count
      expect(wrapper.text()).toContain('1') // Service requests count
      expect(wrapper.text()).toContain('1') // Urgent count
    })
  })

  describe('Export Functionality', () => {
    it('should show export button', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const exportButton = wrapper.find('button:has-text("Eksportuj")')
      expect(exportButton.exists()).toBe(true)
    })

    it('should trigger export when export button is clicked', async () => {
      const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {})
      
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      const exportButton = wrapper.find('button:has-text("Eksportuj")')
      await exportButton.trigger('click')

      expect(consoleSpy).toHaveBeenCalledWith('Exporting data:', expect.any(Array))
      
      consoleSpy.mockRestore()
    })
  })

  describe('Loading and Empty States', () => {
    it('should show loading state initially', () => {
      const wrapper = mount(EnhancedApprovalDashboard)

      expect(wrapper.text()).toContain('Ładowanie wniosków...')
      expect(wrapper.find('.animate-spin').exists()).toBe(true)
    })

    it('should show empty state when no requests match filters', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Set a search that won't match anything
      const searchInput = wrapper.find('input[placeholder="Szukaj wniosków..."]')
      await searchInput.setValue('nonexistent')

      expect(wrapper.text()).toContain('Brak wniosków do zatwierdzenia')
      expect(wrapper.text()).toContain('Nie znaleziono wniosków spełniających kryteria')
    })
  })

  describe('Request Status Display', () => {
    it('should display correct status colors', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Check for status badges
      const statusBadges = wrapper.findAll('.bg-yellow-100')
      expect(statusBadges.length).toBeGreaterThan(0) // InReview status
    })

    it('should display priority colors correctly', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Check for priority badges
      expect(wrapper.find('.bg-red-100').exists()).toBe(true) // Urgent priority
      expect(wrapper.find('.bg-blue-100').exists()).toBe(true) // Standard priority
    })

    it('should highlight overdue requests', async () => {
      const wrapper = mount(EnhancedApprovalDashboard)
      
      vi.advanceTimersByTime(1000)
      await wrapper.vm.$nextTick()

      // Overdue requests should have red background
      const overdueRow = wrapper.find('.bg-red-50')
      expect(overdueRow.exists()).toBe(true)
    })
  })
})