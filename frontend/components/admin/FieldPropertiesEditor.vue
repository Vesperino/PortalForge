<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { X, Plus, Trash2 } from 'lucide-vue-next'
import type { RequestTemplateField, FieldType, ValidationType } from '~/types/requests'

interface Props {
  field: RequestTemplateField | null
  isOpen: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'update:field', field: RequestTemplateField): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const localField = ref<RequestTemplateField | null>(null)
const options = ref<Array<{ value: string; label: string }>>([])
const validationRules = ref<Array<{ type: ValidationType; value: string; message: string }>>([])

// Available validation types
const validationTypes: Array<{ type: ValidationType; label: string; hasValue: boolean }> = [
  { type: 'Required', label: 'Wymagane', hasValue: false },
  { type: 'MinLength', label: 'Minimalna długość', hasValue: true },
  { type: 'MaxLength', label: 'Maksymalna długość', hasValue: true },
  { type: 'Regex', label: 'Wyrażenie regularne', hasValue: true },
  { type: 'Range', label: 'Zakres wartości', hasValue: true },
  { type: 'FileSize', label: 'Rozmiar pliku (MB)', hasValue: true },
  { type: 'FileType', label: 'Typ pliku', hasValue: true }
]

// Watch for field changes
watch(() => props.field, (newField) => {
  if (newField) {
    localField.value = { ...newField }
    loadOptions()
    loadValidationRules()
  }
}, { immediate: true, deep: true })

const loadOptions = () => {
  if (localField.value?.options) {
    try {
      options.value = JSON.parse(localField.value.options)
    } catch {
      options.value = []
    }
  } else {
    options.value = []
  }
}

const loadValidationRules = () => {
  if (localField.value?.validationRules) {
    try {
      const rules = JSON.parse(localField.value.validationRules)
      validationRules.value = Object.entries(rules).map(([type, config]: [string, any]) => ({
        type: type as ValidationType,
        value: config.value || '',
        message: config.message || ''
      }))
    } catch {
      validationRules.value = []
    }
  } else {
    validationRules.value = []
  }
}

const saveField = () => {
  if (!localField.value) return

  // Save options
  if (needsOptions.value) {
    localField.value.options = JSON.stringify(options.value)
  }

  // Save validation rules
  const rulesObject: Record<string, any> = {}
  validationRules.value.forEach(rule => {
    rulesObject[rule.type] = {
      value: rule.value,
      message: rule.message
    }
  })
  localField.value.validationRules = JSON.stringify(rulesObject)

  emit('update:field', localField.value)
  emit('close')
}

const addOption = () => {
  options.value.push({ value: '', label: '' })
}

const removeOption = (index: number) => {
  options.value.splice(index, 1)
}

const addValidationRule = () => {
  validationRules.value.push({ type: 'Required', value: '', message: '' })
}

const removeValidationRule = (index: number) => {
  validationRules.value.splice(index, 1)
}

const needsOptions = computed(() => {
  return localField.value?.fieldType === 'Select' || localField.value?.fieldType === 'MultiSelect'
})

const needsFileSettings = computed(() => {
  return localField.value?.fieldType === 'FileUpload'
})

const needsNumberSettings = computed(() => {
  return localField.value?.fieldType === 'Number' || localField.value?.fieldType === 'Rating'
})
</script>

<template>
  <div
    v-if="isOpen && localField"
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
    @click.self="emit('close')"
  >
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
      <!-- Header -->
      <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
          Właściwości pola: {{ localField.label }}
        </h2>
        <button
          @click="emit('close')"
          class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
        >
          <X class="w-6 h-6" />
        </button>
      </div>

      <!-- Content -->
      <div class="p-6 space-y-6">
        <!-- Basic Properties -->
        <div class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 dark:text-white">Podstawowe właściwości</h3>
          
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Etykieta pola
              </label>
              <input
                v-model="localField.label"
                type="text"
                class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Placeholder
              </label>
              <input
                v-model="localField.placeholder"
                type="text"
                class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Tekst pomocy
            </label>
            <textarea
              v-model="localField.helpText"
              rows="2"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Wartość domyślna
            </label>
            <input
              v-model="localField.defaultValue"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>

          <div class="flex items-center space-x-4">
            <label class="flex items-center">
              <input
                v-model="localField.isRequired"
                type="checkbox"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Pole wymagane</span>
            </label>

            <label class="flex items-center">
              <input
                v-model="localField.isConditional"
                type="checkbox"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Pole warunkowe</span>
            </label>
          </div>
        </div>

        <!-- Options for Select/MultiSelect -->
        <div v-if="needsOptions" class="space-y-4">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium text-gray-900 dark:text-white">Opcje wyboru</h3>
            <button
              @click="addOption"
              class="flex items-center gap-2 px-3 py-1 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
            >
              <Plus class="w-4 h-4" />
              Dodaj opcję
            </button>
          </div>

          <div class="space-y-2">
            <div
              v-for="(option, index) in options"
              :key="index"
              class="flex items-center gap-2"
            >
              <input
                v-model="option.value"
                type="text"
                placeholder="Wartość"
                class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
              <input
                v-model="option.label"
                type="text"
                placeholder="Etykieta"
                class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
              <button
                @click="removeOption(index)"
                class="p-2 text-red-500 hover:text-red-700 transition"
              >
                <Trash2 class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        <!-- Number Settings -->
        <div v-if="needsNumberSettings" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 dark:text-white">Ustawienia numeryczne</h3>
          
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Wartość minimalna
              </label>
              <input
                v-model.number="localField.minValue"
                type="number"
                class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Wartość maksymalna
              </label>
              <input
                v-model.number="localField.maxValue"
                type="number"
                class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>
          </div>
        </div>

        <!-- File Settings -->
        <div v-if="needsFileSettings" class="space-y-4">
          <h3 class="text-lg font-medium text-gray-900 dark:text-white">Ustawienia plików</h3>
          
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Maksymalny rozmiar (MB)
              </label>
              <input
                v-model.number="localField.fileMaxSize"
                type="number"
                min="1"
                max="100"
                class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Dozwolone typy (oddzielone przecinkami)
              </label>
              <input
                v-model="localField.allowedFileTypes"
                type="text"
                placeholder="pdf,doc,docx,jpg,png"
                class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>
          </div>
        </div>

        <!-- Validation Rules -->
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-medium text-gray-900 dark:text-white">Reguły walidacji</h3>
            <button
              @click="addValidationRule"
              class="flex items-center gap-2 px-3 py-1 text-sm bg-green-600 text-white rounded-md hover:bg-green-700 transition"
            >
              <Plus class="w-4 h-4" />
              Dodaj regułę
            </button>
          </div>

          <div class="space-y-3">
            <div
              v-for="(rule, index) in validationRules"
              :key="index"
              class="flex items-center gap-2 p-3 bg-gray-50 dark:bg-gray-700 rounded-md"
            >
              <select
                v-model="rule.type"
                class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-600 dark:text-white"
              >
                <option
                  v-for="validationType in validationTypes"
                  :key="validationType.type"
                  :value="validationType.type"
                >
                  {{ validationType.label }}
                </option>
              </select>
              
              <input
                v-if="validationTypes.find(vt => vt.type === rule.type)?.hasValue"
                v-model="rule.value"
                type="text"
                placeholder="Wartość"
                class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-600 dark:text-white"
              />
              
              <input
                v-model="rule.message"
                type="text"
                placeholder="Komunikat błędu"
                class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-600 dark:text-white"
              />
              
              <button
                @click="removeValidationRule(index)"
                class="p-2 text-red-500 hover:text-red-700 transition"
              >
                <Trash2 class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700">
        <button
          @click="emit('close')"
          class="px-4 py-2 text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-md transition"
        >
          Anuluj
        </button>
        <button
          @click="saveField"
          class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md transition"
        >
          Zapisz zmiany
        </button>
      </div>
    </div>
  </div>
</template>