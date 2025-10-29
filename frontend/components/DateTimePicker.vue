<script setup lang="ts">
import { DatePicker } from 'v-calendar'
import 'v-calendar/style.css'

interface Props {
  modelValue: Date | null
  label?: string
  required?: boolean
  error?: string
}

interface Emits {
  (e: 'update:modelValue', value: Date | null): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const date = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const masks = {
  modelValue: 'YYYY-MM-DD HH:mm:ss',
}
</script>

<template>
  <div class="space-y-2">
    <label v-if="label" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
    </label>
    <DatePicker
      v-model="date"
      mode="dateTime"
      :masks="masks"
      :is24hr="true"
      :timezone="'Europe/Warsaw'"
      class="w-full"
    >
      <template #default="{ inputValue, inputEvents }">
        <input
          :value="inputValue"
          v-on="inputEvents"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          :class="{ 'border-red-500 dark:border-red-500': error }"
          placeholder="Wybierz datę i godzinę"
        />
      </template>
    </DatePicker>
    <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>
  </div>
</template>

<style>
/* VCalendar dark mode support */
.vc-container {
  --vc-bg: theme('colors.white');
  --vc-border: theme('colors.gray.300');
  --vc-text-gray-900: theme('colors.gray.900');
  --vc-text-gray-700: theme('colors.gray.700');
  --vc-text-gray-500: theme('colors.gray.500');
  --vc-text-gray-400: theme('colors.gray.400');
  --vc-accent-50: theme('colors.blue.50');
  --vc-accent-100: theme('colors.blue.100');
  --vc-accent-200: theme('colors.blue.200');
  --vc-accent-300: theme('colors.blue.300');
  --vc-accent-400: theme('colors.blue.400');
  --vc-accent-500: theme('colors.blue.500');
  --vc-accent-600: theme('colors.blue.600');
  --vc-accent-700: theme('colors.blue.700');
  --vc-accent-800: theme('colors.blue.800');
  --vc-accent-900: theme('colors.blue.900');
}

.dark .vc-container {
  --vc-bg: theme('colors.gray.800');
  --vc-border: theme('colors.gray.600');
  --vc-text-gray-900: theme('colors.gray.100');
  --vc-text-gray-700: theme('colors.gray.300');
  --vc-text-gray-500: theme('colors.gray.400');
  --vc-text-gray-400: theme('colors.gray.500');
}
</style>

