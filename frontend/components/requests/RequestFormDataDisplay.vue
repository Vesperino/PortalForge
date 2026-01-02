<script setup lang="ts">
import type { RequestTemplateField } from '~/types/requests'

interface Props {
  formData: string
  fields?: RequestTemplateField[]
}

const props = defineProps<Props>()

const formatFieldName = (key: string): string => {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (str) => str.toUpperCase())
    .trim()
}

const formatFieldValue = (value: unknown): string => {
  if (value === null || value === undefined) return '-'
  if (typeof value === 'boolean') return value ? 'Tak' : 'Nie'
  if (typeof value === 'object') return JSON.stringify(value, null, 2)

  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}/.test(value)) {
    try {
      const date = new Date(value)
      return date.toLocaleDateString('pl-PL', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      })
    } catch {
      return value
    }
  }

  return String(value)
}

interface FormattedField {
  key: string
  value: string
}

const formattedFormData = computed((): FormattedField[] => {
  if (!props.formData) return []

  try {
    const data = JSON.parse(props.formData) as Record<string, unknown>
    const fields = props.fields || []
    const labelById = new Map<string, string>()

    for (const f of fields) {
      const field = f as RequestTemplateField
      const fieldId = (field.id || field.Id)?.toString().toLowerCase()
      if (fieldId) {
        labelById.set(fieldId, field.label)
      }
    }

    return Object.entries(data).map(([key, value]) => {
      const normalizedKey = key.toLowerCase()
      const label = labelById.get(normalizedKey) || formatFieldName(key)
      return { key: label, value: formatFieldValue(value) }
    })
  } catch (err) {
    console.error('Error parsing form data:', err)
    return []
  }
})
</script>

<template>
  <div data-testid="request-form-data-display">
    <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
      Wypełniony formularz
    </h3>
    <div class="space-y-3">
      <div
        v-for="field in formattedFormData"
        :key="field.key"
        class="flex flex-col sm:flex-row sm:items-start gap-2 py-3 border-b border-gray-200 dark:border-gray-700 last:border-b-0"
      >
        <dt
          class="text-sm font-medium text-gray-500 dark:text-gray-400 sm:w-1/3"
        >
          {{ field.key }}
        </dt>
        <dd class="text-sm text-gray-900 dark:text-white sm:w-2/3">
          {{ field.value }}
        </dd>
      </div>
    </div>
  </div>
</template>
