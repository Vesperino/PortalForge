<script setup lang="ts">
import { ref, computed } from 'vue'

interface QuizOption {
  value: string
  label: string
  isCorrect?: boolean
}

interface QuizQuestion {
  id: string
  question: string
  options: string // JSON string
  order: number
}

interface Props {
  questions: QuizQuestion[]
  requestId: string
  stepId: string
  passingScore?: number
}

const props = defineProps<Props>()

interface Emits {
  (e: 'submitted', result: { success: boolean; score: number; passed: boolean }): void
  (e: 'cancel'): void
}

const emit = defineEmits<Emits>()

// State
const selectedAnswers = ref<Record<string, string>>({})
const isSubmitting = ref(false)
const error = ref<string | null>(null)

// Parse quiz options from JSON
const parsedQuestions = computed(() => {
  return props.questions.map(q => {
    try {
      const options: QuizOption[] = JSON.parse(q.options)
      return {
        ...q,
        parsedOptions: options
      }
    } catch {
      return {
        ...q,
        parsedOptions: []
      }
    }
  })
})

// Check if all questions are answered
const allAnswered = computed(() => {
  return props.questions.every(q => selectedAnswers.value[q.id])
})

// Submit quiz
async function submitQuiz() {
  if (!allAnswered.value) {
    error.value = 'Proszę odpowiedzieć na wszystkie pytania'
    return
  }

  isSubmitting.value = true
  error.value = null

  try {
    const config = useRuntimeConfig()
    const { getAuthHeaders } = useAuth()

    const answers = props.questions.map(q => ({
      questionId: q.id,
      selectedAnswer: selectedAnswers.value[q.id]
    }))

    const response = await $fetch<{
      success: boolean
      message: string
      score: number
      passed: boolean
      requiredScore: number
    }>(`${config.public.apiUrl}/api/requests/${props.requestId}/steps/${props.stepId}/quiz`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: {
        answers
      }
    })

    if (response.success) {
      emit('submitted', {
        success: response.passed,
        score: response.score,
        passed: response.passed
      })
    } else {
      error.value = response.message || 'Nie udało się wysłać quizu'
    }
  } catch (err: any) {
    console.error('Quiz submission error:', err)
    error.value = err?.data?.message || 'Wystąpił błąd podczas wysyłania quizu'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="quiz-form bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
    <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
      Quiz wymagany do zatwierdzenia
    </h3>

    <p v-if="passingScore" class="text-sm text-gray-600 dark:text-gray-400 mb-6">
      Wymagany wynik: {{ passingScore }}%
    </p>

    <!-- Error message -->
    <div
      v-if="error"
      class="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg"
    >
      <p class="text-sm text-red-800 dark:text-red-200">{{ error }}</p>
    </div>

    <!-- Questions -->
    <div class="space-y-6">
      <div
        v-for="(question, index) in parsedQuestions"
        :key="question.id"
        class="border-b border-gray-200 dark:border-gray-700 pb-6 last:border-0 last:pb-0"
      >
        <p class="text-sm font-medium text-gray-900 dark:text-white mb-3">
          {{ index + 1 }}. {{ question.question }}
        </p>

        <div class="space-y-2">
          <label
            v-for="option in question.parsedOptions"
            :key="option.value"
            class="flex items-center p-3 border border-gray-200 dark:border-gray-700 rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700 transition"
            :class="{
              'bg-blue-50 dark:bg-blue-900/20 border-blue-500 dark:border-blue-600':
                selectedAnswers[question.id] === option.value
            }"
          >
            <input
              :checked="selectedAnswers[question.id] === option.value"
              @change="selectedAnswers[question.id] = option.value"
              type="radio"
              :name="`question-${question.id}`"
              :value="option.value"
              class="w-4 h-4 text-blue-600 focus:ring-blue-500 dark:focus:ring-blue-600 dark:bg-gray-700 dark:border-gray-600"
            />
            <span class="ml-3 text-sm text-gray-700 dark:text-gray-300">
              {{ option.label }}
            </span>
          </label>
        </div>
      </div>
    </div>

    <!-- Actions -->
    <div class="mt-6 flex gap-3 justify-end">
      <button
        type="button"
        @click="emit('cancel')"
        :disabled="isSubmitting"
        class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-800 dark:text-gray-200 rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
      >
        Anuluj
      </button>
      <button
        type="button"
        @click="submitQuiz"
        :disabled="!allAnswered || isSubmitting"
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
      >
        <span v-if="isSubmitting" class="animate-spin">⏳</span>
        <span>{{ isSubmitting ? 'Wysyłanie...' : 'Wyślij odpowiedzi' }}</span>
      </button>
    </div>
  </div>
</template>
