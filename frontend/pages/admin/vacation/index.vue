<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Calendar, Users, Edit, X, Search, CheckCircle } from 'lucide-vue-next'
import { useVacations } from '~/composables/useVacations'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'admin']
})

interface User {
  id: string
  firstName: string
  lastName: string
  email: string
  department: string
  annualVacationDays: number
  vacationDaysUsed: number
  onDemandVacationDaysUsed: number
  carriedOverVacationDays: number
}

const config = useRuntimeConfig()
const { getAuthHeaders } = useAuth()
const { getUserVacationSummary } = useVacations()
const toast = useNotificationToast()

// State
const users = ref<User[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)
const searchQuery = ref('')

// Selected user for editing
const selectedUser = ref<User | null>(null)
const showEditModal = ref(false)
const newAllowance = ref(26)
const updateReason = ref('')
const isUpdating = ref(false)

// Computed
const filteredUsers = computed(() => {
  if (!searchQuery.value) return users.value

  const query = searchQuery.value.toLowerCase()
  return users.value.filter(user =>
    `${user.firstName} ${user.lastName}`.toLowerCase().includes(query) ||
    user.email.toLowerCase().includes(query) ||
    user.department.toLowerCase().includes(query)
  )
})

const totalUsers = computed(() => filteredUsers.value.length)

// Fetch all users
const fetchUsers = async () => {
  isLoading.value = true
  error.value = null

  try {
    const response = await $fetch(
      `${config.public.apiUrl}/api/admin/users`,
      { headers: getAuthHeaders() }
    ) as { users: User[] }

    users.value = response.users
  } catch (err: any) {
    error.value = err.message || 'Nie udało się pobrać listy użytkowników'
    console.error('Error fetching users:', err)
  } finally {
    isLoading.value = false
  }
}

// Open edit modal
const openEditModal = (user: User) => {
  selectedUser.value = user
  newAllowance.value = user.annualVacationDays
  updateReason.value = ''
  showEditModal.value = true
}

// Update vacation allowance
const updateVacationAllowance = async () => {
  if (!selectedUser.value || !updateReason.value.trim()) {
    toast.warning('Powód zmiany jest wymagany')
    return
  }

  isUpdating.value = true

  try {
    await $fetch(
      `${config.public.apiUrl}/api/admin/users/${selectedUser.value.id}/vacation-allowance`,
      {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: {
          newAnnualDays: newAllowance.value,
          reason: updateReason.value
        }
      }
    )

    // Update local state
    const userIndex = users.value.findIndex(u => u.id === selectedUser.value!.id)
    if (userIndex !== -1) {
      users.value[userIndex].annualVacationDays = newAllowance.value
    }

    // Close modal
    showEditModal.value = false
    selectedUser.value = null

    toast.success('Limit urlopów został zaktualizowany')
  } catch (err: any) {
    console.error('Error updating vacation allowance:', err)
    toast.error('Nie udało się zaktualizować limitu urlopów')
  } finally {
    isUpdating.value = false
  }
}

// Get remaining vacation days
const getRemainingDays = (user: User): number => {
  return user.annualVacationDays + user.carriedOverVacationDays - user.vacationDaysUsed
}

// Get progress color
const getProgressColor = (remaining: number, total: number): string => {
  const percentage = (remaining / total) * 100
  if (percentage > 50) return 'bg-green-500'
  if (percentage > 25) return 'bg-yellow-500'
  return 'bg-red-500'
}

// Load data on mount
onMounted(() => {
  fetchUsers()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 py-8">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center gap-3 mb-2">
          <Calendar class="w-8 h-8 text-blue-600 dark:text-blue-400" />
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
            Zarządzanie Urlopami
          </h1>
        </div>
        <p class="text-gray-600 dark:text-gray-400">
          Przeglądaj i zarządzaj limitami urlopów użytkowników
        </p>
      </div>

      <!-- Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400 mb-1">Użytkownicy</p>
              <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ totalUsers }}</p>
            </div>
            <Users class="w-10 h-10 text-blue-500" />
          </div>
        </div>
      </div>

      <!-- Search -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
        <div class="relative">
          <Search class="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Szukaj użytkownika po imieniu, nazwisku, emailu lub dziale..."
            class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
          />
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="flex items-center justify-center h-64">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
          <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie użytkowników...</p>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-8 text-center">
        <p class="text-red-600 dark:text-red-400">{{ error }}</p>
      </div>

      <!-- Users Table -->
      <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
            <thead class="bg-gray-50 dark:bg-gray-900">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Użytkownik
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Dział
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Limit roczny
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Wykorzystane
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Pozostałe
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Akcje
                </th>
              </tr>
            </thead>
            <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
              <tr v-for="user in filteredUsers" :key="user.id" class="hover:bg-gray-50 dark:hover:bg-gray-700">
                <!-- User -->
                <td class="px-6 py-4 whitespace-nowrap">
                  <div>
                    <div class="text-sm font-medium text-gray-900 dark:text-white">
                      {{ user.firstName }} {{ user.lastName }}
                    </div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">
                      {{ user.email }}
                    </div>
                  </div>
                </td>

                <!-- Department -->
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                  {{ user.department }}
                </td>

                <!-- Annual Limit -->
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                  <span class="font-medium text-gray-900 dark:text-white">
                    {{ user.annualVacationDays }} dni
                  </span>
                  <span v-if="user.carriedOverVacationDays > 0" class="text-xs text-green-600 dark:text-green-400 ml-1">
                    +{{ user.carriedOverVacationDays }} zaległy
                  </span>
                </td>

                <!-- Used -->
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                  {{ user.vacationDaysUsed }} dni
                </td>

                <!-- Remaining -->
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center gap-2">
                    <div class="flex-1">
                      <div class="text-sm font-medium text-gray-900 dark:text-white mb-1">
                        {{ getRemainingDays(user) }} dni
                      </div>
                      <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                        <div
                          :class="['h-2 rounded-full transition-all', getProgressColor(getRemainingDays(user), user.annualVacationDays + user.carriedOverVacationDays)]"
                          :style="{ width: `${(getRemainingDays(user) / (user.annualVacationDays + user.carriedOverVacationDays)) * 100}%` }"
                        />
                      </div>
                    </div>
                  </div>
                </td>

                <!-- Actions -->
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                  <button
                    class="inline-flex items-center gap-1 px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition"
                    @click="openEditModal(user)"
                  >
                    <Edit class="w-4 h-4" />
                    Edytuj
                  </button>
                </td>
              </tr>

              <!-- Empty State -->
              <tr v-if="filteredUsers.length === 0">
                <td colspan="6" class="px-6 py-12 text-center text-gray-500 dark:text-gray-400">
                  <Calendar class="w-12 h-12 mx-auto mb-2 opacity-50" />
                  <p>Brak użytkowników do wyświetlenia</p>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Edit Modal -->
    <div
      v-if="showEditModal && selectedUser"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="showEditModal = false"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full">
        <!-- Modal Header -->
        <div class="p-6 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
            Edytuj limit urlopów
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            {{ selectedUser.firstName }} {{ selectedUser.lastName }}
          </p>
        </div>

        <!-- Modal Body -->
        <div class="p-6 space-y-4">
          <!-- Current Allowance -->
          <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
            <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">Obecny limit roczny:</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ selectedUser.annualVacationDays }} dni
            </p>
          </div>

          <!-- New Allowance -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Nowy limit roczny <span class="text-red-500">*</span>
            </label>
            <input
              v-model.number="newAllowance"
              type="number"
              min="0"
              max="50"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
            />
          </div>

          <!-- Reason -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Powód zmiany <span class="text-red-500">*</span>
            </label>
            <textarea
              v-model="updateReason"
              rows="3"
              placeholder="np. Zmiana umowy, dodatkowe dni za staż pracy..."
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
            />
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700">
          <button
            class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition"
            :disabled="isUpdating"
            @click="showEditModal = false"
          >
            Anuluj
          </button>
          <button
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
            :disabled="isUpdating || !updateReason.trim() || newAllowance === selectedUser.annualVacationDays"
            @click="updateVacationAllowance"
          >
            <CheckCircle v-if="!isUpdating" class="w-4 h-4" />
            {{ isUpdating ? 'Aktualizowanie...' : 'Zapisz zmiany' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
