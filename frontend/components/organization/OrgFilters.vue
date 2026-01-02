<script setup lang="ts">
import type { DepartmentTreeDto } from '~/types/department'

type ViewMode = 'tree' | 'departments' | 'list'

interface Props {
  viewMode: ViewMode
  searchQuery: string
  selectedDepartment: string | null
  departments: DepartmentTreeDto[]
  showSearch?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showSearch: true
})

const emit = defineEmits<{
  'update:viewMode': [mode: ViewMode]
  'update:searchQuery': [query: string]
  'update:selectedDepartment': [departmentId: string | null]
}>()

const localSearchQuery = computed({
  get: () => props.searchQuery,
  set: (value: string) => emit('update:searchQuery', value)
})

const localSelectedDepartment = computed({
  get: () => props.selectedDepartment,
  set: (value: string | null) => emit('update:selectedDepartment', value)
})

const setViewMode = (mode: ViewMode) => {
  emit('update:viewMode', mode)
}
</script>

<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4 max-w-full overflow-hidden">
    <div class="flex flex-col gap-4">
      <div class="flex flex-wrap gap-2">
        <button
          :class="[
            'px-4 py-2 rounded-lg font-medium transition-colors text-sm',
            viewMode === 'tree'
              ? 'bg-blue-600 text-white'
              : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
          ]"
          data-testid="view-mode-tree"
          @click="setViewMode('tree')"
        >
          <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
          </svg>
          Drzewo organizacyjne
        </button>
        <button
          :class="[
            'px-4 py-2 rounded-lg font-medium transition-colors text-sm',
            viewMode === 'departments'
              ? 'bg-blue-600 text-white'
              : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
          ]"
          data-testid="view-mode-departments"
          @click="setViewMode('departments')"
        >
          <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
          Wedlug dzialow
        </button>
        <button
          :class="[
            'px-4 py-2 rounded-lg font-medium transition-colors text-sm',
            viewMode === 'list'
              ? 'bg-blue-600 text-white'
              : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
          ]"
          data-testid="view-mode-list"
          @click="setViewMode('list')"
        >
          <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16" />
          </svg>
          Lista pracownikow
        </button>
      </div>

      <div v-if="showSearch && viewMode === 'list'" class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label for="search" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Szukaj pracownika
          </label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            <input
              id="search"
              v-model="localSearchQuery"
              type="text"
              placeholder="Imie, nazwisko, email..."
              class="pl-10 w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              data-testid="search-input"
            >
          </div>
        </div>

        <div>
          <label for="department" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Dzial
          </label>
          <select
            id="department"
            v-model="localSelectedDepartment"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            data-testid="department-select"
          >
            <option :value="null">
              Wszystkie dzialy
            </option>
            <option v-for="dept in departments" :key="dept.id" :value="dept.id">
              {{ dept.name }}
            </option>
          </select>
        </div>
      </div>
    </div>
  </div>
</template>
