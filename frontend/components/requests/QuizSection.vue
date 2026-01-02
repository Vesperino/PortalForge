<script setup lang="ts">
import type { RequestApprovalStep, QuizQuestion } from '~/types/requests'

interface Props {
  requestId: string
  currentStep: RequestApprovalStep | undefined
  isRequestSubmitter: boolean
  requiresQuiz: boolean
  quizPassed: boolean
  quizScore: number | null | undefined
  hasQuizQuestions: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  quizSubmitted: [result: { success: boolean; score: number; passed: boolean }]
}>()

const showQuizForm = ref(false)

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

const stepId = computed((): string | undefined => {
  const step = props.currentStep
  if (!step) return undefined
  return step.id ?? (step as Record<string, unknown>).Id as string
})

const handleQuizSubmitted = (result: { success: boolean; score: number; passed: boolean }): void => {
  showQuizForm.value = false
  emit('quizSubmitted', result)
}
</script>

<template>
  <div
    v-if="isRequestSubmitter && requiresQuiz && hasQuizQuestions"
    data-testid="quiz-section"
  >
    <RequestQuizResult
      v-if="quizScore !== null && quizScore !== undefined"
      :score="quizScore"
      :passed="quizPassed"
      :passing-score="passingScore"
    />

    <div
      v-else-if="showQuizForm"
      class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
    >
      <RequestQuizForm
        :questions="quizQuestions"
        :request-id="requestId"
        :step-id="stepId"
        :passing-score="passingScore"
        @submitted="handleQuizSubmitted"
        @cancel="showQuizForm = false"
      />
    </div>

    <div
      v-else
      class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
    >
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
        Quiz wymagany
      </h3>
      <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
        Ten etap wymaga zaliczenia quizu. Prosze wypelnic quiz ponizej.
      </p>
      <button
        type="button"
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition"
        data-testid="quiz-section-start-button"
        @click="showQuizForm = true"
      >
        Rozpocznij quiz
      </button>
    </div>
  </div>
</template>
