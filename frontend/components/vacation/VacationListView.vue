<script setup lang="ts">
import { computed, ref } from 'vue'
import type { VacationSchedule } from '~/types/vacation'
import { VacationStatus } from '~/types/vacation'

interface Props {
  vacations: VacationSchedule[]
}

const props = defineProps<Props>()

interface Emits {
  (e: 'vacation-click', vacation: VacationSchedule): void
}

const emit = defineEmits<Emits>()

// Sorting state
const sortColumn = ref<'employee' | 'startDate' | 'endDate' | 'days'>('startDate')
const sortDirection = ref<'asc' | 'desc'>('asc')

// Search and filter state
const searchQuery = ref('')
const statusFilter = ref<VacationStatus | 'all'>('all')

// Pagination
const currentPage = ref(1)
const itemsPerPage = 20

// Debounced search
const debouncedSearch = ref('')
let searchTimeout: NodeJS.Timeout | null = null

const updateSearch = (value: string) => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    debouncedSearch.value = value
    currentPage.value = 1 // Reset to first page
  }, 300)
}

// Calculate vacation duration
const getVacationDuration = (vacation: VacationSchedule): number => {
  const start = new Date(vacation.startDate)
  const end = new Date(vacation.endDate)
  const diffTime = Math.abs(end.getTime() - start.getTime())
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return diffDays + 1
}

// Filtered and sorted vacations
const filteredVacations = computed(() => {
  let result = props.vacations

  // Apply search filter
  if (debouncedSearch.value) {
    const query = debouncedSearch.value.toLowerCase()
    result = result.filter(
      (v) =>
        v.user.firstName.toLowerCase().includes(query) ||
        v.user.lastName.toLowerCase().includes(query) ||
        v.user.email?.toLowerCase().includes(query) ||
        v.substitute.firstName.toLowerCase().includes(query) ||
        v.substitute.lastName.toLowerCase().includes(query)
    )
  }

  // Apply status filter
  if (statusFilter.value !== 'all') {
    result = result.filter((v) => v.status === statusFilter.value)
  }

  // Apply sorting
  result = [...result].sort((a, b) => {
    let compareValue = 0

    switch (sortColumn.value) {
      case 'employee':
        compareValue = `${a.user.firstName} ${a.user.lastName}`.localeCompare(
          `${b.user.firstName} ${b.user.lastName}`
        )
        break
      case 'startDate':
        compareValue = new Date(a.startDate).getTime() - new Date(b.startDate).getTime()
        break
      case 'endDate':
        compareValue = new Date(a.endDate).getTime() - new Date(b.endDate).getTime()
        break
      case 'days':
        compareValue = getVacationDuration(a) - getVacationDuration(b)
        break
    }

    return sortDirection.value === 'asc' ? compareValue : -compareValue
  })

  return result
})

// Paginated vacations
const paginatedVacations = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filteredVacations.value.slice(start, end)
})

// Total pages
const totalPages = computed(() => {
  return Math.ceil(filteredVacations.value.length / itemsPerPage)
})

// Sort by column
const sortBy = (column: typeof sortColumn.value) => {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column
    sortDirection.value = 'asc'
  }
}

// Format date
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

// Get status label and color
const getStatusInfo = (status: VacationStatus) => {
  const info = {
    [VacationStatus.Scheduled]: { label: 'Zaplanowany', color: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300' },
    [VacationStatus.Active]: { label: 'Aktywny', color: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300' },
    [VacationStatus.Completed]: { label: 'Zakończony', color: 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-300' },
    [VacationStatus.Cancelled]: { label: 'Anulowany', color: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300' }
  }
  return info[status]
}

// Change page
const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
  }
}
</script>

<template>
  <div class="vacation-list-view bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
    <!-- Filters and search -->
    <div class="p-4 bg-gray-50 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700 space-y-4">
      <div class="flex flex-col md:flex-row gap-4">
        <!-- Search input -->
        <div class="flex-1">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Szukaj po nazwisku, imieniu lub emailu..."
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-white"
            @input="updateSearch(searchQuery)"
          >
        </div>

        <!-- Status filter -->
        <div class="w-full md:w-48">
          <select
            v-model="statusFilter"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-white"
          >
            <option value="all">
              Wszystkie statusy
            </option>
            <option :value="VacationStatus.Scheduled">
              Zaplanowany
            </option>
            <option :value="VacationStatus.Active">
              Aktywny
            </option>
            <option :value="VacationStatus.Completed">
              Zakończony
            </option>
            <option :value="VacationStatus.Cancelled">
              Anulowany
            </option>
          </select>
        </div>
      </div>

      <!-- Results count -->
      <div class="text-sm text-gray-600 dark:text-gray-400">
        Znaleziono: <span class="font-semibold">{{ filteredVacations.length }}</span> urlopów
      </div>
    </div>

    <!-- Table -->
    <div class="overflow-x-auto">
      <table class="w-full">
        <thead class="bg-gray-50 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700">
          <tr>
            <th
              class="px-4 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-800"
              @click="sortBy('employee')"
            >
              <div class="flex items-center gap-2">
                Pracownik
                <svg
                  v-if="sortColumn === 'employee'"
                  class="w-4 h-4"
                  :class="sortDirection === 'asc' ? 'transform rotate-180' : ''"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              </div>
            </th>
            <th
              class="px-4 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-800"
              @click="sortBy('startDate')"
            >
              <div class="flex items-center gap-2">
                Data rozpoczęcia
                <svg
                  v-if="sortColumn === 'startDate'"
                  class="w-4 h-4"
                  :class="sortDirection === 'asc' ? 'transform rotate-180' : ''"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              </div>
            </th>
            <th
              class="px-4 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-800"
              @click="sortBy('endDate')"
            >
              <div class="flex items-center gap-2">
                Data zakończenia
                <svg
                  v-if="sortColumn === 'endDate'"
                  class="w-4 h-4"
                  :class="sortDirection === 'asc' ? 'transform rotate-180' : ''"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              </div>
            </th>
            <th
              class="px-4 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-800"
              @click="sortBy('days')"
            >
              <div class="flex items-center gap-2">
                Dni
                <svg
                  v-if="sortColumn === 'days'"
                  class="w-4 h-4"
                  :class="sortDirection === 'asc' ? 'transform rotate-180' : ''"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              </div>
            </th>
            <th class="px-4 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider">
              Zastępca
            </th>
            <th class="px-4 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider">
              Status
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
          <tr
            v-for="vacation in paginatedVacations"
            :key="vacation.id"
            class="hover:bg-gray-50 dark:hover:bg-gray-900 cursor-pointer transition-colors"
            @click="emit('vacation-click', vacation)"
          >
            <td class="px-4 py-3">
              <div class="text-sm font-medium text-gray-900 dark:text-white">
                {{ vacation.user.firstName }} {{ vacation.user.lastName }}
              </div>
              <div
                v-if="vacation.user.position"
                class="text-xs text-gray-500 dark:text-gray-400"
              >
                {{ vacation.user.position }}
              </div>
            </td>
            <td class="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">
              {{ formatDate(vacation.startDate) }}
            </td>
            <td class="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">
              {{ formatDate(vacation.endDate) }}
            </td>
            <td class="px-4 py-3 text-sm font-medium text-gray-900 dark:text-white">
              {{ getVacationDuration(vacation) }}
            </td>
            <td class="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">
              {{ vacation.substitute.firstName }} {{ vacation.substitute.lastName }}
            </td>
            <td class="px-4 py-3">
              <span
                class="inline-flex px-2 py-1 text-xs font-medium rounded-full"
                :class="getStatusInfo(vacation.status).color"
              >
                {{ getStatusInfo(vacation.status).label }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Empty state -->
      <div
        v-if="filteredVacations.length === 0"
        class="flex items-center justify-center h-48 text-gray-500 dark:text-gray-400"
      >
        <div class="text-center">
          <svg
            class="mx-auto h-12 w-12 text-gray-400"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <p class="mt-2">Brak wyników</p>
          <p class="text-sm text-gray-400">Zmień kryteria wyszukiwania</p>
        </div>
      </div>
    </div>

    <!-- Pagination -->
    <div
      v-if="totalPages > 1"
      class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 border-t border-gray-200 dark:border-gray-700"
    >
      <div class="text-sm text-gray-600 dark:text-gray-400">
        Strona {{ currentPage }} z {{ totalPages }}
      </div>
      <div class="flex gap-2">
        <button
          class="px-3 py-1 text-sm bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          :disabled="currentPage === 1"
          @click="goToPage(currentPage - 1)"
        >
          Poprzednia
        </button>
        <button
          class="px-3 py-1 text-sm bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          :disabled="currentPage === totalPages"
          @click="goToPage(currentPage + 1)"
        >
          Następna
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vacation-list-view {
  /* Additional styles if needed */
}
</style>
