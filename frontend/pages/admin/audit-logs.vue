<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { Shield, Calendar, User, Filter, Search, ChevronLeft, ChevronRight } from 'lucide-vue-next'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'admin']
})

interface AuditLog {
  id: string
  entityType: string
  entityId: string
  action: string
  userId?: string
  userFullName?: string
  oldValue?: string
  newValue?: string
  reason?: string
  timestamp: string
  ipAddress?: string
}

const config = useRuntimeConfig()
const { getAuthHeaders } = useAuth()

// State
const logs = ref<AuditLog[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

// Filters
const entityTypeFilter = ref('')
const actionFilter = ref('')
const userIdFilter = ref('')
const fromDateFilter = ref('')
const toDateFilter = ref('')
const searchQuery = ref('')

// Pagination
const currentPage = ref(1)
const pageSize = ref(50)
const totalCount = ref(0)

// Computed
const filteredLogs = computed(() => {
  let result = logs.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(log =>
      log.entityType.toLowerCase().includes(query) ||
      log.action.toLowerCase().includes(query) ||
      log.userFullName?.toLowerCase().includes(query) ||
      log.reason?.toLowerCase().includes(query)
    )
  }

  return result
})

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

const showFilters = ref(false)

// Entity types for filter dropdown
const entityTypes = [
  { value: '', label: 'Wszystkie' },
  { value: 'User', label: 'Użytkownik' },
  { value: 'Request', label: 'Wniosek' },
  { value: 'VacationSchedule', label: 'Urlop' },
  { value: 'Department', label: 'Dział' },
  { value: 'SystemSettings', label: 'Ustawienia systemowe' }
]

// Fetch audit logs
const fetchLogs = async () => {
  isLoading.value = true
  error.value = null

  try {
    const params = new URLSearchParams({
      page: currentPage.value.toString(),
      pageSize: pageSize.value.toString()
    })

    if (entityTypeFilter.value) params.append('entityType', entityTypeFilter.value)
    if (actionFilter.value) params.append('action', actionFilter.value)
    if (userIdFilter.value) params.append('userId', userIdFilter.value)
    if (fromDateFilter.value) params.append('fromDate', fromDateFilter.value)
    if (toDateFilter.value) params.append('toDate', toDateFilter.value)

    const response = await $fetch(
      `${config.public.apiUrl}/api/admin/audit-logs?${params.toString()}`,
      { headers: getAuthHeaders() }
    ) as { items: AuditLog[], totalCount: number, page: number, pageSize: number }

    logs.value = response.items
    totalCount.value = response.totalCount
  } catch (err: any) {
    error.value = err.message || 'Nie udało się pobrać logów audytu'
    console.error('Error fetching audit logs:', err)
  } finally {
    isLoading.value = false
  }
}

// Format date
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}

// Get action label in Polish
const getActionLabel = (action: string): string => {
  const labels: Record<string, string> = {
    VacationAllowanceUpdated: 'Zmiana limitu urlopów',
    VacationCancelled: 'Anulowanie urlopu',
    RequestCancelled: 'Anulowanie wniosku',
    DepartmentTransfer: 'Przepięcie działu',
    UserCreated: 'Utworzenie użytkownika',
    UserUpdated: 'Aktualizacja użytkownika',
    UserDeleted: 'Usunięcie użytkownika'
  }
  return labels[action] || action
}

// Get entity type label in Polish
const getEntityTypeLabel = (entityType: string): string => {
  const labels: Record<string, string> = {
    User: 'Użytkownik',
    Request: 'Wniosek',
    VacationSchedule: 'Urlop',
    Department: 'Dział',
    SystemSettings: 'Ustawienia'
  }
  return labels[entityType] || entityType
}

// Get action color
const getActionColor = (action: string): string => {
  if (action.includes('Created')) return 'text-green-600 dark:text-green-400'
  if (action.includes('Updated')) return 'text-blue-600 dark:text-blue-400'
  if (action.includes('Deleted') || action.includes('Cancelled')) return 'text-red-600 dark:text-red-400'
  return 'text-gray-600 dark:text-gray-400'
}

// Reset filters
const resetFilters = () => {
  entityTypeFilter.value = ''
  actionFilter.value = ''
  userIdFilter.value = ''
  fromDateFilter.value = ''
  toDateFilter.value = ''
  searchQuery.value = ''
  currentPage.value = 1
  fetchLogs()
}

// Pagination
const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    fetchLogs()
  }
}

const nextPage = () => goToPage(currentPage.value + 1)
const prevPage = () => goToPage(currentPage.value - 1)

// Watch filters
watch([entityTypeFilter, actionFilter, userIdFilter, fromDateFilter, toDateFilter], () => {
  currentPage.value = 1
  fetchLogs()
})

// Load data on mount
onMounted(() => {
  fetchLogs()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 py-8">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center gap-3 mb-2">
          <Shield class="w-8 h-8 text-blue-600 dark:text-blue-400" />
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
            Logi Audytu
          </h1>
        </div>
        <p class="text-gray-600 dark:text-gray-400">
          Historia wszystkich ważnych akcji w systemie
        </p>
      </div>

      <!-- Filters Card -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
        <!-- Filter Toggle -->
        <button
          class="w-full flex items-center justify-between mb-4"
          @click="showFilters = !showFilters"
        >
          <div class="flex items-center gap-2">
            <Filter class="w-5 h-5 text-gray-500 dark:text-gray-400" />
            <span class="font-medium text-gray-900 dark:text-white">Filtry</span>
          </div>
          <span class="text-sm text-blue-600 dark:text-blue-400">
            {{ showFilters ? 'Ukryj' : 'Pokaż' }}
          </span>
        </button>

        <!-- Filter Fields -->
        <div v-show="showFilters" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <!-- Entity Type -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Typ encji
            </label>
            <select
              v-model="entityTypeFilter"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
            >
              <option v-for="type in entityTypes" :key="type.value" :value="type.value">
                {{ type.label }}
              </option>
            </select>
          </div>

          <!-- Action -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Akcja
            </label>
            <input
              v-model="actionFilter"
              type="text"
              placeholder="np. VacationAllowanceUpdated"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
            />
          </div>

          <!-- From Date -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Od daty
            </label>
            <input
              v-model="fromDateFilter"
              type="date"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
            />
          </div>

          <!-- To Date -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Do daty
            </label>
            <input
              v-model="toDateFilter"
              type="date"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
            />
          </div>

          <!-- Reset Button -->
          <div class="flex items-end">
            <button
              class="w-full px-4 py-2 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg font-medium transition"
              @click="resetFilters"
            >
              Resetuj filtry
            </button>
          </div>
        </div>

        <!-- Search -->
        <div class="mt-4">
          <div class="relative">
            <Search class="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Szukaj w logach..."
              class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
            />
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="flex items-center justify-center h-64">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
          <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie logów...</p>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-8 text-center">
        <p class="text-red-600 dark:text-red-400">{{ error }}</p>
      </div>

      <!-- Logs Table -->
      <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
            <thead class="bg-gray-50 dark:bg-gray-900">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Data i czas
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Użytkownik
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Akcja
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Typ encji
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Szczegóły
                </th>
              </tr>
            </thead>
            <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
              <tr v-for="log in filteredLogs" :key="log.id" class="hover:bg-gray-50 dark:hover:bg-gray-700">
                <!-- Timestamp -->
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
                  <div class="flex items-center gap-2">
                    <Calendar class="w-4 h-4 text-gray-400" />
                    {{ formatDate(log.timestamp) }}
                  </div>
                </td>

                <!-- User -->
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                  <div v-if="log.userFullName" class="flex items-center gap-2">
                    <User class="w-4 h-4 text-gray-400" />
                    <span class="text-gray-900 dark:text-white">{{ log.userFullName }}</span>
                  </div>
                  <span v-else class="text-gray-400 italic">System</span>
                </td>

                <!-- Action -->
                <td class="px-6 py-4 whitespace-nowrap text-sm">
                  <span :class="['font-medium', getActionColor(log.action)]">
                    {{ getActionLabel(log.action) }}
                  </span>
                </td>

                <!-- Entity Type -->
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600 dark:text-gray-400">
                  {{ getEntityTypeLabel(log.entityType) }}
                </td>

                <!-- Details -->
                <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">
                  <div v-if="log.oldValue && log.newValue" class="space-y-1">
                    <div><span class="font-medium">Stara wartość:</span> {{ log.oldValue }}</div>
                    <div><span class="font-medium">Nowa wartość:</span> {{ log.newValue }}</div>
                  </div>
                  <div v-if="log.reason" class="mt-1 text-xs italic">
                    Powód: {{ log.reason }}
                  </div>
                  <div v-if="log.ipAddress" class="mt-1 text-xs text-gray-400">
                    IP: {{ log.ipAddress }}
                  </div>
                </td>
              </tr>

              <!-- Empty State -->
              <tr v-if="filteredLogs.length === 0">
                <td colspan="5" class="px-6 py-12 text-center text-gray-500 dark:text-gray-400">
                  <Shield class="w-12 h-12 mx-auto mb-2 opacity-50" />
                  <p>Brak logów do wyświetlenia</p>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="bg-gray-50 dark:bg-gray-900 px-6 py-4 flex items-center justify-between border-t border-gray-200 dark:border-gray-700">
          <div class="text-sm text-gray-600 dark:text-gray-400">
            Strona {{ currentPage }} z {{ totalPages }} ({{ totalCount }} rekordów)
          </div>
          <div class="flex gap-2">
            <button
              :disabled="currentPage === 1"
              class="px-3 py-2 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition"
              @click="prevPage"
            >
              <ChevronLeft class="w-5 h-5" />
            </button>
            <button
              :disabled="currentPage === totalPages"
              class="px-3 py-2 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition"
              @click="nextPage"
            >
              <ChevronRight class="w-5 h-5" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
