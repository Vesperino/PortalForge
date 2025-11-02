# Frontend: Vacation Calendar

**Module**: Team Vacation Calendar & Substitutions
**Status**: 📋 Planned
**Last Updated**: 2025-10-31

---

## Overview

The Vacation Calendar provides managers with a comprehensive view of their team's vacation schedules through:
1. **3 View Modes**: Timeline (Gantt), Calendar Grid, and List
2. **Real-time Statistics**: Currently on vacation, scheduled vacations, coverage alerts
3. **Conflict Detection**: Visual alerts when >30% of team is on vacation
4. **Export Functionality**: PDF and Excel export for reporting

---

## Page Structure

### Main Vacation Calendar Page

**Route**: `/dashboard/team/vacation-calendar`

**File**: `frontend/pages/dashboard/team/vacation-calendar.vue`

**Access**: Managers and above only (middleware: `['auth', 'manager']`)

```vue
<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 p-8">
    <!-- Header with export buttons -->
    <VacationCalendarHeader
      :current-month="currentMonth"
      @export-pdf="handleExportPdf"
      @export-excel="handleExportExcel"
      @month-change="handleMonthChange"
    />

    <!-- Statistics Dashboard -->
    <VacationStatistics
      :stats="calendarData.statistics"
      :alerts="calendarData.alerts"
      class="mb-8"
    />

    <!-- Conflict Alerts -->
    <VacationAlerts
      v-if="calendarData.alerts.length > 0"
      :alerts="calendarData.alerts"
      class="mb-8"
    />

    <!-- View Tabs -->
    <VacationViewTabs
      v-model="currentView"
      class="mb-6"
    />

    <!-- Filters -->
    <VacationFilters
      v-model:search="searchQuery"
      v-model:status="statusFilter"
      class="mb-6"
    />

    <!-- Main Content - Current View -->
    <div v-if="loading" class="flex justify-center py-12">
      <Spinner size="large" />
    </div>

    <div v-else-if="error" class="text-center py-12">
      <ErrorMessage :message="error" />
    </div>

    <div v-else>
      <!-- Timeline View (Gantt Chart) -->
      <VacationTimelineView
        v-if="currentView === 'timeline'"
        :vacations="filteredVacations"
        :start-date="viewStartDate"
        :end-date="viewEndDate"
        :team-size="calendarData.teamSize"
      />

      <!-- Calendar Grid View -->
      <VacationCalendarGrid
        v-else-if="currentView === 'calendar'"
        :vacations="filteredVacations"
        :current-month="currentMonth"
      />

      <!-- List View -->
      <VacationListView
        v-else
        :vacations="filteredVacations"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { VacationCalendar, VacationSchedule } from '~/types/vacation'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'manager'] // Only managers can access
})

const { getTeamVacationCalendar, exportToPdf, exportToExcel } = useVacationApi()
const authStore = useAuthStore()

// State
const currentView = ref<'timeline' | 'calendar' | 'list'>('timeline')
const currentMonth = ref(new Date())
const calendarData = ref<VacationCalendar>({
  vacations: [],
  teamSize: 0,
  alerts: [],
  statistics: {
    currentlyOnVacation: 0,
    scheduledVacations: 0,
    totalVacationDays: 0,
    averageVacationDays: 0,
    teamSize: 0,
    coveragePercent: 100
  }
})
const loading = ref(true)
const error = ref<string | null>(null)
const searchQuery = ref('')
const statusFilter = ref('')

// Computed
const viewStartDate = computed(() => {
  const date = new Date(currentMonth.value)
  date.setDate(1) // First day of month
  return date
})

const viewEndDate = computed(() => {
  const date = new Date(currentMonth.value)
  date.setMonth(date.getMonth() + 1)
  date.setDate(0) // Last day of month
  return date
})

const filteredVacations = computed(() => {
  let result = calendarData.value.vacations

  // Filter by search query
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(v =>
      v.user.fullName.toLowerCase().includes(query)
    )
  }

  // Filter by status
  if (statusFilter.value) {
    result = result.filter(v => v.status === statusFilter.value)
  }

  return result
})

// Methods
const loadCalendar = async () => {
  loading.value = true
  error.value = null

  try {
    const year = currentMonth.value.getFullYear()
    const month = currentMonth.value.getMonth() + 1

    calendarData.value = await getTeamVacationCalendar(
      authStore.user!.departmentId,
      year,
      month
    )
  } catch (err: any) {
    error.value = err.message || 'Nie udało się załadować kalendarza urlopów'
  } finally {
    loading.value = false
  }
}

const handleMonthChange = (newMonth: Date) => {
  currentMonth.value = newMonth
  loadCalendar()
}

const handleExportPdf = async () => {
  const year = currentMonth.value.getFullYear()
  const month = currentMonth.value.getMonth() + 1

  const blob = await exportToPdf(
    authStore.user!.departmentId,
    year,
    month
  )

  downloadFile(blob, `kalendarz-urlopow-${year}-${month.toString().padStart(2, '0')}.pdf`)
}

const handleExportExcel = async () => {
  const year = currentMonth.value.getFullYear()
  const month = currentMonth.value.getMonth() + 1

  const blob = await exportToExcel(
    authStore.user!.departmentId,
    year,
    month
  )

  downloadFile(blob, `kalendarz-urlopow-${year}-${month.toString().padStart(2, '0')}.xlsx`)
}

onMounted(() => {
  loadCalendar()
})
</script>
```

---

## Components

### 1. VacationTimelineView (Gantt Chart) ⭐

**File**: `frontend/components/vacation/VacationTimelineView.vue`

**Props**:
- `vacations`: `VacationSchedule[]` - List of vacations to display
- `startDate`: `Date` - Timeline start date
- `endDate`: `Date` - Timeline end date
- `teamSize`: `number` - Total team size

```vue
<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 overflow-x-auto">
    <!-- Header: Date labels -->
    <div class="flex mb-4 min-w-max">
      <div class="w-48 flex-shrink-0"></div>
      <div class="flex-1 flex">
        <div
          v-for="day in daysInRange"
          :key="day.getTime()"
          class="flex-1 text-center min-w-[30px]"
        >
          <div class="text-xs font-medium text-gray-600 dark:text-gray-400">
            {{ formatDay(day) }}
          </div>
          <div class="text-[10px] text-gray-500">
            {{ day.getDate() }}
          </div>
        </div>
      </div>
    </div>

    <!-- Rows: Employee + vacation bars -->
    <div
      v-for="employee in employees"
      :key="employee.id"
      class="flex items-center mb-2 hover:bg-gray-50 dark:hover:bg-gray-700/50 rounded-lg p-2 transition min-w-max"
    >
      <!-- Employee info -->
      <div class="w-48 flex-shrink-0 flex items-center gap-3">
        <UserAvatar :user="employee" size="sm" />
        <div class="min-w-0">
          <p class="font-medium text-gray-900 dark:text-white text-sm truncate">
            {{ employee.fullName }}
          </p>
          <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
            {{ employee.position }}
          </p>
        </div>
      </div>

      <!-- Timeline area -->
      <div class="flex-1 relative h-10 min-w-max">
        <!-- Vacation bars -->
        <div
          v-for="vacation in getVacationsForEmployee(employee.id)"
          :key="vacation.id"
          :style="getVacationBarStyle(vacation)"
          :class="getVacationBarClass(vacation)"
          class="absolute h-8 rounded-lg cursor-pointer transition-all hover:scale-105 hover:shadow-lg flex items-center px-2 group"
          @click="showVacationDetails(vacation)"
        >
          <!-- Vacation info (visible on hover for short bars) -->
          <span class="text-xs font-medium text-white truncate">
            {{ vacation.daysCount }} {{ vacation.daysCount === 1 ? 'dzień' : 'dni' }}
          </span>

          <!-- Tooltip on hover -->
          <Tooltip>
            <div class="text-left">
              <p class="font-semibold">{{ vacation.user.fullName }}</p>
              <p class="text-sm">{{ formatDate(vacation.startDate) }} - {{ formatDate(vacation.endDate) }}</p>
              <p class="text-sm">Zastępca: {{ vacation.substitute.fullName }}</p>
            </div>
          </Tooltip>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="employees.length === 0" class="text-center py-12">
      <CalendarOff class="w-16 h-16 mx-auto text-gray-400 mb-4" />
      <p class="text-gray-600 dark:text-gray-400">
        Brak urlopów w tym miesiącu
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { CalendarOff } from 'lucide-vue-next'
import type { VacationSchedule, User } from '~/types'

const props = defineProps<{
  vacations: VacationSchedule[]
  startDate: Date
  endDate: Date
  teamSize: number
}>()

// Get all employees who have vacations in this period
const employees = computed(() => {
  const uniqueEmployees = new Map<string, User>()

  props.vacations.forEach(v => {
    if (!uniqueEmployees.has(v.user.id)) {
      uniqueEmployees.set(v.user.id, v.user)
    }
  })

  return Array.from(uniqueEmployees.values())
    .sort((a, b) => a.fullName.localeCompare(b.fullName))
})

// Generate array of days in range
const daysInRange = computed(() => {
  const days: Date[] = []
  const current = new Date(props.startDate)

  while (current <= props.endDate) {
    days.push(new Date(current))
    current.setDate(current.getDate() + 1)
  }

  return days
})

const getVacationsForEmployee = (employeeId: string) => {
  return props.vacations.filter(v => v.user.id === employeeId)
}

const getVacationBarStyle = (vacation: VacationSchedule) => {
  const totalDays = daysInRange.value.length
  const rangeStart = props.startDate.getTime()
  const dayMs = 1000 * 60 * 60 * 24

  const vacationStart = Math.max(
    new Date(vacation.startDate).getTime(),
    rangeStart
  )
  const vacationEnd = Math.min(
    new Date(vacation.endDate).getTime(),
    props.endDate.getTime()
  )

  const daysFromStart = (vacationStart - rangeStart) / dayMs
  const vacationDays = (vacationEnd - vacationStart) / dayMs + 1

  const left = (daysFromStart / totalDays) * 100
  const width = (vacationDays / totalDays) * 100

  return {
    left: `${left}%`,
    width: `${width}%`
  }
}

const getVacationBarClass = (vacation: VacationSchedule) => {
  const classMap = {
    Scheduled: 'bg-green-500 hover:bg-green-600',
    Active: 'bg-blue-600 hover:bg-blue-700',
    Completed: 'bg-gray-400 hover:bg-gray-500',
    Cancelled: 'bg-red-400 hover:bg-red-500'
  }

  return classMap[vacation.status] || 'bg-gray-400'
}

const formatDay = (date: Date) => {
  return date.toLocaleDateString('pl-PL', { weekday: 'short' })
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL')
}

const showVacationDetails = (vacation: VacationSchedule) => {
  // Emit event or show modal with vacation details
  console.log('Show vacation details:', vacation)
}
</script>
```

---

### 2. VacationCalendarGrid (Month View)

**File**: `frontend/components/vacation/VacationCalendarGrid.vue`

```vue
<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
    <!-- Month header -->
    <div class="flex items-center justify-between mb-6">
      <h3 class="text-xl font-bold text-gray-900 dark:text-white">
        {{ monthName }} {{ year }}
      </h3>

      <div class="flex gap-2">
        <button
          @click="previousMonth"
          class="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded"
        >
          <ChevronLeft class="w-5 h-5" />
        </button>
        <button
          @click="nextMonth"
          class="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded"
        >
          <ChevronRight class="w-5 h-5" />
        </button>
      </div>
    </div>

    <!-- Weekday headers -->
    <div class="grid grid-cols-7 gap-2 mb-2">
      <div
        v-for="day in weekdays"
        :key="day"
        class="text-center text-sm font-semibold text-gray-700 dark:text-gray-300"
      >
        {{ day }}
      </div>
    </div>

    <!-- Calendar days -->
    <div class="grid grid-cols-7 gap-2">
      <div
        v-for="(day, index) in calendarDays"
        :key="index"
        :class="[
          'min-h-[100px] border border-gray-200 dark:border-gray-600 rounded-lg p-2',
          day.isCurrentMonth ? 'bg-white dark:bg-gray-800' : 'bg-gray-50 dark:bg-gray-900',
          day.isToday ? 'ring-2 ring-blue-500' : ''
        ]"
      >
        <!-- Day number -->
        <div class="flex items-center justify-between mb-1">
          <span
            :class="[
              'text-sm font-medium',
              day.isCurrentMonth ? 'text-gray-900 dark:text-white' : 'text-gray-400',
              day.isToday ? 'bg-blue-500 text-white rounded-full w-6 h-6 flex items-center justify-center' : ''
            ]"
          >
            {{ day.date.getDate() }}
          </span>
        </div>

        <!-- Vacations on this day -->
        <div v-if="day.vacations.length > 0" class="space-y-1">
          <div
            v-for="(vacation, vIndex) in day.vacations.slice(0, 3)"
            :key="vacation.id"
            :class="[
              'text-xs px-2 py-1 rounded truncate cursor-pointer',
              getVacationBadgeClass(vacation)
            ]"
            @click="showVacationDetails(vacation)"
          >
            {{ vacation.user.firstName }}
          </div>

          <!-- Show "+X more" if more than 3 -->
          <button
            v-if="day.vacations.length > 3"
            class="text-xs text-blue-600 dark:text-blue-400 hover:underline"
            @click="showDayDetails(day)"
          >
            +{{ day.vacations.length - 3 }} więcej
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ChevronLeft, ChevronRight } from 'lucide-vue-next'
import type { VacationSchedule } from '~/types'

const props = defineProps<{
  vacations: VacationSchedule[]
  currentMonth: Date
}>()

const emit = defineEmits<{
  (e: 'month-change', month: Date): void
}>()

const weekdays = ['Pon', 'Wt', 'Śr', 'Czw', 'Pt', 'Sob', 'Nd']

const monthName = computed(() => {
  return props.currentMonth.toLocaleDateString('pl-PL', { month: 'long' })
})

const year = computed(() => {
  return props.currentMonth.getFullYear()
})

const calendarDays = computed(() => {
  const year = props.currentMonth.getFullYear()
  const month = props.currentMonth.getMonth()

  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)

  // Start from Monday of the week containing first day
  const startDate = new Date(firstDay)
  const dayOfWeek = startDate.getDay()
  const daysToSubtract = dayOfWeek === 0 ? 6 : dayOfWeek - 1
  startDate.setDate(startDate.getDate() - daysToSubtract)

  // End on Sunday of the week containing last day
  const endDate = new Date(lastDay)
  const endDayOfWeek = endDate.getDay()
  const daysToAdd = endDayOfWeek === 0 ? 0 : 7 - endDayOfWeek
  endDate.setDate(endDate.getDate() + daysToAdd)

  // Generate all days
  const days = []
  const current = new Date(startDate)
  const today = new Date()
  today.setHours(0, 0, 0, 0)

  while (current <= endDate) {
    const date = new Date(current)
    const isCurrentMonth = date.getMonth() === month
    const isToday = date.getTime() === today.getTime()

    // Find vacations for this day
    const dayVacations = props.vacations.filter(v => {
      const start = new Date(v.startDate)
      const end = new Date(v.endDate)
      return date >= start && date <= end
    })

    days.push({
      date,
      isCurrentMonth,
      isToday,
      vacations: dayVacations
    })

    current.setDate(current.getDate() + 1)
  }

  return days
})

const getVacationBadgeClass = (vacation: VacationSchedule) => {
  const classMap = {
    Scheduled: 'bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-200',
    Active: 'bg-blue-100 dark:bg-blue-900/30 text-blue-800 dark:text-blue-200',
    Completed: 'bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200'
  }

  return classMap[vacation.status] || classMap.Completed
}

const previousMonth = () => {
  const newMonth = new Date(props.currentMonth)
  newMonth.setMonth(newMonth.getMonth() - 1)
  emit('month-change', newMonth)
}

const nextMonth = () => {
  const newMonth = new Date(props.currentMonth)
  newMonth.setMonth(newMonth.getMonth() + 1)
  emit('month-change', newMonth)
}

const showVacationDetails = (vacation: VacationSchedule) => {
  // Show modal with vacation details
  console.log('Show vacation:', vacation)
}

const showDayDetails = (day: any) => {
  // Show modal with all vacations for this day
  console.log('Show day:', day)
}
</script>
```

---

### 3. VacationListView (Table)

**File**: `frontend/components/vacation/VacationListView.vue`

```vue
<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
    <!-- Table -->
    <div class="overflow-x-auto">
      <table class="w-full">
        <thead class="bg-gray-50 dark:bg-gray-700">
          <tr>
            <th
              v-for="column in columns"
              :key="column.key"
              class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-600"
              @click="sortBy(column.key)"
            >
              <div class="flex items-center gap-2">
                {{ column.label }}
                <component
                  :is="getSortIcon(column.key)"
                  v-if="sortColumn === column.key"
                  class="w-4 h-4"
                />
              </div>
            </th>
          </tr>
        </thead>

        <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
          <tr
            v-for="vacation in sortedVacations"
            :key="vacation.id"
            class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition"
          >
            <!-- Employee -->
            <td class="px-6 py-4 whitespace-nowrap">
              <div class="flex items-center gap-3">
                <UserAvatar :user="vacation.user" size="sm" />
                <div>
                  <p class="font-medium text-gray-900 dark:text-white">
                    {{ vacation.user.fullName }}
                  </p>
                  <p class="text-sm text-gray-500 dark:text-gray-400">
                    {{ vacation.user.position }}
                  </p>
                </div>
              </div>
            </td>

            <!-- Start Date -->
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
              {{ formatDate(vacation.startDate) }}
            </td>

            <!-- End Date -->
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
              {{ formatDate(vacation.endDate) }}
            </td>

            <!-- Days Count -->
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
              {{ vacation.daysCount }} {{ vacation.daysCount === 1 ? 'dzień' : 'dni' }}
            </td>

            <!-- Substitute -->
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white">
              {{ vacation.substitute.fullName }}
            </td>

            <!-- Status -->
            <td class="px-6 py-4 whitespace-nowrap">
              <span
                :class="[
                  'px-3 py-1 text-xs font-medium rounded-full',
                  getStatusBadgeClass(vacation.status)
                ]"
              >
                {{ getStatusLabel(vacation.status) }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div class="px-6 py-4 border-t border-gray-200 dark:border-gray-700 flex items-center justify-between">
      <div class="text-sm text-gray-700 dark:text-gray-300">
        Showing {{ paginatedVacations.length }} of {{ sortedVacations.length }} vacations
      </div>

      <div class="flex gap-2">
        <button
          :disabled="currentPage === 1"
          @click="currentPage--"
          class="px-3 py-1 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50"
        >
          Previous
        </button>
        <button
          :disabled="currentPage === totalPages"
          @click="currentPage++"
          class="px-3 py-1 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ArrowUp, ArrowDown } from 'lucide-vue-next'
import type { VacationSchedule } from '~/types'

const props = defineProps<{
  vacations: VacationSchedule[]
}>()

const columns = [
  { key: 'user', label: 'Pracownik' },
  { key: 'startDate', label: 'Data rozpoczęcia' },
  { key: 'endDate', label: 'Data zakończenia' },
  { key: 'daysCount', label: 'Liczba dni' },
  { key: 'substitute', label: 'Zastępca' },
  { key: 'status', label: 'Status' }
]

const sortColumn = ref<string>('startDate')
const sortDirection = ref<'asc' | 'desc'>('asc')
const currentPage = ref(1)
const itemsPerPage = 20

const sortedVacations = computed(() => {
  const sorted = [...props.vacations]

  sorted.sort((a, b) => {
    let aValue, bValue

    switch (sortColumn.value) {
      case 'user':
        aValue = a.user.fullName
        bValue = b.user.fullName
        break
      case 'startDate':
        aValue = new Date(a.startDate).getTime()
        bValue = new Date(b.startDate).getTime()
        break
      case 'endDate':
        aValue = new Date(a.endDate).getTime()
        bValue = new Date(b.endDate).getTime()
        break
      case 'daysCount':
        aValue = a.daysCount
        bValue = b.daysCount
        break
      case 'substitute':
        aValue = a.substitute.fullName
        bValue = b.substitute.fullName
        break
      case 'status':
        aValue = a.status
        bValue = b.status
        break
      default:
        return 0
    }

    if (aValue < bValue) return sortDirection.value === 'asc' ? -1 : 1
    if (aValue > bValue) return sortDirection.value === 'asc' ? 1 : -1
    return 0
  })

  return sorted
})

const paginatedVacations = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return sortedVacations.value.slice(start, end)
})

const totalPages = computed(() => {
  return Math.ceil(sortedVacations.value.length / itemsPerPage)
})

const sortBy = (column: string) => {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column
    sortDirection.value = 'asc'
  }
}

const getSortIcon = (column: string) => {
  if (sortColumn.value !== column) return null
  return sortDirection.value === 'asc' ? ArrowUp : ArrowDown
}

const getStatusBadgeClass = (status: string) => {
  const classMap = {
    Scheduled: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-200',
    Active: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-200',
    Completed: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200',
    Cancelled: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-200'
  }

  return classMap[status] || classMap.Completed
}

const getStatusLabel = (status: string) => {
  const labels = {
    Scheduled: 'Zaplanowany',
    Active: 'Aktywny',
    Completed: 'Zakończony',
    Cancelled: 'Anulowany'
  }

  return labels[status] || status
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}
</script>
```

---

## Composables

### useVacationApi

**File**: `frontend/composables/useVacationApi.ts`

```typescript
export const useVacationApi = () => {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()

  const getTeamVacationCalendar = async (
    departmentId: string,
    year: number,
    month: number
  ): Promise<VacationCalendar> => {
    const response = await $fetch(`${config.public.apiBase}/vacation-schedules/team`, {
      method: 'GET',
      params: { departmentId, year, month },
      headers: {
        Authorization: `Bearer ${authStore.token}`
      }
    })

    return response
  }

  const getMySubstitutions = async (): Promise<VacationSchedule[]> => {
    const response = await $fetch(`${config.public.apiBase}/vacation-schedules/my-substitutions`, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${authStore.token}`
      }
    })

    return response
  }

  const exportToPdf = async (
    departmentId: string,
    year: number,
    month: number
  ): Promise<Blob> => {
    const response = await $fetch(`${config.public.apiBase}/vacation-schedules/export/pdf`, {
      method: 'GET',
      params: { departmentId, year, month },
      headers: {
        Authorization: `Bearer ${authStore.token}`
      },
      responseType: 'blob'
    })

    return response
  }

  const exportToExcel = async (
    departmentId: string,
    year: number,
    month: number
  ): Promise<Blob> => {
    const response = await $fetch(`${config.public.apiBase}/vacation-schedules/export/excel`, {
      method: 'GET',
      params: { departmentId, year, month },
      headers: {
        Authorization: `Bearer ${authStore.token}`
      },
      responseType: 'blob'
    })

    return response
  }

  return {
    getTeamVacationCalendar,
    getMySubstitutions,
    exportToPdf,
    exportToExcel
  }
}
```

---

## Types

**File**: `frontend/types/vacation.ts`

```typescript
export interface VacationSchedule {
  id: string
  userId: string
  user: User
  substituteUserId: string
  substitute: User
  startDate: string
  endDate: string
  sourceRequestId: string
  status: VacationStatus
  createdAt: string
  daysCount: number
}

export type VacationStatus = 'Scheduled' | 'Active' | 'Completed' | 'Cancelled'

export interface VacationCalendar {
  vacations: VacationSchedule[]
  teamSize: number
  alerts: VacationAlert[]
  statistics: VacationStatistics
}

export interface VacationAlert {
  date: string
  type: 'COVERAGE_LOW' | 'COVERAGE_CRITICAL'
  affectedEmployees: User[]
  coveragePercent: number
  message: string
}

export interface VacationStatistics {
  currentlyOnVacation: number
  scheduledVacations: number
  totalVacationDays: number
  averageVacationDays: number
  teamSize: number
  coveragePercent: number
}
```

---

## Utilities

### Download File Helper

```typescript
export const downloadFile = (blob: Blob, filename: string) => {
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}
```

---

## Accessibility

1. **Keyboard Navigation**: All interactive elements focusable with Tab
2. **ARIA Labels**: Proper labels for screen readers
3. **Color Contrast**: WCAG AA compliant
4. **Focus Indicators**: Visible focus states

---

## Performance Optimizations

1. **Virtual Scrolling**: For large lists (>100 items)
2. **Lazy Loading**: Load vacation details on demand
3. **Caching**: Cache calendar data for 5 minutes
4. **Debouncing**: Debounce search input (300ms)

---

## Mobile Responsiveness

- Timeline view: Horizontal scroll on mobile
- Calendar grid: Collapse to weekly view on small screens
- List view: Stack columns vertically on mobile
