<template>
  <div class="icon-picker">
    <div class="mb-3">
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
        Wybierz ikone
      </label>
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Szukaj ikony... (np. urlop, sprzet, dokument)"
        class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
      >
    </div>

    <div class="mb-3 flex gap-2 overflow-x-auto pb-2">
      <button
        v-for="category in categories"
        :key="category.id"
        type="button"
        @click="selectedCategory = category.id"
        :class="[
          'px-3 py-1.5 rounded-lg text-sm font-medium whitespace-nowrap transition-colors',
          selectedCategory === category.id
            ? 'bg-blue-600 text-white'
            : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-300 dark:hover:bg-gray-600'
        ]"
      >
        {{ category.label }}
      </button>
    </div>

    <div class="icon-grid grid grid-cols-6 sm:grid-cols-8 md:grid-cols-10 gap-2 max-h-96 overflow-y-auto p-2 border border-gray-200 dark:border-gray-700 rounded-lg bg-gray-50 dark:bg-gray-800">
      <button
        v-for="icon in filteredIcons"
        :key="icon.name"
        type="button"
        @click="selectIcon(icon.name)"
        :class="[
          'flex flex-col items-center justify-center p-3 rounded-lg transition-all hover:bg-blue-50 dark:hover:bg-blue-900',
          selectedIcon === icon.name ? 'bg-blue-100 dark:bg-blue-800 ring-2 ring-blue-500' : 'bg-white dark:bg-gray-700'
        ]"
        :title="icon.label"
      >
        <Icon :name="icon.iconifyName" class="w-8 h-8" />
        <span class="text-xs mt-1 text-gray-600 dark:text-gray-400 truncate w-full text-center">
          {{ icon.label }}
        </span>
      </button>
    </div>

    <div v-if="selectedIcon" class="mt-3 p-3 bg-blue-50 dark:bg-blue-900/30 rounded-lg flex items-center justify-between">
      <div class="flex items-center gap-2">
        <Icon :name="getIconifyName(selectedIcon)" class="w-6 h-6" />
        <span class="text-sm font-medium text-blue-900 dark:text-blue-300">
          Wybrano: {{ getIconLabel(selectedIcon) }}
        </span>
      </div>
      <button
        type="button"
        @click="clearSelection"
        class="text-sm text-red-600 hover:text-red-700 dark:text-red-400"
      >
        Wyczysc
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

interface IconDefinition {
  name: string
  label: string
  iconifyName: string
  category: string
  keywords: string[]
}

const props = defineProps<{
  modelValue?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const searchQuery = ref('')
const selectedIcon = ref(props.modelValue || '')
const selectedCategory = ref('all')

const categories = [
  { id: 'all', label: 'Wszystkie' },
  { id: 'vacation', label: 'Urlopy' },
  { id: 'equipment', label: 'Sprzet' },
  { id: 'documents', label: 'Dokumenty' },
  { id: 'safety', label: 'BHP' },
  { id: 'training', label: 'Szkolenia' },
  { id: 'general', label: 'Pozostale' }
]

const icons: IconDefinition[] = [
  // Urlopy
  { name: 'beach-umbrella', label: 'Urlop na plazy', iconifyName: 'fluent-emoji-flat:beach-with-umbrella', category: 'vacation', keywords: ['urlop', 'wakacje', 'plaza', 'odpoczynek'] },
  { name: 'plane', label: 'Delegacja', iconifyName: 'fluent-emoji-flat:airplane', category: 'vacation', keywords: ['delegacja', 'podroz', 'samolot'] },
  { name: 'calendar', label: 'Plan urlopow', iconifyName: 'heroicons:calendar-days', category: 'vacation', keywords: ['kalendarz', 'termin', 'plan'] },

  // Sprzet i IT
  { name: 'laptop', label: 'Sprzet IT', iconifyName: 'heroicons:computer-desktop', category: 'equipment', keywords: ['sprzet', 'laptop', 'komputer'] },
  { name: 'toolbox', label: 'Narzędzia', iconifyName: 'heroicons:wrench-screwdriver', category: 'equipment', keywords: ['narzedzia', 'serwis', 'naprawa'] },

  // Dokumenty i administracja
  { name: 'document', label: 'Dokument', iconifyName: 'heroicons:document-text', category: 'documents', keywords: ['dokument', 'wniosek', 'formularz'] },
  { name: 'folder', label: 'Folder', iconifyName: 'heroicons:folder', category: 'documents', keywords: ['folder', 'archiwum', 'dokumenty'] },
  { name: 'clipboard', label: 'Lista dokumentow', iconifyName: 'heroicons:clipboard-document-list', category: 'documents', keywords: ['lista', 'kroki', 'formularz'] },

  // BHP i bezpieczenstwo
  { name: 'shield', label: 'Bezpieczenstwo', iconifyName: 'heroicons:shield-check', category: 'safety', keywords: ['bhp', 'bezpieczenstwo', 'ochrona'] },
  { name: 'warning', label: 'Zgloszenie zagrozenia', iconifyName: 'heroicons:exclamation-triangle', category: 'safety', keywords: ['zagrozenie', 'incydent', 'alert'] },

  // Szkolenia i wiedza
  { name: 'graduation', label: 'Szkolenie', iconifyName: 'heroicons:academic-cap', category: 'training', keywords: ['szkolenie', 'kurs', 'nauka'] },
  { name: 'book', label: 'Materialy szkoleniowe', iconifyName: 'heroicons:book-open', category: 'training', keywords: ['materialy', 'nauka', 'wiedza'] },

  // Ogolne
  { name: 'users', label: 'Zespol', iconifyName: 'heroicons:user-group', category: 'general', keywords: ['zespol', 'ludzie', 'dzial'] },
  { name: 'bell', label: 'Powiadomienia', iconifyName: 'heroicons:bell', category: 'general', keywords: ['powiadomienia', 'alarm', 'komunikaty'] },
  { name: 'check', label: 'Zatwierdzone', iconifyName: 'heroicons:check-circle', category: 'general', keywords: ['zatwierdzone', 'akceptacja', 'status'] }
]

const filteredIcons = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  return icons.filter(icon => {
    const matchesCategory = selectedCategory.value === 'all' || icon.category === selectedCategory.value
    if (!matchesCategory) {
      return false
    }

    if (!query) {
      return true
    }

    const haystack = [
      icon.name,
      icon.label,
      icon.iconifyName,
      ...icon.keywords
    ]
      .join(' ')
      .toLowerCase()

    return haystack.includes(query)
  })
})

const getIconifyName = (iconName: string) => {
  return icons.find(icon => icon.name === iconName)?.iconifyName || icons[0]?.iconifyName || ''
}

const getIconLabel = (iconName: string) => {
  return icons.find(icon => icon.name === iconName)?.label || iconName
}

const selectIcon = (iconName: string) => {
  selectedIcon.value = iconName
  emit('update:modelValue', iconName)
}

const clearSelection = () => {
  selectedIcon.value = ''
  emit('update:modelValue', '')
}

watch(
  () => props.modelValue,
  newValue => {
    selectedIcon.value = newValue || ''
  }
)
</script>

<style scoped>
.icon-picker {
  @apply w-full;
}

.icon-grid::-webkit-scrollbar {
  width: 8px;
}

.icon-grid::-webkit-scrollbar-track {
  @apply bg-gray-100 dark:bg-gray-700 rounded;
}

.icon-grid::-webkit-scrollbar-thumb {
  @apply bg-gray-300 dark:bg-gray-600 rounded;
}

.icon-grid::-webkit-scrollbar-thumb:hover {
  @apply bg-gray-400 dark:bg-gray-500;
}
</style>
