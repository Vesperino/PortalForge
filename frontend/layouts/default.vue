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

const { getEmployees } = useMockData()
const employees = getEmployees()
const fallbackUser = employees.length > 0 ? employees[0] : null

const authUser = computed(() => authStore.user)

const currentUser = computed(() => authUser.value ?? fallbackUser ?? null)

const roleLabels: Record<string, string> = {
  admin: 'Administrator',
  marketing: 'Marketing',
  manager: 'Manager',
  hr: 'HR',
  employee: 'Pracownik'
}

const formatRoleLabel = (role?: string | null) => {
  if (!role) return null
  const normalized = role.toString().toLowerCase()
  return roleLabels[normalized] ?? normalized.charAt(0).toUpperCase() + normalized.slice(1)
}

const userInitials = computed(() => {
  const user: any = currentUser.value
  if (!user) {
    return 'U'
  }

  const firstInitial = user.firstName?.trim()?.charAt(0)
  const lastInitial = user.lastName?.trim()?.charAt(0)

  const initials = `${firstInitial ?? ''}${lastInitial ?? ''}`.toUpperCase()
  if (initials) {
    return initials
  }

  if (user.email) {
    return user.email.slice(0, 2).toUpperCase()
  }

  return 'U'
})

const userDisplayName = computed(() => {
  const user: any = currentUser.value
  if (!user) {
    return 'Uzytkownik'
  }

  const first = user.firstName?.trim()
  const last = user.lastName?.trim()

  if (first || last) {
    return [first, last].filter(Boolean).join(' ')
  }

  return user.email ?? 'Uzytkownik'
})

const userDisplayEmail = computed(() => {
  const user: any = currentUser.value
  return user?.email ?? 'user@example.com'
})

const userDisplayMeta = computed(() => {
  const user: any = currentUser.value

  if (!user) {
    return 'Stanowisko'
  }

  const positionName = user.position?.name
  if (positionName) {
    return positionName
  }

  return formatRoleLabel(user.role) ?? 'Stanowisko'
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
          <button aria-label="Powiadomienia" class="header-icon-btn">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
            <span class="notification-dot" />
          </button>

          <button aria-label="Tryb ciemny" class="header-icon-btn" @click="toggleDarkMode">
            <svg v-if="isDark" class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            <svg v-else class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
            </svg>
          </button>

          <div class="user-menu">
            <button aria-label="Menu uĹĽytkownika" class="user-menu-btn" @click="toggleUserMenu">
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
                    {{ currentUser ? `${currentUser.firstName} ${currentUser.lastName}` : 'UĹĽytkownik' }}
                  </p>
                  <p class="user-dropdown-email">{{ userDisplayEmail }}</p>
                  <p class="user-dropdown-position">{{ userDisplayMeta }}</p>
                </div>

                <div class="user-dropdown-actions">
                  <NuxtLink to="/dashboard/account" class="dropdown-action-item" @click="closeUserMenu">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                    MĂłj profil
                  </NuxtLink>
                  <button class="dropdown-action-item" @click="handleLogout">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                    </svg>
                    Wyloguj
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
        <p>&copy; 2025 PortalForge. Wszystkie prawa zastrzeĹĽone.</p>
        <div class="footer-links">
          <a href="#">Polityka prywatnoĹ›ci</a>
          <a href="#">Kontakt</a>
        </div>
      </footer>
    </div>
  </div>
</template>

