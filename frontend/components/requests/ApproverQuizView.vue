<script setup lang="ts">
import type { RequestApprovalStep, QuizQuestion, QuizAnswer } from '~/types/requests'

interface Props {
  currentStep: RequestApprovalStep | undefined
  isCurrentApprover: boolean
  requiresQuiz: boolean
  quizPassed: boolean
  quizScore: number | null | undefined
  hasQuizQuestions: boolean
}

const props = defineProps<Props>()

const passingScore = computed((): number => {
  const step = props.currentStep
  if (!step) return 70
  return step.passingScore ?? (step as Record<string, unknown>).PassingScore as number ?? 70
})

const quizQuestions = computed((): QuizQuestion[] => {
  const step = props.currentStep
  if (!step) return []
  return step.quizQuestions ?? (step as Record<string, unknown>).QuizQuestions as QuizQuestion[] ?? []
})

const quizAnswers = computed((): QuizAnswer[] => {
  const step = props.currentStep
  if (!step) return []
  return step.quizAnswers ?? (step as Record<string, unknown>).QuizAnswers as QuizAnswer[] ?? []
})
</script>

<template>
  <div
    v-if="isCurrentApprover && requiresQuiz && hasQuizQuestions"
    class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
    data-testid="approver-quiz-view"
  >
    <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
      Wyniki quizu wnioskodawcy
    </h3>

    <div v-if="quizScore !== null && quizScore !== undefined">
      <RequestQuizResult
        :score="quizScore"
        :passed="quizPassed"
        :passing-score="passingScore"
      />

      <div class="mt-6">
        <h4 class="text-sm font-semibold text-gray-900 dark:text-white mb-3">
          Szczegolowe odpowiedzi
        </h4>
        <RequestQuizAnswersDisplay
          :questions="quizQuestions"
          :answers="quizAnswers"
        />
      </div>
    </div>

    <div
      v-else
      class="p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg"
    >
      <div class="flex items-start gap-3">
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
            d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
        <div>
          <p class="text-sm font-medium text-blue-900 dark:text-blue-100 mb-1">
            Oczekiwanie na wypelnienie quizu przez wnioskodawce
          </p>
          <p class="text-sm text-blue-800 dark:text-blue-200">
            Wnioskodawca musi najpierw wypelnic wymagany quiz. Po wypelnieniu quizu przez wnioskodawce, wyniki pojawia sie tutaj.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
