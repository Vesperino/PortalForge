<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Zarządzanie Grupami Ról</h1>
        <p class="mt-2 text-gray-600">Przeglądaj grupy ról i przypisane uprawnienia</p>
      </div>

      <!-- Loading State -->
      <div v-if="roleGroupsStore.loading" class="bg-white rounded-lg shadow-md p-12 text-center">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">Ładowanie grup ról...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="roleGroupsStore.error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
        <p class="text-red-800">{{ roleGroupsStore.error }}</p>
      </div>

      <!-- Role Groups Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="roleGroup in roleGroupsStore.roleGroups"
          :key="roleGroup.id"
          class="bg-white rounded-lg shadow-md p-6"
        >
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-xl font-semibold text-gray-900">{{ roleGroup.name }}</h3>
            <span
              v-if="roleGroup.isSystemRole"
              class="px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800"
            >
              System
            </span>
          </div>
          
          <p class="text-gray-600 mb-4">{{ roleGroup.description }}</p>
          
          <div class="mb-4">
            <div class="flex items-center text-sm text-gray-500">
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
              {{ roleGroup.userCount }} {{ roleGroup.userCount === 1 ? 'użytkownik' : 'użytkowników' }}
            </div>
          </div>

          <div class="border-t border-gray-200 pt-4">
            <h4 class="text-sm font-medium text-gray-700 mb-2">Uprawnienia ({{ roleGroup.permissions.length }})</h4>
            <div class="space-y-1 max-h-48 overflow-y-auto">
              <div
                v-for="permission in roleGroup.permissions"
                :key="permission.id"
                class="text-xs text-gray-600 flex items-start"
              >
                <svg class="w-4 h-4 mr-1 text-green-500 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
                </svg>
                <span>{{ permission.description }}</span>
              </div>
            </div>
          </div>
        </div>
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
  title: 'Grupy Ról - Panel Administracyjny',
})

const roleGroupsStore = useRoleGroupsStore()

// Fetch role groups on mount
onMounted(() => {
  roleGroupsStore.fetchRoleGroups(true)
})
</script>

