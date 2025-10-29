<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Panel Administracyjny</h1>
        <p class="mt-2 text-gray-600">Zarządzaj użytkownikami, rolami i uprawnieniami</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <!-- Users Card -->
        <NuxtLink
          to="/admin/users"
          class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-blue-100 rounded-lg">
              <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 mb-2">Użytkownicy</h2>
          <p class="text-gray-600">Zarządzaj kontami użytkowników</p>
        </NuxtLink>

        <!-- Role Groups Card -->
        <NuxtLink
          to="/admin/roles"
          class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-green-100 rounded-lg">
              <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 mb-2">Grupy Ról</h2>
          <p class="text-gray-600">Zarządzaj grupami ról i uprawnieniami</p>
        </NuxtLink>

        <!-- Audit Logs Card -->
        <NuxtLink
          to="/admin/audit"
          class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-purple-100 rounded-lg">
              <svg class="w-8 h-8 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 mb-2">Logi Audytu</h2>
          <p class="text-gray-600">Przeglądaj historię zmian</p>
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

