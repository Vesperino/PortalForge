import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import Sidebar from '~/components/layout/Sidebar.vue'
import { useAuthStore } from '~/stores/auth'
import { UserRole } from '~/types/auth'

const mockRouter = {
  push: vi.fn()
}

const mockRoute = {
  path: '/dashboard'
}

vi.mock('#app', () => ({
  useRoute: () => mockRoute,
  useRouter: () => mockRouter
}))

describe('Sidebar', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    mockRoute.path = '/dashboard'

    globalThis.useRoute = vi.fn(() => mockRoute)
    globalThis.useRouter = vi.fn(() => mockRouter)
  })

  const globalStubs = {
    NuxtLink: {
      template: '<a :href="to" :class="$attrs.class" @click="$emit(\'click\')"><slot /></a>',
      props: ['to']
    }
  }

  describe('Basic Rendering', () => {
    it('should render sidebar component', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('.app-sidebar').exists()).toBe(true)
    })

    it('should display PortalForge logo', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('.sidebar-logo').text()).toBe('PortalForge')
    })

    it('should have close button', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const closeButton = wrapper.find('.sidebar-close-btn')
      expect(closeButton.exists()).toBe(true)
    })
  })

  describe('Open/Close State', () => {
    it('should apply is-open class when isOpen is true', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: true
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('.app-sidebar').classes()).toContain('is-open')
    })

    it('should not apply is-open class when isOpen is false', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('.app-sidebar').classes()).not.toContain('is-open')
    })

    it('should show overlay when isOpen is true', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: true
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('.sidebar-overlay').exists()).toBe(true)
    })

    it('should not show overlay when isOpen is false', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('.sidebar-overlay').exists()).toBe(false)
    })
  })

  describe('Close Events', () => {
    it('should emit close event when close button is clicked', async () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: true
        },
        global: {
          stubs: globalStubs
        }
      })

      const closeButton = wrapper.find('.sidebar-close-btn')
      await closeButton.trigger('click')

      expect(wrapper.emitted('close')).toBeTruthy()
    })

    it('should emit close event when overlay is clicked', async () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: true
        },
        global: {
          stubs: globalStubs
        }
      })

      const overlay = wrapper.find('.sidebar-overlay')
      await overlay.trigger('click')

      expect(wrapper.emitted('close')).toBeTruthy()
    })

    it('should emit close event when navigation item is clicked', async () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: true
        },
        global: {
          stubs: globalStubs
        }
      })

      const navItem = wrapper.find('.sidebar-nav-item')
      await navItem.trigger('click')

      expect(wrapper.emitted('close')).toBeTruthy()
    })
  })

  describe('Navigation Items', () => {
    it('should render navigation items for regular user', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const navItems = wrapper.findAll('.sidebar-nav-item')
      expect(navItems.length).toBeGreaterThan(0)
    })

    it('should include home navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Strona główna')
    })

    it('should include calendar navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Kalendarz wydarzeń')
    })

    it('should include vacation calendar navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Kalendarz urlopów')
    })

    it('should include news navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Aktualności')
    })

    it('should include account navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Moje konto')
    })

    it('should include organization structure navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Struktura organizacyjna')
    })

    it('should include documents navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Dokumenty')
    })

    it('should include requests navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Wnioski')
    })

    it('should include services navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Serwisy wewnętrzne')
    })

    it('should include AI assistant navigation item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Asystent AI')
    })
  })

  describe('Admin Navigation Items', () => {
    it('should not show admin items for regular user', () => {
      const authStore = useAuthStore()
      authStore.setUser({
        id: '1',
        email: 'user@example.com',
        firstName: 'John',
        lastName: 'Doe',
        isEmailVerified: true,
        role: UserRole.Employee
      })

      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).not.toContain('Logi audytu')
      expect(wrapper.text()).not.toContain('Zarządzanie urlopami')
    })

    it('should show admin items for Admin role', () => {
      const authStore = useAuthStore()
      authStore.setUser({
        id: '1',
        email: 'admin@example.com',
        firstName: 'Admin',
        lastName: 'User',
        isEmailVerified: true,
        role: UserRole.Admin
      })
      authStore.user!.roles = ['Admin']

      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Logi audytu')
      expect(wrapper.text()).toContain('Zarządzanie urlopami')
    })

    it('should show admin items for HR role', () => {
      const authStore = useAuthStore()
      authStore.setUser({
        id: '1',
        email: 'hr@example.com',
        firstName: 'HR',
        lastName: 'User',
        isEmailVerified: true,
        role: UserRole.HR
      })
      authStore.user!.roles = ['HR']

      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.text()).toContain('Logi audytu')
      expect(wrapper.text()).toContain('Zarządzanie urlopami')
    })
  })

  describe('Active Navigation State', () => {
    it('should apply active class to current route', () => {
      mockRoute.path = '/dashboard'

      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const homeLink = wrapper.find('a[href="/dashboard"]')
      expect(homeLink.classes()).toContain('is-active')
    })

    it('should not apply active class to non-current route', () => {
      mockRoute.path = '/dashboard'

      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const newsLink = wrapper.find('a[href="/dashboard/news"]')
      expect(newsLink.classes()).not.toContain('is-active')
    })

    it('should update active state when route changes', async () => {
      mockRoute.path = '/dashboard/news'

      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const newsLink = wrapper.find('a[href="/dashboard/news"]')
      expect(newsLink.classes()).toContain('is-active')
    })
  })

  describe('Navigation Links', () => {
    it('should have correct href for home', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const homeLink = wrapper.find('a[href="/dashboard"]')
      expect(homeLink.exists()).toBe(true)
    })

    it('should have correct href for calendar', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const calendarLink = wrapper.find('a[href="/dashboard/calendar"]')
      expect(calendarLink.exists()).toBe(true)
    })

    it('should have correct href for vacation calendar', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const vacationLink = wrapper.find('a[href="/dashboard/team/vacation-calendar"]')
      expect(vacationLink.exists()).toBe(true)
    })

    it('should have correct href for news', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const newsLink = wrapper.find('a[href="/dashboard/news"]')
      expect(newsLink.exists()).toBe(true)
    })

    it('should have correct href for account', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const accountLink = wrapper.find('a[href="/dashboard/account"]')
      expect(accountLink.exists()).toBe(true)
    })

    it('should have correct href for organization', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const orgLink = wrapper.find('a[href="/dashboard/organization"]')
      expect(orgLink.exists()).toBe(true)
    })

    it('should have correct href for documents', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const docsLink = wrapper.find('a[href="/dashboard/documents"]')
      expect(docsLink.exists()).toBe(true)
    })

    it('should have correct href for requests', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const requestsLink = wrapper.find('a[href="/dashboard/requests"]')
      expect(requestsLink.exists()).toBe(true)
    })

    it('should have correct href for services', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const servicesLink = wrapper.find('a[href="/dashboard/services"]')
      expect(servicesLink.exists()).toBe(true)
    })

    it('should have correct href for AI chat', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const aiLink = wrapper.find('a[href="/dashboard/chat-ai"]')
      expect(aiLink.exists()).toBe(true)
    })
  })

  describe('Icons', () => {
    it('should render icons for all navigation items', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const icons = wrapper.findAll('.nav-icon svg')
      const navItems = wrapper.findAll('.sidebar-nav-item')

      expect(icons.length).toBe(navItems.length)
    })

    it('should have icon container for each nav item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const navItems = wrapper.findAll('.sidebar-nav-item')
      navItems.forEach(item => {
        expect(item.find('.nav-icon').exists()).toBe(true)
      })
    })
  })

  describe('Navigation Labels', () => {
    it('should have label container for each nav item', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      const navItems = wrapper.findAll('.sidebar-nav-item')
      navItems.forEach(item => {
        expect(item.find('.nav-label').exists()).toBe(true)
      })
    })
  })

  describe('Default Props', () => {
    it('should default isOpen to false when not provided', () => {
      const wrapper = mount(Sidebar, {
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('.app-sidebar').classes()).not.toContain('is-open')
    })
  })

  describe('Accessibility', () => {
    it('should have aria-label on close button', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: true
        },
        global: {
          stubs: globalStubs
        }
      })

      const closeButton = wrapper.find('.sidebar-close-btn')
      expect(closeButton.attributes('aria-label')).toBe('Zamknij menu')
    })

    it('should have nav element for navigation', () => {
      const wrapper = mount(Sidebar, {
        props: {
          isOpen: false
        },
        global: {
          stubs: globalStubs
        }
      })

      expect(wrapper.find('nav.sidebar-nav').exists()).toBe(true)
    })
  })
})
