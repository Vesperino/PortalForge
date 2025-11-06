<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-[10002]" @click.self="$emit('close')">
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-y-auto">
      <!-- Header -->
      <div class="p-6 border-b border-gray-200 dark:border-gray-700 sticky top-0 bg-white dark:bg-gray-800 z-10">
        <div class="flex items-center justify-between">
          <div>
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
              Quiz wymagany do zatwierdzenia
            </h2>
            <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
              Wymagany wynik: {{ passingScore }}%
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

      <!-- Quiz Content -->
      <div class="p-6">
        <!-- Before submission -->
        <div v-if="!submitted" class="space-y-6">
          <div
            v-for="(question, index) in extendedQuestions"
            :key="question.id"
            class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-6"
          >
            <div class="flex items-start gap-3 mb-4">
              <div class="flex-shrink-0 w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center font-bold">
                {{ index + 1 }}
              </div>
              <h3 class="text-lg font-medium text-gray-900 dark:text-white">
                {{ question.question }}
              </h3>
            </div>

            <div class="ml-11 space-y-2">
              <label
                v-for="option in question.parsedOptions"
                :key="option.value"
                class="flex items-start p-3 rounded-lg border border-gray-200 dark:border-gray-600 hover:bg-white dark:hover:bg-gray-700 cursor-pointer transition-colors"
                :class="{
                  'bg-blue-50 dark:bg-blue-900/30 border-blue-500': question.id && answers[question.id] === option.value
                }"
              >
                <input
                  v-if="question.id"
                  v-model="answers[question.id]"
                  type="radio"
                  :name="`question-${question.id}`"
                  :value="option.value"
                  class="mt-1 w-4 h-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                >
                <span class="ml-3 text-gray-700 dark:text-gray-300">
                  {{ option.label }}
                </span>
              </label>
            </div>
          </div>

          <!-- Progress -->
          <div class="bg-blue-50 dark:bg-blue-900/30 rounded-lg p-4">
            <div class="flex items-center justify-between text-sm">
              <span class="text-blue-900 dark:text-blue-300">
                Odpowiedziano: {{ answeredCount }} / {{ extendedQuestions.length }}
              </span>
              <span class="text-blue-600 dark:text-blue-400 font-medium">
                {{ Math.round((answeredCount / extendedQuestions.length) * 100) }}%
              </span>
            </div>
            <div class="mt-2 w-full bg-blue-200 dark:bg-blue-900 rounded-full h-2">
              <div
                class="bg-blue-600 dark:bg-blue-500 h-2 rounded-full transition-all"
                :style="{ width: `${(answeredCount / extendedQuestions.length) * 100}%` }"
              />
            </div>
          </div>
        </div>

        <!-- After submission -->
        <div v-else class="space-y-6">
          <!-- Results Summary -->
          <div
            :class="[
              'rounded-lg p-6 text-center',
              quizPassed
                ? 'bg-green-50 dark:bg-green-900/30 border-2 border-green-500'
                : 'bg-red-50 dark:bg-red-900/30 border-2 border-red-500'
            ]"
          >
            <component
              :is="quizPassed ? CheckCircle : XCircle"
              :class="[
                'w-16 h-16 mx-auto mb-4',
                quizPassed ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
              ]"
            />
            <h3
              :class="[
                'text-2xl font-bold mb-2',
                quizPassed ? 'text-green-900 dark:text-green-100' : 'text-red-900 dark:text-red-100'
              ]"
            >
              {{ quizPassed ? 'Gratulacje! Zdałeś quiz!' : 'Niestety, nie zdałeś quizu' }}
            </h3>
            <p
              :class="[
                'text-lg',
                quizPassed ? 'text-green-700 dark:text-green-300' : 'text-red-700 dark:text-red-300'
              ]"
            >
              Twój wynik: <strong>{{ score }}%</strong>
            </p>
            <p
              :class="[
                'text-sm mt-1',
                quizPassed ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
              ]"
            >
              (Wymagane: {{ passingScore }}%)
            </p>
          </div>

          <!-- Detailed Answers -->
          <div class="space-y-4">
            <h4 class="text-lg font-semibold text-gray-900 dark:text-white">
              Szczegółowe odpowiedzi
            </h4>

            <div
              v-for="(question, index) in extendedQuestions"
              :key="question.id"
              class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-6"
            >
              <div class="flex items-start gap-3 mb-4">
                <div class="flex-shrink-0 w-8 h-8 bg-gray-600 text-white rounded-full flex items-center justify-center font-bold">
                  {{ index + 1 }}
                </div>
                <div class="flex-1">
                  <h5 class="text-lg font-medium text-gray-900 dark:text-white mb-3">
                    {{ question.question }}
                  </h5>

                  <div class="space-y-2">
                    <div
                      v-for="option in question.parsedOptions"
                      :key="option.value"
                      :class="[
                        'p-3 rounded-lg border',
                        option.isCorrect
                          ? 'bg-green-50 dark:bg-green-900/30 border-green-500'
                          : question.id && answers[question.id] === option.value
                          ? 'bg-red-50 dark:bg-red-900/30 border-red-500'
                          : 'border-gray-200 dark:border-gray-600'
                      ]"
                    >
                      <div class="flex items-center justify-between">
                        <span
:class="[
                          'text-gray-700 dark:text-gray-300',
                          option.isCorrect && 'font-medium text-green-900 dark:text-green-100',
                          !option.isCorrect && question.id && answers[question.id] === option.value && 'font-medium text-red-900 dark:text-red-100'
                        ]">
                          {{ option.label }}
                        </span>
                        <span>
                          <CheckCircle v-if="option.isCorrect" class="w-5 h-5 text-green-600 dark:text-green-400" />
                          <XCircle v-else-if="question.id && answers[question.id] === option.value" class="w-5 h-5 text-red-600 dark:text-red-400" />
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="p-6 border-t border-gray-200 dark:border-gray-700 sticky bottom-0 bg-white dark:bg-gray-800">
        <div class="flex items-center justify-between">
          <button
            v-if="!submitted"
            class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
            @click="$emit('close')"
          >
            Anuluj
          </button>
          <div v-else />

          <button
            v-if="!submitted"
            :disabled="answeredCount < extendedQuestions.length"
            class="px-6 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg font-medium transition-colors"
            @click="submitQuiz"
          >
            Prześlij odpowiedzi
          </button>
          <button
            v-else
            class="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
            @click="$emit('close')"
          >
            Zamknij
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { X, CheckCircle, XCircle } from 'lucide-vue-next'
import type { QuizQuestion, QuizOption } from '~/types/requests'

interface ExtendedQuestion extends QuizQuestion {
  parsedOptions: QuizOption[]
}

const props = defineProps<{
  questions: QuizQuestion[]
  passingScore: number
}>()

const emit = defineEmits<{
  close: []
  submit: [score: number, passed: boolean, answers: Record<string, string>]
}>()

const answers = ref<Record<string, string>>({})
const submitted = ref(false)
const score = ref(0)
const quizPassed = ref(false)

const extendedQuestions = computed<ExtendedQuestion[]>(() => {
  return props.questions.map(q => ({
    ...q,
    parsedOptions: JSON.parse(q.options) as QuizOption[]
  }))
})

const answeredCount = computed(() => {
  return Object.keys(answers.value).length
})

const submitQuiz = () => {
  let correctCount = 0
  
  extendedQuestions.value.forEach(question => {
    const userAnswer = answers.value[question.id!]
    const correctOption = question.parsedOptions.find(opt => opt.isCorrect)
    
    if (userAnswer === correctOption?.value) {
      correctCount++
    }
  })
  
  const calculatedScore = Math.round((correctCount / extendedQuestions.value.length) * 100)
  const passed = calculatedScore >= props.passingScore
  
  score.value = calculatedScore
  quizPassed.value = passed
  submitted.value = true
  
  emit('submit', calculatedScore, passed, answers.value)
}
</script>

