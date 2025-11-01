<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <NuxtLink
          to="/admin"
          class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 mb-4"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Powrót do panelu administracyjnego
        </NuxtLink>
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Uprawnienia Organizacyjne</h1>
          <p class="mt-2 text-gray-600 dark:text-gray-400">
            Zarządzaj widocznością działów dla poszczególnych użytkowników
          </p>
        </div>
      </div>

      <!-- Info Banner -->
      <div class="bg-blue-50 dark:bg-blue-900/30 border border-blue-200 dark:border-blue-800 rounded-lg p-4 mb-6">
        <div class="flex items-start">
          <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div class="text-sm text-blue-800 dark:text-blue-300">
            <p class="font-semibold mb-1">Domyślne uprawnienia</p>
            <p>Każdy użytkownik domyślnie widzi tylko swój dział. Tutaj możesz nadać dodatkowe uprawnienia do przeglądania innych działów.</p>
          </div>
        </div>
      </div>

      <!-- Search -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Szukaj użytkownika
            </label>
            <input
              v-model="searchTerm"
              type="text"
              placeholder="Email, imię, nazwisko..."
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @input="debouncedSearch"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Dział
            </label>
            <input
              v-model="departmentFilter"
              type="text"
              placeholder="Filtruj po dziale..."
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @input="debouncedSearch"
            />
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 dark:border-blue-400"></div>
        <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie użytkowników...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 rounded-lg p-4 mb-6">
        <p class="text-red-800 dark:text-red-300">{{ error }}</p>
      </div>

      <!-- Users List -->
      <div v-else-if="users.length > 0" class="space-y-4">
        <div
          v-for="user in filteredUsers"
          :key="user.id"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
        >
          <!-- User Info Header -->
          <div class="flex items-center justify-between mb-4">
            <div class="flex items-center">
              <div class="flex-shrink-0 h-12 w-12">
                <div class="h-12 w-12 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                  <span class="text-blue-600 dark:text-blue-400 font-medium text-lg">
                    {{ (user.firstName || '').charAt(0) }}{{ (user.lastName || '').charAt(0) }}
                  </span>
                </div>
              </div>
              <div class="ml-4">
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                  {{ user.firstName }} {{ user.lastName }}
                </h3>
                <p class="text-sm text-gray-500 dark:text-gray-400">{{ user.email }}</p>
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  {{ user.department || 'Brak działu' }} • {{ user.position || 'Brak stanowiska' }}
                </p>
              </div>
            </div>

            <!-- Save Button -->
            <button
              @click="savePermissions(user.id)"
              :disabled="savingUserId === user.id"
              class="inline-flex items-center px-4 py-2 bg-blue-600 dark:bg-blue-500 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <svg
                v-if="savingUserId !== user.id"
                class="w-5 h-5 mr-2"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
              </svg>
              <div v-else class="w-5 h-5 mr-2 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
              {{ savingUserId === user.id ? 'Zapisywanie...' : 'Zapisz' }}
            </button>
          </div>

          <!-- Permissions Form -->
          <div class="border-t border-gray-200 dark:border-gray-700 pt-4">
            <!-- View All Departments Checkbox -->
            <template v-if="userPermissions[user.id]">
              <div class="mb-4">
                <label class="flex items-center cursor-pointer">
                  <input
                    v-model="userPermissions[user.id]!.canViewAllDepartments"
                    type="checkbox"
                    class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600"
                    @change="handleViewAllChange(user.id)"
                  />
                  <span class="ml-2 text-sm font-medium text-gray-900 dark:text-white">
                    Może przeglądać wszystkie działy
                  </span>
                </label>
                <p class="ml-6 text-xs text-gray-500 dark:text-gray-400">
                  Użytkownik będzie miał dostęp do wszystkich działów w organizacji
                </p>
              </div>

              <!-- Specific Departments Selection -->
              <div v-if="!userPermissions[user.id]!.canViewAllDepartments" class="mt-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Widoczne działy
                </label>
                <p class="text-xs text-gray-500 dark:text-gray-400 mb-3">
                  Wybierz działy, które użytkownik może przeglądać (domyślnie widzi tylko swój dział)
                </p>

                <!-- Departments Loading -->
                <div v-if="departmentsLoading" class="text-sm text-gray-600 dark:text-gray-400">
                  Ładowanie działów...
                </div>

                <!-- Departments List -->
                <div v-else class="space-y-2 max-h-60 overflow-y-auto border border-gray-300 dark:border-gray-600 rounded-lg p-3">
                  <label
                    v-for="dept in departments"
                    :key="dept.id"
                    class="flex items-center cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700 p-2 rounded"
                  >
                    <input
                      v-model="userPermissions[user.id]!.visibleDepartmentIds"
                      :value="dept.id"
                      type="checkbox"
                      class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600"
                    />
                    <span class="ml-2 text-sm text-gray-900 dark:text-white">
                      {{ dept.name }}
                    </span>
                    <span
                      v-if="user.department && user.department === dept.name"
                      class="ml-2 text-xs text-gray-500 dark:text-gray-400 italic"
                    >
                      (dział użytkownika)
                    </span>
                  </label>
                </div>

                <!-- Selected Count -->
                <p class="mt-2 text-xs text-gray-600 dark:text-gray-400">
                  Wybrano: {{ userPermissions[user.id]!.visibleDepartmentIds.length }}
                  {{ userPermissions[user.id]!.visibleDepartmentIds.length === 1 ? 'dział' : 'działów' }}
                </p>
              </div>
            </template>
          </div>
        </div>

        <!-- Empty State -->
        <div
          v-if="filteredUsers.length === 0"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center"
        >
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Brak użytkowników</h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Nie znaleziono użytkowników spełniających kryteria wyszukiwania
          </p>
        </div>

        <!-- Pagination -->
        <div
          v-if="filteredUsers.length > 0"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md px-4 py-3 flex items-center justify-between"
        >
          <div>
            <p class="text-sm text-gray-700 dark:text-gray-300">
              Wyświetlanie
              <span class="font-medium">{{ (currentPage - 1) * pageSize + 1 }}</span>
              -
              <span class="font-medium">{{ Math.min(currentPage * pageSize, totalUsers) }}</span>
              z
              <span class="font-medium">{{ totalUsers }}</span>
              użytkowników
            </p>
          </div>
          <div class="flex gap-2">
            <button
              @click="previousPage"
              :disabled="currentPage === 1"
              class="relative inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Poprzednia
            </button>
            <button
              @click="nextPage"
              :disabled="currentPage === totalPages"
              class="relative inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Następna
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { User } from '~/types/auth'
import type { DepartmentDto, DepartmentTreeDto } from '~/types/department'
import type { OrganizationalPermissionDto, UpdateOrganizationalPermissionRequest } from '~/types/permission'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default',
})

useHead({
  title: 'Uprawnienia Organizacyjne - Panel Administracyjny',
})

const config = useRuntimeConfig()
const authStore = useAuthStore()
const { getAuthHeaders } = useAuth()
const apiUrl = config.public.apiUrl

// State
const users = ref<User[]>([])
const departments = ref<DepartmentDto[]>([])
const userPermissions = ref<Record<string, UpdateOrganizationalPermissionRequest>>({})
const loading = ref(false)
const departmentsLoading = ref(false)
const error = ref<string | null>(null)
const savingUserId = ref<string | null>(null)

// Filters and pagination
const searchTerm = ref('')
const departmentFilter = ref('')
const currentPage = ref(1)
const pageSize = ref(10)
const totalUsers = ref(0)

// Computed
const filteredUsers = computed(() => {
  let filtered = users.value

  // Apply search filter
  if (searchTerm.value) {
    const search = searchTerm.value.toLowerCase()
    filtered = filtered.filter(
      (user) =>
        user.email.toLowerCase().includes(search) ||
        user.firstName?.toLowerCase().includes(search) ||
        user.lastName?.toLowerCase().includes(search)
    )
  }

  // Apply department filter
  if (departmentFilter.value) {
    const deptSearch = departmentFilter.value.toLowerCase()
    filtered = filtered.filter((user) => user.department?.toLowerCase().includes(deptSearch))
  }

  totalUsers.value = filtered.length

  // Apply pagination
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filtered.slice(start, end)
})

const totalPages = computed(() => Math.ceil(totalUsers.value / pageSize.value))

// Methods
const fetchUsers = async () => {
  loading.value = true
  error.value = null

  try {
    const response = await $fetch(`${apiUrl}/api/admin/users`, {
      headers: getAuthHeaders(),
    }) as any

    if (response && response.users && Array.isArray(response.users)) {
      users.value = response.users as User[]
    } else if (Array.isArray(response)) {
      users.value = response as User[]
    } else {
      console.error('Response is not in expected format:', response)
      users.value = []
      error.value = 'Nieprawidłowy format danych z serwera'
      return
    }

    for (const user of users.value) {
      await loadUserPermissions(user.id)
    }
  } catch (err: any) {
    console.error('Error fetching users:', err)
    error.value = err.message || 'Nie udało się załadować użytkowników'
    users.value = []
  } finally {
    loading.value = false
  }
}

const fetchDepartments = async () => {
  departmentsLoading.value = true

  try {
    const response = await $fetch(`${apiUrl}/api/departments/tree`, {
      headers: getAuthHeaders(),
    }) as DepartmentTreeDto[]

    // Flatten tree structure to simple list for checkboxes
    const flattenDepartments = (depts: DepartmentTreeDto[]): DepartmentDto[] => {
      const result: DepartmentDto[] = []
      for (const dept of depts) {
        result.push({
          id: dept.id,
          name: dept.name,
          description: dept.description,
          parentDepartmentId: dept.parentDepartmentId,
          departmentHeadId: dept.departmentHeadId,
          departmentHeadName: dept.departmentHeadName,
          isActive: dept.isActive,
          level: dept.level,
          employeeCount: dept.employeeCount,
          createdAt: new Date().toISOString()
        })
        if (dept.children && dept.children.length > 0) {
          result.push(...flattenDepartments(dept.children))
        }
      }
      return result
    }

    departments.value = flattenDepartments(response)
  } catch (err) {
    console.error('Error fetching departments:', err)
  } finally {
    departmentsLoading.value = false
  }
}

const loadUserPermissions = async (userId: string) => {
  try {
    const response = await $fetch(
      `${apiUrl}/api/admin/permissions/organizational/${userId}`,
      {
        headers: getAuthHeaders(),
      }
    ) as OrganizationalPermissionDto

    userPermissions.value[userId] = {
      canViewAllDepartments: response.canViewAllDepartments,
      visibleDepartmentIds: response.visibleDepartmentIds,
    }
  } catch (err) {
    console.error(`Error loading permissions for user ${userId}:`, err)
    // Initialize with default permissions if loading fails
    userPermissions.value[userId] = {
      canViewAllDepartments: false,
      visibleDepartmentIds: [],
    }
  }
}

const savePermissions = async (userId: string) => {
  savingUserId.value = userId

  try {
    await $fetch(
      `${apiUrl}/api/admin/permissions/organizational/${userId}`,
      {
        method: 'PUT',
        headers: {
          ...getAuthHeaders(),
          'Content-Type': 'application/json',
        },
        body: userPermissions.value[userId],
      }
    )

    // Show success notification (you can add a toast notification library)
    console.log('Permissions saved successfully')
  } catch (err: any) {
    console.error('Error saving permissions:', err)
    alert(err.message || 'Nie udało się zapisać uprawnień')
  } finally {
    savingUserId.value = null
  }
}

const handleViewAllChange = (userId: string) => {
  // Clear selected departments when "view all" is enabled
  if (userPermissions.value[userId] && userPermissions.value[userId].canViewAllDepartments) {
    userPermissions.value[userId]!.visibleDepartmentIds = []
  }
}

// Pagination
const previousPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--
  }
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
  }
}

// Debounced search
let debounceTimeout: NodeJS.Timeout | null = null

const debouncedSearch = () => {
  if (debounceTimeout) {
    clearTimeout(debounceTimeout)
  }
  debounceTimeout = setTimeout(() => {
    currentPage.value = 1 // Reset to first page on search
  }, 300)
}

// Lifecycle
onMounted(async () => {
  await Promise.all([fetchUsers(), fetchDepartments()])
})
</script>
