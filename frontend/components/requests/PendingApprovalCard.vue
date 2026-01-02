<script setup lang="ts">
import { X } from 'lucide-vue-next'
import type { Request, RequestApprovalStep } from '~/types/requests'

interface Props {
  request: Request
}

const props = defineProps<Props>()

const emit = defineEmits<{
  approve: [request: Request]
  reject: [request: Request]
}>()

const { getIconifyName } = useIconMapping()

const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const getCurrentStep = (request: Request): RequestApprovalStep | undefined => {
  return request.approvalSteps.find(
    (step: RequestApprovalStep) => step.status === 'InReview'
  )
}

const getCurrentStepLabel = (request: Request): string => {
  const currentStep = getCurrentStep(request)
  return currentStep ? `U ${currentStep.approverName}` : '-'
}

const canApprove = computed((): boolean => {
  const currentStep = getCurrentStep(props.request)
  if (!currentStep) return false

  const requiresQuiz =
    currentStep.requiresQuiz || (currentStep as Record<string, unknown>).RequiresQuiz
  if (!requiresQuiz) return true

  const quizPassed =
    currentStep.quizPassed || (currentStep as Record<string, unknown>).QuizPassed
  return quizPassed === true
})

const quizStatusMessage = computed((): string | null => {
  const currentStep = getCurrentStep(props.request)
  if (!currentStep) return null

  const requiresQuiz =
    currentStep.requiresQuiz || (currentStep as Record<string, unknown>).RequiresQuiz
  if (!requiresQuiz) return null

  const quizScore =
    currentStep.quizScore ?? (currentStep as Record<string, unknown>).QuizScore
  const quizPassed =
    currentStep.quizPassed || (currentStep as Record<string, unknown>).QuizPassed

  if (quizScore === null || quizScore === undefined) {
    return 'Wnioskodawca musi najpierw wypełnić wymagany quiz.'
  }

  if (!quizPassed) {
    return `Quiz niezaliczony (wynik: ${quizScore}%). Wnioskodawca musi zaliczyć quiz.`
  }

  return null
})

const priorityClass = computed((): string => {
  return props.request.priority === 'Urgent'
    ? 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
    : 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300'
})

const priorityLabel = computed((): string => {
  return props.request.priority === 'Urgent' ? 'Pilne' : 'Standardowy'
})

const handleApprove = (): void => {
  emit('approve', props.request)
}

const handleReject = (): void => {
  emit('reject', props.request)
}
</script>

<template>
  <div
    class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 hover:shadow-md transition-shadow"
    data-testid="pending-approval-card"
  >
    <div class="flex items-start justify-between gap-4">
      <div class="flex-1">
        <div class="flex items-center gap-3 mb-2">
          <Icon
            :name="getIconifyName(request.requestTemplateIcon)"
            class="w-6 h-6 text-blue-600"
          />
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
            {{ request.requestTemplateName }}
          </h3>
          <span
            class="px-2 py-1 text-xs font-medium rounded-full"
            :class="priorityClass"
          >
            {{ priorityLabel }}
          </span>
        </div>

        <div class="space-y-1 text-sm text-gray-600 dark:text-gray-300 mb-4">
          <p>
            <span class="font-medium">Numer:</span> {{ request.requestNumber }}
          </p>
          <p>
            <span class="font-medium">Wnioskodawca:</span>
            {{ request.submittedByName }}
          </p>
          <p>
            <span class="font-medium">Data złożenia:</span>
            {{ formatDate(request.submittedAt) }}
          </p>
        </div>

        <div
          class="p-3 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg border border-yellow-200 dark:border-yellow-800"
        >
          <p class="text-sm font-medium text-yellow-900 dark:text-yellow-100">
            Oczekuje na Twoją decyzję
          </p>
          <p class="text-xs text-yellow-700 dark:text-yellow-300 mt-1">
            {{ getCurrentStepLabel(request) }}
          </p>
        </div>

        <div
          v-if="!canApprove"
          class="mt-3 p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800"
        >
          <p class="text-xs font-medium text-blue-900 dark:text-blue-100 mb-1">
            Oczekiwanie na quiz
          </p>
          <p class="text-xs text-blue-800 dark:text-blue-200">
            {{ quizStatusMessage }}
          </p>
        </div>
      </div>

      <div class="flex flex-col gap-2">
        <button
          :disabled="!canApprove"
          class="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium transition-colors flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:bg-green-600"
          data-testid="approve-button"
          @click="handleApprove"
        >
          <Icon name="heroicons:check" class="w-5 h-5" />
          Zatwierdź
        </button>
        <button
          class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
          data-testid="reject-button"
          @click="handleReject"
        >
          <X class="w-5 h-5" />
          Odrzuć
        </button>
        <NuxtLink
          :to="`/dashboard/requests/${request.id}`"
          class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors text-center"
        >
          Szczegóły
        </NuxtLink>
      </div>
    </div>
  </div>
</template>
