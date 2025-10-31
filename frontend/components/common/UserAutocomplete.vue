<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { X, ChevronDown } from 'lucide-vue-next'

interface User {
  id: string
  firstName: string
  lastName: string
  email: string
  position: string | null
  departmentId: string | null
}

interface Props {
  modelValue?: string | null
  departmentId?: string | null
  placeholder?: string
  required?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  departmentId: null,
  placeholder: 'Wpisz imię lub nazwisko...',
  required: false
})

interface Emits {
  (e: 'update:modelValue', userId: string | null): void
  (e: 'select', user: User | null): void
}

const emit = defineEmits<Emits>()

// State
const searchQuery = ref('')
const results = ref<User[]>([])
const selectedUser = ref<User | null>(null)
const isOpen = ref(false)
const isLoading = ref(false)
const highlightedIndex = ref(-1)

// Refs
const inputRef = ref<HTMLInputElement | null>(null)
const dropdownRef = ref<HTMLDivElement | null>(null)

// Debounced search
let searchTimeout: NodeJS.Timeout | null = null

// Full name computed
const getUserFullName = (user: User): string => {
  return `${user.firstName} ${user.lastName}`
}

// Get initials for avatar
const getUserInitials = (user: User): string => {
  return `${user.firstName[0]}${user.lastName[0]}`.toUpperCase()
}

// Get avatar color based on user ID
const getAvatarColor = (userId: string): string => {
  const colors = [
    'bg-blue-500',
    'bg-green-500',
    'bg-purple-500',
    'bg-orange-500',
    'bg-pink-500',
    'bg-indigo-500',
    'bg-teal-500',
    'bg-red-500'
  ] as const
  // Simple hash function to get consistent color for user
  const hash = userId.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0)
  return colors[hash % colors.length] as string
}

// Search users
const searchUsers = async () => {
  if (!searchQuery.value || searchQuery.value.length < 2) {
    results.value = []
    isOpen.value = false
    return
  }

  isLoading.value = true

  try {
    // Build query params
    const params = new URLSearchParams({
      q: searchQuery.value,
      limit: '10'
    })

    if (props.departmentId) {
      params.append('departmentId', props.departmentId)
    }

    const data = await $fetch<User[]>(`/api/users/search?${params.toString()}`)
    results.value = data
    isOpen.value = true
    highlightedIndex.value = -1
  } catch (err) {
    console.error('Error searching users:', err)
    results.value = []
  } finally {
    isLoading.value = false
  }
}

// Debounced search handler
const handleInput = () => {
  if (searchTimeout) clearTimeout(searchTimeout)

  searchTimeout = setTimeout(() => {
    searchUsers()
  }, 300)
}

// Select user
const selectUser = (user: User) => {
  selectedUser.value = user
  searchQuery.value = ''
  results.value = []
  isOpen.value = false
  emit('update:modelValue', user.id)
  emit('select', user)
}

// Clear selection
const clearSelection = () => {
  selectedUser.value = null
  searchQuery.value = ''
  results.value = []
  isOpen.value = false
  emit('update:modelValue', null)
  emit('select', null)
}

// Close dropdown when clicking outside
const handleClickOutside = (event: MouseEvent) => {
  if (
    inputRef.value &&
    !inputRef.value.contains(event.target as Node) &&
    dropdownRef.value &&
    !dropdownRef.value.contains(event.target as Node)
  ) {
    isOpen.value = false
  }
}

// Keyboard navigation
const handleKeydown = (event: KeyboardEvent) => {
  if (!isOpen.value || results.value.length === 0) return

  switch (event.key) {
    case 'ArrowDown':
      event.preventDefault()
      highlightedIndex.value = Math.min(highlightedIndex.value + 1, results.value.length - 1)
      break

    case 'ArrowUp':
      event.preventDefault()
      highlightedIndex.value = Math.max(highlightedIndex.value - 1, -1)
      break

    case 'Enter':
      event.preventDefault()
      if (highlightedIndex.value >= 0 && highlightedIndex.value < results.value.length) {
        selectUser(results.value[highlightedIndex.value])
      }
      break

    case 'Escape':
      event.preventDefault()
      isOpen.value = false
      highlightedIndex.value = -1
      break
  }
}

// Watch modelValue prop to load selected user
watch(
  () => props.modelValue,
  async (newValue) => {
    if (newValue && !selectedUser.value) {
      // Load user by ID if modelValue is provided but no user is selected
      try {
        const user = await $fetch<User>(`/api/users/${newValue}`)
        selectedUser.value = user
      } catch (err) {
        console.error('Error loading user:', err)
      }
    } else if (!newValue) {
      selectedUser.value = null
    }
  },
  { immediate: true }
)

// Event listeners
onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  if (searchTimeout) clearTimeout(searchTimeout)
})
</script>

<template>
  <div class="user-autocomplete">
    <!-- Search Input (shown when no user is selected) -->
    <div v-if="!selectedUser" class="relative">
      <input
        ref="inputRef"
        v-model="searchQuery"
        type="text"
        :placeholder="placeholder"
        :required="required"
        class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-900 text-gray-900 dark:text-white pr-10"
        @input="handleInput"
        @keydown="handleKeydown"
        @focus="isOpen = results.length > 0"
      >

      <!-- Loading Spinner -->
      <div
        v-if="isLoading"
        class="absolute right-3 top-1/2 -translate-y-1/2"
      >
        <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600" />
      </div>

      <!-- Dropdown Icon -->
      <ChevronDown
        v-else
        class="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400"
      />

      <!-- Dropdown Results -->
      <div
        v-if="isOpen && results.length > 0"
        ref="dropdownRef"
        class="absolute z-20 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg max-h-64 overflow-y-auto"
      >
        <div
          v-for="(user, index) in results"
          :key="user.id"
          class="flex items-center gap-3 p-3 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer transition-colors"
          :class="{
            'bg-gray-100 dark:bg-gray-700': index === highlightedIndex
          }"
          @click="selectUser(user)"
          @mouseenter="highlightedIndex = index"
        >
          <!-- Avatar -->
          <div
            class="w-10 h-10 rounded-full flex items-center justify-center text-white font-semibold text-sm flex-shrink-0"
            :class="getAvatarColor(user.id)"
          >
            {{ getUserInitials(user) }}
          </div>

          <!-- User Info -->
          <div class="flex-1 min-w-0">
            <p class="font-medium text-gray-900 dark:text-white truncate">
              {{ getUserFullName(user) }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400 truncate">
              {{ user.position || 'Brak stanowiska' }}
            </p>
          </div>
        </div>
      </div>

      <!-- No Results -->
      <div
        v-if="isOpen && !isLoading && searchQuery.length >= 2 && results.length === 0"
        ref="dropdownRef"
        class="absolute z-20 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg p-4 text-center"
      >
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Nie znaleziono użytkowników
        </p>
      </div>
    </div>

    <!-- Selected User Display -->
    <div
      v-else
      class="flex items-center gap-3 p-3 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg"
    >
      <!-- Avatar -->
      <div
        class="w-10 h-10 rounded-full flex items-center justify-center text-white font-semibold text-sm flex-shrink-0"
        :class="getAvatarColor(selectedUser.id)"
      >
        {{ getUserInitials(selectedUser) }}
      </div>

      <!-- User Info -->
      <div class="flex-1 min-w-0">
        <p class="font-medium text-gray-900 dark:text-white truncate">
          {{ getUserFullName(selectedUser) }}
        </p>
        <p class="text-sm text-gray-600 dark:text-gray-400 truncate">
          {{ selectedUser.position || 'Brak stanowiska' }}
        </p>
      </div>

      <!-- Clear Button -->
      <button
        type="button"
        class="flex-shrink-0 p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
        @click="clearSelection"
      >
        <X class="w-5 h-5" />
      </button>
    </div>
  </div>
</template>

<style scoped>
.user-autocomplete {
  position: relative;
}
</style>
