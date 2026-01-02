<script setup lang="ts">
import { FileText, Clock } from 'lucide-vue-next'
import type { RequestTemplate } from '~/types/requests'

interface Props {
  templates: RequestTemplate[]
  loading?: boolean
  searchQuery?: string
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  searchQuery: ''
})

const emit = defineEmits<{
  select: [template: RequestTemplate]
  'update:searchQuery': [value: string]
}>()

const { getIconifyName } = useIconMapping()

const localSearchQuery = computed({
  get: () => props.searchQuery,
  set: (value: string) => emit('update:searchQuery', value)
})

const filteredTemplates = computed(() => {
  if (!localSearchQuery.value) return props.templates

  const query = localSearchQuery.value.toLowerCase()
  return props.templates.filter(
    (t: RequestTemplate) =>
      t.name.toLowerCase().includes(query) ||
      t.description.toLowerCase().includes(query) ||
      t.category.toLowerCase().includes(query)
  )
})

const handleSelect = (template: RequestTemplate): void => {
  emit('select', template)
}
</script>

<template>
  <div class="space-y-6" data-testid="template-selector">
    <div class="flex flex-col sm:flex-row gap-4">
      <div class="flex-1">
        <input
          v-model="localSearchQuery"
          type="text"
          placeholder="Szukaj szablonów..."
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-800 dark:text-white"
          data-testid="template-search-input"
        >
      </div>
    </div>

    <div v-if="loading" class="text-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
    </div>

    <div v-else-if="filteredTemplates.length === 0" class="text-center py-12">
      <FileText class="w-16 h-16 mx-auto text-gray-400 mb-4" />
      <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
        Brak dostępnych szablonów
      </h3>
      <p class="text-gray-600 dark:text-gray-400">
        Nie znaleziono szablonów spełniających kryteria wyszukiwania
      </p>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <button
        v-for="template in filteredTemplates"
        :key="template.id"
        class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 hover:shadow-md hover:border-blue-500 transition-all text-left p-6 group"
        data-testid="template-card"
        @click="handleSelect(template)"
      >
        <div class="flex items-start justify-between mb-4">
          <Icon
            :name="getIconifyName(template.icon)"
            class="w-12 h-12 group-hover:scale-110 transition-transform"
          />
          <span class="px-2 py-1 text-xs font-medium bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 rounded-full">
            {{ template.category }}
          </span>
        </div>

        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
          {{ template.name }}
        </h3>

        <p class="text-sm text-gray-600 dark:text-gray-400 mb-4 line-clamp-2">
          {{ template.description }}
        </p>

        <div class="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
          <span v-if="template.estimatedProcessingDays">
            <Clock class="w-3 h-3 inline mr-1" />
            {{ template.estimatedProcessingDays }} dni
          </span>
          <span>
            {{ template.fields.length }} pól
          </span>
        </div>
      </button>
    </div>
  </div>
</template>
