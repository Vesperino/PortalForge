<script setup lang="ts">
import { computed, ref } from 'vue'
import type { VacationSchedule, TimelineRow, VacationBarStyle } from '~/types/vacation'
import { VacationStatus } from '~/types/vacation'

interface Props {
  vacations: VacationSchedule[]
  startDate: Date
  endDate: Date
}

const props = defineProps<Props>()

interface Emits {
  (e: 'vacation-click', vacation: VacationSchedule): void
}

const emit = defineEmits<Emits>()

// Hover state
const hoveredVacation = ref<string | null>(null)

// Calculate days in the range
const daysInRange = computed(() => {
  const days: Date[] = []
  const current = new Date(props.startDate)
  const end = new Date(props.endDate)

  while (current <= end) {
    days.push(new Date(current))
    current.setDate(current.getDate() + 1)
  }

  return days
})

// Group vacations by employee
const timelineRows = computed<TimelineRow[]>(() => {
  const employeeMap = new Map<string, TimelineRow>()

  props.vacations.forEach((vacation) => {
    const userId = vacation.userId
    if (!employeeMap.has(userId)) {
      employeeMap.set(userId, {
        employee: vacation.user,
        vacations: []
      })
    }
    employeeMap.get(userId)!.vacations.push(vacation)
  })

  // Sort by employee name
  return Array.from(employeeMap.values()).sort((a, b) =>
    `${a.employee.firstName} ${a.employee.lastName}`.localeCompare(
      `${b.employee.firstName} ${b.employee.lastName}`
    )
  )
})

// Calculate vacation bar position and width
const getVacationBarStyle = (vacation: VacationSchedule): VacationBarStyle => {
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

  const daysFromStart = Math.floor((vacationStart - rangeStart) / dayMs)
  const vacationDays = Math.floor((vacationEnd - vacationStart) / dayMs) + 1

  // Color by status
  let backgroundColor = ''
  switch (vacation.status) {
    case VacationStatus.Scheduled:
      backgroundColor = 'bg-green-500'
      break
    case VacationStatus.Active:
      backgroundColor = 'bg-blue-500'
      break
    case VacationStatus.Completed:
      backgroundColor = 'bg-gray-400'
      break
    case VacationStatus.Cancelled:
      backgroundColor = 'bg-red-400'
      break
  }

  return {
    left: `${(daysFromStart / totalDays) * 100}%`,
    width: `${(vacationDays / totalDays) * 100}%`,
    backgroundColor
  }
}

// Format date for display
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

// Get status label
const getStatusLabel = (status: VacationStatus): string => {
  const labels = {
    [VacationStatus.Scheduled]: 'Zaplanowany',
    [VacationStatus.Active]: 'Aktywny',
    [VacationStatus.Completed]: 'Zakończony',
    [VacationStatus.Cancelled]: 'Anulowany'
  }
  return labels[status]
}

// Calculate vacation duration
const getVacationDuration = (vacation: VacationSchedule): number => {
  const start = new Date(vacation.startDate)
  const end = new Date(vacation.endDate)
  const diffTime = Math.abs(end.getTime() - start.getTime())
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return diffDays + 1
}
</script>

<template>
  <div class="vacation-timeline bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
    <!-- Timeline Header (Dates) -->
    <div class="flex border-b border-gray-200 dark:border-gray-700">
      <div class="w-48 flex-shrink-0 bg-gray-50 dark:bg-gray-900 p-3 font-semibold text-sm text-gray-700 dark:text-gray-300">
        Pracownik
      </div>
      <div class="flex-1 overflow-x-auto">
        <div class="flex min-w-full">
          <div
            v-for="(day, index) in daysInRange"
            :key="index"
            class="flex-1 min-w-[30px] text-center p-2 text-xs font-medium text-gray-600 dark:text-gray-400 border-r border-gray-100 dark:border-gray-700"
          >
            <div>{{ day.getDate() }}</div>
            <div class="text-[10px] text-gray-400">
              {{ day.toLocaleDateString('pl-PL', { weekday: 'short' }).substr(0, 2) }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Timeline Rows (Employees with vacation bars) -->
    <div class="overflow-y-auto max-h-[600px]">
      <div
        v-for="(row, rowIndex) in timelineRows"
        :key="row.employee.id"
        class="flex border-b border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors"
      >
        <!-- Employee name -->
        <div class="w-48 flex-shrink-0 p-3 bg-gray-50 dark:bg-gray-900">
          <div class="text-sm font-medium text-gray-900 dark:text-white truncate">
            {{ row.employee.firstName }} {{ row.employee.lastName }}
          </div>
          <div
            v-if="row.employee.position"
            class="text-xs text-gray-500 dark:text-gray-400 truncate"
          >
            {{ row.employee.position }}
          </div>
        </div>

        <!-- Timeline bars container -->
        <div class="flex-1 relative h-16 p-2">
          <div
            v-for="vacation in row.vacations"
            :key="vacation.id"
            class="absolute h-8 rounded cursor-pointer transition-all duration-200 hover:ring-2 hover:ring-offset-1 hover:ring-blue-500 hover:z-10"
            :class="getVacationBarStyle(vacation).backgroundColor"
            :style="{
              left: getVacationBarStyle(vacation).left,
              width: getVacationBarStyle(vacation).width,
              top: '8px'
            }"
            @mouseenter="hoveredVacation = vacation.id"
            @mouseleave="hoveredVacation = null"
            @click="emit('vacation-click', vacation)"
          >
            <!-- Inline day count label on bar -->
            <span class="text-[10px] font-semibold text-white px-2 select-none">
              {{ getVacationDuration(vacation) }}d
            </span>
            <!-- Tooltip -->
            <div
              v-if="hoveredVacation === vacation.id"
              class="absolute z-20 bottom-full left-0 mb-2 px-3 py-2 bg-gray-900 text-white text-xs rounded shadow-lg whitespace-nowrap"
            >
              <div class="font-semibold mb-1">
                {{ row.employee.firstName }} {{ row.employee.lastName }}
              </div>
              <div>
                <span class="text-gray-300">Urlop:</span>
                {{ formatDate(vacation.startDate) }} - {{ formatDate(vacation.endDate) }}
              </div>
              <div>
                <span class="text-gray-300">Dni:</span> {{ getVacationDuration(vacation) }}
              </div>
              <div>
                <span class="text-gray-300">Zastępca:</span>
                {{ vacation.substitute ? `${vacation.substitute.firstName} ${vacation.substitute.lastName}` : '—' }}
              </div>
              <div>
                <span class="text-gray-300">Status:</span> {{ getStatusLabel(vacation.status) }}
              </div>
              <!-- Arrow pointing down -->
              <div class="absolute top-full left-4 w-0 h-0 border-l-4 border-r-4 border-t-4 border-transparent border-t-gray-900" />
            </div>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <div
        v-if="timelineRows.length === 0"
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
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
          <p class="mt-2">Brak urlopów w wybranym okresie</p>
        </div>
      </div>
    </div>

    <!-- Legend -->
    <div class="flex gap-4 p-4 bg-gray-50 dark:bg-gray-900 border-t border-gray-200 dark:border-gray-700">
      <div class="flex items-center gap-2 text-sm">
        <div class="w-4 h-4 rounded bg-green-500" />
        <span class="text-gray-700 dark:text-gray-300">Zaplanowany</span>
      </div>
      <div class="flex items-center gap-2 text-sm">
        <div class="w-4 h-4 rounded bg-blue-500" />
        <span class="text-gray-700 dark:text-gray-300">Aktywny</span>
      </div>
      <div class="flex items-center gap-2 text-sm">
        <div class="w-4 h-4 rounded bg-gray-400" />
        <span class="text-gray-700 dark:text-gray-300">Zakończony</span>
      </div>
      <div class="flex items-center gap-2 text-sm">
        <div class="w-4 h-4 rounded bg-red-400" />
        <span class="text-gray-700 dark:text-gray-300">Anulowany</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vacation-timeline {
  position: relative;
}
</style>
