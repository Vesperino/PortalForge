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
        <h1 class="text-3xl font-bold text-gray-900">Dodaj Nowego Użytkownika</h1>
        <p class="mt-2 text-gray-600">Wypełnij formularz aby utworzyć nowe konto użytkownika</p>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
        <p class="text-red-800">{{ error }}</p>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleSubmit" class="bg-white rounded-lg shadow-md p-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Email -->
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Email <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.email"
              type="email"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="jan.kowalski@example.com"
            />
          </div>

          <!-- Password -->
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Hasło <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.password"
              type="password"
              required
              minlength="8"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Minimum 8 znaków"
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
              placeholder="Jan"
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
              placeholder="Kowalski"
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
              placeholder="IT"
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
              placeholder="Developer"
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
              placeholder="+48 123 456 789"
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

          <!-- Must Change Password -->
          <div class="md:col-span-2">
            <label class="flex items-center">
              <input
                type="checkbox"
                v-model="form.mustChangePassword"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <span class="ml-2 text-sm text-gray-700">
                Wymagaj zmiany hasła przy pierwszym logowaniu
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
            {{ loading ? 'Tworzenie...' : 'Utwórz Użytkownika' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CreateUserRequest } from '~/stores/admin'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default',
})

useHead({
  title: 'Dodaj Użytkownika - Panel Administracyjny',
})

const adminStore = useAdminStore()
const roleGroupsStore = useRoleGroupsStore()

const form = ref<CreateUserRequest>({
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  department: '',
  position: '',
  phoneNumber: '',
  role: 'Employee',
  roleGroupIds: [],
  mustChangePassword: true,
})

const loading = ref(false)
const error = ref<string | null>(null)

const handleSubmit = async () => {
  loading.value = true
  error.value = null

  try {
    await adminStore.createUser(form.value)
    await navigateTo('/admin/users')
  } catch (err: any) {
    error.value = err.message || 'Nie udało się utworzyć użytkownika'
  } finally {
    loading.value = false
  }
}

// Fetch role groups on mount
onMounted(() => {
  roleGroupsStore.fetchRoleGroups()
})
</script>

