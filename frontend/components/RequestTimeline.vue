<template>
  <div class="request-timeline">
    <h3 class="text-lg font-semibold mb-4 text-gray-900 dark:text-white">
      Przebieg zatwierdzania
    </h3>

    <div class="relative">
      <!-- Timeline line -->
      <div class="absolute left-4 top-0 bottom-0 w-0.5 bg-gray-200 dark:bg-gray-700" />

      <!-- Timeline items -->
      <div class="space-y-6">
        <div
          v-for="step in steps"
          :key="step.id"
          class="relative flex items-start gap-4"
        >
          <!-- Icon/Status indicator -->
          <div
            :class="[
              'relative z-10 flex items-center justify-center w-8 h-8 rounded-full border-2',
              getStepClasses(step.status)
            ]"
          >
            <CheckCircle v-if="step.status === 'Approved'" class="w-4 h-4" />
            <XCircle v-else-if="step.status === 'Rejected'" class="w-4 h-4" />
            <Clock v-else-if="step.status === 'InReview'" class="w-4 h-4" />
            <FileQuestion v-else-if="step.status === 'RequiresSurvey'" class="w-4 h-4" />
            <AlertCircle v-else-if="step.status === 'SurveyFailed'" class="w-4 h-4" />
            <Circle v-else class="w-4 h-4" />
          </div>

          <!-- Content -->
          <div class="flex-1 pb-6">
            <div class="bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
              <div class="flex items-start justify-between mb-2">
                <div>
                  <h4 class="font-medium text-gray-900 dark:text-white">
                    Etap {{ step.stepOrder }}: {{ step.approverName }}
                  </h4>
                  <p class="text-sm text-gray-500 dark:text-gray-400">
                    {{ getStepStatusLabel(step.status) }}
                  </p>
                </div>
                <span
                  :class="[
                    'px-2 py-1 text-xs font-medium rounded-full',
                    getStatusBadgeClasses(step.status)
                  ]"
                >
                  {{ getStepStatusLabel(step.status) }}
                </span>
              </div>

              <!-- Dates -->
              <div class="mt-2 space-y-1 text-sm text-gray-600 dark:text-gray-400">
                <div v-if="step.startedAt" class="flex items-center gap-2">
                  <Clock class="w-4 h-4" />
                  <span>Rozpoczęto: {{ formatDate(step.startedAt) }}</span>
                </div>
                <div v-if="step.finishedAt" class="flex items-center gap-2">
                  <CheckCircle class="w-4 h-4" />
                  <span>Zakończono: {{ formatDate(step.finishedAt) }}</span>
                </div>
              </div>

              <!-- Comment -->
              <div v-if="step.comment" class="mt-3 p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg">
                <p class="text-sm text-gray-700 dark:text-gray-300">
                  <strong>Komentarz:</strong> {{ step.comment }}
                </p>
              </div>

              <!-- Quiz info -->
              <div v-if="step.requiresQuiz" class="mt-3 flex items-center gap-2 text-sm">
                <FileQuestion class="w-4 h-4 text-purple-500" />
                <span class="text-gray-700 dark:text-gray-300">
                  Wymaga quizu
                </span>
                <span v-if="step.quizScore !== null && step.quizScore !== undefined" class="ml-auto">
                  Wynik: <strong :class="step.quizPassed ? 'text-green-600' : 'text-red-600'">
                    {{ step.quizScore }}%
                  </strong>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { CheckCircle, XCircle, Clock, Circle, FileQuestion, AlertCircle } from 'lucide-vue-next'
import type { RequestApprovalStep } from '~/types/requests'

defineProps<{
  steps: RequestApprovalStep[]
}>()

const getStepClasses = (status: string) => {
  switch (status) {
    case 'Approved':
      return 'bg-green-100 dark:bg-green-900 border-green-500 text-green-600 dark:text-green-400'
    case 'Rejected':
      return 'bg-red-100 dark:bg-red-900 border-red-500 text-red-600 dark:text-red-400'
    case 'InReview':
      return 'bg-blue-100 dark:bg-blue-900 border-blue-500 text-blue-600 dark:text-blue-400 animate-pulse'
    case 'RequiresSurvey':
      return 'bg-purple-100 dark:bg-purple-900 border-purple-500 text-purple-600 dark:text-purple-400'
    case 'SurveyFailed':
      return 'bg-orange-100 dark:bg-orange-900 border-orange-500 text-orange-600 dark:text-orange-400'
    case 'Pending':
      return 'bg-gray-100 dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-400'
    default:
      return 'bg-gray-100 dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-400'
  }
}

const getStatusBadgeClasses = (status: string) => {
  switch (status) {
    case 'Approved':
      return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
    case 'Rejected':
      return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
    case 'InReview':
      return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
    case 'RequiresSurvey':
      return 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200'
    case 'SurveyFailed':
      return 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200'
    case 'Pending':
      return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200'
    default:
      return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200'
  }
}

const getStepStatusLabel = (status: string) => {
  const labels: Record<string, string> = {
    Pending: 'Oczekujący',
    InReview: 'W trakcie oceny',
    Approved: 'Zatwierdzony',
    Rejected: 'Odrzucony',
    RequiresSurvey: 'Wymaga quizu',
    SurveyFailed: 'Quiz niezdany'
  }
  return labels[status] || status
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
.request-timeline {
  @apply w-full;
}
</style>

