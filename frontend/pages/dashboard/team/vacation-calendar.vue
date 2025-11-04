<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { VacationCalendar, VacationSchedule, ViewMode, CalendarDay } from '~/types/vacation'
import type { DepartmentDto } from '~/types/department'
import VacationTimelineView from '~/components/vacation/VacationTimelineView.vue'
import VacationCalendarGrid from '~/components/vacation/VacationCalendarGrid.vue'
import VacationListView from '~/components/vacation/VacationListView.vue'

// Meta tags
definePageMeta({
  middleware: ['auth', 'verified'],
  layout: 'default'
})

// Page title
useHead({
  title: 'Kalendarz urlopów zespołu - PortalForge'
})

const config = useRuntimeConfig()
const authStore = useAuthStore()
const { getAuthHeaders } = useAuth()
const toast = useNotificationToast()

// State
const currentView = ref<ViewMode>('timeline')
const currentMonth = ref(new Date())
const calendarData = ref<VacationCalendar | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)
const selectedDepartmentId = ref<string | null>(null)

// Organizational permissions
const canViewAllDepartments = ref(false)
const visibleDepartments = ref<DepartmentDto[]>([])
const isLoadingPermissions = ref(true)

// Date range for current month
const startDate = computed(() => {
  const date = new Date(currentMonth.value)
  date.setDate(1)
  return date
})

const endDate = computed(() => {
  const date = new Date(currentMonth.value)
  date.setMonth(date.getMonth() + 1)
  date.setDate(0)
  return date
})

// Check if user can see department selector
const canSelectDepartment = computed(() => {
  return canViewAllDepartments.value || visibleDepartments.value.length > 1
})

// Load organizational permissions
const loadPermissions = async () => {
  const userId = authStore.user?.id
  if (!userId) {
    error.value = 'Brak zalogowanego użytkownika'
    return
  }

  isLoadingPermissions.value = true

  try {
    // Check if user is Admin or HR - they should see all departments
    const userRole = authStore.user?.role
    const isAdminOrHR = userRole === 'Admin' || userRole === 'HR'

    // Fetch organizational permissions
    const permissions = await $fetch(
      `${config.public.apiUrl}/api/admin/permissions/organizational/${userId}`,
      { headers: getAuthHeaders() }
    ) as { canViewAllDepartments: boolean; visibleDepartmentIds: string[] }

    // Grant full access to Admin and HR users regardless of organizational permissions
    canViewAllDepartments.value = isAdminOrHR || permissions.canViewAllDepartments

    // Fetch department tree
    const allDepartments = await $fetch(
      `${config.public.apiUrl}/api/departments`,
      { headers: getAuthHeaders() }
    ) as DepartmentDto[]

    if (canViewAllDepartments.value) {
      // Admin/HR or users with full permissions can see all departments
      visibleDepartments.value = allDepartments
    } else if (permissions.visibleDepartmentIds.length > 0) {
      // User has specific department permissions
      visibleDepartments.value = allDepartments.filter(dept =>
        permissions.visibleDepartmentIds.includes(dept.id)
      )
    } else {
      // User can only see their own department
      const userDepartmentId = authStore.user?.departmentId
      if (userDepartmentId) {
        const userDept = allDepartments.find(d => d.id === userDepartmentId)
        if (userDept) {
          visibleDepartments.value = [userDept]
        }
      }
    }

    // Auto-select user's department or first visible department
    if (visibleDepartments.value.length > 0) {
      const userDept = visibleDepartments.value.find(d => d.id === authStore.user?.departmentId)
      selectedDepartmentId.value = userDept?.id || visibleDepartments.value[0].id
      await fetchCalendarData()
    } else {
      error.value = 'Brak dostępnych działów do wyświetlenia'
    }
  } catch (err: any) {
    console.error('Error loading permissions:', err)
    error.value = err.message || 'Nie udało się załadować uprawnień'
  } finally {
    isLoadingPermissions.value = false
  }
}

// Fetch calendar data from API
const fetchCalendarData = async () => {
  if (!selectedDepartmentId.value) {
    error.value = 'Wybierz dział'
    return
  }

  isLoading.value = true
  error.value = null

  try {
    const year = currentMonth.value.getFullYear()
    const month = currentMonth.value.getMonth() + 1

    const response = await $fetch(
      `${config.public.apiUrl}/api/vacation-schedules/team?departmentId=${selectedDepartmentId.value}&year=${year}&month=${month}`,
      { headers: getAuthHeaders() }
    ) as VacationCalendar

    calendarData.value = response
  } catch (err: any) {
    console.error('Error fetching calendar data:', err)
    error.value = err.message || 'Nie udało się pobrać danych kalendarza'
  } finally {
    isLoading.value = false
  }
}

// Handle month change
const handleMonthChange = (newDate: Date) => {
  currentMonth.value = newDate
  fetchCalendarData()
}

// Handle vacation click (show details modal)
const handleVacationClick = (vacation: VacationSchedule) => {
  // TODO: Open modal with vacation details
  console.log('Vacation clicked:', vacation)
}

// Handle day click (calendar grid view)
const handleDayClick = (day: CalendarDay) => {
  // TODO: Show modal with all vacations for this day
  console.log('Day clicked:', day)
}

// Export to PDF
const handleExportPdf = async () => {
  if (!selectedDepartmentId.value) return

  try {
    const year = currentMonth.value.getFullYear()
    const month = currentMonth.value.getMonth() + 1

    const response = await $fetch(
      `${config.public.apiUrl}/api/vacation-schedules/export/pdf?departmentId=${selectedDepartmentId.value}&year=${year}&month=${month}`,
      { headers: getAuthHeaders(), responseType: 'blob' }
    )

    // Create download link
    const blob = new Blob([response as any], { type: 'application/pdf' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `kalendarz-urlopow-${year}-${month.toString().padStart(2, '0')}.pdf`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  } catch (err: any) {
    console.error('Export PDF error:', err)
    if (err.statusCode === 501) {
      toast.info('Eksport PDF będzie dostępny w przyszłej wersji', 'Użyj eksportu Excel jako alternatywy.')
    } else {
      toast.error('Nie udało się wyeksportować do PDF')
    }
  }
}

// Export to Excel
const handleExportExcel = async () => {
  if (!selectedDepartmentId.value) return

  try {
    const year = currentMonth.value.getFullYear()
    const month = currentMonth.value.getMonth() + 1

    const response = await $fetch(
      `${config.public.apiUrl}/api/vacation-schedules/export/excel?departmentId=${selectedDepartmentId.value}&year=${year}&month=${month}`,
      { headers: getAuthHeaders(), responseType: 'blob' }
    )

    // Create download link
    const blob = new Blob([response as any], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `kalendarz-urlopow-${year}-${month.toString().padStart(2, '0')}.xlsx`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  } catch (err: any) {
    console.error('Export Excel error:', err)
    if (err.statusCode === 501) {
      toast.info('Eksport Excel będzie dostępny w przyszłej wersji')
    } else {
      toast.error('Nie udało się wyeksportować do Excel')
    }
  }
}

// Format month/year for display
const monthYearLabel = computed(() => {
  return currentMonth.value.toLocaleDateString('pl-PL', {
    month: 'long',
    year: 'numeric'
  })
})

// Get selected department name
const selectedDepartmentName = computed(() => {
  const dept = visibleDepartments.value.find(d => d.id === selectedDepartmentId.value)
  return dept?.name || 'Nieznany dział'
})

// Load permissions and calendar on mount
onMounted(async () => {
  await loadPermissions()
})
</script>

<template>
  <div class="vacation-calendar-page p-6 space-y-6">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
          📅 Kalendarz urlopów zespołu
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
          {{ monthYearLabel }} • {{ selectedDepartmentName }}
        </p>
      </div>

      <div class="flex gap-3">
        <button
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
          :disabled="!selectedDepartmentId || isLoading"
          @click="handleExportPdf"
        >
          <svg
            class="w-5 h-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"
            />
          </svg>
          Eksport PDF
        </button>

        <button
          class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
          :disabled="!selectedDepartmentId || isLoading"
          @click="handleExportExcel"
        >
          <svg
            class="w-5 h-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            />
          </svg>
          Eksport Excel
        </button>
      </div>
    </div>

    <!-- Department Selector (only if user can select departments) -->
    <div
      v-if="canSelectDepartment && !isLoadingPermissions"
      class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4"
    >
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        {{ canViewAllDepartments ? 'Wybierz dział (możesz przeglądać wszystkie działy)' : 'Wybierz dział' }}
      </label>
      <select
        v-model="selectedDepartmentId"
        @change="fetchCalendarData"
        class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
      >
        <option
          v-for="dept in visibleDepartments"
          :key="dept.id"
          :value="dept.id"
        >
          {{ dept.name }}
        </option>
      </select>
    </div>

    <!-- Info banner for single department users -->
    <div
      v-else-if="!canSelectDepartment && !isLoadingPermissions"
      class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4"
    >
      <div class="flex items-start">
        <svg
          class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 mr-3"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
        <div class="text-sm text-blue-800 dark:text-blue-300">
          <p class="font-semibold mb-1">Widok kalendarza dla Twojego działu</p>
          <p>Obecnie przeglądasz kalendarz dla: <strong>{{ selectedDepartmentName }}</strong></p>
        </div>
      </div>
    </div>

    <!-- Loading Permissions State -->
    <div
      v-if="isLoadingPermissions"
      class="flex items-center justify-center h-64"
    >
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
        <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie uprawnień...</p>
      </div>
    </div>

    <!-- Main Content -->
    <template v-else>
      <!-- Statistics Cards -->
      <div
        v-if="calendarData"
        class="grid grid-cols-1 md:grid-cols-3 gap-4"
      >
        <div class="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4 border border-blue-200 dark:border-blue-800">
          <div class="flex items-center gap-3">
            <div class="p-3 bg-blue-100 dark:bg-blue-900/40 rounded-lg">
              <svg
                class="w-6 h-6 text-blue-600 dark:text-blue-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
                />
              </svg>
            </div>
            <div>
              <div class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ calendarData.statistics.currentlyOnVacation }}
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">
                Obecnie na urlopie
              </div>
              <div class="text-xs text-gray-500 dark:text-gray-500">
                z {{ calendarData.statistics.teamSize }} pracowników
              </div>
            </div>
          </div>
        </div>

        <div class="bg-green-50 dark:bg-green-900/20 rounded-lg p-4 border border-green-200 dark:border-green-800">
          <div class="flex items-center gap-3">
            <div class="p-3 bg-green-100 dark:bg-green-900/40 rounded-lg">
              <svg
                class="w-6 h-6 text-green-600 dark:text-green-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>
            <div>
              <div class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ calendarData.statistics.scheduledVacations }}
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">
                Zaplanowanych urlopów
              </div>
              <div class="text-xs text-gray-500 dark:text-gray-500">
                w tym miesiącu
              </div>
            </div>
          </div>
        </div>

        <div
          v-if="calendarData.alerts.length > 0"
          class="bg-red-50 dark:bg-red-900/20 rounded-lg p-4 border border-red-200 dark:border-red-800"
        >
          <div class="flex items-center gap-3">
            <div class="p-3 bg-red-100 dark:bg-red-900/40 rounded-lg">
              <svg
                class="w-6 h-6 text-red-600 dark:text-red-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
            </div>
            <div>
              <div class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ calendarData.alerts.length }}
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">
                Alerty kolizji
              </div>
              <div class="text-xs text-gray-500 dark:text-gray-500 truncate">
                {{ calendarData.alerts[0]?.message || 'Brak alertów' }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Alerts Section -->
      <div
        v-if="calendarData && calendarData.alerts.length > 0"
        class="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg p-4"
      >
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-3 flex items-center gap-2">
          <svg
            class="w-5 h-5 text-yellow-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
          Ostrzeżenia o konfliktach urlopów
        </h3>
        <div class="space-y-2">
          <div
            v-for="alert in calendarData.alerts.slice(0, 3)"
            :key="alert.date"
            class="flex items-start gap-3 p-3 bg-white dark:bg-gray-800 rounded border"
            :class="{
              'border-red-300 dark:border-red-700': alert.type === 'COVERAGE_CRITICAL',
              'border-yellow-300 dark:border-yellow-700': alert.type === 'COVERAGE_LOW'
            }"
          >
            <div class="text-sm flex-1">
              <p class="font-medium text-gray-900 dark:text-white">
                {{ alert.message }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">
                Data: {{ new Date(alert.date).toLocaleDateString('pl-PL') }}
              </p>
            </div>
            <span
              class="px-2 py-1 text-xs font-medium rounded"
              :class="{
                'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300': alert.type === 'COVERAGE_CRITICAL',
                'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300': alert.type === 'COVERAGE_LOW'
              }"
            >
              {{ alert.coveragePercent.toFixed(0) }}%
            </span>
          </div>
        </div>
      </div>

      <!-- View Tabs -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
        <div class="border-b border-gray-200 dark:border-gray-700">
          <nav class="flex -mb-px">
            <button
              class="px-6 py-3 text-sm font-medium border-b-2 transition-colors"
              :class="{
                'border-blue-500 text-blue-600 dark:text-blue-400': currentView === 'timeline',
                'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300': currentView !== 'timeline'
              }"
              @click="currentView = 'timeline'"
            >
              Timeline (Gantt)
            </button>
            <button
              class="px-6 py-3 text-sm font-medium border-b-2 transition-colors"
              :class="{
                'border-blue-500 text-blue-600 dark:text-blue-400': currentView === 'calendar',
                'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300': currentView !== 'calendar'
              }"
              @click="currentView = 'calendar'"
            >
              Kalendarz
            </button>
            <button
              class="px-6 py-3 text-sm font-medium border-b-2 transition-colors"
              :class="{
                'border-blue-500 text-blue-600 dark:text-blue-400': currentView === 'list',
                'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300': currentView !== 'list'
              }"
              @click="currentView = 'list'"
            >
              Lista
            </button>
          </nav>
        </div>

        <!-- Loading State -->
        <div
          v-if="isLoading"
          class="flex items-center justify-center h-64"
        >
          <div class="text-center">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
            <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie danych...</p>
          </div>
        </div>

        <!-- Error State -->
        <div
          v-else-if="error"
          class="flex items-center justify-center h-64"
        >
          <div class="text-center">
            <svg
              class="mx-auto h-12 w-12 text-red-500"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <p class="mt-4 text-red-600 dark:text-red-400">{{ error }}</p>
          </div>
        </div>

        <!-- Empty State -->
        <div
          v-else-if="!calendarData"
          class="flex items-center justify-center h-64"
        >
          <div class="text-center text-gray-500 dark:text-gray-400">
            <svg
              class="mx-auto h-12 w-12 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
            <p class="mt-4">Ładowanie kalendarza...</p>
          </div>
        </div>

        <!-- View Components -->
        <div v-else class="p-6">
          <VacationTimelineView
            v-if="currentView === 'timeline'"
            :vacations="calendarData.vacations"
            :start-date="startDate"
            :end-date="endDate"
            @vacation-click="handleVacationClick"
          />

          <VacationCalendarGrid
            v-if="currentView === 'calendar'"
            :vacations="calendarData.vacations"
            :current-month="currentMonth"
            @day-click="handleDayClick"
            @month-change="handleMonthChange"
          />

          <VacationListView
            v-if="currentView === 'list'"
            :vacations="calendarData.vacations"
            @vacation-click="handleVacationClick"
          />
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.vacation-calendar-page {
  /* Additional styles if needed */
}
</style>
