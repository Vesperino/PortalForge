<script setup lang="ts">
interface Props {
  score: number
  passed: boolean
  passingScore: number
}

defineProps<Props>()
</script>

<template>
  <div
    class="quiz-result rounded-lg p-4 border"
    :class="{
      'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800': passed,
      'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800': !passed
    }"
  >
    <div class="flex items-start gap-3">
      <div
        class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center"
        :class="{
          'bg-green-500': passed,
          'bg-red-500': !passed
        }"
      >
        <svg
          v-if="passed"
          class="w-5 h-5 text-white"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M5 13l4 4L19 7"
          />
        </svg>
        <svg
          v-else
          class="w-5 h-5 text-white"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M6 18L18 6M6 6l12 12"
          />
        </svg>
      </div>
      <div class="flex-1">
        <h4
          class="text-sm font-semibold mb-1"
          :class="{
            'text-green-800 dark:text-green-200': passed,
            'text-red-800 dark:text-red-200': !passed
          }"
        >
          {{ passed ? 'Quiz zaliczony!' : 'Quiz niezaliczony' }}
        </h4>
        <p
          class="text-sm"
          :class="{
            'text-green-700 dark:text-green-300': passed,
            'text-red-700 dark:text-red-300': !passed
          }"
        >
          Wynik: {{ score }}% (wymagane: {{ passingScore }}%)
        </p>
        <p
          v-if="!passed"
          class="text-xs mt-2 text-red-600 dark:text-red-400"
        >
          Niestety nie udało się zaliczyć quizu. Skontaktuj się z administratorem aby ponownie spróbować.
        </p>
      </div>
    </div>
  </div>
</template>
