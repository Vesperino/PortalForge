<script setup lang="ts">
import { ref, computed } from 'vue'
import { useAuthStore } from '~/stores/auth'

const authStore = useAuthStore()
const colorMode = useColorMode()
const isUserMenuOpen = ref(false)
const isMobileSidebarOpen = ref(false)

// Get current user from mock data
const { getEmployees } = useMockData()
const employees = getEmployees()
const currentUser = computed(() => employees.length > 0 ? employees[0] : null)

// Employee vacation and time-off data (mock data for intranet)
const vacationDaysTotal = 26
const vacationDaysUsed = 8
const vacationDaysLeft = computed(() => vacationDaysTotal - vacationDaysUsed)
const sickDaysThisYear = 3

const toggleDarkMode = () => {
  colorMode.value = colorMode.value === 'dark' ? 'light' : 'dark'
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

// Close dropdown when clicking outside
onMounted(() => {
  if (process.client) {
    document.addEventListener('click', (e) => {
      const target = e.target as HTMLElement
      if (!target.closest('.user-dropdown')) {
        isUserMenuOpen.value = false
      }
    })
  }
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors duration-300">
    <div class="flex h-screen">
      <!-- Desktop Sidebar -->
      <Sidebar />

      <!-- Main Content Area -->
      <div class="flex-1 flex flex-col">
        <!-- Top Navigation Bar -->
        <header class="bg-white dark:bg-gray-800 shadow-sm z-10 border-b border-gray-200 dark:border-gray-700">
          <div class="flex items-center justify-between p-4">
            <!-- Mobile menu button -->
            <button
              @click="toggleMobileSidebar"
              class="md:hidden p-2 rounded-md text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
              aria-label="Toggle sidebar"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>

            <!-- Right side - User menu -->
            <div class="flex items-center gap-2 md:gap-4">
              <!-- Notifications -->
              <button
                type="button"
                class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 focus:ring-2 focus:ring-blue-500 relative"
                aria-label="Notifications"
              >
                <svg class="w-6 h-6 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                </svg>
                <!-- Notification badge -->
                <span class="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full" />
              </button>

              <!-- Dark mode toggle -->
              <button
                type="button"
                class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 focus:ring-2 focus:ring-blue-500 transition-colors"
                aria-label="Toggle dark mode"
                @click="toggleDarkMode"
              >
                <!-- Sun icon for light mode -->
                <svg v-if="colorMode.value === 'dark'" class="w-6 h-6 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                <!-- Moon icon for dark mode -->
                <svg v-else class="w-6 h-6 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
              </button>

              <!-- User dropdown -->
              <div class="relative user-dropdown">
                <button
                  type="button"
                  class="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 focus:ring-2 focus:ring-blue-500 transition-colors"
                  aria-label="User menu"
                  @click="toggleUserMenu"
                >
                  <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold text-sm">
                    {{ currentUser ? ((currentUser.firstName?.[0] || '') + (currentUser.lastName?.[0] || '')) : 'U' }}
                  </div>
                  <svg class="w-4 h-4 text-gray-600 dark:text-gray-300 hidden sm:block transition-transform" :class="{ 'rotate-180': isUserMenuOpen }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </button>

                <!-- Dropdown menu -->
                <Transition
                  enter-active-class="transition ease-out duration-100"
                  enter-from-class="transform opacity-0 scale-95"
                  enter-to-class="transform opacity-100 scale-100"
                  leave-active-class="transition ease-in duration-75"
                  leave-from-class="transform opacity-100 scale-100"
                  leave-to-class="transform opacity-0 scale-95"
                >
                  <div
                    v-if="isUserMenuOpen"
                    class="absolute right-0 mt-2 w-80 rounded-lg shadow-lg bg-white dark:bg-gray-800 ring-1 ring-black ring-opacity-5 divide-y divide-gray-100 dark:divide-gray-700 z-50"
                  >
                    <!-- User info -->
                    <div class="px-4 py-3">
                      <p class="text-sm font-medium text-gray-900 dark:text-white">
                        {{ currentUser ? `${currentUser.firstName} ${currentUser.lastName}` : 'Użytkownik' }}
                      </p>
                      <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                        {{ currentUser?.email || 'user@example.com' }}
                      </p>
                      <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                        {{ currentUser?.position || 'Stanowisko' }}
                      </p>
                    </div>

                    <!-- Vacation & Time Off Stats -->
                    <div class="px-4 py-3 bg-blue-50 dark:bg-blue-900/20">
                      <p class="text-xs font-semibold text-gray-700 dark:text-gray-300 mb-2">
                        Urlopy i absencje
                      </p>
                      <div class="space-y-2">
                        <div class="flex items-center justify-between">
                          <div class="flex items-center gap-2">
                            <svg class="w-4 h-4 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                            </svg>
                            <span class="text-xs text-gray-600 dark:text-gray-400">Dni urlopu pozostało</span>
                          </div>
                          <span class="text-sm font-bold text-blue-600 dark:text-blue-400">
                            {{ vacationDaysLeft }} / {{ vacationDaysTotal }}
                          </span>
                        </div>
                        <div class="flex items-center justify-between">
                          <div class="flex items-center gap-2">
                            <svg class="w-4 h-4 text-orange-600 dark:text-orange-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                            </svg>
                            <span class="text-xs text-gray-600 dark:text-gray-400">Dni chorobowe w tym roku</span>
                          </div>
                          <span class="text-sm font-bold text-orange-600 dark:text-orange-400">{{ sickDaysThisYear }}</span>
                        </div>
                        <div class="flex items-center justify-between">
                          <div class="flex items-center gap-2">
                            <svg class="w-4 h-4 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                            </svg>
                            <span class="text-xs text-gray-600 dark:text-gray-400">Staż pracy</span>
                          </div>
                          <span class="text-sm font-bold text-green-600 dark:text-green-400">{{ currentUser?.yearsOfService || 0 }} lat</span>
                        </div>
                      </div>
                    </div>

                    <!-- Menu items -->
                    <div class="py-1">
                      <NuxtLink
                        to="/dashboard/account"
                        class="block px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                        @click="closeUserMenu"
                      >
                        <div class="flex items-center gap-2">
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                          </svg>
                          Moje konto
                        </div>
                      </NuxtLink>
                      <button
                        type="button"
                        class="w-full text-left px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                      >
                        <div class="flex items-center gap-2">
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                          </svg>
                          Ustawienia
                        </div>
                      </button>
                    </div>

                    <!-- Logout -->
                    <div class="py-1">
                      <button
                        type="button"
                        class="w-full text-left px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                        @click="handleLogout"
                      >
                        <div class="flex items-center gap-2">
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                          </svg>
                          Wyloguj
                        </div>
                      </button>
                    </div>
                  </div>
                </Transition>
              </div>
            </div>
          </div>
        </header>

        <!-- Page Content -->
        <main class="flex-1 overflow-y-auto px-4 sm:px-6 lg:px-8 py-8">
          <slot />
        </main>

        <!-- Footer -->
        <footer class="border-t border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
          <div class="px-4 sm:px-6 lg:px-8 py-6">
            <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
              <p class="text-sm text-gray-600 dark:text-gray-400">
                &copy; {{ new Date().getFullYear() }} PortalForge. Wszystkie prawa zastrzeżone.
              </p>
              <div class="flex gap-6">
                <a href="#" class="text-sm text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400">
                  Pomoc
                </a>
                <a href="#" class="text-sm text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400">
                  Polityka prywatności
                </a>
                <a href="#" class="text-sm text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400">
                  Kontakt
                </a>
              </div>
            </div>
          </div>
        </footer>
      </div>

      <!-- Mobile Sidebar -->
      <div
        v-if="isMobileSidebarOpen"
        class="md:hidden fixed inset-0 z-40 bg-gray-900 bg-opacity-50"
        @click="closeMobileSidebar"
      >
        <div
          class="absolute inset-y-0 left-0 w-64 bg-white dark:bg-gray-800 shadow-md"
          @click.stop
        >
          <div class="flex flex-col h-full">
            <!-- Mobile Sidebar Header -->
            <div class="p-4 border-b border-gray-200 dark:border-gray-700 flex justify-between items-center">
              <h2 class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                PortalForge
              </h2>
              <button
                @click="closeMobileSidebar"
                class="p-2 rounded-md text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700"
                aria-label="Close navigation"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <!-- Mobile Navigation - reuse navItems from Sidebar -->
            <nav class="flex-1 p-4 space-y-2 overflow-y-auto">
              <NuxtLink
                to="/dashboard"
                class="flex items-center p-3 rounded-lg transition-colors hover:bg-gray-100 dark:hover:bg-gray-700"
                active-class="bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
                @click="closeMobileSidebar"
              >
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                </svg>
                <span>Strona główna</span>
              </NuxtLink>
              <NuxtLink
                to="/dashboard/calendar"
                class="flex items-center p-3 rounded-lg transition-colors hover:bg-gray-100 dark:hover:bg-gray-700"
                active-class="bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
                @click="closeMobileSidebar"
              >
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span>Kalendarz</span>
              </NuxtLink>
              <NuxtLink
                to="/dashboard/news"
                class="flex items-center p-3 rounded-lg transition-colors hover:bg-gray-100 dark:hover:bg-gray-700"
                active-class="bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
                @click="closeMobileSidebar"
              >
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z" />
                </svg>
                <span>Aktualności</span>
              </NuxtLink>
              <NuxtLink
                to="/dashboard/account"
                class="flex items-center p-3 rounded-lg transition-colors hover:bg-gray-100 dark:hover:bg-gray-700"
                active-class="bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
                @click="closeMobileSidebar"
              >
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <span>Moje konto</span>
              </NuxtLink>
              <NuxtLink
                to="/dashboard/organization"
                class="flex items-center p-3 rounded-lg transition-colors hover:bg-gray-100 dark:hover:bg-gray-700"
                active-class="bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
                @click="closeMobileSidebar"
              >
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
                <span>Struktura organizacyjna</span>
              </NuxtLink>
              <NuxtLink
                to="/dashboard/documents"
                class="flex items-center p-3 rounded-lg transition-colors hover:bg-gray-100 dark:hover:bg-gray-700"
                active-class="bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
                @click="closeMobileSidebar"
              >
                <svg class="w-5 h-5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <span>Dokumenty</span>
              </NuxtLink>
            </nav>

            <!-- Mobile Footer with Dark Mode -->
            <div class="p-4 border-t border-gray-200 dark:border-gray-700">
              <div class="flex justify-between items-center">
                <span class="text-sm text-gray-600 dark:text-gray-400">Motyw</span>
                <button
                  @click="toggleDarkMode"
                  class="p-2 rounded-md hover:bg-gray-100 dark:hover:bg-gray-700"
                >
                  <svg v-if="colorMode.value === 'dark'" class="w-5 h-5 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                  </svg>
                  <svg v-else class="w-5 h-5 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
