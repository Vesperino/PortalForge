<template>
  <div class="fixed inset-0 z-50 overflow-y-auto" @click.self="$emit('close')">
    <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
      <!-- Background overlay -->
      <div class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75" @click="$emit('close')"></div>

      <!-- Modal panel -->
      <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full">
        <form @submit.prevent="handleSubmit">
          <!-- Header -->
          <div class="bg-gray-50 dark:bg-gray-700 px-6 py-4 border-b border-gray-200 dark:border-gray-600">
            <div class="flex items-center justify-between">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white">
                {{ isEditing ? 'Edytuj Serwis' : 'Dodaj Nowy Serwis' }}
              </h3>
              <button
                type="button"
                @click="$emit('close')"
                class="text-gray-400 hover:text-gray-500 dark:hover:text-gray-300"
              >
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Body -->
          <div class="px-6 py-4 max-h-[70vh] overflow-y-auto">
            <div class="space-y-4">
              <!-- Name -->
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Nazwa <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="form.name"
                  type="text"
                  required
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="np. System HR"
                />
              </div>

              <!-- Description -->
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Opis
                </label>
                <textarea
                  v-model="form.description"
                  rows="3"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Krótki opis serwisu..."
                ></textarea>
              </div>

              <!-- URL -->
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  URL <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="form.url"
                  type="url"
                  required
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="https://example.com"
                />
              </div>

              <!-- Icon -->
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Ikona
                </label>
                <div class="flex gap-2">
                  <select
                    v-model="form.iconType"
                    class="px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    <option value="emoji">Emoji</option>
                    <option value="font">Font Icon</option>
                    <option value="image">Obrazek</option>
                  </select>
                  <input
                    v-model="form.icon"
                    type="text"
                    class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    :placeholder="iconPlaceholder"
                  />
                </div>
                <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
                  {{ iconHelpText }}
                </p>
              </div>

              <!-- Category -->
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Kategoria
                </label>
                <select
                  v-model="form.categoryId"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option :value="undefined">Brak kategorii</option>
                  <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
                </select>
              </div>

              <!-- Display Order -->
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Kolejność wyświetlania
                </label>
                <input
                  v-model.number="form.displayOrder"
                  type="number"
                  min="0"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <!-- Checkboxes -->
              <div class="space-y-2">
                <label class="flex items-center">
                  <input
                    v-model="form.isGlobal"
                    type="checkbox"
                    class="rounded border-gray-300 dark:border-gray-600 text-blue-600 focus:ring-blue-500 dark:bg-gray-700"
                  />
                  <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                    Globalny (widoczny dla wszystkich działów)
                  </span>
                </label>

                <label class="flex items-center">
                  <input
                    v-model="form.isPinned"
                    type="checkbox"
                    class="rounded border-gray-300 dark:border-gray-600 text-blue-600 focus:ring-blue-500 dark:bg-gray-700"
                  />
                  <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                    Przypięty (wyświetlany na górze)
                  </span>
                </label>

                <label class="flex items-center">
                  <input
                    v-model="form.isActive"
                    type="checkbox"
                    class="rounded border-gray-300 dark:border-gray-600 text-blue-600 focus:ring-blue-500 dark:bg-gray-700"
                  />
                  <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                    Aktywny
                  </span>
                </label>
              </div>

              <!-- Department Selection (if not global) -->
              <div v-if="!form.isGlobal">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Działy <span class="text-red-500">*</span>
                </label>
                <select
                  v-model="form.departmentIds"
                  multiple
                  size="5"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option v-for="dept in departments" :key="dept.id" :value="dept.id">
                    {{ dept.name }}
                  </option>
                </select>
                <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
                  Przytrzymaj Ctrl/Cmd aby zaznaczyć wiele działów
                </p>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="bg-gray-50 dark:bg-gray-700 px-6 py-4 border-t border-gray-200 dark:border-gray-600 flex justify-end gap-3">
            <button
              type="button"
              @click="$emit('close')"
              class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-500"
            >
              Anuluj
            </button>
            <button
              type="submit"
              :disabled="saving"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 dark:bg-blue-500 rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ saving ? 'Zapisywanie...' : (isEditing ? 'Zapisz zmiany' : 'Utwórz serwis') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { InternalService, InternalServiceCategory } from '~/types/internal-services'
import type { Department } from '~/types'

const props = defineProps<{
  service?: InternalService | null
  categories: InternalServiceCategory[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'saved'): void
}>()

const { createService, updateService } = useInternalServicesApi()

const isEditing = computed(() => !!props.service)
const saving = ref(false)

const form = reactive({
  name: '',
  description: '',
  url: '',
  icon: '',
  iconType: 'emoji' as 'emoji' | 'image' | 'font',
  categoryId: undefined as string | undefined,
  displayOrder: 0,
  isActive: true,
  isGlobal: false,
  isPinned: false,
  departmentIds: [] as string[]
})

const departments = ref<Department[]>([])

const iconPlaceholder = computed(() => {
  switch (form.iconType) {
    case 'emoji':
      return '🔗'
    case 'font':
      return 'fas fa-link'
    case 'image':
      return 'https://example.com/icon.png'
    default:
      return ''
  }
})

const iconHelpText = computed(() => {
  switch (form.iconType) {
    case 'emoji':
      return 'Wklej emoji, np. 🔗 📧 📅'
    case 'font':
      return 'Klasa Font Awesome, np. fas fa-link'
    case 'image':
      return 'URL do obrazka'
    default:
      return ''
  }
})

async function loadDepartments() {
  // TODO: Load departments from API
  departments.value = []
}

async function handleSubmit() {
  saving.value = true
  try {
    if (isEditing.value && props.service) {
      await updateService(props.service.id, {
        id: props.service.id,
        ...form
      })
    } else {
      await createService(form)
    }
    emit('saved')
  } catch (err: any) {
    alert(`Błąd: ${err.message || 'Nie udało się zapisać serwisu'}`)
  } finally {
    saving.value = false
  }
}

// Initialize form with service data if editing
watchEffect(() => {
  if (props.service) {
    form.name = props.service.name
    form.description = props.service.description
    form.url = props.service.url
    form.icon = props.service.icon || ''
    form.iconType = props.service.iconType
    form.categoryId = props.service.categoryId
    form.displayOrder = props.service.displayOrder
    form.isActive = props.service.isActive
    form.isGlobal = props.service.isGlobal
    form.isPinned = props.service.isPinned
    form.departmentIds = props.service.departmentIds
  }
})

onMounted(() => {
  loadDepartments()
})
</script>
