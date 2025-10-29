<template>
  <div class="min-h-screen bg-gray-50">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <NuxtLink
          to="/admin/users"
          class="inline-flex items-center text-blue-600 hover:text-blue-800 mb-4"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Powrót do listy użytkowników
        </NuxtLink>
        <h1 class="text-3xl font-bold text-gray-900">Edytuj Użytkownika</h1>
        <p class="mt-2 text-gray-600">Zaktualizuj dane użytkownika</p>
      </div>

      <!-- Loading State -->
      <div v-if="loading && !form" class="bg-white rounded-lg shadow-md p-12 text-center">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">Ładowanie danych użytkownika...</p>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
        <p class="text-red-800">{{ error }}</p>
      </div>

      <!-- Form -->
      <form v-if="form" @submit.prevent="handleSubmit" class="bg-white rounded-lg shadow-md p-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Email (read-only) -->
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Email
            </label>
            <input
              :value="currentUser?.email"
              type="email"
              disabled
              class="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-100 text-gray-500"
            />
          </div>

          <!-- First Name -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Imię <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.firstName"
              type="text"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          <!-- Last Name -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Nazwisko <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.lastName"
              type="text"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          <!-- Department -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Dział <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.department"
              type="text"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          <!-- Position -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Stanowisko <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.position"
              type="text"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          <!-- Phone Number -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Numer telefonu
            </label>
            <input
              v-model="form.phoneNumber"
              type="tel"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          <!-- Role -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Rola <span class="text-red-500">*</span>
            </label>
            <select
              v-model="form.role"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="Employee">Employee</option>
              <option value="Manager">Manager</option>
              <option value="HR">HR</option>
              <option value="Marketing">Marketing</option>
              <option value="Admin">Admin</option>
            </select>
          </div>

          <!-- Role Groups -->
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Grupy Ról
            </label>
            <div v-if="roleGroupsStore.loading" class="text-gray-500">
              Ładowanie grup ról...
            </div>
            <div v-else class="space-y-2">
              <label
                v-for="roleGroup in roleGroupsStore.roleGroups"
                :key="roleGroup.id"
                class="flex items-center"
              >
                <input
                  type="checkbox"
                  :value="roleGroup.id"
                  v-model="form.roleGroupIds"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <span class="ml-2 text-sm text-gray-700">
                  {{ roleGroup.name }}
                  <span class="text-gray-500">({{ roleGroup.description }})</span>
                </span>
              </label>
            </div>
          </div>

          <!-- Is Active -->
          <div class="md:col-span-2">
            <label class="flex items-center">
              <input
                type="checkbox"
                v-model="form.isActive"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <span class="ml-2 text-sm text-gray-700">
                Konto aktywne
              </span>
            </label>
          </div>
        </div>

        <!-- Actions -->
        <div class="mt-6 flex justify-end space-x-4">
          <NuxtLink
            to="/admin/users"
            class="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Anuluj
          </NuxtLink>
          <button
            type="submit"
            :disabled="loading"
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
          >
            {{ loading ? 'Zapisywanie...' : 'Zapisz Zmiany' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { UpdateUserRequest } from '~/stores/admin'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default',
})

useHead({
  title: 'Edytuj Użytkownika - Panel Administracyjny',
})

const route = useRoute()
const adminStore = useAdminStore()
const roleGroupsStore = useRoleGroupsStore()

const userId = route.params.id as string
const currentUser = computed(() => adminStore.currentUser)

const form = ref<UpdateUserRequest | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const loadUser = async () => {
  loading.value = true
  error.value = null

  try {
    const user = await adminStore.fetchUserById(userId)
    
    // Map role groups names to IDs
    const roleGroupIds = roleGroupsStore.roleGroups
      .filter(rg => user.roleGroups.includes(rg.name))
      .map(rg => rg.id)

    form.value = {
      firstName: user.firstName,
      lastName: user.lastName,
      department: user.department,
      position: user.position,
      phoneNumber: user.phoneNumber || '',
      role: user.role,
      roleGroupIds: roleGroupIds,
      isActive: user.isActive,
    }
  } catch (err: any) {
    error.value = err.message || 'Nie udało się załadować danych użytkownika'
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!form.value) return

  loading.value = true
  error.value = null

  try {
    await adminStore.updateUser(userId, form.value)
    await navigateTo('/admin/users')
  } catch (err: any) {
    error.value = err.message || 'Nie udało się zaktualizować użytkownika'
  } finally {
    loading.value = false
  }
}

// Fetch data on mount
onMounted(async () => {
  await roleGroupsStore.fetchRoleGroups()
  await loadUser()
})
</script>

