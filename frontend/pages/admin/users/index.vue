<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8 flex justify-between items-center">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Zarządzanie Użytkownikami</h1>
          <p class="mt-2 text-gray-600">Przeglądaj i zarządzaj kontami użytkowników</p>
        </div>
        <NuxtLink
          to="/admin/users/create"
          class="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Dodaj Użytkownika
        </NuxtLink>
      </div>

      <!-- Filters -->
      <div class="bg-white rounded-lg shadow-md p-6 mb-6">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Szukaj</label>
            <input
              v-model="filters.searchTerm"
              type="text"
              placeholder="Email, imię, nazwisko..."
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @input="debouncedSearch"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Dział</label>
            <input
              v-model="filters.department"
              type="text"
              placeholder="Wszystkie działy"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @input="debouncedSearch"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Rola</label>
            <select
              v-model="filters.role"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @change="fetchUsers"
            >
              <option value="">Wszystkie role</option>
              <option value="Admin">Admin</option>
              <option value="Manager">Manager</option>
              <option value="HR">HR</option>
              <option value="Marketing">Marketing</option>
              <option value="Employee">Employee</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Status</label>
            <select
              v-model="filters.isActive"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @change="fetchUsers"
            >
              <option :value="undefined">Wszyscy</option>
              <option :value="true">Aktywni</option>
              <option :value="false">Nieaktywni</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="adminStore.loading" class="bg-white rounded-lg shadow-md p-12 text-center">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">Ładowanie użytkowników...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="adminStore.error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
        <p class="text-red-800">{{ adminStore.error }}</p>
      </div>

      <!-- Users Table -->
      <div v-else class="bg-white rounded-lg shadow-md overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Użytkownik
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Dział / Stanowisko
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Rola
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Akcje
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="user in adminStore.users" :key="user.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="flex-shrink-0 h-10 w-10">
                    <div class="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                      <span class="text-blue-600 font-medium">
                        {{ user.firstName[0] }}{{ user.lastName[0] }}
                      </span>
                    </div>
                  </div>
                  <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900">
                      {{ user.firstName }} {{ user.lastName }}
                    </div>
                    <div class="text-sm text-gray-500">{{ user.email }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ user.department }}</div>
                <div class="text-sm text-gray-500">{{ user.position }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                  :class="getRoleBadgeClass(user.role)"
                >
                  {{ user.role }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                  :class="user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                >
                  {{ user.isActive ? 'Aktywny' : 'Nieaktywny' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <NuxtLink
                  :to="`/admin/users/${user.id}`"
                  class="text-blue-600 hover:text-blue-900 mr-4"
                >
                  Edytuj
                </NuxtLink>
                <button
                  @click="confirmDelete(user)"
                  class="text-red-600 hover:text-red-900"
                >
                  Usuń
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Pagination -->
        <div class="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
          <div class="flex-1 flex justify-between sm:hidden">
            <button
              @click="previousPage"
              :disabled="adminStore.pageNumber === 1"
              class="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
            >
              Poprzednia
            </button>
            <button
              @click="nextPage"
              :disabled="adminStore.pageNumber === adminStore.totalPages"
              class="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
            >
              Następna
            </button>
          </div>
          <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
            <div>
              <p class="text-sm text-gray-700">
                Wyświetlanie
                <span class="font-medium">{{ (adminStore.pageNumber - 1) * adminStore.pageSize + 1 }}</span>
                -
                <span class="font-medium">{{ Math.min(adminStore.pageNumber * adminStore.pageSize, adminStore.totalCount) }}</span>
                z
                <span class="font-medium">{{ adminStore.totalCount }}</span>
                wyników
              </p>
            </div>
            <div>
              <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                <button
                  @click="previousPage"
                  :disabled="adminStore.pageNumber === 1"
                  class="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                >
                  Poprzednia
                </button>
                <button
                  @click="nextPage"
                  :disabled="adminStore.pageNumber === adminStore.totalPages"
                  class="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                >
                  Następna
                </button>
              </nav>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { UserDto } from '~/stores/admin'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default',
})

useHead({
  title: 'Zarządzanie Użytkownikami - Panel Administracyjny',
})

const adminStore = useAdminStore()

const filters = ref({
  searchTerm: '',
  department: '',
  role: '',
  isActive: undefined as boolean | undefined,
})

let debounceTimeout: NodeJS.Timeout | null = null

const debouncedSearch = () => {
  if (debounceTimeout) {
    clearTimeout(debounceTimeout)
  }
  debounceTimeout = setTimeout(() => {
    fetchUsers()
  }, 500)
}

const fetchUsers = async () => {
  await adminStore.fetchUsers({
    searchTerm: filters.value.searchTerm || undefined,
    department: filters.value.department || undefined,
    role: filters.value.role || undefined,
    isActive: filters.value.isActive,
    pageNumber: adminStore.pageNumber,
    pageSize: adminStore.pageSize,
  })
}

const previousPage = () => {
  if (adminStore.pageNumber > 1) {
    adminStore.pageNumber--
    fetchUsers()
  }
}

const nextPage = () => {
  if (adminStore.pageNumber < adminStore.totalPages) {
    adminStore.pageNumber++
    fetchUsers()
  }
}

const getRoleBadgeClass = (role: string) => {
  const classes: Record<string, string> = {
    Admin: 'bg-purple-100 text-purple-800',
    Manager: 'bg-blue-100 text-blue-800',
    HR: 'bg-green-100 text-green-800',
    Marketing: 'bg-yellow-100 text-yellow-800',
    Employee: 'bg-gray-100 text-gray-800',
  }
  return classes[role] || 'bg-gray-100 text-gray-800'
}

const confirmDelete = async (user: UserDto) => {
  if (confirm(`Czy na pewno chcesz usunąć użytkownika ${user.firstName} ${user.lastName}?`)) {
    try {
      await adminStore.deleteUser(user.id)
      await fetchUsers()
    } catch (error) {
      console.error('Error deleting user:', error)
    }
  }
}

// Fetch users on mount
onMounted(() => {
  fetchUsers()
})
</script>

