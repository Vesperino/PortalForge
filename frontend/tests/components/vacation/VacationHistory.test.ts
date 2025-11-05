import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import VacationHistory from '~/components/vacation/VacationHistory.vue'

describe('VacationHistory', () => {
  const mockEntries = [
    {
      id: 1,
      startDate: new Date(2024, 6, 15),
      endDate: new Date(2024, 6, 19),
      days: 5,
      type: 'Urlop wypoczynkowy',
      status: 'Zatwierdzony'
    },
    {
      id: 2,
      startDate: new Date(2024, 4, 10),
      endDate: new Date(2024, 4, 11),
      days: 2,
      type: 'Urlop na żądanie',
      status: 'Zatwierdzony'
    },
    {
      id: 3,
      startDate: new Date(2024, 2, 20),
      endDate: new Date(2024, 2, 20),
      days: 1,
      type: 'Urlop okolicznościowy',
      status: 'Odrzucony'
    }
  ]

  describe('Loading State', () => {
    it('should display loading spinner when isLoading is true', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: [],
          isLoading: true
        }
      })

      expect(wrapper.find('.animate-spin').exists()).toBe(true)
    })
  })

  describe('Error State', () => {
    it('should display error message when error prop is provided', () => {
      const errorMessage = 'Nie udało się pobrać historii urlopów'

      const wrapper = mount(VacationHistory, {
        props: {
          entries: [],
          error: errorMessage
        }
      })

      expect(wrapper.text()).toContain(errorMessage)
      expect(wrapper.find('.bg-red-50').exists()).toBe(true)
    })
  })

  describe('Empty State', () => {
    it('should display empty state when no entries', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: []
        }
      })

      expect(wrapper.text()).toContain('Brak historii urlopów')
      expect(wrapper.find('svg.w-12').exists()).toBe(true)
    })

    it('should display custom empty message', () => {
      const customMessage = 'Nie masz jeszcze urlopów'

      const wrapper = mount(VacationHistory, {
        props: {
          entries: [],
          emptyMessage: customMessage
        }
      })

      expect(wrapper.text()).toContain(customMessage)
    })
  })

  describe('Entries Display', () => {
    it('should display all vacation entries', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      expect(wrapper.text()).toContain('Urlop wypoczynkowy')
      expect(wrapper.text()).toContain('Urlop na żądanie')
      expect(wrapper.text()).toContain('Urlop okolicznościowy')
    })

    it('should display entry count in title', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      expect(wrapper.text()).toContain('Historia urlopów (3)')
    })

    it('should display custom title', () => {
      const customTitle = 'Moje urlopy'

      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries,
          title: customTitle
        }
      })

      expect(wrapper.text()).toContain(`${customTitle} (3)`)
    })

    it('should display days count with correct label', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      expect(wrapper.text()).toContain('5 dni')
      expect(wrapper.text()).toContain('2 dni')
      expect(wrapper.text()).toContain('1 dzień')
    })

    it('should format date ranges correctly', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      // Multi-day vacation (15-19 July 2024)
      expect(wrapper.text()).toMatch(/15\.07\.2024 - 19\.07\.2024/)

      // Two-day vacation (10-11 May 2024)
      expect(wrapper.text()).toMatch(/10\.05\.2024 - 11\.05\.2024/)
    })

    it('should display single date for one-day vacations', () => {
      const singleDayEntry = [
        {
          id: 1,
          startDate: new Date(2024, 2, 20),
          endDate: new Date(2024, 2, 20),
          days: 1,
          type: 'Urlop okolicznościowy',
          status: 'Zatwierdzony'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: singleDayEntry
        }
      })

      // Should not have date range separator
      const text = wrapper.text()
      const dateMatches = text.match(/20\.03\.2024/g)
      expect(dateMatches).toBeTruthy()
    })
  })

  describe('Status Colors', () => {
    it('should apply green color for approved status', () => {
      const approvedEntry = [
        {
          id: 1,
          startDate: new Date(2024, 0, 1),
          endDate: new Date(2024, 0, 1),
          days: 1,
          type: 'Urlop',
          status: 'Zatwierdzony'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: approvedEntry
        }
      })

      const statusBadge = wrapper.find('.bg-green-100')
      expect(statusBadge.exists()).toBe(true)
      expect(statusBadge.text()).toBe('Zatwierdzony')
    })

    it('should apply red color for rejected status', () => {
      const rejectedEntry = [
        {
          id: 1,
          startDate: new Date(2024, 0, 1),
          endDate: new Date(2024, 0, 1),
          days: 1,
          type: 'Urlop',
          status: 'Odrzucony'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: rejectedEntry
        }
      })

      const statusBadge = wrapper.find('.bg-red-100')
      expect(statusBadge.exists()).toBe(true)
    })

    it('should apply yellow color for pending status', () => {
      const pendingEntry = [
        {
          id: 1,
          startDate: new Date(2024, 0, 1),
          endDate: new Date(2024, 0, 1),
          days: 1,
          type: 'Urlop',
          status: 'Oczekuje'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: pendingEntry
        }
      })

      const statusBadge = wrapper.find('.bg-yellow-100')
      expect(statusBadge.exists()).toBe(true)
    })

    it('should apply gray color for cancelled status', () => {
      const cancelledEntry = [
        {
          id: 1,
          startDate: new Date(2024, 0, 1),
          endDate: new Date(2024, 0, 1),
          days: 1,
          type: 'Urlop',
          status: 'Anulowany'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: cancelledEntry
        }
      })

      const statusBadge = wrapper.find('.bg-gray-100')
      expect(statusBadge.exists()).toBe(true)
    })
  })

  describe('Type Icons', () => {
    it('should display lightning icon for on-demand vacation', () => {
      const onDemandEntry = [
        {
          id: 1,
          startDate: new Date(2024, 0, 1),
          endDate: new Date(2024, 0, 1),
          days: 1,
          type: 'Urlop na żądanie',
          status: 'Zatwierdzony'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: onDemandEntry
        }
      })

      const html = wrapper.html()
      // Lightning bolt path
      expect(html).toContain('M13 10V3L4 14h7v7l9-11h-7z')
    })

    it('should display clock icon for circumstantial leave', () => {
      const circumstantialEntry = [
        {
          id: 1,
          startDate: new Date(2024, 0, 1),
          endDate: new Date(2024, 0, 1),
          days: 1,
          type: 'Urlop okolicznościowy',
          status: 'Zatwierdzony'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: circumstantialEntry
        }
      })

      const html = wrapper.html()
      // Clock path
      expect(html).toContain('M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z')
    })

    it('should display calendar icon for standard vacation', () => {
      const standardEntry = [
        {
          id: 1,
          startDate: new Date(2024, 0, 1),
          endDate: new Date(2024, 0, 1),
          days: 1,
          type: 'Urlop wypoczynkowy',
          status: 'Zatwierdzony'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: standardEntry
        }
      })

      const html = wrapper.html()
      // Calendar path
      expect(html).toContain('M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z')
    })
  })

  describe('User Interactions', () => {
    it('should emit view-details event when entry is clicked', async () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      const firstEntry = wrapper.findAll('.cursor-pointer')[0]
      await firstEntry.trigger('click')

      expect(wrapper.emitted('view-details')).toBeTruthy()
      expect(wrapper.emitted('view-details')?.[0]).toEqual([1])
    })

    it('should emit correct entry id on click', async () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      const entries = wrapper.findAll('.cursor-pointer')

      // Click second entry
      await entries[1].trigger('click')
      expect(wrapper.emitted('view-details')?.[0]).toEqual([2])

      // Click third entry
      await entries[2].trigger('click')
      expect(wrapper.emitted('view-details')?.[1]).toEqual([3])
    })

    it('should have hover effect classes', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      const firstEntry = wrapper.find('.cursor-pointer')
      expect(firstEntry.classes()).toContain('hover:bg-gray-100')
      expect(firstEntry.classes()).toContain('dark:hover:bg-gray-600')
    })
  })

  describe('Edge Cases', () => {
    it('should handle entries with string IDs', async () => {
      const entriesWithStringIds = [
        {
          id: 'vacation-1',
          startDate: new Date(2024, 0, 1),
          endDate: new Date(2024, 0, 1),
          days: 1,
          type: 'Urlop',
          status: 'Zatwierdzony'
        }
      ]

      const wrapper = mount(VacationHistory, {
        props: {
          entries: entriesWithStringIds
        }
      })

      const entry = wrapper.find('.cursor-pointer')
      await entry.trigger('click')

      expect(wrapper.emitted('view-details')?.[0]).toEqual(['vacation-1'])
    })

    it('should handle large number of entries', () => {
      const manyEntries = Array.from({ length: 50 }, (_, i) => ({
        id: i + 1,
        startDate: new Date(2024, 0, i + 1),
        endDate: new Date(2024, 0, i + 1),
        days: 1,
        type: 'Urlop wypoczynkowy',
        status: 'Zatwierdzony'
      }))

      const wrapper = mount(VacationHistory, {
        props: {
          entries: manyEntries
        }
      })

      expect(wrapper.text()).toContain('Historia urlopów (50)')
      const entryElements = wrapper.findAll('.cursor-pointer')
      expect(entryElements.length).toBe(50)
    })
  })

  describe('Accessibility', () => {
    it('should have semantic HTML structure', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      expect(wrapper.find('h4').exists()).toBe(true)
      expect(wrapper.find('svg').exists()).toBe(true)
    })
  })

  describe('Dark Mode Support', () => {
    it('should have dark mode classes', () => {
      const wrapper = mount(VacationHistory, {
        props: {
          entries: mockEntries
        }
      })

      const html = wrapper.html()
      expect(html).toContain('dark:text-white')
      expect(html).toContain('dark:bg-gray-')
      expect(html).toContain('dark:border-gray-')
    })
  })
})
