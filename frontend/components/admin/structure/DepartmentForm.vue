<script setup lang="ts">
import { computed } from 'vue'
import type { DepartmentTreeDto } from '~/types/department'
import type { User } from '~/types/auth'
import type { DepartmentFormData } from '~/composables/useDepartmentStructure'

interface Props {
  isOpen: boolean
  isLoading?: boolean
  editingDepartment: DepartmentTreeDto | null
  formData: DepartmentFormData
  formErrors: Record<string, string>
  selectedHead: User | null
  selectedDirector: User | null
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false
})

const emit = defineEmits<{
  'update:formData': [data: DepartmentFormData]
  'update:selectedHead': [user: User | null]
  'update:selectedDirector': [user: User | null]
  'save': []
  'close': []
}>()

const localFormData = computed({
  get: () => props.formData,
  set: (value: DepartmentFormData) => emit('update:formData', value)
})

const modalTitle = computed(() =>
  props.editingDepartment ? 'Edytuj dzial' : 'Dodaj nowy dzial'
)

const saveButtonText = computed(() =>
  props.editingDepartment ? 'Zapisz zmiany' : 'Utworz dzial'
)

function handleHeadSelected(user: User | null): void {
  emit('update:selectedHead', user)
  localFormData.value = {
    ...localFormData.value,
    departmentHeadId: user?.id || null
  }
}

function handleDirectorSelected(user: User | null): void {
  emit('update:selectedDirector', user)
  localFormData.value = {
    ...localFormData.value,
    departmentDirectorId: user?.id || null
  }
}

function handleNameInput(event: Event): void {
  const target = event.target as HTMLInputElement
  localFormData.value = {
    ...localFormData.value,
    name: target.value
  }
}

function handleDescriptionInput(event: Event): void {
  const target = event.target as HTMLTextAreaElement
  localFormData.value = {
    ...localFormData.value,
    description: target.value || null
  }
}

function handleSave(): void {
  emit('save')
}

function handleClose(): void {
  emit('close')
}
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
  >
    <div class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-lg w-full max-h-[90vh] overflow-y-auto">
      <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white flex items-center gap-2">
          <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
          {{ modalTitle }}
        </h2>
        <button
          class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
          data-testid="close-modal"
          @click="handleClose"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div class="p-6 space-y-5">
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Nazwa dzialu <span class="text-red-500">*</span>
          </label>
          <input
            :value="formData.name"
            type="text"
            class="w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
            :class="{
              'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white': !formErrors.name,
              'border-red-500 dark:border-red-500 bg-red-50 dark:bg-red-900/20': formErrors.name
            }"
            placeholder="np. Dzial IT, Marketing, HR"
            data-testid="department-name-input"
            @input="handleNameInput"
          >
          <p v-if="formErrors.name" class="mt-2 text-sm text-red-600 dark:text-red-400 flex items-center gap-1">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {{ formErrors.name }}
          </p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Opis dzialu
          </label>
          <textarea
            :value="formData.description || ''"
            rows="3"
            class="w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
            :class="{
              'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white': !formErrors.description,
              'border-red-500 dark:border-red-500 bg-red-50 dark:bg-red-900/20': formErrors.description
            }"
            placeholder="Opcjonalny opis dzialu i jego zadan"
            data-testid="department-description-input"
            @input="handleDescriptionInput"
          />
          <p v-if="formErrors.description" class="mt-2 text-sm text-red-600 dark:text-red-400 flex items-center gap-1">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {{ formErrors.description }}
          </p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Kierownik dzialu
          </label>
          <CommonUserAutocomplete
            :selected-user="selectedHead"
            placeholder="Wyszukaj kierownika po imieniu, nazwisku lub email"
            @update:selected-user="handleHeadSelected"
          />
          <p class="mt-2 text-xs text-gray-500 dark:text-gray-400 flex items-center gap-1">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Zacznij wpisywac imie lub nazwisko aby wyszukac pracownika
          </p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Dyrektor dzialu (opcjonalnie)
          </label>
          <CommonUserAutocomplete
            :selected-user="selectedDirector"
            placeholder="Wyszukaj dyrektora..."
            @update:selected-user="handleDirectorSelected"
          />
          <p class="mt-2 text-xs text-gray-500 dark:text-gray-400">
            Mozesz pozostawic puste.
          </p>
        </div>
      </div>

      <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
        <button
          class="px-5 py-2.5 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
          data-testid="cancel-button"
          @click="handleClose"
        >
          Anuluj
        </button>
        <button
          class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
          :disabled="isLoading"
          data-testid="save-button"
          @click="handleSave"
        >
          <span v-if="isLoading" class="flex items-center gap-2">
            <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            Zapisywanie...
          </span>
          <span v-else>{{ saveButtonText }}</span>
        </button>
      </div>
    </div>
  </div>
</template>
