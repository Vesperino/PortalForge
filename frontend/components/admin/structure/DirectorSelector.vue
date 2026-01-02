<script setup lang="ts">
import type { User } from '~/types/auth'

interface Props {
  selectedUser: User | null
  label?: string
  placeholder?: string
  helpText?: string
  required?: boolean
}

withDefaults(defineProps<Props>(), {
  label: 'Dyrektor dzialu',
  placeholder: 'Wyszukaj dyrektora...',
  helpText: 'Mozesz pozostawic puste.',
  required: false
})

const emit = defineEmits<{
  'update:selectedUser': [user: User | null]
}>()

function handleUserSelected(user: User | null): void {
  emit('update:selectedUser', user)
}
</script>

<template>
  <div class="director-selector">
    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
    </label>
    <CommonUserAutocomplete
      :selected-user="selectedUser"
      :placeholder="placeholder"
      @update:selected-user="handleUserSelected"
    />
    <p v-if="helpText" class="mt-2 text-xs text-gray-500 dark:text-gray-400">
      {{ helpText }}
    </p>
  </div>
</template>
