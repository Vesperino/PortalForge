<template>
  <div class="fixed inset-0 z-[10002] overflow-y-auto" @click.self="$emit('close')">
    <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
      <!-- Background overlay -->
      <div class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75" @click="$emit('close')"/>

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
                class="text-gray-400 hover:text-gray-500 dark:hover:text-gray-300"
                @click="$emit('close')"
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
                >
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
                />
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
                >
              </div>

              <!-- Icon -->
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Ikona
                </label>
                <InternalServicesServiceIconUpload
                  v-model="form.icon"
                  :max-size-m-b="5"
                />
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
                >
              </div>

              <!-- Checkboxes -->
              <div class="space-y-2">
                <label class="flex items-center">
                  <input
                    v-model="form.isGlobal"
                    type="checkbox"
                    class="rounded border-gray-300 dark:border-gray-600 text-blue-600 focus:ring-blue-500 dark:bg-gray-700"
                  >
                  <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                    Globalny (widoczny dla wszystkich działów)
                  </span>
                </label>

                <label class="flex items-center">
                  <input
                    v-model="form.isPinned"
                    type="checkbox"
                    class="rounded border-gray-300 dark:border-gray-600 text-blue-600 focus:ring-blue-500 dark:bg-gray-700"
                  >
                  <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                    Przypięty (wyświetlany na górze)
                  </span>
                </label>

                <label class="flex items-center">
                  <input
                    v-model="form.isActive"
                    type="checkbox"
                    class="rounded border-gray-300 dark:border-gray-600 text-blue-600 focus:ring-blue-500 dark:bg-gray-700"
                  >
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
                  size="8"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option v-for="dept in departmentsFlat" :key="dept.id" :value="dept.id">
                    {{ dept.level > 0 ? '└─'.repeat(dept.level) + ' ' : '' }}{{ dept.name }}
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
              class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-500"
              @click="$emit('close')"
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
import type { DepartmentTreeDto } from '~/types/department'

const props = defineProps<{
  service?: InternalService | null
  categories: InternalServiceCategory[]
}>()

const emit = defineEmits<{
  (e: 'close' | 'saved'): void
}>()

const config = useRuntimeConfig()
const apiUrl = config.public.apiUrl
const authStore = useAuthStore()
const { createService, updateService } = useInternalServicesApi()
const toast = useNotificationToast()

const isEditing = computed(() => !!props.service)
const saving = ref(false)

const form = reactive({
  name: '',
  description: '',
  url: '',
  icon: '',
  iconType: 'image' as 'emoji' | 'image' | 'font',
  categoryId: undefined as string | undefined,
  displayOrder: 0,
  isActive: true,
  isGlobal: false,
  isPinned: false,
  departmentIds: [] as string[]
})

const departmentsTree = ref<DepartmentTreeDto[]>([])

const getAuthHeaders = (): Record<string, string> | undefined => {
  const token = authStore.accessToken
  if (token) {
    return { Authorization: `Bearer ${token}` }
  }
  return undefined
}

// Flatten departments tree for multi-select
const departmentsFlat = computed(() => {
  const flattenDepartments = (depts: DepartmentTreeDto[], level = 0): Array<DepartmentTreeDto & { level: number }> => {
    return depts.flatMap(dept => [
      { ...dept, level },
      ...flattenDepartments(dept.children || [], level + 1)
    ])
  }
  return flattenDepartments(departmentsTree.value)
})

async function loadDepartments() {
  try {
    const response = await $fetch<DepartmentTreeDto[]>(`${apiUrl}/api/departments/tree`, {
      headers: getAuthHeaders()
    })
    departmentsTree.value = response
  } catch (err: unknown) {
    console.error('Error loading departments:', err)
  }
}

async function handleSubmit() {
  saving.value = true
  try {
    if (isEditing.value && props.service) {
      await updateService(props.service.id, {
        id: props.service.id,
        ...form
      })
      toast.success('Serwis został zaktualizowany')
    } else {
      await createService(form)
      toast.success('Serwis został utworzony')
    }
    emit('saved')
  } catch (err: unknown) {
    toast.error('Nie udało się zapisać serwisu', err instanceof Error ? err.message : 'Nieznany błąd')
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
