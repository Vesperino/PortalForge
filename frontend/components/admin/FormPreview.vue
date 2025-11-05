<script setup lang="ts">
import { ref, computed } from 'vue'
import type { RequestTemplateField } from '~/types/requests'

interface Props {
  fields: RequestTemplateField[]
}

const props = defineProps<Props>()

const formData = ref<Record<string, any>>({})

// Initialize form data with default values
const initializeFormData = () => {
  const data: Record<string, any> = {}
  props.fields.forEach(field => {
    if (field.defaultValue) {
      data[field.id || ''] = field.defaultValue
    } else {
      switch (field.fieldType) {
        case 'Checkbox':
          data[field.id || ''] = false
          break
        case 'MultiSelect':
          data[field.id || ''] = []
          break
        case 'Rating':
          data[field.id || ''] = 0
          break
        default:
          data[field.id || ''] = ''
      }
    }
  })
  formData.value = data
}

// Initialize form data when component mounts
initializeFormData()

const getSelectOptions = (field: RequestTemplateField) => {
  try {
    return field.options ? JSON.parse(field.options) : []
  } catch {
    return []
  }
}

const getAllowedFileTypes = (field: RequestTemplateField) => {
  try {
    return field.allowedFileTypes ? JSON.parse(field.allowedFileTypes) : []
  } catch {
    return []
  }
}

const formatFileSize = (bytes: number) => {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}
</script>

<template>
  <div class="max-w-2xl mx-auto bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6">
    <div class="mb-6">
      <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
        Podgląd formularza
      </h2>
      <p class="text-gray-600 dark:text-gray-400">
        Tak będzie wyglądał formularz dla użytkowników
      </p>
    </div>

    <form class="space-y-6" @submit.prevent>
      <div v-for="field in fields" :key="field.id" class="space-y-2">
        <!-- Field Label -->
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">
          {{ field.label }}
          <span v-if="field.isRequired" class="text-red-500 ml-1">*</span>
        </label>

        <!-- Help Text -->
        <p v-if="field.helpText" class="text-sm text-gray-500 dark:text-gray-400">
          {{ field.helpText }}
        </p>

        <!-- Text Input -->
        <input
          v-if="field.fieldType === 'Text'"
          v-model="formData[field.id || '']"
          type="text"
          :placeholder="field.placeholder"
          :required="field.isRequired"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
        />

        <!-- Textarea -->
        <textarea
          v-else-if="field.fieldType === 'Textarea'"
          v-model="formData[field.id || '']"
          :placeholder="field.placeholder"
          :required="field.isRequired"
          rows="4"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
        />

        <!-- Number Input -->
        <input
          v-else-if="field.fieldType === 'Number'"
          v-model.number="formData[field.id || '']"
          type="number"
          :placeholder="field.placeholder"
          :required="field.isRequired"
          :min="field.minValue"
          :max="field.maxValue"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
        />

        <!-- Select -->
        <select
          v-else-if="field.fieldType === 'Select'"
          v-model="formData[field.id || '']"
          :required="field.isRequired"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
        >
          <option value="">{{ field.placeholder || 'Wybierz opcję...' }}</option>
          <option
            v-for="option in getSelectOptions(field)"
            :key="option.value"
            :value="option.value"
          >
            {{ option.label }}
          </option>
        </select>

        <!-- Multi Select -->
        <div v-else-if="field.fieldType === 'MultiSelect'" class="space-y-2">
          <div
            v-for="option in getSelectOptions(field)"
            :key="option.value"
            class="flex items-center"
          >
            <input
              :id="`${field.id}_${option.value}`"
              v-model="formData[field.id || '']"
              type="checkbox"
              :value="option.value"
              class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label
              :for="`${field.id}_${option.value}`"
              class="ml-2 block text-sm text-gray-900 dark:text-gray-300"
            >
              {{ option.label }}
            </label>
          </div>
        </div>

        <!-- Date -->
        <input
          v-else-if="field.fieldType === 'Date'"
          v-model="formData[field.id || '']"
          type="date"
          :required="field.isRequired"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
        />

        <!-- Date Range -->
        <div v-else-if="field.fieldType === 'DateRange'" class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-xs text-gray-500 dark:text-gray-400 mb-1">Od</label>
            <input
              v-model="formData[field.id + '_start' || '']"
              type="date"
              :required="field.isRequired"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
          <div>
            <label class="block text-xs text-gray-500 dark:text-gray-400 mb-1">Do</label>
            <input
              v-model="formData[field.id + '_end' || '']"
              type="date"
              :required="field.isRequired"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>

        <!-- Checkbox -->
        <div v-else-if="field.fieldType === 'Checkbox'" class="flex items-center">
          <input
            :id="field.id"
            v-model="formData[field.id || '']"
            type="checkbox"
            :required="field.isRequired"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <label
            :for="field.id"
            class="ml-2 block text-sm text-gray-900 dark:text-gray-300"
          >
            {{ field.placeholder || 'Zaznacz to pole' }}
          </label>
        </div>

        <!-- File Upload -->
        <div v-else-if="field.fieldType === 'FileUpload'" class="space-y-2">
          <input
            type="file"
            :required="field.isRequired"
            :accept="getAllowedFileTypes(field).map(type => '.' + type).join(',')"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          />
          <div class="text-xs text-gray-500 dark:text-gray-400">
            <div v-if="field.fileMaxSize">
              Maksymalny rozmiar: {{ formatFileSize(field.fileMaxSize * 1024 * 1024) }}
            </div>
            <div v-if="getAllowedFileTypes(field).length > 0">
              Dozwolone typy: {{ getAllowedFileTypes(field).join(', ') }}
            </div>
          </div>
        </div>

        <!-- Rating -->
        <div v-else-if="field.fieldType === 'Rating'" class="flex items-center space-x-1">
          <button
            v-for="star in 5"
            :key="star"
            type="button"
            @click="formData[field.id || ''] = star"
            :class="[
              'text-2xl transition',
              star <= (formData[field.id || ''] || 0)
                ? 'text-yellow-400'
                : 'text-gray-300 dark:text-gray-600 hover:text-yellow-300'
            ]"
          >
            ⭐
          </button>
          <span class="ml-2 text-sm text-gray-600 dark:text-gray-400">
            {{ formData[field.id || ''] || 0 }}/5
          </span>
        </div>

        <!-- Signature -->
        <div v-else-if="field.fieldType === 'Signature'" class="space-y-2">
          <div class="w-full h-32 border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-md flex items-center justify-center bg-gray-50 dark:bg-gray-700">
            <div class="text-center text-gray-500 dark:text-gray-400">
              <div class="text-2xl mb-2">✍️</div>
              <div class="text-sm">Kliknij, aby dodać podpis</div>
            </div>
          </div>
        </div>

        <!-- User Picker -->
        <div v-else-if="field.fieldType === 'UserPicker'">
          <select
            v-model="formData[field.id || '']"
            :required="field.isRequired"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">Wybierz użytkownika...</option>
            <option value="user1">Jan Kowalski</option>
            <option value="user2">Anna Nowak</option>
            <option value="user3">Piotr Wiśniewski</option>
          </select>
        </div>

        <!-- Department Picker -->
        <div v-else-if="field.fieldType === 'DepartmentPicker'">
          <select
            v-model="formData[field.id || '']"
            :required="field.isRequired"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">Wybierz dział...</option>
            <option value="it">IT</option>
            <option value="hr">HR</option>
            <option value="finance">Finanse</option>
            <option value="marketing">Marketing</option>
          </select>
        </div>
      </div>

      <!-- Submit Button -->
      <div class="pt-6 border-t border-gray-200 dark:border-gray-700">
        <button
          type="submit"
          class="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition"
        >
          Wyślij wniosek
        </button>
      </div>
    </form>
  </div>
</template>