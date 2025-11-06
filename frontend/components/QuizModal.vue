<template>
  <Teleport to="body">
    <div
      v-if="show"
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
      @click.self="closeModal"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-hidden flex flex-col">
        <!-- Header -->
        <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <div class="flex items-center justify-between">
            <div>
              <h3 class="text-xl font-bold text-gray-900 dark:text-white">
                Quiz wymagany przed zatwierdzeniem
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-300 mt-1">
                Odpowiedz poprawnie na {{ passingScore }}% pytań aby móc zatwierdzić wniosek
              </p>
            </div>
            <button
              :disabled="submitting"
              class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
              @click="closeModal"
            >
              <Icon name="heroicons:x-mark" class="w-6 h-6" />
            </button>
          </div>
        </div>

        <!-- Quiz Result (if completed) -->
        <div v-if="quizResult" class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <div
            :class="[
              'p-4 rounded-lg',
              quizResult.passed
                ? 'bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800'
                : 'bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800'
            ]"
          >
            <div class="flex items-start gap-3">
              <Icon
                :name="quizResult.passed ? 'heroicons:check-circle' : 'heroicons:x-circle'"
                :class="[
                  'w-6 h-6 mt-0.5',
                  quizResult.passed ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
                ]"
              />
              <div class="flex-1">
                <h4
                  :class="[
                    'font-semibold',
                    quizResult.passed ? 'text-green-900 dark:text-green-100' : 'text-red-900 dark:text-red-100'
                  ]"
                >
                  {{ quizResult.passed ? '✅ Quiz zaliczony!' : '❌ Quiz niezaliczony' }}
                </h4>
                <p
                  :class="[
                    'text-sm mt-1',
                    quizResult.passed ? 'text-green-700 dark:text-green-300' : 'text-red-700 dark:text-red-300'
                  ]"
                >
                  {{ quizResult.message }}
                </p>
                <div class="mt-2 text-sm font-medium">
                  <span :class="quizResult.passed ? 'text-green-900 dark:text-green-100' : 'text-red-900 dark:text-red-100'">
                    Twój wynik: {{ quizResult.score }}%
                  </span>
                  <span class="text-gray-500 dark:text-gray-400 mx-2">•</span>
                  <span class="text-gray-700 dark:text-gray-300">
                    Wymagane: {{ quizResult.requiredScore }}%
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Questions -->
        <div v-if="!quizResult" class="flex-1 overflow-y-auto px-6 py-4">
          <div class="space-y-6">
            <div
              v-for="(question, index) in sortedQuestions"
              :key="question.id"
              class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-4"
            >
              <div class="flex gap-3">
                <div class="flex-shrink-0 w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center font-semibold">
                  {{ index + 1 }}
                </div>
                <div class="flex-1">
                  <h4 class="font-medium text-gray-900 dark:text-white mb-3">
                    {{ question.question }}
                  </h4>

                  <div class="space-y-2">
                    <label
                      v-for="option in getOptions(question)"
                      :key="option.value"
                      class="flex items-start gap-3 p-3 rounded-lg border border-gray-200 dark:border-gray-600 hover:bg-white dark:hover:bg-gray-700 cursor-pointer transition-colors"
                      :class="{
                        'bg-white dark:bg-gray-700 border-blue-500 dark:border-blue-400': answers[question.id] === option.value
                      }"
                    >
                      <input
                        v-model="answers[question.id]"
                        type="radio"
                        :name="`question-${question.id}`"
                        :value="option.value"
                        class="mt-1 text-blue-600 focus:ring-blue-500"
                      >
                      <span class="flex-1 text-sm text-gray-700 dark:text-gray-300">
                        {{ option.label }}
                      </span>
                    </label>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50">
          <div v-if="!quizResult" class="flex items-center justify-between">
            <div class="text-sm text-gray-600 dark:text-gray-400">
              Odpowiedzi: {{ Object.keys(answers).length }} / {{ questions.length }}
            </div>
            <div class="flex gap-3">
              <button
                :disabled="submitting"
                class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
                @click="closeModal"
              >
                Anuluj
              </button>
              <button
                :disabled="!allQuestionsAnswered || submitting"
                class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
                @click="submitQuiz"
              >
                <Icon v-if="submitting" name="svg-spinners:ring-resize" class="w-5 h-5" />
                {{ submitting ? 'Sprawdzanie...' : 'Wyślij odpowiedzi' }}
              </button>
            </div>
          </div>
          <div v-else class="flex justify-end gap-3">
            <button
              v-if="!quizResult.passed"
              class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
              @click="retakeQuiz"
            >
              Spróbuj ponownie
            </button>
            <button
              class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
              @click="closeModal"
            >
              Zamknij
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
interface QuizQuestion {
  id: string
  question: string
  options: string
  order: number
}

interface QuizOption {
  value: string
  label: string
  isCorrect?: boolean
}

interface QuizResult {
  success: boolean
  message: string
  score: number
  passed: boolean
  requiredScore: number
}

interface Props {
  show: boolean
  questions: QuizQuestion[]
  passingScore: number
  requestId: string
  stepId: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  quizPassed: []
}>()

const answers = ref<Record<string, string>>({})
const submitting = ref(false)
const quizResult = ref<QuizResult | null>(null)

const sortedQuestions = computed(() => {
  return [...props.questions].sort((a, b) => a.order - b.order)
})

const allQuestionsAnswered = computed(() => {
  return props.questions.every(q => answers.value[q.id])
})

const getOptions = (question: QuizQuestion): QuizOption[] => {
  try {
    return JSON.parse(question.options)
  } catch {
    return []
  }
}

const submitQuiz = async () => {
  if (!allQuestionsAnswered.value) return

  submitting.value = true
  quizResult.value = null

  try {
    const answersArray = Object.entries(answers.value).map(([questionId, selectedAnswer]) => ({
      questionId,
      selectedAnswer
    }))

    const config = useRuntimeConfig()
    const authStore = useAuthStore()

    const result = await $fetch<QuizResult>(
      `${config.public.apiUrl}/api/requests/${props.requestId}/steps/${props.stepId}/quiz`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${authStore.token}`,
          'Content-Type': 'application/json'
        },
        body: {
          answers: answersArray
        }
      }
    )

    quizResult.value = result

    if (result.passed) {
      emit('quizPassed')
    }
  } catch (error: any) {
    console.error('Error submitting quiz:', error)
    quizResult.value = {
      success: false,
      message: error.data?.message || 'Nie udało się wysłać odpowiedzi. Spróbuj ponownie.',
      score: 0,
      passed: false,
      requiredScore: props.passingScore
    }
  } finally {
    submitting.value = false
  }
}

const retakeQuiz = () => {
  quizResult.value = null
  answers.value = {}
}

const closeModal = () => {
  if (!submitting.value) {
    emit('close')
  }
}

// Reset state when modal opens
watch(() => props.show, (newValue) => {
  if (newValue) {
    answers.value = {}
    quizResult.value = null
  }
})
</script>
