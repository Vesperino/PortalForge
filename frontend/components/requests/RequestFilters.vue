<script setup lang="ts">
import type { RequestStatus } from '~/types/requests'

interface Props {
  searchPlaceholder?: string
  showStatusFilter?: boolean
  modelValue: string
  statusValue?: string
}

const props = withDefaults(defineProps<Props>(), {
  searchPlaceholder: 'Szukaj...',
  showStatusFilter: false,
  statusValue: ''
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'update:statusValue': [value: string]
}>()

const searchQuery = computed({
  get: () => props.modelValue,
  set: (value: string) => emit('update:modelValue', value)
})

const statusFilter = computed({
  get: () => props.statusValue,
  set: (value: string) => emit('update:statusValue', value)
})

interface StatusOption {
  value: RequestStatus | ''
  label: string
}

const statusOptions: StatusOption[] = [
  { value: '', label: 'Wszystkie statusy' },
  { value: 'InReview', label: 'W trakcie' },
  { value: 'Approved', label: 'Zatwierdzone' },
  { value: 'Rejected', label: 'Odrzucone' },
  { value: 'AwaitingSurvey', label: 'Wymaga quizu' }
]
</script>

<template>
  <div class="flex flex-col sm:flex-row gap-4" data-testid="request-filters">
    <div class="flex-1">
      <input
        v-model="searchQuery"
        type="text"
        :placeholder="searchPlaceholder"
        class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-800 dark:text-white"
        data-testid="request-search-input"
      >
    </div>
    <select
      v-if="showStatusFilter"
      v-model="statusFilter"
      class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-800 dark:text-white"
      data-testid="request-status-filter"
    >
      <option
        v-for="option in statusOptions"
        :key="option.value"
        :value="option.value"
      >
        {{ option.label }}
      </option>
    </select>
  </div>
</template>
