<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Plus, Eye, Settings, Trash2, GripVertical } from 'lucide-vue-next'
import type { RequestTemplateField, FieldType } from '~/types/requests'
import draggable from 'vuedraggable'

interface Props {
  fields: RequestTemplateField[]
  previewMode?: boolean
}

interface Emits {
  (e: 'update:fields', fields: RequestTemplateField[]): void
  (e: 'field-selected', field: RequestTemplateField | null): void
}

const props = withDefaults(defineProps<Props>(), {
  previewMode: false
})

const emit = defineEmits<Emits>()

const localFields = ref<RequestTemplateField[]>([...props.fields])
const selectedField = ref<RequestTemplateField | null>(null)
const showPreview = ref(false)
const dragOptions = {
  animation: 200,
  group: 'form-fields',
  disabled: false,
  ghostClass: 'ghost'
}

// Available field types for the palette
const fieldTypes: Array<{ type: FieldType; label: string; icon: string; description: string }> = [
  { type: 'Text', label: 'Tekst', icon: '📝', description: 'Pojedyncza linia tekstu' },
  { type: 'Textarea', label: 'Obszar tekstu', icon: '📄', description: 'Wieloliniowy tekst' },
  { type: 'Number', label: 'Liczba', icon: '🔢', description: 'Pole numeryczne' },
  { type: 'Select', label: 'Lista wyboru', icon: '📋', description: 'Pojedynczy wybór z listy' },
  { type: 'MultiSelect', label: 'Wielokrotny wybór', icon: '☑️', description: 'Wiele opcji do wyboru' },
  { type: 'Date', label: 'Data', icon: '📅', description: 'Wybór daty' },
  { type: 'DateRange', label: 'Zakres dat', icon: '📆', description: 'Wybór zakresu dat' },
  { type: 'Checkbox', label: 'Pole wyboru', icon: '✅', description: 'Tak/Nie' },
  { type: 'FileUpload', label: 'Plik', icon: '📎', description: 'Przesyłanie plików' },
  { type: 'Rating', label: 'Ocena', icon: '⭐', description: 'Ocena gwiazdkowa' },
  { type: 'Signature', label: 'Podpis', icon: '✍️', description: 'Podpis elektroniczny' },
  { type: 'UserPicker', label: 'Wybór użytkownika', icon: '👤', description: 'Wybór użytkownika z systemu' },
  { type: 'DepartmentPicker', label: 'Wybór działu', icon: '🏢', description: 'Wybór działu organizacji' }
]

// Watch for changes in fields prop
watch(() => props.fields, (newFields) => {
  localFields.value = [...newFields]
}, { deep: true })

// Watch for changes in local fields and emit updates
watch(localFields, (newFields) => {
  emit('update:fields', newFields)
}, { deep: true })

const addField = (fieldType: FieldType) => {
  const newField: RequestTemplateField = {
    id: `field_${Date.now()}`,
    label: `Nowe pole ${fieldType}`,
    fieldType,
    placeholder: '',
    isRequired: false,
    order: localFields.value.length,
    helpText: '',
    validationRules: '{}',
    conditionalLogic: '{}',
    isConditional: false,
    defaultValue: '',
    autoCompleteSource: '',
    fileMaxSize: fieldType === 'FileUpload' ? 10 : undefined,
    allowedFileTypes: fieldType === 'FileUpload' ? '["pdf","doc","docx","jpg","png"]' : undefined
  }
  
  localFields.value.push(newField)
  selectField(newField)
}

const selectField = (field: RequestTemplateField) => {
  selectedField.value = field
  emit('field-selected', field)
}

const removeField = (index: number) => {
  if (selectedField.value?.id === localFields.value[index].id) {
    selectedField.value = null
    emit('field-selected', null)
  }
  localFields.value.splice(index, 1)
  updateFieldOrders()
}

const updateFieldOrders = () => {
  localFields.value.forEach((field, index) => {
    field.order = index
  })
}

const onDragEnd = () => {
  updateFieldOrders()
}

const duplicateField = (field: RequestTemplateField) => {
  const duplicated: RequestTemplateField = {
    ...field,
    id: `field_${Date.now()}`,
    label: `${field.label} (kopia)`,
    order: localFields.value.length
  }
  localFields.value.push(duplicated)
}
</script>

<template>
  <div class="flex h-full bg-gray-50 dark:bg-gray-900">
    <!-- Field Palette -->
    <div class="w-80 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 p-4 overflow-y-auto">
      <div class="mb-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Komponenty formularza
        </h3>
        
        <div class="space-y-2">
          <button
            v-for="fieldType in fieldTypes"
            :key="fieldType.type"
            @click="addField(fieldType.type)"
            class="w-full p-3 text-left bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 rounded-lg border border-gray-200 dark:border-gray-600 transition group"
          >
            <div class="flex items-center gap-3">
              <span class="text-2xl">{{ fieldType.icon }}</span>
              <div class="flex-1">
                <div class="font-medium text-gray-900 dark:text-white">
                  {{ fieldType.label }}
                </div>
                <div class="text-sm text-gray-500 dark:text-gray-400">
                  {{ fieldType.description }}
                </div>
              </div>
              <Plus class="w-5 h-5 text-gray-400 group-hover:text-blue-500" />
            </div>
          </button>
        </div>
      </div>
    </div>

    <!-- Form Builder Area -->
    <div class="flex-1 flex flex-col">
      <!-- Toolbar -->
      <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-4">
        <div class="flex items-center justify-between">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
            Kreator formularza
          </h2>
          <div class="flex items-center gap-2">
            <button
              @click="showPreview = !showPreview"
              :class="[
                'px-4 py-2 rounded-lg border transition',
                showPreview
                  ? 'bg-blue-500 text-white border-blue-500'
                  : 'bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-600'
              ]"
            >
              <Eye class="w-4 h-4 mr-2" />
              {{ showPreview ? 'Edycja' : 'Podgląd' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Form Canvas -->
      <div class="flex-1 p-6 overflow-y-auto">
        <div class="max-w-4xl mx-auto">
          <div v-if="!showPreview" class="space-y-4">
            <!-- Empty State -->
            <div v-if="localFields.length === 0" class="text-center py-12">
              <div class="text-6xl mb-4">📝</div>
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
                Rozpocznij tworzenie formularza
              </h3>
              <p class="text-gray-500 dark:text-gray-400 mb-6">
                Przeciągnij komponenty z palety po lewej stronie lub kliknij na nie, aby dodać do formularza
              </p>
            </div>

            <!-- Draggable Fields -->
            <draggable
              v-model="localFields"
              v-bind="dragOptions"
              @end="onDragEnd"
              item-key="id"
              class="space-y-4"
            >
              <template #item="{ element: field, index }">
                <div
                  :class="[
                    'group relative bg-white dark:bg-gray-800 border-2 rounded-lg p-4 transition cursor-pointer',
                    selectedField?.id === field.id
                      ? 'border-blue-500 shadow-lg'
                      : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600'
                  ]"
                  @click="selectField(field)"
                >
                  <!-- Drag Handle -->
                  <div class="absolute left-2 top-1/2 transform -translate-y-1/2 opacity-0 group-hover:opacity-100 transition">
                    <GripVertical class="w-5 h-5 text-gray-400 cursor-grab" />
                  </div>

                  <!-- Field Content -->
                  <div class="ml-8">
                    <div class="flex items-center justify-between mb-2">
                      <div class="flex items-center gap-2">
                        <span class="text-lg">
                          {{ fieldTypes.find(ft => ft.type === field.fieldType)?.icon }}
                        </span>
                        <span class="font-medium text-gray-900 dark:text-white">
                          {{ field.label }}
                        </span>
                        <span v-if="field.isRequired" class="text-red-500">*</span>
                      </div>
                      
                      <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition">
                        <button
                          @click.stop="duplicateField(field)"
                          class="p-1 text-gray-400 hover:text-blue-500 transition"
                          title="Duplikuj pole"
                        >
                          <Settings class="w-4 h-4" />
                        </button>
                        <button
                          @click.stop="removeField(index)"
                          class="p-1 text-gray-400 hover:text-red-500 transition"
                          title="Usuń pole"
                        >
                          <Trash2 class="w-4 h-4" />
                        </button>
                      </div>
                    </div>

                    <!-- Field Preview -->
                    <div class="text-sm text-gray-500 dark:text-gray-400">
                      {{ fieldTypes.find(ft => ft.type === field.fieldType)?.description }}
                      <span v-if="field.placeholder"> • {{ field.placeholder }}</span>
                    </div>
                  </div>
                </div>
              </template>
            </draggable>
          </div>

          <!-- Preview Mode -->
          <div v-else>
            <FormPreview :fields="localFields" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ghost {
  opacity: 0.5;
  background: #c8ebfb;
}
</style>