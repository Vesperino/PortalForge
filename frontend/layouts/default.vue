<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useAuthStore } from '~/stores/auth'

const authStore = useAuthStore()
const colorMode = useColorMode()
const isUserMenuOpen = ref(false)
const isMobileSidebarOpen = ref(false)

const isDark = computed(() => colorMode.value === 'dark')

const { getEmployees } = useMockData()
const employees = getEmployees()
const currentUser = computed(() => employees.length > 0 ? employees[0] : null)

function forceTextColorUpdate() {
  if (process.client) {
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
  await authStore.logout()
  navigateTo('/auth/login')
}

onMounted(() => {
  if (process.client) {
    document.addEventListener('click', (e) => {
      const target = e.target as HTMLElement
      if (!target.closest('.user-dropdown')) {
        isUserMenuOpen.value = false
      }
    })

    const router = useRouter()
    router.afterEach(() => {
      closeMobileSidebar()
    })
  }
})
</script>

<template>
  <div class="app-layout">
    <!-- Sidebar -->
    <Sidebar :is-open="isMobileSidebarOpen" @close="closeMobileSidebar" />

    <!-- Main Content -->
    <div class="app-main">
      <!-- Top Header -->
      <header class="app-header">
        <button
          class="hamburger-btn"
          @click="toggleMobileSidebar"
          aria-label="Toggle menu"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>

        <div class="header-right">
          <!-- Notifications -->
          <button class="icon-btn" aria-label="Notifications">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
            <span class="notification-badge"></span>
          </button>

          <!-- Dark Mode Toggle -->
          <button
            class="icon-btn"
            @click="toggleDarkMode"
            aria-label="Toggle dark mode"
          >
            <svg v-if="isDark" class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            <svg v-else class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
            </svg>
          </button>

          <!-- User Menu -->
          <div class="user-dropdown">
            <button
              class="user-btn"
              @click="toggleUserMenu"
              aria-label="User menu"
            >
              <div class="user-avatar">
                {{ currentUser ? ((currentUser.firstName?.[0] || '') + (currentUser.lastName?.[0] || '')) : 'U' }}
              </div>
              <svg class="w-4 h-4 chevron" :class="{ 'chevron-open': isUserMenuOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </button>

            <!-- Dropdown Menu -->
            <transition name="dropdown">
              <div v-if="isUserMenuOpen" class="dropdown-menu">
                <div class="user-info">
                  <p class="user-name">
                    {{ currentUser ? `${currentUser.firstName} ${currentUser.lastName}` : 'Użytkownik' }}
                  </p>
                  <p class="user-email">
                    {{ currentUser?.email || 'user@example.com' }}
                  </p>
                  <p class="user-position">
                    {{ currentUser?.position?.name || 'Stanowisko' }}
                  </p>
                </div>

                <div class="menu-items">
                  <NuxtLink to="/dashboard/account" class="menu-item" @click="closeUserMenu">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                    Mój profil
                  </NuxtLink>
                  <button class="menu-item" @click="handleLogout">
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

      <!-- Page Content -->
      <main class="app-content">
        <slot />
      </main>

      <!-- Footer -->
      <footer class="app-footer">
        <div class="footer-content">
          <p>&copy; 2025 PortalForge. Wszystkie prawa zastrzeżone.</p>
          <div class="footer-links">
            <a href="#">Polityka prywatności</a>
            <a href="#">Kontakt</a>
          </div>
        </div>
      </footer>
    </div>
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  min-height: 100vh;
  background-color: #f9fafb;
}

.dark .app-layout {
  background-color: #111827;
}

.app-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  margin-left: 0;
}

@media (min-width: 768px) {
  .app-main {
    margin-left: 256px;
  }
}

/* Header */
.app-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  background-color: white;
  border-bottom: 1px solid #e5e7eb;
  position: sticky;
  top: 0;
  z-index: 10;
}

.dark .app-header {
  background-color: #1f2937;
  border-bottom-color: #374151;
}

.hamburger-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.5rem;
  border-radius: 0.375rem;
  color: #4b5563;
  background: none;
  border: none;
  cursor: pointer;
}

.hamburger-btn:hover {
  background-color: #f3f4f6;
}

.dark .hamburger-btn {
  color: #9ca3af;
}

.dark .hamburger-btn:hover {
  background-color: #374151;
}

@media (min-width: 768px) {
  .hamburger-btn {
    display: none;
  }
}

.header-right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.icon-btn {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.5rem;
  border-radius: 0.5rem;
  color: #4b5563;
  background: none;
  border: none;
  cursor: pointer;
}

.icon-btn:hover {
  background-color: #f3f4f6;
}

.dark .icon-btn {
  color: #9ca3af;
}

.dark .icon-btn:hover {
  background-color: #374151;
}

.notification-badge {
  position: absolute;
  top: 0.25rem;
  right: 0.25rem;
  width: 0.5rem;
  height: 0.5rem;
  background-color: #ef4444;
  border-radius: 9999px;
}

/* User Dropdown */
.user-dropdown {
  position: relative;
}

.user-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem;
  border-radius: 0.5rem;
  background: none;
  border: none;
  cursor: pointer;
  transition: background-color 0.2s;
}

.user-btn:hover {
  background-color: #f3f4f6;
}

.dark .user-btn:hover {
  background-color: #374151;
}

.user-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: 9999px;
  background-color: #3b82f6;
  color: white;
  font-size: 0.875rem;
  font-weight: 600;
}

.chevron {
  color: #6b7280;
  transition: transform 0.2s;
}

.chevron-open {
  transform: rotate(180deg);
}

.dark .chevron {
  color: #9ca3af;
}

@media (max-width: 640px) {
  .chevron {
    display: none;
  }
}

/* Dropdown Menu */
.dropdown-menu {
  position: absolute;
  right: 0;
  top: calc(100% + 0.5rem);
  width: 16rem;
  background-color: white;
  border-radius: 0.5rem;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
  border: 1px solid #e5e7eb;
  overflow: hidden;
  z-index: 50;
}

.dark .dropdown-menu {
  background-color: #1f2937;
  border-color: #374151;
}

.user-info {
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
}

.dark .user-info {
  border-bottom-color: #374151;
}

.user-name {
  font-size: 0.875rem;
  font-weight: 500;
  color: #111827;
}

.dark .user-name {
  color: white;
}

.user-email {
  font-size: 0.75rem;
  color: #6b7280;
  margin-top: 0.25rem;
}

.dark .user-email {
  color: #9ca3af;
}

.user-position {
  font-size: 0.75rem;
  color: #6b7280;
  margin-top: 0.25rem;
}

.dark .user-position {
  color: #9ca3af;
}

.menu-items {
  padding: 0.25rem;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 100%;
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  color: #374151;
  text-decoration: none;
  background: none;
  border: none;
  border-radius: 0.375rem;
  cursor: pointer;
  transition: background-color 0.2s;
  text-align: left;
}

.menu-item:hover {
  background-color: #f3f4f6;
}

.dark .menu-item {
  color: #d1d5db;
}

.dark .menu-item:hover {
  background-color: #374151;
}

/* Dropdown Transition */
.dropdown-enter-active,
.dropdown-leave-active {
  transition: all 0.2s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Content */
.app-content {
  flex: 1;
  padding: 1.5rem;
}

@media (min-width: 768px) {
  .app-content {
    padding: 2rem;
  }
}

/* Footer */
.app-footer {
  padding: 1.5rem 1rem;
  background-color: white;
  border-top: 1px solid #e5e7eb;
}

.dark .app-footer {
  background-color: #1f2937;
  border-top-color: #374151;
}

.footer-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  font-size: 0.875rem;
  color: #6b7280;
}

@media (min-width: 768px) {
  .footer-content {
    flex-direction: row;
  }
}

.dark .footer-content {
  color: #9ca3af;
}

.footer-links {
  display: flex;
  gap: 1.5rem;
}

.footer-links a {
  color: #6b7280;
  text-decoration: none;
  transition: color 0.2s;
}

.footer-links a:hover {
  color: #3b82f6;
}

.dark .footer-links a {
  color: #9ca3af;
}

.dark .footer-links a:hover {
  color: #60a5fa;
}
</style>
