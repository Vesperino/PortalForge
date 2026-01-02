<script setup lang="ts">
import type { User } from '~/types/auth'
import type { FlatDepartment, QuickEditFormData } from '~/composables/useDepartmentStructure'

interface Props {
  isOpen: boolean
  isLoading?: boolean
  user: User | null
  formData: QuickEditFormData
  departments: FlatDepartment[]
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false
})

const emit = defineEmits<{
  'update:formData': [data: QuickEditFormData]
  'save': []
  'close': []
}>()

function handleDepartmentChange(event: Event): void {
  const target = event.target as HTMLSelectElement
  const selectedDeptName = target.value

  const dept = props.departments.find(d => d.name === selectedDeptName)
  if (dept) {
    emit('update:formData', {
      ...props.formData,
      departmentId: dept.id,
      departmentName: dept.name
    })
  }
}

function handlePositionUpdate(positionId: string | null): void {
  emit('update:formData', {
    ...props.formData,
    positionId
  })
}

function handlePositionNameUpdate(positionName: string): void {
  emit('update:formData', {
    ...props.formData,
    position: positionName
  })
}

function handleSave(): void {
  emit('save')
}

function handleClose(): void {
  emit('close')
}

function getInitials(firstName: string, lastName: string): string {
  return `${firstName[0] || ''}${lastName[0] || ''}`.toUpperCase()
}

function getDeptIndent(level: number): string {
  return level > 0 ? '\u2514\u2500'.repeat(level) + ' ' : ''
}
</script>

<template>
  <div
    v-if="isOpen && user"
    class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
  >
    <div class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-lg w-full">
      <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white flex items-center gap-2">
          <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
          Szybka edycja pracownika
        </h2>
        <button
          class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
          data-testid="close-quick-edit"
          @click="handleClose"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div class="p-6 space-y-5">
        <div class="flex items-center gap-4 pb-4 border-b border-gray-200 dark:border-gray-700">
          <div class="w-12 h-12 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white font-semibold text-lg">
            {{ getInitials(user.firstName || '', user.lastName || '') }}
          </div>
          <div>
            <h3 class="text-base font-semibold text-gray-900 dark:text-white">
              {{ user.firstName }} {{ user.lastName }}
            </h3>
            <p class="text-sm text-gray-600 dark:text-gray-400">{{ user.email }}</p>
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Dzial <span class="text-red-500">*</span>
          </label>
          <select
            :value="formData.departmentName"
            required
            class="w-full px-4 py-2.5 border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
            data-testid="department-select"
            @change="handleDepartmentChange"
          >
            <option value="">Wybierz dzial</option>
            <option
              v-for="dept in departments"
              :key="dept.id"
              :value="dept.name"
            >
              {{ getDeptIndent(dept.level) }}{{ dept.name }}
            </option>
          </select>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Stanowisko <span class="text-red-500">*</span>
          </label>
          <CommonPositionAutocomplete
            :model-value="formData.positionId"
            :initial-position-name="formData.position"
            placeholder="Wpisz lub wybierz stanowisko..."
            required
            @update:model-value="handlePositionUpdate"
            @update:position-name="handlePositionNameUpdate"
          />
        </div>
      </div>

      <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
        <button
          class="px-5 py-2.5 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
          @click="handleClose"
        >
          Anuluj
        </button>
        <button
          class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
          :disabled="isLoading || !formData.departmentName || !formData.position"
          data-testid="save-quick-edit"
          @click="handleSave"
        >
          <span v-if="isLoading" class="flex items-center gap-2">
            <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            Zapisywanie...
          </span>
          <span v-else>Zapisz zmiany</span>
        </button>
      </div>
    </div>
  </div>
</template>
