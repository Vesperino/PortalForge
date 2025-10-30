<script setup lang="ts">
interface Props {
  modelValue: string[]
  allowCustom?: boolean
  suggestions?: string[]
  label?: string
  placeholder?: string
}

interface Emits {
  (e: 'update:modelValue', value: string[]): void
}

const props = withDefaults(defineProps<Props>(), {
  allowCustom: true,
  suggestions: () => [],
  placeholder: 'Dodaj hashtag...'
})

const emit = defineEmits<Emits>()

const inputValue = ref('')
const showSuggestions = ref(false)
const filteredSuggestions = ref<string[]>([])

function normalizeHashtag(tag: string): string {
  // Remove # if present, trim, lowercase
  let normalized = tag.trim().toLowerCase()
  if (normalized.startsWith('#')) {
    normalized = normalized.substring(1)
  }
  
  // Validate: only alphanumeric and underscore, max 30 chars
  normalized = normalized.replace(/[^a-z0-9_]/g, '')
  normalized = normalized.substring(0, 30)
  
  return normalized ? `#${normalized}` : ''
}

function addHashtag(tag: string) {
  const normalized = normalizeHashtag(tag)
  
  if (!normalized) return
  
  // Check if already exists (case insensitive)
  const exists = props.modelValue.some(t => t.toLowerCase() === normalized.toLowerCase())
  
  if (!exists) {
    const newTags = [...props.modelValue, normalized]
    emit('update:modelValue', newTags)
  }
  
  inputValue.value = ''
  showSuggestions.value = false
}

function removeHashtag(tag: string) {
  const newTags = props.modelValue.filter(t => t.toLowerCase() !== tag.toLowerCase())
  emit('update:modelValue', newTags)
}

function handleInput() {
  const value = inputValue.value.trim()
  
  if (!value) {
    filteredSuggestions.value = []
    showSuggestions.value = false
    return
  }
  
  // Filter suggestions based on input
  filteredSuggestions.value = props.suggestions
    .filter(s => {
      const normalized = normalizeHashtag(s)
      const search = normalizeHashtag(value)
      return normalized.toLowerCase().includes(search.toLowerCase())
    })
    .filter(s => !props.modelValue.includes(normalizeHashtag(s)))
    .slice(0, 5)
  
  showSuggestions.value = filteredSuggestions.value.length > 0
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter' || event.key === ',' || event.key === ' ') {
    event.preventDefault()
    if (inputValue.value.trim()) {
      addHashtag(inputValue.value)
    }
  } else if (event.key === 'Backspace' && !inputValue.value && props.modelValue.length > 0) {
    // Remove last tag if input is empty
    const newTags = [...props.modelValue]
    newTags.pop()
    emit('update:modelValue', newTags)
  }
}

function selectSuggestion(suggestion: string) {
  addHashtag(suggestion)
}
</script>

<template>
  <div class="space-y-2">
    <label v-if="label" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
      {{ label }}
    </label>
    
    <div class="relative">
      <!-- Tags Display and Input -->
      <div class="flex flex-wrap gap-2 p-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 min-h-[60px] focus-within:ring-2 focus-within:ring-blue-500 focus-within:border-transparent">
        <!-- Existing tags -->
        <span
          v-for="tag in modelValue"
          :key="tag"
          class="inline-flex items-center gap-1 px-3 py-1 bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 rounded-full text-sm font-medium"
        >
          {{ tag }}
          <button
            type="button"
            class="hover:text-blue-600 dark:hover:text-blue-300 transition-colors"
            @click="removeHashtag(tag)"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </span>
        
        <!-- Input -->
        <input
          v-model="inputValue"
          type="text"
          class="flex-1 min-w-[120px] outline-none bg-transparent text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500"
          :placeholder="modelValue.length === 0 ? placeholder : ''"
          @input="handleInput"
          @keydown="handleKeydown"
          @focus="handleInput"
          @blur="showSuggestions = false"
        >
      </div>
      
      <!-- Suggestions Dropdown -->
      <div
        v-if="showSuggestions && filteredSuggestions.length > 0"
        class="absolute z-10 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg max-h-48 overflow-y-auto"
      >
        <button
          v-for="suggestion in filteredSuggestions"
          :key="suggestion"
          type="button"
          class="w-full text-left px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm transition-colors"
          @mousedown.prevent="selectSuggestion(suggestion)"
        >
          {{ normalizeHashtag(suggestion) }}
        </button>
      </div>
    </div>
    
    <p class="text-xs text-gray-500 dark:text-gray-400">
      Wciśnij Enter, przecinek lub spację aby dodać hashtag. Maksymalnie 30 znaków, tylko litery, cyfry i podkreślnik.
    </p>
  </div>
</template>

