<script setup lang="ts">
import { ref, computed } from 'vue'
import { Save, Settings } from 'lucide-vue-next'
import type { RequestTemplateField, RequestTemplate } from '~/types/requests'

interface Props {
  template?: RequestTemplate
}

interface Emits {
  (e: 'save', template: Partial<RequestTemplate>): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const fields = ref<RequestTemplateField[]>(props.template?.fields || [])
const selectedField = ref<RequestTemplateField | null>(null)
const showFieldEditor = ref(false)

const templateData = ref({
  name: props.template?.name || '',
  description: props.template?.description || '',
  category: props.template?.category || '',
  icon: props.template?.icon || '📝',
  requiresApproval: props.template?.requiresApproval ?? true,
  estimatedProcessingDays: props.template?.estimatedProcessingDays || 3,
  isActive: props.template?.isActive ?? true
})

const onFieldsUpdate = (updatedFields: RequestTemplateField[]) => {
  fields.value = updatedFields
}

const onFieldSelected = (field: RequestTemplateField | null) => {
  selectedField.value = field
  if (field) {
    showFieldEditor.value = true
  }
}

const onFieldUpdate = (updatedField: RequestTemplateField) => {
  const index = fields.value.findIndex(f => f.id === updatedField.id)
  if (index !== -1) {
    fields.value[index] = updatedField
  }
  selectedField.value = null
  showFieldEditor.value = false
}

const saveTemplate = () => {
  const template: Partial<RequestTemplate> = {
    ...templateData.value,
    fields: fields.value
  }
  emit('save', template)
}

const isValid = computed(() => {
  return templateData.value.name.trim() !== '' && 
         templateData.value.description.trim() !== '' &&
         fields.value.length > 0
})
</script>

<template>
  <div class="h-screen flex flex-col bg-gray-50 dark:bg-gray-900">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-4">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
            {{ props.template ? 'Edytuj szablon' : 'Nowy szablon wniosku' }}
          </h1>
          <p class="text-gray-600 dark:text-gray-400 mt-1">
            Utwórz dynamiczny formularz z zaawansowanymi funkcjami
          </p>
        </div>
        
        <div class="flex items-center gap-3">
          <button
            @click="saveTemplate"
            :disabled="!isValid"
            :class="[
              'flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition',
              isValid
                ? 'bg-blue-600 hover:bg-blue-700 text-white'
                : 'bg-gray-300 dark:bg-gray-600 text-gray-500 dark:text-gray-400 cursor-not-allowed'
            ]"
          >
            <Save class="w-4 h-4" />
            Zapisz szablon
          </button>
        </div>
      </div>
    </div>

    <!-- Template Settings -->
    <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-4">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Nazwa szablonu
          </label>
          <input
            v-model="templateData.name"
            type="text"
            placeholder="Wprowadź nazwę szablonu"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          />
        </div>
        
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Kategoria
          </label>
          <select
            v-model="templateData.category"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">Wybierz kategorię</option>
            <option value="HR">Zasoby ludzkie</option>
            <option value="IT">Technologie informatyczne</option>
            <option value="Finance">Finanse</option>
            <option value="Operations">Operacje</option>
            <option value="General">Ogólne</option>
          </select>
        </div>
        
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Ikona
          </label>
          <input
            v-model="templateData.icon"
            type="text"
            placeholder="📝"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          />
        </div>
      </div>
      
      <div class="mt-4">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
          Opis
        </label>
        <textarea
          v-model="templateData.description"
          rows="2"
          placeholder="Opisz cel i zastosowanie tego szablonu"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
        />
      </div>
      
      <div class="mt-4 flex items-center gap-6">
        <label class="flex items-center">
          <input
            v-model="templateData.requiresApproval"
            type="checkbox"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Wymaga zatwierdzenia</span>
        </label>
        
        <label class="flex items-center">
          <input
            v-model="templateData.isActive"
            type="checkbox"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Aktywny</span>
        </label>
        
        <div class="flex items-center gap-2">
          <label class="text-sm text-gray-700 dark:text-gray-300">
            Szacowany czas realizacji:
          </label>
          <input
            v-model.number="templateData.estimatedProcessingDays"
            type="number"
            min="1"
            max="30"
            class="w-16 px-2 py-1 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          />
          <span class="text-sm text-gray-700 dark:text-gray-300">dni</span>
        </div>
      </div>
    </div>

    <!-- Form Builder -->
    <div class="flex-1 overflow-hidden">
      <FormBuilder
        :fields="fields"
        @update:fields="onFieldsUpdate"
        @field-selected="onFieldSelected"
      />
    </div>

    <!-- Field Properties Editor Modal -->
    <FieldPropertiesEditor
      :field="selectedField"
      :is-open="showFieldEditor"
      @close="showFieldEditor = false"
      @update:field="onFieldUpdate"
    />
  </div>
</template>