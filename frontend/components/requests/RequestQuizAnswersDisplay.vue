<script setup lang="ts">
import { computed } from 'vue'
import { CheckCircle, XCircle } from 'lucide-vue-next'

interface QuizQuestion {
  id: string
  question: string
  options: string // JSON string
  order: number
}

interface QuizAnswer {
  questionId: string
  selectedAnswer: string
  isCorrect: boolean
  answeredAt: string
}

interface Props {
  questions: QuizQuestion[]
  answers: QuizAnswer[]
}

const props = defineProps<Props>()

interface QuizOptionParsed {
  value: string
  label: string
  isCorrect?: boolean
}

// Parse options JSON
const parseOptions = (optionsJson: string): QuizOptionParsed[] => {
  try {
    return JSON.parse(optionsJson) as QuizOptionParsed[]
  } catch {
    return []
  }
}

// Create a map of answers by question ID
const answersMap = computed(() => {
  const map = new Map<string, QuizAnswer>()
  props.answers.forEach(answer => {
    map.set(answer.questionId, answer)
  })
  return map
})

// Combine questions with answers
const questionsWithAnswers = computed(() => {
  return [...props.questions]
    .sort((a, b) => a.order - b.order)
    .map(question => {
      const answer = answersMap.value.get(question.id)
      const options = parseOptions(question.options)
      const correctOption = options.find((opt: QuizOptionParsed) => opt.isCorrect)

      return {
        question: question.question,
        options,
        selectedAnswer: answer?.selectedAnswer,
        isCorrect: answer?.isCorrect ?? false,
        correctAnswer: correctOption?.label
      }
    })
})
</script>

<template>
  <div class="space-y-6">
    <div
      v-for="(item, index) in questionsWithAnswers"
      :key="index"
      class="border border-gray-200 dark:border-gray-700 rounded-lg p-4"
    >
      <!-- Question -->
      <div class="mb-3">
        <div class="flex items-start gap-2">
          <span class="text-sm font-semibold text-gray-700 dark:text-gray-300">
            {{ index + 1 }}.
          </span>
          <h4 class="text-sm font-semibold text-gray-900 dark:text-white flex-1">
            {{ item.question }}
          </h4>
          <!-- Correct/Incorrect indicator -->
          <div
            v-if="item.selectedAnswer"
            class="flex-shrink-0"
            :class="{
              'text-green-600 dark:text-green-400': item.isCorrect,
              'text-red-600 dark:text-red-400': !item.isCorrect
            }"
          >
            <CheckCircle v-if="item.isCorrect" class="w-5 h-5" />
            <XCircle v-else class="w-5 h-5" />
          </div>
        </div>
      </div>

      <!-- Options -->
      <div class="space-y-2 ml-6">
        <div
          v-for="option in item.options"
          :key="option.value"
          class="flex items-center gap-2 text-sm p-2 rounded"
          :class="{
            'bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800':
              item.selectedAnswer === option.value && item.isCorrect,
            'bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800':
              item.selectedAnswer === option.value && !item.isCorrect,
            'bg-gray-50 dark:bg-gray-800/50':
              item.selectedAnswer !== option.value
          }"
        >
          <div class="flex-shrink-0">
            <div
              class="w-4 h-4 rounded-full border-2 flex items-center justify-center"
              :class="{
                'border-green-600 bg-green-600': item.selectedAnswer === option.value && item.isCorrect,
                'border-red-600 bg-red-600': item.selectedAnswer === option.value && !item.isCorrect,
                'border-gray-300 dark:border-gray-600': item.selectedAnswer !== option.value
              }"
            >
              <div
                v-if="item.selectedAnswer === option.value"
                class="w-2 h-2 rounded-full bg-white"
              />
            </div>
          </div>
          <span
            class="flex-1"
            :class="{
              'text-green-900 dark:text-green-100 font-medium':
                item.selectedAnswer === option.value && item.isCorrect,
              'text-red-900 dark:text-red-100 font-medium':
                item.selectedAnswer === option.value && !item.isCorrect,
              'text-gray-700 dark:text-gray-300':
                item.selectedAnswer !== option.value
            }"
          >
            {{ option.label }}
          </span>
          <!-- Show if this is the correct answer -->
          <span
            v-if="option.isCorrect && item.selectedAnswer !== option.value"
            class="text-xs text-green-600 dark:text-green-400 font-medium"
          >
            Prawidłowa odpowiedź
          </span>
        </div>
      </div>

      <!-- Show selected vs correct answer when wrong -->
      <div
        v-if="item.selectedAnswer && !item.isCorrect"
        class="mt-3 ml-6 text-xs text-gray-600 dark:text-gray-400"
      >
        <p>
          <span class="font-medium text-red-600 dark:text-red-400">Wybrano:</span>
          {{ item.options.find((o: QuizOptionParsed) => o.value === item.selectedAnswer)?.label }}
        </p>
        <p>
          <span class="font-medium text-green-600 dark:text-green-400">Poprawna odpowiedź:</span>
          {{ item.correctAnswer }}
        </p>
      </div>
    </div>
  </div>
</template>
