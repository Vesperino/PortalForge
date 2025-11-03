<script setup lang="ts">
import { computed } from 'vue'
import type { VacationSummary } from '~/composables/useVacations'

interface Props {
  summary: VacationSummary | null
  isLoading?: boolean
  error?: string | null
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false,
  error: null
})

const formatDate = (date: string | null) => {
  if (!date) return null

  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(new Date(date))
}

const vacationPercentage = computed(() => {
  if (!props.summary || props.summary.annualVacationDays === 0) return 0
  return Math.round((props.summary.vacationDaysUsed / props.summary.annualVacationDays) * 100)
})
</script>

<template>
  <div class="space-y-6">
    <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
      Podsumowanie urlopów
    </h3>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex justify-center items-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
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

    <!-- Summary Cards -->
    <template v-else-if="summary">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <!-- Annual Vacation Days -->
        <div class="p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
          <div class="flex items-center gap-3">
            <svg class="w-8 h-8 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <div>
              <p class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                {{ summary.annualVacationDays }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400">
                Dni urlopu w roku
              </p>
            </div>
          </div>
        </div>

        <!-- Remaining Days -->
        <div class="p-4 bg-green-50 dark:bg-green-900/20 rounded-lg border border-green-200 dark:border-green-800">
          <div class="flex items-center gap-3">
            <svg class="w-8 h-8 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <p class="text-2xl font-bold text-green-600 dark:text-green-400">
                {{ summary.vacationDaysRemaining }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400">
                Dni pozostało
              </p>
            </div>
          </div>
        </div>

        <!-- Used Days -->
        <div class="p-4 bg-orange-50 dark:bg-orange-900/20 rounded-lg border border-orange-200 dark:border-orange-800">
          <div class="flex items-center gap-3">
            <svg class="w-8 h-8 text-orange-600 dark:text-orange-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <p class="text-2xl font-bold text-orange-600 dark:text-orange-400">
                {{ summary.vacationDaysUsed }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400">
                Dni wykorzystano
              </p>
            </div>
          </div>
        </div>

        <!-- On-Demand Vacation -->
        <div class="p-4 bg-purple-50 dark:bg-purple-900/20 rounded-lg border border-purple-200 dark:border-purple-800">
          <div class="flex items-center gap-3">
            <svg class="w-8 h-8 text-purple-600 dark:text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
            <div>
              <p class="text-2xl font-bold text-purple-600 dark:text-purple-400">
                {{ summary.onDemandVacationDaysRemaining }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400">
                Dni na żądanie
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Progress Bar -->
      <div>
        <div class="flex justify-between text-sm mb-2">
          <span class="text-gray-700 dark:text-gray-300">Wykorzystanie urlopu</span>
          <span class="font-semibold text-gray-900 dark:text-white">
            {{ summary.vacationDaysUsed }} / {{ summary.annualVacationDays }} dni ({{ vacationPercentage }}%)
          </span>
        </div>
        <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-3">
          <div
            class="bg-gradient-to-r from-blue-500 to-blue-600 h-3 rounded-full transition-all duration-300"
            :style="{ width: `${vacationPercentage}%` }"
          />
        </div>
      </div>

      <!-- Carried Over Vacation -->
      <div
        v-if="summary.carriedOverVacationDays > 0"
        class="p-4 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg"
      >
        <div class="flex items-start gap-3">
          <svg class="w-6 h-6 text-yellow-600 dark:text-yellow-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          <div class="flex-1">
            <p class="text-sm font-medium text-yellow-800 dark:text-yellow-200">
              Urlop zaległy: {{ summary.carriedOverVacationDays }} dni
            </p>
            <p v-if="summary.carriedOverExpiryDate" class="text-xs text-yellow-600 dark:text-yellow-400 mt-1">
              Wygasa: {{ formatDate(summary.carriedOverExpiryDate) }}
            </p>
          </div>
        </div>
      </div>

      <!-- Additional Info -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
        <div class="p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <p class="text-gray-600 dark:text-gray-400">Urlop na żądanie wykorzystany</p>
          <p class="text-lg font-semibold text-gray-900 dark:text-white mt-1">
            {{ summary.onDemandVacationDaysUsed }} / 4 dni
          </p>
        </div>

        <div class="p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
          <p class="text-gray-600 dark:text-gray-400">Urlop okolicznościowy</p>
          <p class="text-lg font-semibold text-gray-900 dark:text-white mt-1">
            {{ summary.circumstantialLeaveDaysUsed }} dni
          </p>
        </div>
      </div>

      <!-- Total Available -->
      <div class="p-4 bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-200 dark:border-indigo-800 rounded-lg">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-indigo-600 dark:text-indigo-400 font-medium">
              Łącznie dostępne dni urlopu
            </p>
            <p class="text-xs text-indigo-500 dark:text-indigo-500 mt-1">
              Uwzględnia urlop roczny i zaległy
            </p>
          </div>
          <p class="text-3xl font-bold text-indigo-600 dark:text-indigo-400">
            {{ summary.totalAvailableVacationDays }}
          </p>
        </div>
      </div>
    </template>
  </div>
</template>
