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
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"/>
        <p class="mt-4 text-gray-600">Ładowanie danych użytkownika...</p>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
        <p class="text-red-800">{{ error }}</p>
      </div>

      <!-- Form -->
      <form v-if="form" class="bg-white rounded-lg shadow-md p-6" @submit.prevent="handleSubmit">
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
            >
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
            >
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
            >
          </div>

          <!-- Department -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Dział <span class="text-red-500">*</span>
            </label>
            <select
              v-model="form.department"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @change="handleDepartmentChange"
            >
              <option value="">Wybierz dział</option>
              <option
                v-for="dept in departmentsFlat"
                :key="dept.id"
                :value="dept.name"
              >
                {{ dept.level > 0 ? '└─'.repeat(dept.level) + ' ' : '' }}{{ dept.name }}
              </option>
            </select>
          </div>

          <!-- Position -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Stanowisko <span class="text-red-500">*</span>
            </label>
            <PositionAutocomplete
              :model-value="positionId"
              placeholder="Wpisz lub wybierz stanowisko..."
              required
              @update:model-value="handlePositionUpdate"
              @update:position-name="handlePositionNameUpdate"
            />
          </div>

          <!-- Phone Number -->
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Numer telefonu
            </label>
            <input
              v-model="form.phoneNumber"
              type="tel"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
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
                  v-model="form.roleGroupIds"
                  type="checkbox"
                  :value="roleGroup.id"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                >
                <span class="ml-2 text-sm text-gray-700">
                  {{ roleGroup.name }}
                  <span class="text-gray-500">({{ roleGroup.description }})</span>
                </span>
              </label>
            </div>
          </div>

          <!-- New Password -->
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Resetuj hasło
            </label>
            <input
              v-model="form.NewPassword"
              type="password"
              placeholder="Pozostaw puste, aby nie zmieniać hasła"
              :class="[
                'w-full px-3 py-2 border rounded-lg focus:ring-2 focus:border-transparent',
                passwordError ? 'border-red-300 focus:ring-red-500' : 'border-gray-300 focus:ring-blue-500'
              ]"
              @input="validatePassword"
            >
            <p v-if="passwordError" class="mt-1 text-xs text-red-600">
              {{ passwordError }}
            </p>
            <p v-else class="mt-1 text-xs text-gray-500">
              Pozostaw puste, aby nie zmieniać hasła. Hasło musi zawierać minimum 8 znaków, w tym wielką literę, małą literę i cyfrę.
            </p>
          </div>

          <!-- Is Active -->
          <div class="md:col-span-2">
            <label class="flex items-center">
              <input
                v-model="form.isActive"
                type="checkbox"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              >
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
import type { DepartmentTreeDto } from '~/types/department'
import PositionAutocomplete from '~/components/common/PositionAutocomplete.vue'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default',
})

useHead({
  title: 'Edytuj Użytkownika - Panel Administracyjny',
})

const config = useRuntimeConfig()
const apiUrl = config.public.apiUrl
const route = useRoute()
const adminStore = useAdminStore()
const roleGroupsStore = useRoleGroupsStore()
const authStore = useAuthStore()

const getAuthHeaders = (): Record<string, string> | undefined => {
  const token = authStore.accessToken
  if (token) {
    return { Authorization: `Bearer ${token}` }
  }
  return undefined
}

const userId = route.params.id as string
const currentUser = computed(() => adminStore.currentUser)

const form = ref<UpdateUserRequest | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const passwordError = ref<string | null>(null)
const departments = ref<DepartmentTreeDto[]>([])
const positionId = ref<string | null>(null)
const positionName = ref<string>('')
const departmentId = ref<string | null>(null)

const loadDepartments = async () => {
  try {
    const response = await $fetch<DepartmentTreeDto[]>(`${apiUrl}/api/departments/tree`, {
      headers: getAuthHeaders()
    })
    departments.value = response
  } catch (err: any) {
    console.error('Error loading departments:', err)
  }
}

// Flatten departments tree
const departmentsFlat = computed(() => {
  const flattenDepartments = (depts: DepartmentTreeDto[], level = 0): DepartmentTreeDto[] => {
    return depts.flatMap(dept => [
      { ...dept, level },
      ...flattenDepartments(dept.children || [], level + 1)
    ])
  }
  return flattenDepartments(departments.value)
})

const loadUser = async () => {
  loading.value = true
  error.value = null

  try {
    const user = await adminStore.fetchUserById(userId)

    // Map role groups names to IDs
    const roleGroupIds = roleGroupsStore.roleGroups
      .filter(rg => user.roleGroups.includes(rg.name))
      .map(rg => rg.id)

    // Set position name for autocomplete
    positionName.value = user.position || ''
    positionId.value = user.positionId || null

    // Set department ID
    departmentId.value = user.departmentId || null

    form.value = {
      firstName: user.firstName,
      lastName: user.lastName,
      department: user.department,
      departmentId: user.departmentId || null,
      position: user.position,
      positionId: user.positionId || null,
      phoneNumber: user.phoneNumber || '',
      role: user.role,
      roleGroupIds: roleGroupIds,
      isActive: user.isActive,
      NewPassword: undefined,  // PascalCase to match backend
    }
  } catch (err: any) {
    error.value = err.message || 'Nie udało się załadować danych użytkownika'
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!form.value) return

  // Validate password if provided
  if (form.value.NewPassword && form.value.NewPassword.trim() !== '') {
    const validationResult = validatePassword()
    if (!validationResult) {
      return // Stop submission if password validation fails
    }
  }

  loading.value = true
  error.value = null

  try {
    // Update position with the current value from autocomplete
    form.value.position = positionName.value
    form.value.positionId = positionId.value

    // Ensure departmentId is set
    form.value.departmentId = departmentId.value

    // Remove NewPassword if empty
    if (!form.value.NewPassword || form.value.NewPassword.trim() === '') {
      delete form.value.NewPassword
    }

    await adminStore.updateUser(userId, form.value)

    // Show success notification
    showSuccessToast()

    await navigateTo('/admin/users')
  } catch (err: any) {
    error.value = err.message || 'Nie udało się zaktualizować użytkownika'
  } finally {
    loading.value = false
  }
}

// Handle position autocomplete updates
const handlePositionUpdate = (value: string | null) => {
  positionId.value = value
}

const handlePositionNameUpdate = (name: string) => {
  positionName.value = name
  if (form.value) {
    form.value.position = name
  }
}

// Handle department change
const handleDepartmentChange = (event: Event) => {
  const target = event.target as HTMLSelectElement
  const selectedDeptName = target.value

  // Find the department to get its ID
  const findDeptByName = (depts: DepartmentTreeDto[], name: string): DepartmentTreeDto | null => {
    for (const dept of depts) {
      if (dept.name === name) return dept
      if (dept.children && dept.children.length > 0) {
        const found = findDeptByName(dept.children, name)
        if (found) return found
      }
    }
    return null
  }

  const dept = findDeptByName(departments.value, selectedDeptName)
  if (dept && form.value) {
    departmentId.value = dept.id
    form.value.department = dept.name
    form.value.departmentId = dept.id
  }
}

// Password validation
const validatePassword = (): boolean => {
  if (!form.value?.NewPassword || form.value.NewPassword.trim() === '') {
    passwordError.value = null
    return true // Empty password is valid (means no change)
  }

  const password = form.value.NewPassword

  if (password.length < 8) {
    passwordError.value = 'Hasło musi zawierać minimum 8 znaków'
    return false
  }

  if (!/[A-Z]/.test(password)) {
    passwordError.value = 'Hasło musi zawierać przynajmniej jedną wielką literę'
    return false
  }

  if (!/[a-z]/.test(password)) {
    passwordError.value = 'Hasło musi zawierać przynajmniej jedną małą literę'
    return false
  }

  if (!/[0-9]/.test(password)) {
    passwordError.value = 'Hasło musi zawierać przynajmniej jedną cyfrę'
    return false
  }

  passwordError.value = null
  return true
}

// Success toast notification
const showSuccessToast = () => {
  // Create toast element
  const toast = document.createElement('div')
  toast.className = 'fixed top-4 right-4 bg-green-500 text-white px-6 py-4 rounded-lg shadow-lg z-50 flex items-center gap-3 animate-fade-in'
  toast.innerHTML = `
    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
    </svg>
    <span class="font-medium">Dane użytkownika zostały zaktualizowane pomyślnie</span>
  `
  document.body.appendChild(toast)

  // Remove toast after 3 seconds
  setTimeout(() => {
    toast.style.opacity = '0'
    toast.style.transition = 'opacity 0.3s'
    setTimeout(() => {
      document.body.removeChild(toast)
    }, 300)
  }, 3000)
}

// Fetch data on mount
onMounted(async () => {
  await Promise.all([
    roleGroupsStore.fetchRoleGroups(),
    loadDepartments()
  ])
  await loadUser()
})
</script>

