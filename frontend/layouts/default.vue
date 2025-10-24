<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '~/stores/auth'

const authStore = useAuthStore()

const handleLogout = async () => {
  await authStore.logout()
  navigateTo('/auth/login')
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <!-- Sidebar -->
    <Sidebar />

    <!-- Main Content Area -->
    <div class="lg:pl-64">
      <!-- Top Navigation Bar -->
      <header class="sticky top-0 z-30 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div class="px-4 sm:px-6 lg:px-8 py-4">
          <div class="flex items-center justify-between">
            <!-- Left side - could add breadcrumbs here -->
            <div class="flex-1">
              <h1 class="text-lg font-semibold text-gray-900 dark:text-white lg:hidden">
                PortalForge
              </h1>
            </div>

            <!-- Right side - User menu -->
            <div class="flex items-center gap-4">
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
                class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 focus:ring-2 focus:ring-blue-500"
                aria-label="Toggle dark mode"
              >
                <svg class="w-6 h-6 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
              </button>

              <!-- User dropdown -->
              <div class="relative">
                <button
                  type="button"
                  class="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 focus:ring-2 focus:ring-blue-500"
                  aria-label="User menu"
                >
                  <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold text-sm">
                    U
                  </div>
                  <svg class="w-4 h-4 text-gray-600 dark:text-gray-300 hidden sm:block" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
              </div>

              <!-- Logout Button -->
              <button
                type="button"
                class="px-4 py-2 text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg focus:ring-2 focus:ring-red-500 transition-colors"
                @click="handleLogout"
              >
                Wyloguj
              </button>
            </div>
          </div>
        </div>
      </header>

      <!-- Page Content -->
      <main class="px-4 sm:px-6 lg:px-8 py-8">
        <slot />
      </main>

      <!-- Footer -->
      <footer class="border-t border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 mt-auto">
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
  </div>
</template>
