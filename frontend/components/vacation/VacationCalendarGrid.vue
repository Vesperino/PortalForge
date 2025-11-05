<script setup lang="ts">
import { computed } from 'vue'
import type { VacationSchedule, CalendarDay } from '~/types/vacation'

interface Props {
  vacations: VacationSchedule[]
  currentMonth: Date
}

const props = defineProps<Props>()

interface Emits {
  (e: 'day-click', day: CalendarDay): void
  (e: 'month-change', date: Date): void
}

const emit = defineEmits<Emits>()

// Weekday labels
const weekDays = ['Pon', 'Wt', 'Śr', 'Czw', 'Pt', 'Sob', 'Nie']

// Calculate calendar days (5-6 weeks starting from Monday before month start)
const calendarDays = computed<CalendarDay[]>(() => {
  const year = props.currentMonth.getFullYear()
  const month = props.currentMonth.getMonth()

  // First day of current month
  const firstDay = new Date(year, month, 1)

  // Find Monday before or on first day of month
  const startDate = new Date(firstDay)
  const dayOfWeek = startDate.getDay()
  const daysToSubtract = dayOfWeek === 0 ? 6 : dayOfWeek - 1 // Monday is 0
  startDate.setDate(startDate.getDate() - daysToSubtract)

  // Generate 42 days (6 weeks)
  const days: CalendarDay[] = []
  const today = new Date()
  today.setHours(0, 0, 0, 0)

  for (let i = 0; i < 42; i++) {
    const currentDate = new Date(startDate)
    currentDate.setDate(startDate.getDate() + i)

    // Find vacations for this day
    const dayVacations = props.vacations.filter((vacation) => {
      const vStart = new Date(vacation.startDate)
      const vEnd = new Date(vacation.endDate)
      vStart.setHours(0, 0, 0, 0)
      vEnd.setHours(0, 0, 0, 0)
      const current = new Date(currentDate)
      current.setHours(0, 0, 0, 0)

      return current >= vStart && current <= vEnd
    })

    days.push({
      date: currentDate,
      isCurrentMonth: currentDate.getMonth() === month,
      isToday:
        currentDate.getFullYear() === today.getFullYear() &&
        currentDate.getMonth() === today.getMonth() &&
        currentDate.getDate() === today.getDate(),
      vacations: dayVacations
    })
  }

  return days
})

// Navigate to previous month
const prevMonth = () => {
  const newDate = new Date(props.currentMonth)
  newDate.setMonth(newDate.getMonth() - 1)
  emit('month-change', newDate)
}

// Navigate to next month
const nextMonth = () => {
  const newDate = new Date(props.currentMonth)
  newDate.setMonth(newDate.getMonth() + 1)
  emit('month-change', newDate)
}

// Format month and year
const monthYearLabel = computed(() => {
  return props.currentMonth.toLocaleDateString('pl-PL', {
    month: 'long',
    year: 'numeric'
  })
})

// Handle day click
const handleDayClick = (day: CalendarDay) => {
  if (day.vacations.length > 0 || day.isCurrentMonth) {
    emit('day-click', day)
  }
}
</script>

<template>
  <div class="vacation-calendar-grid bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
    <!-- Header with month navigation -->
    <div class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700">
      <button
        class="p-2 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors"
        @click="prevMonth"
      >
        <svg
          class="w-5 h-5 text-gray-600 dark:text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M15 19l-7-7 7-7"
          />
        </svg>
      </button>

      <h2 class="text-lg font-semibold text-gray-900 dark:text-white capitalize">
        {{ monthYearLabel }}
      </h2>

      <button
        class="p-2 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors"
        @click="nextMonth"
      >
        <svg
          class="w-5 h-5 text-gray-600 dark:text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 5l7 7-7 7"
          />
        </svg>
      </button>
    </div>

    <!-- Weekday headers -->
    <div class="grid grid-cols-7 border-b border-gray-200 dark:border-gray-700">
      <div
        v-for="day in weekDays"
        :key="day"
        class="p-3 text-center text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wider"
      >
        {{ day }}
      </div>
    </div>

    <!-- Calendar grid -->
    <div class="grid grid-cols-7">
      <div
        v-for="(day, index) in calendarDays"
        :key="index"
        class="min-h-[100px] border-r border-b border-gray-100 dark:border-gray-700 p-2 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-900 transition-colors relative"
        :class="{
          'bg-gray-50 dark:bg-gray-900': !day.isCurrentMonth,
          'ring-2 ring-blue-500 ring-inset': day.isToday
        }"
        @click="handleDayClick(day)"
      >
        <!-- Day number -->
        <div
          class="text-sm font-medium mb-1"
          :class="{
            'text-gray-400 dark:text-gray-600': !day.isCurrentMonth,
            'text-gray-900 dark:text-white': day.isCurrentMonth && !day.isToday,
            'text-blue-600 dark:text-blue-400 font-bold': day.isToday
          }"
        >
          {{ day.date.getDate() }}
        </div>

        <!-- Vacation badges (show max 3, then "+X more") -->
        <div class="space-y-1">
          <div
            v-for="vacation in day.vacations.slice(0, 3)"
            :key="vacation.id"
            class="text-xs px-2 py-0.5 rounded truncate"
            :class="{
              'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300': vacation.status === 'Scheduled',
              'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300': vacation.status === 'Active',
              'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-300': vacation.status === 'Completed'
            }"
            :title="`${vacation.user.firstName} ${vacation.user.lastName}`"
          >
            {{ vacation.user.firstName }} {{ vacation.user.lastName.charAt(0) }}.
          </div>

          <!-- "+X more" badge -->
          <div
            v-if="day.vacations.length > 3"
            class="text-xs px-2 py-0.5 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded font-medium text-center"
          >
            +{{ day.vacations.length - 3 }} więcej
          </div>
        </div>

        <!-- Today indicator (blue dot in corner) -->
        <div
          v-if="day.isToday"
          class="absolute top-1 right-1 w-2 h-2 bg-blue-500 rounded-full"
        />
      </div>
    </div>

    <!-- Footer with legend -->
    <div class="flex gap-4 p-4 bg-gray-50 dark:bg-gray-900 border-t border-gray-200 dark:border-gray-700 text-xs">
      <div class="flex items-center gap-2">
        <div class="w-3 h-3 rounded bg-green-100 dark:bg-green-900/30 border border-green-300" />
        <span class="text-gray-700 dark:text-gray-300">Zaplanowany</span>
      </div>
      <div class="flex items-center gap-2">
        <div class="w-3 h-3 rounded bg-blue-100 dark:bg-blue-900/30 border border-blue-300" />
        <span class="text-gray-700 dark:text-gray-300">Aktywny</span>
      </div>
      <div class="flex items-center gap-2">
        <div class="w-3 h-3 rounded bg-gray-100 dark:bg-gray-900/30 border border-gray-300" />
        <span class="text-gray-700 dark:text-gray-300">Zakończony</span>
      </div>
      <div class="flex items-center gap-2 ml-auto">
        <div class="w-3 h-3 rounded-full bg-blue-500" />
        <span class="text-gray-700 dark:text-gray-300">Dzisiaj</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vacation-calendar-grid {
  /* Additional styles if needed */
}
</style>
