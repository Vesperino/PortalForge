<script setup lang="ts">
interface NavItem {
  name: string
  label: string
  icon: string
  path: string
}

const navItems: NavItem[] = [
  {
    name: 'home',
    label: 'Strona główna',
    icon: 'home',
    path: '/dashboard'
  },
  {
    name: 'calendar',
    label: 'Kalendarz',
    icon: 'calendar',
    path: '/dashboard/calendar'
  },
  {
    name: 'news',
    label: 'Aktualności',
    icon: 'news',
    path: '/dashboard/news'
  },
  {
    name: 'account',
    label: 'Moje konto',
    icon: 'account',
    path: '/dashboard/account'
  },
  {
    name: 'organization',
    label: 'Struktura organizacyjna',
    icon: 'org',
    path: '/dashboard/organization'
  },
  {
    name: 'documents',
    label: 'Dokumenty',
    icon: 'documents',
    path: '/dashboard/documents'
  }
]
</script>

<template>
  <!-- Desktop Sidebar - Always visible on md and up -->
  <aside class="w-64 hidden md:flex flex-col bg-white dark:bg-gray-800 shadow-md border-r border-gray-200 dark:border-gray-700 h-screen sticky top-0">
    <div class="flex flex-col h-full">
      <!-- Sidebar Header -->
      <div class="p-4 border-b border-gray-200 dark:border-gray-700">
        <h2 class="text-2xl font-bold text-blue-600 dark:text-blue-400">
          PortalForge
        </h2>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 p-4 space-y-2 overflow-y-auto" aria-label="Main navigation">
        <NuxtLink
          v-for="item in navItems"
          :key="item.name"
          :to="item.path"
          class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors group"
          :class="[
            'hover:bg-gray-100 dark:hover:bg-gray-700',
            'focus:ring-2 focus:ring-blue-500 focus:outline-none'
          ]"
          active-class="bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
        >
            <!-- Icons -->
            <span class="w-6 h-6 flex-shrink-0">
              <svg
                v-if="item.icon === 'home'"
                class="w-6 h-6 text-gray-600 dark:text-gray-400 group-hover:text-blue-600 dark:group-hover:text-blue-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
              </svg>
              <svg
                v-else-if="item.icon === 'calendar'"
                class="w-6 h-6 text-gray-600 dark:text-gray-400 group-hover:text-blue-600 dark:group-hover:text-blue-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <svg
                v-else-if="item.icon === 'news'"
                class="w-6 h-6 text-gray-600 dark:text-gray-400 group-hover:text-blue-600 dark:group-hover:text-blue-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z" />
              </svg>
              <svg
                v-else-if="item.icon === 'account'"
                class="w-6 h-6 text-gray-600 dark:text-gray-400 group-hover:text-blue-600 dark:group-hover:text-blue-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
              <svg
                v-else-if="item.icon === 'org'"
                class="w-6 h-6 text-gray-600 dark:text-gray-400 group-hover:text-blue-600 dark:group-hover:text-blue-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              <svg
                v-else-if="item.icon === 'documents'"
                class="w-6 h-6 text-gray-600 dark:text-gray-400 group-hover:text-blue-600 dark:group-hover:text-blue-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
          </span>
          <span class="text-sm font-medium text-gray-900 dark:text-white">
            {{ item.label }}
          </span>
        </NuxtLink>
      </nav>

      <!-- User Section -->
      <div class="p-4 border-t border-gray-200 dark:border-gray-700">
        <div class="flex items-center gap-3 px-4 py-3">
          <div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold">
            U
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
              Użytkownik
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
              user@example.com
            </p>
          </div>
        </div>
      </div>
    </div>
  </aside>
</template>
