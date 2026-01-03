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
      (t.description || '').toLowerCase().includes(query) ||
      (t.category || '').toLowerCase().includes(query)
  )
})

const handleSelect = (template: RequestTemplate): void => {
  emit('select', template)
}
</script>

<template>
  <div class="space-y-6" data-testid="template-selector">
    <div class="relative">
      <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
        <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
      </div>
      <input
        v-model="localSearchQuery"
        type="text"
        placeholder="Szukaj szablonów..."
        class="w-full pl-11 pr-4 py-3 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:text-white placeholder-gray-400 transition-all"
        data-testid="template-search-input"
      >
    </div>

    <div v-if="loading" class="flex items-center justify-center py-16">
      <div class="animate-spin rounded-full h-10 w-10 border-2 border-blue-600 border-t-transparent" />
    </div>

    <div v-else-if="filteredTemplates.length === 0" class="text-center py-16">
      <div class="w-16 h-16 mx-auto mb-4 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center">
        <FileText class="w-8 h-8 text-gray-400" />
      </div>
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
        Brak dostepnych szablonow
      </h3>
      <p class="text-gray-500 dark:text-gray-400 max-w-sm mx-auto">
        Nie znaleziono szablonow spelniajacych kryteria wyszukiwania
      </p>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
      <button
        v-for="template in filteredTemplates"
        :key="template.id"
        class="relative bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 hover:border-blue-400 dark:hover:border-blue-500 hover:shadow-lg transition-all duration-200 text-left p-5 group overflow-hidden"
        data-testid="template-card"
        @click="handleSelect(template)"
      >
        <div class="absolute top-0 right-0 w-24 h-24 bg-gradient-to-br from-blue-500/5 to-purple-500/5 rounded-bl-full" />

        <div class="flex items-start gap-4 mb-4">
          <div class="flex-shrink-0 w-12 h-12 bg-gradient-to-br from-blue-50 to-blue-100 dark:from-blue-900/30 dark:to-blue-800/30 rounded-xl flex items-center justify-center group-hover:scale-105 transition-transform">
            <Icon
              :name="getIconifyName(template.icon)"
              class="w-6 h-6 text-blue-600 dark:text-blue-400"
            />
          </div>
          <div class="flex-1 min-w-0">
            <h3 class="text-base font-semibold text-gray-900 dark:text-white truncate group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
              {{ template.name }}
            </h3>
            <span class="inline-block mt-1 px-2 py-0.5 text-xs font-medium bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 rounded-md">
              {{ template.category }}
            </span>
          </div>
        </div>

        <p class="text-sm text-gray-600 dark:text-gray-400 line-clamp-2 mb-4">
          {{ template.description }}
        </p>

        <div v-if="template.estimatedProcessingDays" class="flex items-center text-xs text-gray-500 dark:text-gray-400">
          <Clock class="w-3.5 h-3.5 mr-1.5" />
          <span>Czas realizacji: {{ template.estimatedProcessingDays }} dni</span>
        </div>

        <div class="absolute bottom-5 right-5 opacity-0 group-hover:opacity-100 transition-opacity">
          <svg class="w-5 h-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </div>
      </button>
    </div>
  </div>
</template>
