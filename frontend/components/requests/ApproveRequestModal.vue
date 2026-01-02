<script setup lang="ts">
import type { Request, RequestApprovalStep } from '~/types/requests'

interface Props {
  request: Request | null
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  close: []
  confirm: [comment: string]
}>()

const comment = ref('')

const isOpen = computed(() => props.request !== null)

const getCurrentStep = (
  request: Request
): RequestApprovalStep | undefined => {
  return request.approvalSteps.find(
    (step: RequestApprovalStep) => step.status === 'InReview'
  )
}

const canApprove = computed((): boolean => {
  if (!props.request) return false

  const currentStep = getCurrentStep(props.request)
  if (!currentStep) return false

  const requiresQuiz =
    currentStep.requiresQuiz ||
    (currentStep as Record<string, unknown>).RequiresQuiz
  if (!requiresQuiz) return true

  const quizPassed =
    currentStep.quizPassed ||
    (currentStep as Record<string, unknown>).QuizPassed
  return quizPassed === true
})

const quizStatusMessage = computed((): string | null => {
  if (!props.request) return null

  const currentStep = getCurrentStep(props.request)
  if (!currentStep) return null

  const requiresQuiz =
    currentStep.requiresQuiz ||
    (currentStep as Record<string, unknown>).RequiresQuiz
  if (!requiresQuiz) return null

  const quizScore =
    currentStep.quizScore ??
    (currentStep as Record<string, unknown>).QuizScore
  const quizPassed =
    currentStep.quizPassed ||
    (currentStep as Record<string, unknown>).QuizPassed

  if (quizScore === null || quizScore === undefined) {
    return 'Wnioskodawca musi najpierw wypełnić wymagany quiz.'
  }

  if (!quizPassed) {
    return `Quiz niezaliczony (wynik: ${quizScore}%). Wnioskodawca musi zaliczyć quiz.`
  }

  return null
})

const handleClose = (): void => {
  comment.value = ''
  emit('close')
}

const handleConfirm = (): void => {
  emit('confirm', comment.value)
  comment.value = ''
}
</script>

<template>
  <BaseModal
    :is-open="isOpen"
    title="Zatwierdź wniosek"
    size="md"
    @close="handleClose"
  >
    <div data-testid="approve-request-modal">
      <p class="text-gray-600 dark:text-gray-300 mb-4">
        Czy na pewno chcesz zatwierdzić wniosek
        <strong>{{ request?.requestNumber }}</strong>?
      </p>

      <div
        v-if="!canApprove"
        class="mb-4 p-3 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg"
      >
        <div class="flex items-start gap-2">
          <svg
            class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 flex-shrink-0"
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
          <div>
            <p
              class="text-sm font-medium text-blue-900 dark:text-blue-100 mb-1"
            >
              Nie można zatwierdzić
            </p>
            <p class="text-sm text-blue-800 dark:text-blue-200">
              {{ quizStatusMessage }}
            </p>
          </div>
        </div>
      </div>

      <div class="mb-4">
        <label
          class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
        >
          Komentarz (opcjonalny)
        </label>
        <textarea
          v-model="comment"
          rows="3"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          placeholder="Dodaj komentarz..."
          data-testid="approve-request-comment-input"
        />
      </div>
    </div>

    <template #footer>
      <div class="flex gap-3 justify-end">
        <button
          :disabled="loading"
          class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
          data-testid="approve-request-cancel-btn"
          @click="handleClose"
        >
          Anuluj
        </button>
        <button
          :disabled="loading || !canApprove"
          class="px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg font-medium transition-colors"
          data-testid="approve-request-confirm-btn"
          @click="handleConfirm"
        >
          {{ loading ? 'Zatwierdzanie...' : 'Zatwierdź' }}
        </button>
      </div>
    </template>
  </BaseModal>
</template>
