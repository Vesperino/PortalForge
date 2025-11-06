<template>
  <Teleport to="body">
    <div
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-[100] p-4"
      @click.self="$emit('close')"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <!-- Header -->
        <div class="p-6 border-b border-gray-200 dark:border-gray-700 sticky top-0 bg-white dark:bg-gray-800 z-10">
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
                Konfiguracja quizu - Etap {{ stepOrder }}
              </h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Zdefiniuj pytania i odpowiedzi dla zatwierdzającego na tym etapie
              </p>
            </div>
            <button
              class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
              @click="$emit('close')"
            >
              <X class="w-6 h-6" />
            </button>
          </div>
        </div>

        <!-- Content -->
        <div class="p-6 space-y-6">
          <!-- Passing Score -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Próg zdawalności (%)
            </label>
            <input
              v-model.number="localPassingScore"
              type="number"
              min="0"
              max="100"
              placeholder="80"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            >
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
              Minimalny wynik procentowy wymagany do zaliczenia quizu
            </p>
          </div>

          <!-- Add Question Button -->
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
              Pytania ({{ localQuestions.length }})
            </h3>
            <button
              class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
              @click="addQuestion"
            >
              <Plus class="w-4 h-4 mr-2" />
              Dodaj pytanie
            </button>
          </div>

          <!-- Empty State -->
          <div v-if="localQuestions.length === 0" class="text-center py-12 bg-gray-50 dark:bg-gray-700/50 rounded-lg">
            <Icon name="heroicons:question-mark-circle" class="w-16 h-16 mx-auto text-gray-400 mb-4" />
            <p class="text-gray-600 dark:text-gray-400">
              Brak pytań. Kliknij "Dodaj pytanie" aby rozpocząć.
            </p>
          </div>

          <!-- Questions List -->
          <div v-else class="space-y-6">
            <div
              v-for="(question, qIndex) in localQuestions"
              :key="qIndex"
              class="bg-gray-50 dark:bg-gray-700/50 border border-gray-200 dark:border-gray-600 rounded-lg p-4"
            >
              <div class="flex items-start gap-4 mb-4">
                <div class="flex-shrink-0 w-8 h-8 bg-purple-600 text-white rounded-full flex items-center justify-center font-bold">
                  {{ qIndex + 1 }}
                </div>

                <div class="flex-1">
                  <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Treść pytania
                  </label>
                  <input
                    v-model="question.question"
                    type="text"
                    placeholder="Wpisz pytanie..."
                    class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white focus:ring-2 focus:ring-blue-500"
                  >
                </div>

                <button
                  class="text-red-600 hover:text-red-700 p-2 mt-5"
                  @click="removeQuestion(qIndex)"
                >
                  <Trash2 class="w-5 h-5" />
                </button>
              </div>

              <!-- Answers -->
              <div class="ml-12 space-y-3">
                <div class="flex items-center justify-between">
                  <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                    Odpowiedzi
                  </label>
                  <button
                    class="text-sm text-blue-600 hover:text-blue-700 font-medium"
                    @click="addOption(qIndex)"
                  >
                    + Dodaj odpowiedź
                  </button>
                </div>

                <div v-if="question.options.length === 0" class="text-sm text-gray-500 dark:text-gray-400 py-2">
                  Brak odpowiedzi. Dodaj co najmniej 2 odpowiedzi.
                </div>

                <div
                  v-for="(option, oIndex) in question.options"
                  :key="oIndex"
                  class="flex items-center gap-2"
                >
                  <input
                    v-model="option.isCorrect"
                    type="checkbox"
                    :title="option.isCorrect ? 'Poprawna odpowiedź' : 'Niepoprawna odpowiedź'"
                    class="w-5 h-5 text-green-600 border-gray-300 rounded focus:ring-green-500 cursor-pointer"
                  >
                  <input
                    v-model="option.label"
                    type="text"
                    placeholder="Treść odpowiedzi..."
                    class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white text-sm focus:ring-2 focus:ring-blue-500"
                  >
                  <button
                    class="text-red-600 hover:text-red-700"
                    @click="removeOption(qIndex, oIndex)"
                  >
                    <X class="w-4 h-4" />
                  </button>
                </div>

                <!-- Hint -->
                <p class="text-xs text-gray-500 dark:text-gray-400 italic">
                  <Icon name="heroicons:information-circle" class="w-3 h-3 inline mr-1" />
                  Zaznacz checkbox przy poprawnej odpowiedzi
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="p-6 border-t border-gray-200 dark:border-gray-700 sticky bottom-0 bg-white dark:bg-gray-800">
          <div class="flex items-center justify-end gap-3">
            <button
              class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
              @click="$emit('close')"
            >
              Anuluj
            </button>
            <button
              :disabled="!isValid"
              class="px-6 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg font-medium transition-colors"
              @click="saveQuiz"
            >
              Zapisz quiz
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { X, Plus, Trash2 } from 'lucide-vue-next'
import type { QuizQuestionDto } from '~/types/requests'

interface QuizOption {
  value: string
  label: string
  isCorrect: boolean
}

interface LocalQuestion {
  id?: string
  question: string
  options: QuizOption[]
  order: number
}

const props = defineProps<{
  questions: QuizQuestionDto[]
  passingScore?: number
  stepOrder: number
}>()

const emit = defineEmits<{
  close: []
  save: [data: { questions: QuizQuestionDto[], passingScore: number }]
}>()

const localQuestions = ref<LocalQuestion[]>([])
const localPassingScore = ref(props.passingScore || 80)

// Initialize from props
onMounted(() => {
  if (props.questions && props.questions.length > 0) {
    localQuestions.value = props.questions.map((q, index) => ({
      id: q.id,
      question: q.question,
      options: JSON.parse(q.options || '[]') as QuizOption[],
      order: q.order || index + 1
    }))
  }
})

const addQuestion = () => {
  localQuestions.value.push({
    question: '',
    options: [],
    order: localQuestions.value.length + 1
  })
}

const removeQuestion = (index: number) => {
  localQuestions.value.splice(index, 1)
  // Reorder
  localQuestions.value.forEach((q, i) => q.order = i + 1)
}

const addOption = (questionIndex: number) => {
  const question = localQuestions.value[questionIndex]
  if (!question) return

  question.options.push({
    value: `option_${Date.now()}_${Math.random()}`,
    label: '',
    isCorrect: false
  })
}

const removeOption = (questionIndex: number, optionIndex: number) => {
  const question = localQuestions.value[questionIndex]
  if (!question) return

  question.options.splice(optionIndex, 1)
}

const isValid = computed(() => {
  if (localQuestions.value.length === 0) return true // Allow empty quiz

  // Each question must have:
  // - Non-empty question text
  // - At least 2 options
  // - At least one correct option
  return localQuestions.value.every(q => {
    if (!q.question.trim()) return false
    if (q.options.length < 2) return false
    if (!q.options.some(o => o.isCorrect)) return false
    if (!q.options.every(o => o.label.trim())) return false
    return true
  })
})

const saveQuiz = () => {
  // Convert local format to DTO format
  const quizQuestions: QuizQuestionDto[] = localQuestions.value.map(q => ({
    id: q.id || crypto.randomUUID(),
    question: q.question,
    options: JSON.stringify(q.options),
    order: q.order
  }))

  emit('save', {
    questions: quizQuestions,
    passingScore: localPassingScore.value
  })
}
</script>
