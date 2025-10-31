<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Panel Administracyjny</h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">Zarządzaj użytkownikami, rolami i uprawnieniami</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <!-- Users Card -->
        <NuxtLink
          to="/admin/users"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-blue-100 dark:bg-blue-900/30 rounded-lg">
              <svg class="w-8 h-8 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">Użytkownicy</h2>
          <p class="text-gray-600 dark:text-gray-400">Zarządzaj kontami użytkowników</p>
        </NuxtLink>

        <!-- Organizational Structure Card -->
        <NuxtLink
          to="/admin/structure"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-cyan-100 dark:bg-cyan-900/30 rounded-lg">
              <svg class="w-8 h-8 text-cyan-600 dark:text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">Struktura organizacyjna</h2>
          <p class="text-gray-600 dark:text-gray-400">Zarządzaj działami i hierarchią firmy</p>
        </NuxtLink>

        <!-- Role Groups Card -->
        <NuxtLink
          to="/admin/roles"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-green-100 dark:bg-green-900/30 rounded-lg">
              <svg class="w-8 h-8 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">Grupy Ról</h2>
          <p class="text-gray-600 dark:text-gray-400">Zarządzaj grupami ról i uprawnieniami</p>
        </NuxtLink>

        <!-- Organizational Permissions Card -->
        <NuxtLink
          to="/admin/permissions/organizational"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-teal-100 dark:bg-teal-900/30 rounded-lg">
              <svg class="w-8 h-8 text-teal-600 dark:text-teal-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">Uprawnienia widoczności</h2>
          <p class="text-gray-600 dark:text-gray-400">Zarządzaj dostępem do działów organizacyjnych</p>
        </NuxtLink>

        <!-- Audit Logs Card -->
        <NuxtLink
          to="/admin/audit"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-purple-100 dark:bg-purple-900/30 rounded-lg">
              <svg class="w-8 h-8 text-purple-600 dark:text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">Logi Audytu</h2>
          <p class="text-gray-600 dark:text-gray-400">Przeglądaj historię zmian</p>
        </NuxtLink>

        <!-- System Settings Card -->
        <NuxtLink
          to="/admin/settings/system"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-orange-100 dark:bg-orange-900/30 rounded-lg">
              <svg class="w-8 h-8 text-orange-600 dark:text-orange-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">Ustawienia systemowe</h2>
          <p class="text-gray-600 dark:text-gray-400">Konfiguracja Storage i innych ustawień globalnych</p>
        </NuxtLink>

        <!-- Request Templates Card -->
        <NuxtLink
          to="/admin/request-templates"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-indigo-100 dark:bg-indigo-900/30 rounded-lg">
              <svg class="w-8 h-8 text-indigo-600 dark:text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">Szablony wniosków</h2>
          <p class="text-gray-600 dark:text-gray-400">Zarządzaj szablonami wniosków i formularzami</p>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default',
})

useHead({
  title: 'Panel Administracyjny - PortalForge',
})
</script>

