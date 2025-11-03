import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import VacationSummary from '~/components/vacation/VacationSummary.vue'
import type { VacationSummary as VacationSummaryType } from '~/composables/useVacations'

describe('VacationSummary', () => {
  const mockSummary: VacationSummaryType = {
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

  describe('Loading State', () => {
    it('should display loading spinner when isLoading is true', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: null,
          isLoading: true
        }
      })

      expect(wrapper.find('.animate-spin').exists()).toBe(true)
      expect(wrapper.text()).toContain('Podsumowanie urlopów')
    })
  })

  describe('Error State', () => {
    it('should display error message when error prop is provided', () => {
      const errorMessage = 'Nie udało się pobrać danych urlopowych'

      const wrapper = mount(VacationSummary, {
        props: {
          summary: null,
          error: errorMessage
        }
      })

      expect(wrapper.text()).toContain(errorMessage)
      expect(wrapper.find('.bg-red-50').exists()).toBe(true)
    })
  })

  describe('Summary Display', () => {
    it('should display all vacation statistics correctly', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      // Annual vacation days
      expect(wrapper.text()).toContain('26')
      expect(wrapper.text()).toContain('Dni urlopu w roku')

      // Remaining days
      expect(wrapper.text()).toContain('16')
      expect(wrapper.text()).toContain('Dni pozostało')

      // Used days
      expect(wrapper.text()).toContain('10')
      expect(wrapper.text()).toContain('Dni wykorzystano')

      // On-demand remaining
      expect(wrapper.text()).toContain('2')
      expect(wrapper.text()).toContain('Dni na żądanie')
    })

    it('should calculate vacation percentage correctly', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      // 10/26 * 100 = 38.46% rounded to 38%
      expect(wrapper.text()).toContain('10 / 26 dni (38%)')
    })

    it('should display progress bar with correct width', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      const progressBar = wrapper.find('.bg-gradient-to-r')
      expect(progressBar.attributes('style')).toContain('width: 38%')
    })

    it('should display on-demand vacation usage', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      expect(wrapper.text()).toContain('2 / 4 dni')
      expect(wrapper.text()).toContain('Urlop na żądanie wykorzystany')
    })

    it('should display circumstantial leave usage', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      expect(wrapper.text()).toContain('1 dni')
      expect(wrapper.text()).toContain('Urlop okolicznościowy')
    })

    it('should display total available vacation days', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      expect(wrapper.text()).toContain('21')
      expect(wrapper.text()).toContain('Łącznie dostępne dni urlopu')
    })
  })

  describe('Carried Over Vacation', () => {
    it('should display carried over vacation warning when days > 0', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      expect(wrapper.text()).toContain('Urlop zaległy: 5 dni')
      expect(wrapper.text()).toContain('Wygasa:')
      expect(wrapper.find('.bg-yellow-50').exists()).toBe(true)
    })

    it('should not display carried over warning when days = 0', () => {
      const summaryWithoutCarriedOver: VacationSummaryType = {
        ...mockSummary,
        carriedOverVacationDays: 0,
        carriedOverExpiryDate: null
      }

      const wrapper = mount(VacationSummary, {
        props: {
          summary: summaryWithoutCarriedOver
        }
      })

      expect(wrapper.text()).not.toContain('Urlop zaległy')
    })

    it('should format carried over expiry date correctly', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      // Polish date format: "30 września 2025"
      expect(wrapper.text()).toMatch(/Wygasa:.*września 2025/)
    })
  })

  describe('Edge Cases', () => {
    it('should handle 0% vacation usage', () => {
      const summaryNoUsage: VacationSummaryType = {
        ...mockSummary,
        vacationDaysUsed: 0,
        vacationDaysRemaining: 26
      }

      const wrapper = mount(VacationSummary, {
        props: {
          summary: summaryNoUsage
        }
      })

      expect(wrapper.text()).toContain('0 / 26 dni (0%)')
      const progressBar = wrapper.find('.bg-gradient-to-r')
      expect(progressBar.attributes('style')).toContain('width: 0%')
    })

    it('should handle 100% vacation usage', () => {
      const summaryFullUsage: VacationSummaryType = {
        ...mockSummary,
        vacationDaysUsed: 26,
        vacationDaysRemaining: 0
      }

      const wrapper = mount(VacationSummary, {
        props: {
          summary: summaryFullUsage
        }
      })

      expect(wrapper.text()).toContain('26 / 26 dni (100%)')
      const progressBar = wrapper.find('.bg-gradient-to-r')
      expect(progressBar.attributes('style')).toContain('width: 100%')
    })

    it('should handle all on-demand days used', () => {
      const summaryFullOnDemand: VacationSummaryType = {
        ...mockSummary,
        onDemandVacationDaysUsed: 4,
        onDemandVacationDaysRemaining: 0
      }

      const wrapper = mount(VacationSummary, {
        props: {
          summary: summaryFullOnDemand
        }
      })

      expect(wrapper.text()).toContain('4 / 4 dni')
      expect(wrapper.text()).toContain('0')
      expect(wrapper.text()).toContain('Dni na żądanie')
    })

    it('should handle zero annual vacation days', () => {
      const summaryZeroAnnual: VacationSummaryType = {
        ...mockSummary,
        annualVacationDays: 0,
        vacationDaysUsed: 0,
        vacationDaysRemaining: 0
      }

      const wrapper = mount(VacationSummary, {
        props: {
          summary: summaryZeroAnnual
        }
      })

      expect(wrapper.text()).toContain('0 / 0 dni (0%)')
    })
  })

  describe('Responsive Layout', () => {
    it('should render all vacation cards', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      // Should have 4 summary cards
      const cards = wrapper.findAll('.p-4.rounded-lg.border')
      expect(cards.length).toBeGreaterThanOrEqual(4)
    })

    it('should have grid layout classes', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      const grid = wrapper.find('.grid.grid-cols-1.md\\:grid-cols-4')
      expect(grid.exists()).toBe(true)
    })
  })

  describe('Dark Mode Support', () => {
    it('should have dark mode classes', () => {
      const wrapper = mount(VacationSummary, {
        props: {
          summary: mockSummary
        }
      })

      const html = wrapper.html()
      expect(html).toContain('dark:text-white')
      expect(html).toContain('dark:bg-gray-')
    })
  })
})
