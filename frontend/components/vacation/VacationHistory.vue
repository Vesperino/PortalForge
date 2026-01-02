<script setup lang="ts">
interface VacationHistoryEntry {
  id: number | string
  startDate: Date
  endDate: Date
  days: number
  type: string
  status: string
}

interface Props {
  entries: VacationHistoryEntry[]
  isLoading?: boolean
  error?: string | null
  title?: string
  emptyMessage?: string
}

withDefaults(defineProps<Props>(), {
  isLoading: false,
  error: null,
  title: 'Historia urlopów',
  emptyMessage: 'Brak historii urlopów'
})

interface Emits {
  (e: 'view-details', id: number | string): void
}

const emit = defineEmits<Emits>()

const formatDateShort = (date: Date) => {
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  }).format(date)
}

const formatDateRange = (startDate: Date, endDate: Date) => {
  const start = formatDateShort(startDate)
  const end = formatDateShort(endDate)
  return start === end ? start : `${start} - ${end}`
}

const getStatusColor = (status: string) => {
  const statusLower = String(status ?? '').toLowerCase()

  // Generic statuses used in requests
  if (statusLower.includes('zatwierdzony') || statusLower.includes('approved')) {
    return 'bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200'
  }
  if (statusLower.includes('odrzucony') || statusLower.includes('rejected')) {
    return 'bg-red-100 dark:bg-red-900 text-red-800 dark:text-red-200'
  }
  if (statusLower.includes('oczekuje') || statusLower.includes('pending') || statusLower.includes('w trakcie')) {
    return 'bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200'
  }
  if (statusLower.includes('anulowany') || statusLower.includes('cancelled')) {
    return 'bg-gray-100 dark:bg-gray-900 text-gray-800 dark:text-gray-200'
  }

  // Vacation-specific statuses
  if (statusLower.includes('scheduled')) {
    return 'bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200'
  }
  if (statusLower.includes('active')) {
    return 'bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200'
  }
  if (statusLower.includes('completed')) {
    return 'bg-gray-100 dark:bg-gray-900 text-gray-800 dark:text-gray-200'
  }

  return 'bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200'
}

const getTypeIcon = (type: string) => {
  const typeLower = type.toLowerCase()

  if (typeLower.includes('na żądanie') || typeLower.includes('on-demand')) {
    return 'M13 10V3L4 14h7v7l9-11h-7z' // lightning bolt
  } else if (typeLower.includes('okolicznościowy') || typeLower.includes('circumstantial')) {
    return 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z' // clock
  }

  return 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z' // calendar
}

const getDaysLabel = (days: number) => {
  if (days === 1) return 'dzień'
  return 'dni'
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <h4 class="font-semibold text-gray-900 dark:text-white flex items-center gap-2">
        <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        {{ title }} ({{ entries.length }})
      </h4>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex justify-center items-center py-8">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
    </div>

    <!-- Error State -->
    <div
      v-else-if="error"
      class="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg"
    >
      <p class="text-sm text-red-600 dark:text-red-400">
        {{ error }}
      </p>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="entries.length === 0"
      class="text-center py-8 text-gray-500 dark:text-gray-400"
    >
      <svg class="w-12 h-12 mx-auto mb-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
      </svg>
      <p class="text-sm">{{ emptyMessage }}</p>
    </div>

    <!-- History Entries -->
    <div v-else class="space-y-2">
      <div
        v-for="entry in entries"
        :key="entry.id"
        class="p-3 rounded-lg bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-600 transition-colors cursor-pointer"
        @click="emit('view-details', entry.id)"
      >
        <div class="flex items-start justify-between">
          <div class="flex-1">
            <div class="flex items-center gap-2 mb-1">
              <svg class="w-4 h-4 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="getTypeIcon(entry.type)" />
              </svg>
              <span class="text-sm font-medium text-gray-900 dark:text-white">
                {{ entry.type }}
              </span>
              <span
                class="px-2 py-0.5 text-xs font-medium rounded-full"
                :class="getStatusColor(entry.status)"
              >
                {{ entry.status }}
              </span>
            </div>
            <p class="text-xs text-gray-600 dark:text-gray-400">
              {{ formatDateRange(entry.startDate, entry.endDate) }}
            </p>
          </div>
          <div class="text-right ml-3">
            <span class="text-sm font-bold text-blue-600 dark:text-blue-400">
              {{ entry.days }} {{ getDaysLabel(entry.days) }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
