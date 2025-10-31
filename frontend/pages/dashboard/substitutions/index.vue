<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Calendar, CheckCircle, Clock, User } from 'lucide-vue-next'
import type { VacationSchedule } from '~/types/vacation'
import { VacationStatus } from '~/types/vacation'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

// State
const substitutions = ref<VacationSchedule[]>([])
const isLoading = ref(true)
const error = ref<string | null>(null)

// Filtered substitutions by status
const activeSubstitutions = computed(() => {
  return substitutions.value.filter(s => s.status === VacationStatus.Active)
})

const scheduledSubstitutions = computed(() => {
  return substitutions.value.filter(s => s.status === VacationStatus.Scheduled)
})

const completedSubstitutions = computed(() => {
  return substitutions.value
    .filter(s => s.status === VacationStatus.Completed)
    .slice(0, 5) // Last 5 completed
})

// Check if there are any substitutions
const hasAnySubstitutions = computed(() => {
  return substitutions.value.length > 0
})

// Format date range
const formatDateRange = (startDate: string, endDate: string): string => {
  const start = new Date(startDate)
  const end = new Date(endDate)

  const startFormatted = start.toLocaleDateString('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })

  const endFormatted = end.toLocaleDateString('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })

  return `${startFormatted} - ${endFormatted}`
}

// Calculate vacation duration
const getVacationDuration = (vacation: VacationSchedule): number => {
  const start = new Date(vacation.startDate)
  const end = new Date(vacation.endDate)
  const diffTime = Math.abs(end.getTime() - start.getTime())
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return diffDays + 1
}

// Get user full name
const getUserFullName = (user: { firstName: string; lastName: string }): string => {
  return `${user.firstName} ${user.lastName}`
}

// Load substitutions
const loadSubstitutions = async () => {
  isLoading.value = true
  error.value = null

  try {
    const data = await $fetch<VacationSchedule[]>('/api/vacation-schedules/my-substitutions')
    substitutions.value = data
  } catch (err: any) {
    error.value = err.message || 'Nie udało się pobrać zastępstw'
    console.error('Error loading substitutions:', err)
  } finally {
    isLoading.value = false
  }
}

// Navigate to approvals (filtered by user being substituted)
const viewApprovals = (userId: string) => {
  // Navigate to approvals page with filter
  navigateTo(`/dashboard/approvals?userId=${userId}`)
}

// Load data on mount
onMounted(() => {
  loadSubstitutions()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Page Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
          Moje zastępstwa
        </h1>
        <p class="text-gray-600 dark:text-gray-400">
          Zobacz aktywne i zaplanowane zastępstwa podczas urlopów innych osób
        </p>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="flex items-center justify-center h-64">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
          <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie zastępstw...</p>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
        <svg
          class="w-16 h-16 text-red-400 mx-auto mb-4"
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
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
          Błąd
        </h3>
        <p class="text-gray-600 dark:text-gray-400">
          {{ error }}
        </p>
      </div>

      <!-- Empty State -->
      <div v-else-if="!hasAnySubstitutions" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
        <svg
          class="w-16 h-16 text-gray-400 mx-auto mb-4"
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
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
          Brak zastępstw
        </h3>
        <p class="text-gray-600 dark:text-gray-400">
          Obecnie nie masz żadnych aktywnych ani zaplanowanych zastępstw
        </p>
      </div>

      <!-- Substitutions List -->
      <div v-else class="space-y-6">
        <!-- Active Substitutions -->
        <div v-if="activeSubstitutions.length > 0">
          <div class="flex items-center gap-2 mb-4">
            <div class="w-3 h-3 rounded-full bg-green-500 animate-pulse" />
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
              Aktywne zastępstwa ({{ activeSubstitutions.length }})
            </h2>
          </div>

          <div class="space-y-4">
            <div
              v-for="substitution in activeSubstitutions"
              :key="substitution.id"
              class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6"
            >
              <div class="flex items-start justify-between">
                <div class="flex items-start gap-4">
                  <!-- Avatar -->
                  <div class="w-12 h-12 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center flex-shrink-0">
                    <User class="w-6 h-6 text-green-600 dark:text-green-400" />
                  </div>

                  <!-- Info -->
                  <div class="flex-1">
                    <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-1">
                      Zastępujesz: {{ getUserFullName(substitution.user) }}
                    </h3>
                    <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 mb-3">
                      <Calendar class="w-4 h-4" />
                      <span>{{ formatDateRange(substitution.startDate, substitution.endDate) }}</span>
                      <span class="text-gray-400">•</span>
                      <span>{{ getVacationDuration(substitution) }} dni</span>
                    </div>

                    <button
                      class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors"
                      @click="viewApprovals(substitution.userId)"
                    >
                      <CheckCircle class="w-4 h-4" />
                      Zobacz wnioski do zatwierdzenia
                    </button>
                  </div>
                </div>

                <!-- Status Badge -->
                <span class="px-3 py-1 text-sm font-medium rounded-full bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300">
                  Aktywny
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Scheduled Substitutions -->
        <div v-if="scheduledSubstitutions.length > 0">
          <div class="flex items-center gap-2 mb-4">
            <div class="w-3 h-3 rounded-full bg-blue-500" />
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
              Zaplanowane zastępstwa ({{ scheduledSubstitutions.length }})
            </h2>
          </div>

          <div class="space-y-4">
            <div
              v-for="substitution in scheduledSubstitutions"
              :key="substitution.id"
              class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6"
            >
              <div class="flex items-start justify-between">
                <div class="flex items-start gap-4">
                  <!-- Avatar -->
                  <div class="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center flex-shrink-0">
                    <Clock class="w-6 h-6 text-blue-600 dark:text-blue-400" />
                  </div>

                  <!-- Info -->
                  <div class="flex-1">
                    <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-1">
                      Będziesz zastępować: {{ getUserFullName(substitution.user) }}
                    </h3>
                    <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
                      <Calendar class="w-4 h-4" />
                      <span>{{ formatDateRange(substitution.startDate, substitution.endDate) }}</span>
                      <span class="text-gray-400">•</span>
                      <span>{{ getVacationDuration(substitution) }} dni</span>
                    </div>
                  </div>
                </div>

                <!-- Status Badge -->
                <span class="px-3 py-1 text-sm font-medium rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300">
                  Zaplanowany
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Completed Substitutions -->
        <div v-if="completedSubstitutions.length > 0">
          <div class="flex items-center gap-2 mb-4">
            <div class="w-3 h-3 rounded-full bg-gray-400" />
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
              Zakończone zastępstwa (ostatnie 5)
            </h2>
          </div>

          <div class="space-y-4">
            <div
              v-for="substitution in completedSubstitutions"
              :key="substitution.id"
              class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 opacity-75"
            >
              <div class="flex items-start justify-between">
                <div class="flex items-start gap-4">
                  <!-- Avatar -->
                  <div class="w-12 h-12 rounded-full bg-gray-100 dark:bg-gray-700 flex items-center justify-center flex-shrink-0">
                    <CheckCircle class="w-6 h-6 text-gray-600 dark:text-gray-400" />
                  </div>

                  <!-- Info -->
                  <div class="flex-1">
                    <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-1">
                      Zastępowałeś/łaś: {{ getUserFullName(substitution.user) }}
                    </h3>
                    <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
                      <Calendar class="w-4 h-4" />
                      <span>{{ formatDateRange(substitution.startDate, substitution.endDate) }}</span>
                      <span class="text-gray-400">•</span>
                      <span>{{ getVacationDuration(substitution) }} dni</span>
                    </div>
                  </div>
                </div>

                <!-- Status Badge -->
                <span class="px-3 py-1 text-sm font-medium rounded-full bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-300">
                  Zakończony
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Additional styles if needed */
</style>
