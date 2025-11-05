<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { Upload, X, Star, User, Building2, Calendar, FileText } from 'lucide-vue-next'
import type { RequestTemplateField, RequestTemplate } from '~/types/requests'

interface Props {
  template: RequestTemplate
  initialData?: Record<string, any>
  readonly?: boolean
}

interface Emits {
  (e: 'update:formData', data: Record<string, any>): void
  (e: 'validation-change', isValid: boolean): void
}

const props = withDefaults(defineProps<Props>(), {
  readonly: false
})

const emit = defineEmits<Emits>()

const formData = ref<Record<string, any>>({})
const validationErrors = ref<Record<string, string>>({})
const uploadProgress = ref<Record<string, number>>({})
const autoCompleteOptions = ref<Record<string, Array<{ value: string; label: string }>>>({})
const conditionalFieldsVisible = ref<Record<string, boolean>>({})

// Initialize form data
onMounted(() => {
  initializeFormData()
  evaluateConditionalFields()
})

const initializeFormData = () => {
  const data: Record<string, any> = { ...props.initialData }
  
  props.template.fields.forEach(field => {
    const fieldId = field.id || field.Id || ''
    if (!(fieldId in data)) {
      if (field.defaultValue) {
        data[fieldId] = field.defaultValue
      } else {
        switch (field.fieldType) {
          case 'Checkbox':
            data[fieldId] = false
            break
          case 'MultiSelect':
            data[fieldId] = []
            break
          case 'Rating':
            data[fieldId] = 0
            break
          case 'DateRange':
            data[fieldId] = { start: '', end: '' }
            break
          default:
            data[fieldId] = ''
        }
      }
    }
  })
  
  formData.value = data
  emit('update:formData', data)
}

// Watch for form data changes
watch(formData, (newData) => {
  emit('update:formData', newData)
  validateForm()
  evaluateConditionalFields()
}, { deep: true })

const validateForm = () => {
  const errors: Record<string, string> = {}
  let isValid = true

  props.template.fields.forEach(field => {
    const fieldId = field.id || field.Id || ''
    const value = formData.value[fieldId]
    
    // Skip validation for hidden conditional fields
    if (field.isConditional && !conditionalFieldsVisible.value[fieldId]) {
      return
    }

    // Required field validation
    if (field.isRequired && (!value || (Array.isArray(value) && value.length === 0))) {
      errors[fieldId] = 'To pole jest wymagane'
      isValid = false
      return
    }

    // Custom validation rules
    if (field.validationRules && value) {
      try {
        const rules = JSON.parse(field.validationRules)
        
        // Min/Max length validation
        if (rules.MinLength && typeof value === 'string' && value.length < parseInt(rules.MinLength.value)) {
          errors[fieldId] = rules.MinLength.message || `Minimalna długość: ${rules.MinLength.value} znaków`
          isValid = false
        }
        
        if (rules.MaxLength && typeof value === 'string' && value.length > parseInt(rules.MaxLength.value)) {
          errors[fieldId] = rules.MaxLength.message || `Maksymalna długość: ${rules.MaxLength.value} znaków`
          isValid = false
        }
        
        // Regex validation
        if (rules.Regex && typeof value === 'string') {
          const regex = new RegExp(rules.Regex.value)
          if (!regex.test(value)) {
            errors[fieldId] = rules.Regex.message || 'Nieprawidłowy format'
            isValid = false
          }
        }
        
        // Range validation for numbers
        if (rules.Range && typeof value === 'number') {
          const [min, max] = rules.Range.value.split('-').map(Number)
          if (value < min || value > max) {
            errors[fieldId] = rules.Range.message || `Wartość musi być między ${min} a ${max}`
            isValid = false
          }
        }
      } catch (e) {
        console.warn('Invalid validation rules for field:', fieldId)
      }
    }
  })

  validationErrors.value = errors
  emit('validation-change', isValid)
}

const evaluateConditionalFields = () => {
  props.template.fields.forEach(field => {
    const fieldId = field.id || field.Id || ''
    
    if (field.isConditional && field.conditionalLogic) {
      try {
        const logic = JSON.parse(field.conditionalLogic)
        let isVisible = true
        
        // Simple conditional logic evaluation
        if (logic.dependsOn && logic.condition && logic.value) {
          const dependentValue = formData.value[logic.dependsOn]
          
          switch (logic.condition) {
            case 'equals':
              isVisible = dependentValue === logic.value
              break
            case 'not_equals':
              isVisible = dependentValue !== logic.value
              break
            case 'contains':
              isVisible = Array.isArray(dependentValue) && dependentValue.includes(logic.value)
              break
            case 'not_empty':
              isVisible = !!dependentValue
              break
            case 'empty':
              isVisible = !dependentValue
              break
          }
        }
        
        conditionalFieldsVisible.value[fieldId] = isVisible
      } catch (e) {
        conditionalFieldsVisible.value[fieldId] = true
      }
    } else {
      conditionalFieldsVisible.value[fieldId] = true
    }
  })
}

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

const handleFileUpload = async (field: RequestTemplateField, event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  const fieldId = field.id || field.Id || ''
  
  // Validate file size
  if (field.fileMaxSize && file.size > field.fileMaxSize * 1024 * 1024) {
    validationErrors.value[fieldId] = `Plik jest za duży. Maksymalny rozmiar: ${field.fileMaxSize}MB`
    return
  }
  
  // Validate file type
  const allowedTypes = getAllowedFileTypes(field)
  if (allowedTypes.length > 0) {
    const fileExtension = file.name.split('.').pop()?.toLowerCase()
    if (!fileExtension || !allowedTypes.includes(fileExtension)) {
      validationErrors.value[fieldId] = `Niedozwolony typ pliku. Dozwolone: ${allowedTypes.join(', ')}`
      return
    }
  }
  
  // Simulate file upload with progress
  uploadProgress.value[fieldId] = 0
  
  const uploadSimulation = setInterval(() => {
    uploadProgress.value[fieldId] += 10
    if (uploadProgress.value[fieldId] >= 100) {
      clearInterval(uploadSimulation)
      formData.value[fieldId] = {
        name: file.name,
        size: file.size,
        type: file.type,
        url: URL.createObjectURL(file) // In real app, this would be the uploaded file URL
      }
      delete uploadProgress.value[fieldId]
      delete validationErrors.value[fieldId]
    }
  }, 100)
}

const removeFile = (fieldId: string) => {
  formData.value[fieldId] = null
}

const setRating = (fieldId: string, rating: number) => {
  formData.value[fieldId] = rating
}

const searchAutoComplete = async (field: RequestTemplateField, query: string) => {
  if (!field.autoCompleteSource || query.length < 2) return
  
  const fieldId = field.id || field.Id || ''
  
  // Simulate API call for autocomplete
  // In real implementation, this would call the API endpoint specified in field.autoCompleteSource
  const mockOptions = [
    { value: 'option1', label: `${query} - Opcja 1` },
    { value: 'option2', label: `${query} - Opcja 2` },
    { value: 'option3', label: `${query} - Opcja 3` }
  ]
  
  autoCompleteOptions.value[fieldId] = mockOptions
}

const formatFileSize = (bytes: number) => {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

const visibleFields = computed(() => {
  return props.template.fields.filter(field => {
    const fieldId = field.id || field.Id || ''
    return conditionalFieldsVisible.value[fieldId] !== false
  })
})
</script>

<template>
  <div class="space-y-6">
    <div
      v-for="field in visibleFields"
      :key="field.id || field.Id"
      class="space-y-2"
    >
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
      <div v-if="field.fieldType === 'Text'" class="relative">
        <input
          v-model="formData[field.id || field.Id || '']"
          type="text"
          :placeholder="field.placeholder"
          :readonly="readonly"
          :class="[
            'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white',
            validationErrors[field.id || field.Id || '']
              ? 'border-red-300 dark:border-red-600'
              : 'border-gray-300 dark:border-gray-600'
          ]"
          @input="field.autoCompleteSource && searchAutoComplete(field, $event.target.value)"
        />
        
        <!-- Autocomplete dropdown -->
        <div
          v-if="autoCompleteOptions[field.id || field.Id || '']?.length"
          class="absolute z-10 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-md shadow-lg max-h-60 overflow-auto"
        >
          <button
            v-for="option in autoCompleteOptions[field.id || field.Id || '']"
            :key="option.value"
            type="button"
            @click="formData[field.id || field.Id || ''] = option.value; autoCompleteOptions[field.id || field.Id || ''] = []"
            class="w-full px-3 py-2 text-left hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-900 dark:text-white"
          >
            {{ option.label }}
          </button>
        </div>
      </div>

      <!-- Textarea -->
      <textarea
        v-else-if="field.fieldType === 'Textarea'"
        v-model="formData[field.id || field.Id || '']"
        :placeholder="field.placeholder"
        :readonly="readonly"
        rows="4"
        :class="[
          'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white',
          validationErrors[field.id || field.Id || '']
            ? 'border-red-300 dark:border-red-600'
            : 'border-gray-300 dark:border-gray-600'
        ]"
      />

      <!-- Number Input -->
      <input
        v-else-if="field.fieldType === 'Number'"
        v-model.number="formData[field.id || field.Id || '']"
        type="number"
        :placeholder="field.placeholder"
        :readonly="readonly"
        :min="field.minValue"
        :max="field.maxValue"
        :class="[
          'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white',
          validationErrors[field.id || field.Id || '']
            ? 'border-red-300 dark:border-red-600'
            : 'border-gray-300 dark:border-gray-600'
        ]"
      />

      <!-- Select -->
      <select
        v-else-if="field.fieldType === 'Select'"
        v-model="formData[field.id || field.Id || '']"
        :disabled="readonly"
        :class="[
          'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white',
          validationErrors[field.id || field.Id || '']
            ? 'border-red-300 dark:border-red-600'
            : 'border-gray-300 dark:border-gray-600'
        ]"
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
            :id="`${field.id || field.Id}_${option.value}`"
            v-model="formData[field.id || field.Id || '']"
            type="checkbox"
            :value="option.value"
            :disabled="readonly"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <label
            :for="`${field.id || field.Id}_${option.value}`"
            class="ml-2 block text-sm text-gray-900 dark:text-gray-300"
          >
            {{ option.label }}
          </label>
        </div>
      </div>

      <!-- Date -->
      <input
        v-else-if="field.fieldType === 'Date'"
        v-model="formData[field.id || field.Id || '']"
        type="date"
        :readonly="readonly"
        :class="[
          'w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white',
          validationErrors[field.id || field.Id || '']
            ? 'border-red-300 dark:border-red-600'
            : 'border-gray-300 dark:border-gray-600'
        ]"
      />

      <!-- Date Range -->
      <div v-else-if="field.fieldType === 'DateRange'" class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-xs text-gray-500 dark:text-gray-400 mb-1">
            <Calendar class="w-3 h-3 inline mr-1" />
            Data od
          </label>
          <input
            v-model="formData[field.id || field.Id || ''].start"
            type="date"
            :readonly="readonly"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          />
        </div>
        <div>
          <label class="block text-xs text-gray-500 dark:text-gray-400 mb-1">
            <Calendar class="w-3 h-3 inline mr-1" />
            Data do
          </label>
          <input
            v-model="formData[field.id || field.Id || ''].end"
            type="date"
            :readonly="readonly"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          />
        </div>
      </div>

      <!-- Checkbox -->
      <div v-else-if="field.fieldType === 'Checkbox'" class="flex items-center">
        <input
          :id="field.id || field.Id"
          v-model="formData[field.id || field.Id || '']"
          type="checkbox"
          :disabled="readonly"
          class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <label
          :for="field.id || field.Id"
          class="ml-2 block text-sm text-gray-900 dark:text-gray-300"
        >
          {{ field.placeholder || 'Zaznacz to pole' }}
        </label>
      </div>

      <!-- File Upload -->
      <div v-else-if="field.fieldType === 'FileUpload'" class="space-y-3">
        <div
          v-if="!formData[field.id || field.Id || ''] && !readonly"
          class="border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg p-6 text-center hover:border-gray-400 dark:hover:border-gray-500 transition"
        >
          <Upload class="w-8 h-8 mx-auto text-gray-400 mb-2" />
          <div class="text-sm text-gray-600 dark:text-gray-400 mb-2">
            Kliknij lub przeciągnij plik tutaj
          </div>
          <input
            type="file"
            :accept="getAllowedFileTypes(field).map(type => '.' + type).join(',')"
            @change="handleFileUpload(field, $event)"
            class="hidden"
            :id="`file-${field.id || field.Id}`"
          />
          <label
            :for="`file-${field.id || field.Id}`"
            class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-blue-700 bg-blue-100 hover:bg-blue-200 cursor-pointer"
          >
            Wybierz plik
          </label>
          <div class="text-xs text-gray-500 dark:text-gray-400 mt-2">
            <div v-if="field.fileMaxSize">
              Maksymalny rozmiar: {{ formatFileSize(field.fileMaxSize * 1024 * 1024) }}
            </div>
            <div v-if="getAllowedFileTypes(field).length > 0">
              Dozwolone typy: {{ getAllowedFileTypes(field).join(', ') }}
            </div>
          </div>
        </div>

        <!-- Upload Progress -->
        <div
          v-if="uploadProgress[field.id || field.Id || ''] !== undefined"
          class="space-y-2"
        >
          <div class="flex items-center justify-between text-sm">
            <span class="text-gray-600 dark:text-gray-400">Przesyłanie...</span>
            <span class="text-gray-600 dark:text-gray-400">{{ uploadProgress[field.id || field.Id || ''] }}%</span>
          </div>
          <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
            <div
              class="bg-blue-600 h-2 rounded-full transition-all duration-300"
              :style="{ width: `${uploadProgress[field.id || field.Id || '']}%` }"
            />
          </div>
        </div>

        <!-- Uploaded File -->
        <div
          v-if="formData[field.id || field.Id || '']"
          class="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg border border-gray-200 dark:border-gray-600"
        >
          <div class="flex items-center gap-3">
            <FileText class="w-8 h-8 text-blue-500" />
            <div>
              <div class="font-medium text-gray-900 dark:text-white">
                {{ formData[field.id || field.Id || ''].name }}
              </div>
              <div class="text-sm text-gray-500 dark:text-gray-400">
                {{ formatFileSize(formData[field.id || field.Id || ''].size) }}
              </div>
            </div>
          </div>
          <button
            v-if="!readonly"
            @click="removeFile(field.id || field.Id || '')"
            class="p-1 text-gray-400 hover:text-red-500 transition"
          >
            <X class="w-5 h-5" />
          </button>
        </div>
      </div>

      <!-- Rating -->
      <div v-else-if="field.fieldType === 'Rating'" class="flex items-center space-x-1">
        <button
          v-for="star in 5"
          :key="star"
          type="button"
          :disabled="readonly"
          @click="setRating(field.id || field.Id || '', star)"
          :class="[
            'text-2xl transition',
            star <= (formData[field.id || field.Id || ''] || 0)
              ? 'text-yellow-400'
              : 'text-gray-300 dark:text-gray-600',
            !readonly && 'hover:text-yellow-300 cursor-pointer'
          ]"
        >
          <Star class="w-6 h-6" :fill="star <= (formData[field.id || field.Id || ''] || 0) ? 'currentColor' : 'none'" />
        </button>
        <span class="ml-2 text-sm text-gray-600 dark:text-gray-400">
          {{ formData[field.id || field.Id || ''] || 0 }}/5
        </span>
      </div>

      <!-- Signature -->
      <div v-else-if="field.fieldType === 'Signature'" class="space-y-2">
        <div
          v-if="!formData[field.id || field.Id || ''] && !readonly"
          class="w-full h-32 border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-md flex items-center justify-center bg-gray-50 dark:bg-gray-700 cursor-pointer hover:border-gray-400 dark:hover:border-gray-500 transition"
          @click="formData[field.id || field.Id || ''] = 'signature_placeholder'"
        >
          <div class="text-center text-gray-500 dark:text-gray-400">
            <div class="text-2xl mb-2">✍️</div>
            <div class="text-sm">Kliknij, aby dodać podpis</div>
          </div>
        </div>
        <div
          v-else-if="formData[field.id || field.Id || '']"
          class="w-full h-32 border border-gray-300 dark:border-gray-600 rounded-md flex items-center justify-center bg-blue-50 dark:bg-blue-900/20 relative"
        >
          <div class="text-center text-blue-600 dark:text-blue-400">
            <div class="text-2xl mb-2">✍️</div>
            <div class="text-sm font-medium">Podpis dodany</div>
          </div>
          <button
            v-if="!readonly"
            @click="formData[field.id || field.Id || ''] = null"
            class="absolute top-2 right-2 p-1 text-gray-400 hover:text-red-500 transition"
          >
            <X class="w-4 h-4" />
          </button>
        </div>
      </div>

      <!-- User Picker -->
      <div v-else-if="field.fieldType === 'UserPicker'" class="relative">
        <div class="flex items-center">
          <User class="w-5 h-5 text-gray-400 absolute left-3" />
          <select
            v-model="formData[field.id || field.Id || '']"
            :disabled="readonly"
            class="w-full pl-10 pr-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">Wybierz użytkownika...</option>
            <option value="user1">Jan Kowalski</option>
            <option value="user2">Anna Nowak</option>
            <option value="user3">Piotr Wiśniewski</option>
          </select>
        </div>
      </div>

      <!-- Department Picker -->
      <div v-else-if="field.fieldType === 'DepartmentPicker'" class="relative">
        <div class="flex items-center">
          <Building2 class="w-5 h-5 text-gray-400 absolute left-3" />
          <select
            v-model="formData[field.id || field.Id || '']"
            :disabled="readonly"
            class="w-full pl-10 pr-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">Wybierz dział...</option>
            <option value="it">IT</option>
            <option value="hr">HR</option>
            <option value="finance">Finanse</option>
            <option value="marketing">Marketing</option>
          </select>
        </div>
      </div>

      <!-- Validation Error -->
      <p
        v-if="validationErrors[field.id || field.Id || '']"
        class="text-sm text-red-600 dark:text-red-400"
      >
        {{ validationErrors[field.id || field.Id || ''] }}
      </p>
    </div>
  </div>
</template>