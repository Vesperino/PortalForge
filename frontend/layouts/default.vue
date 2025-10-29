<script setup lang="ts">
import { ref, computed, watch, onBeforeUnmount } from 'vue'
import { useAuthStore } from '~/stores/auth'

const authStore = useAuthStore()
const colorMode = useColorMode()
const router = useRouter()
const isUserMenuOpen = ref(false)
const isMobileSidebarOpen = ref(false)
const isLoggingOut = ref(false)

const isDark = computed(() => colorMode.value === 'dark')

type UnknownRecord = Record<string, unknown>

const { getEmployees } = useMockData()
const employees = getEmployees()
const fallbackUser = employees.length > 0 ? employees[0] : null

const toRecord = (value: unknown): UnknownRecord | null =>
  typeof value === 'object' && value !== null ? value as UnknownRecord : null

const fallbackRecord = toRecord(fallbackUser)

const currentUserRecord = computed<UnknownRecord | null>(() => toRecord(authStore.user) ?? fallbackRecord)

const roleLabels: Record<string, string> = {
  admin: 'Administrator',
  marketing: 'Marketing',
  manager: 'Manager',
  hr: 'HR',
  employee: 'Pracownik'
}

const getStringField = (record: UnknownRecord | null, key: string): string | null => {
  if (!record) {
    return null
  }

  const value = record[key]
  if (typeof value === 'string') {
    const trimmed = value.trim()
    return trimmed.length > 0 ? trimmed : null
  }

  return null
}

const getPositionName = (record: UnknownRecord | null): string | null => {
  if (!record) {
    return null
  }

  const position = record.position
  if (typeof position === 'object' && position !== null) {
    const nameValue = (position as UnknownRecord).name
    if (typeof nameValue === 'string') {
      const trimmed = nameValue.trim()
      return trimmed.length > 0 ? trimmed : null
    }
  }

  return null
}

const formatRoleLabel = (role?: string | null) => {
  if (!role) return null
  const normalized = role.toString().toLowerCase()
  return roleLabels[normalized] ?? normalized.charAt(0).toUpperCase() + normalized.slice(1)
}

const userInitials = computed(() => {
  const record = currentUserRecord.value
  if (!record) {
    return 'U'
  }

  const firstInitial = getStringField(record, 'firstName')?.charAt(0)
  const lastInitial = getStringField(record, 'lastName')?.charAt(0)

  const initials = `${firstInitial ?? ''}${lastInitial ?? ''}`.toUpperCase()
  if (initials) {
    return initials
  }

  const email = getStringField(record, 'email')
  if (email) {
    return email.slice(0, 2).toUpperCase()
  }

  return 'U'
})

const userDisplayName = computed(() => {
  const record = currentUserRecord.value
  if (!record) {
    return 'User'
  }

  const first = getStringField(record, 'firstName')
  const last = getStringField(record, 'lastName')

  if (first || last) {
    return [first, last].filter(Boolean).join(' ')
  }

  return getStringField(record, 'email') ?? 'User'
})

const userDisplayEmail = computed(() => {
  const record = currentUserRecord.value
  return getStringField(record, 'email') ?? 'user@example.com'
})

const userDisplayMeta = computed(() => {
  const record = currentUserRecord.value
  if (!record) {
    return 'Role'
  }

  const positionName = getPositionName(record)
  if (positionName) {
    return positionName
  }

  return formatRoleLabel(getStringField(record, 'role')) ?? 'Role'
})

const { logout: performLogout } = useAuth()

function forceTextColorUpdate() {
  if (import.meta.client) {
    document.documentElement.classList.add('force-color-refresh')
    setTimeout(() => {
      document.documentElement.classList.remove('force-color-refresh')
    }, 50)
  }
}

watch(() => isDark.value, () => {
  forceTextColorUpdate()
})

const toggleDarkMode = () => {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

const toggleUserMenu = () => {
  isUserMenuOpen.value = !isUserMenuOpen.value
}

const closeUserMenu = () => {
  isUserMenuOpen.value = false
}

const toggleMobileSidebar = () => {
  isMobileSidebarOpen.value = !isMobileSidebarOpen.value
}

const closeMobileSidebar = () => {
  isMobileSidebarOpen.value = false
}

const handleLogout = async () => {
  if (isLoggingOut.value) {
    return
  }

  isLoggingOut.value = true
  closeUserMenu()

  try {
    await performLogout()
  } catch (error) {
    console.error('Logout error:', error)
  } finally {
    isLoggingOut.value = false
  }
}

let documentClickHandler: ((event: MouseEvent) => void) | null = null

onMounted(() => {
  if (import.meta.client) {
    documentClickHandler = (e: MouseEvent) => {
      const target = e.target as HTMLElement
      // Check if click is outside both the dropdown and the button that opens it
      if (!target.closest('.user-menu') && isUserMenuOpen.value) {
        isUserMenuOpen.value = false
      }
    }

    document.addEventListener('click', documentClickHandler)
  }
})

onBeforeUnmount(() => {
  if (documentClickHandler) {
    document.removeEventListener('click', documentClickHandler)
    documentClickHandler = null
  }
})

watch(
  () => router.currentRoute.value.fullPath,
  () => {
    closeUserMenu()
    closeMobileSidebar()
  }
)
</script>

<template>
  <div class="app-layout">
    <LayoutSidebar :is-open="isMobileSidebarOpen" @close="closeMobileSidebar" />

    <div class="app-main">
      <header class="app-header">
        <button aria-label="Toggle menu" class="hamburger-btn" @click="toggleMobileSidebar">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>

        <div class="header-actions">
          <button aria-label="Notifications" class="header-icon-btn">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
            <span class="notification-dot" />
          </button>

          <button aria-label="Toggle dark mode" class="header-icon-btn" @click="toggleDarkMode">
            <svg v-if="isDark" class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            <svg v-else class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
            </svg>
          </button>

          <div class="user-menu">
            <button aria-label="User menu" class="user-menu-btn" @click="toggleUserMenu">
              <div class="user-avatar">
                {{ userInitials }}
              </div>
              <svg class="chevron-icon" :class="{ 'is-open': isUserMenuOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </button>

            <transition name="dropdown">
              <div v-if="isUserMenuOpen" class="user-dropdown">
                <div class="user-dropdown-info">
                  <p class="user-dropdown-name">
                    {{ userDisplayName }}
                  </p>
                  <p class="user-dropdown-email">{{ userDisplayEmail }}</p>
                  <p class="user-dropdown-position">{{ userDisplayMeta }}</p>
                </div>

                <div class="user-dropdown-actions">
                  <NuxtLink to="/dashboard/account" class="dropdown-action-item" @click="closeUserMenu">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                    My profile
                  </NuxtLink>
                  <button class="dropdown-action-item" @click="handleLogout">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                    </svg>
                    Log out
                  </button>
                </div>
              </div>
            </transition>
          </div>
        </div>
      </header>

      <main class="app-content">
        <slot />
      </main>

      <footer class="app-footer">
        <p>&copy; 2025 PortalForge. All rights reserved.</p>
        <div class="footer-links">
          <a href="#">Privacy policy</a>
          <a href="#">Contact</a>
        </div>
      </footer>
    </div>
  </div>
</template>












