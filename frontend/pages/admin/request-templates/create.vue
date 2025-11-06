<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <NuxtLink
          to="/admin/request-templates"
          class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-700 mb-4"
        >
          <ArrowLeft class="w-4 h-4 mr-2" />
          Powrót do listy
        </NuxtLink>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
          Utwórz szablon wniosku
        </h1>
        <p class="text-gray-600 dark:text-gray-400">
          Zdefiniuj nowy szablon wniosku z polami, przepływem zatwierdzeń i opcjonalnym quizem
        </p>
      </div>

      <!-- Progress Steps -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <button
            v-for="(step, index) in steps"
            :key="index"
            :class="[
              'flex-1 py-3 px-4 text-sm font-medium transition-colors',
              currentStep === index
                ? 'bg-blue-600 text-white'
                : currentStep > index
                ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                : 'bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400',
              index === 0 ? 'rounded-l-lg' : '',
              index === steps.length - 1 ? 'rounded-r-lg' : 'border-r border-gray-300 dark:border-gray-600'
            ]"
            @click="currentStep = index"
          >
            {{ step }}
          </button>
        </div>
      </div>

      <!-- Form -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <!-- Step 1: Basic Info -->
        <div v-if="currentStep === 0" class="space-y-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Nazwa szablonu *
            </label>
            <input
              v-model="form.name"
              type="text"
              required
              placeholder="np. Zamówienie sprzętu IT"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            >
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Opis *
            </label>
            <textarea
              v-model="form.description"
              rows="3"
              required
              placeholder="Opisz cel i zastosowanie tego szablonu..."
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Kategoria *
            </label>
            <input
              v-model="form.category"
              type="text"
              required
              placeholder="np. Hardware, Software, HR"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            >
          </div>

          <div>
            <IconPicker v-model="form.icon" />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Dział (opcjonalne)
            </label>
            <input
              v-model="form.departmentId"
              type="text"
              placeholder="Zostaw puste dla wszystkich działów"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            >
            <p class="mt-1 text-sm text-gray-500">
              Jeśli uzupełnisz, szablon będzie dostępny tylko dla tego działu
            </p>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Szacowany czas procesowania (dni)
            </label>
            <input
              v-model.number="form.estimatedProcessingDays"
              type="number"
              min="1"
              placeholder="7"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            >
          </div>

          <div class="flex items-center">
            <input
              id="requiresApproval"
              v-model="form.requiresApproval"
              type="checkbox"
              class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
            >
            <label for="requiresApproval" class="ml-2 text-sm text-gray-700 dark:text-gray-300">
              Wymaga zatwierdzenia
            </label>
          </div>

          <div class="flex items-center">
            <input
              id="requiresSubstituteSelection"
              v-model="form.requiresSubstituteSelection"
              type="checkbox"
              class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
            >
            <label for="requiresSubstituteSelection" class="ml-2 text-sm text-gray-700 dark:text-gray-300">
              Wymaga wyboru zastępcy (np. dla wniosków urlopowych)
            </label>
          </div>

          <div v-if="form.requiresSubstituteSelection" class="mt-4 p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
            <p class="text-sm text-blue-800 dark:text-blue-300">
              <strong>Info:</strong> Pole "Wybierz zastępcę" zostanie automatycznie dodane do formularza.
            </p>
          </div>
        </div>

        <!-- Step 2: Form Fields -->
        <div v-else-if="currentStep === 1" class="space-y-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
              Pola formularza
            </h3>
            <button
              class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
              @click="addField"
            >
              <Plus class="w-4 h-4 mr-2" />
              Dodaj pole
            </button>
          </div>

          <div v-if="form.fields.length === 0" class="text-center py-8 text-gray-500">
            Brak pól. Kliknij "Dodaj pole" aby rozpocząć.
          </div>

          <draggable
            v-model="form.fields"
            item-key="order"
            handle=".drag-handle"
            class="space-y-4"
          >
            <template #item="{ element: field, index }">
              <div class="bg-gray-50 dark:bg-gray-700/50 border border-gray-200 dark:border-gray-600 rounded-lg p-4">
                <div class="flex items-start gap-4">
                  <div class="drag-handle cursor-move mt-2">
                    <GripVertical class="w-5 h-5 text-gray-400" />
                  </div>
                  
                  <div class="flex-1 space-y-3">
                    <div class="grid grid-cols-2 gap-4">
                      <div>
                        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Etykieta
                        </label>
                        <input
                          v-model="field.label"
                          type="text"
                          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
                        >
                      </div>
                      <div>
                        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Typ pola
                        </label>
                        <select
                          v-model="field.fieldType"
                          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
                        >
                          <option value="Text">Tekst</option>
                          <option value="Textarea">Pole tekstowe</option>
                          <option value="Number">Liczba</option>
                          <option value="Select">Lista wyboru</option>
                          <option value="Date">Data</option>
                          <option value="Checkbox">Checkbox</option>
                        </select>
                      </div>
                    </div>

                    <div v-if="field.fieldType !== 'Select' && field.fieldType !== 'Checkbox'">
                      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        Placeholder
                      </label>
                      <input
                        v-model="field.placeholder"
                        type="text"
                        :placeholder="field.fieldType === 'Date' ? 'np. Wybierz datę' : 'Wpisz tekst pomocniczy...'"
                        class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
                      >
                    </div>

                    <!-- Options for Select and Checkbox -->
                    <div v-if="field.fieldType === 'Select' || field.fieldType === 'Checkbox'" class="space-y-2">
                      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                        Opcje do wyboru
                      </label>
                      <div v-if="!field.options || field.options.length === 0" class="text-sm text-gray-500">
                        Brak opcji. Dodaj opcje poniżej.
                      </div>
                      <div v-for="(option, optIndex) in field.options || []" :key="optIndex" class="flex items-center gap-2">
                        <input
                          v-model="option.label"
                          type="text"
                          placeholder="Etykieta opcji"
                          class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white text-sm"
                        >
                        <input
                          v-model="option.value"
                          type="text"
                          placeholder="Wartość"
                          class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white text-sm"
                        >
                        <button
                          class="text-red-600 hover:text-red-700"
                          @click="removeFieldOption(index, optIndex)"
                        >
                          <X class="w-4 h-4" />
                        </button>
                      </div>
                      <button
                        class="text-sm text-blue-600 hover:text-blue-700"
                        @click="addFieldOption(index)"
                      >
                        + Dodaj opcję
                      </button>
                    </div>

                    <div class="flex items-center gap-4">
                      <label class="flex items-center">
                        <input
                          v-model="field.isRequired"
                          type="checkbox"
                          class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                        >
                        <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                          Pole wymagane
                        </span>
                      </label>
                    </div>
                  </div>

                  <button
                    class="text-red-600 hover:text-red-700 p-2"
                    @click="removeField(index)"
                  >
                    <Trash2 class="w-5 h-5" />
                  </button>
                </div>
              </div>
            </template>
          </draggable>
        </div>

        <!-- Step 3: Approval Flow -->
        <div v-else-if="currentStep === 2" class="space-y-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
              Przepływ zatwierdzeń
            </h3>
            <button
              class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
              @click="addApprovalStep"
            >
              <Plus class="w-4 h-4 mr-2" />
              Dodaj etap
            </button>
          </div>

          <div v-if="form.approvalStepTemplates.length === 0" class="text-center py-8 text-gray-500">
            Brak etapów. Dodaj etapy zatwierdzania.
          </div>

          <div v-if="loadingData" class="text-center py-8">
            <Icon name="svg-spinners:ring-resize" class="w-8 h-8 mx-auto text-blue-600" />
            <p class="mt-2 text-gray-600 dark:text-gray-400">Ładowanie danych...</p>
          </div>

          <div v-else class="space-y-4">
            <ApprovalStepEditor
              v-for="(step, index) in form.approvalStepTemplates"
              :key="index"
              :step="step"
              :users="users"
              :role-groups="roleGroups"
              :departments="departments"
              @update:step="(updatedStep) => form.approvalStepTemplates[index] = updatedStep"
              @remove="removeApprovalStep(index)"
            />
          </div>
        </div>

        <!-- Navigation Buttons -->
        <div class="flex items-center justify-between mt-8 pt-6 border-t border-gray-200 dark:border-gray-700">
          <button
            v-if="currentStep > 0"
            class="inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700"
            @click="currentStep--"
          >
            <ArrowLeft class="w-4 h-4 mr-2" />
            Wstecz
          </button>
          <div v-else />

          <button
            v-if="currentStep < steps.length - 1"
            class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg"
            @click="currentStep++"
          >
            Dalej
            <ArrowRight class="w-4 h-4 ml-2" />
          </button>
          <button
            v-else
            :disabled="saving"
            class="inline-flex items-center px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg disabled:opacity-50"
            @click="saveTemplate"
          >
            <Save class="w-4 h-4 mr-2" />
            {{ saving ? 'Zapisywanie...' : 'Zapisz szablon' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ArrowLeft, ArrowRight, Plus, Trash2, X, Save, GripVertical } from 'lucide-vue-next'
import draggable from 'vuedraggable'
import type { RequestTemplateField, RequestApprovalStepTemplate } from '~/types/requests'
import type { UserDto } from '~/stores/admin'
import type { RoleGroupDto } from '~/stores/roleGroups'
import type { DepartmentDto } from '~/composables/useDepartmentsApi'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'admin', 'request-templates-admin']
})

const { createTemplate } = useRequestsApi()
const { getUsers } = useUsersApi()
const { getAllRoleGroups } = useRoleGroupApi()
const { getDepartments } = useDepartmentsApi()

// Load users, role groups, and departments
const users = ref<UserDto[]>([])
const roleGroups = ref<RoleGroupDto[]>([])
const departments = ref<DepartmentDto[]>([])
const loadingData = ref(true)

onMounted(async () => {
  try {
    const [usersResult, groupsResult, departmentsResult] = await Promise.all([
      getUsers({ isActive: true, pageSize: 1000 }),
      getAllRoleGroups(false),
      getDepartments()
    ])
    users.value = usersResult.users
    roleGroups.value = groupsResult
    departments.value = departmentsResult
  } catch (error) {
    console.error('Error loading users and groups:', error)
  } finally {
    loadingData.value = false
  }
})

const steps = ['Podstawowe info', 'Pola formularza', 'Przepływ zatwierdzeń']
const currentStep = ref(0)
const saving = ref(false)

const form = ref({
  name: '',
  description: '',
  icon: 'FileText',
  category: '',
  departmentId: '',
  requiresApproval: true,
  requiresSubstituteSelection: false,
  estimatedProcessingDays: undefined as number | undefined,
  fields: [] as RequestTemplateField[],
  approvalStepTemplates: [] as RequestApprovalStepTemplate[],
})

const addField = () => {
  form.value.fields.push({
    label: '',
    fieldType: 'Text',
    placeholder: '',
    isRequired: false,
    order: form.value.fields.length + 1,
    options: []
  })
}

const removeField = (index: number) => {
  form.value.fields.splice(index, 1)
  // Reorder
  form.value.fields.forEach((f, i) => f.order = i + 1)
}

const addFieldOption = (fieldIndex: number) => {
  const field = form.value.fields[fieldIndex]
  if (!field.options) {
    field.options = []
  }
  field.options.push({
    label: '',
    value: ''
  })
}

const removeFieldOption = (fieldIndex: number, optionIndex: number) => {
  const field = form.value.fields[fieldIndex]
  if (field.options) {
    field.options.splice(optionIndex, 1)
  }
}

const addApprovalStep = () => {
  form.value.approvalStepTemplates.push({
    stepOrder: form.value.approvalStepTemplates.length + 1,
    approverType: 'Role',
    approverRole: 'Manager',
    requiresQuiz: false
  })
}

const removeApprovalStep = (index: number) => {
  form.value.approvalStepTemplates.splice(index, 1)
  // Reorder
  form.value.approvalStepTemplates.forEach((s, i) => s.stepOrder = i + 1)
}

const validateApprovalSteps = (): string | null => {
  for (const step of form.value.approvalStepTemplates) {
    if (step.approverType === 'Role' && !step.approverRole) {
      return `Krok ${step.stepOrder}: Wybierz rolę zatwierdzającego`
    }
    if (step.approverType === 'SpecificUser' && !step.specificUserId) {
      return `Krok ${step.stepOrder}: Wybierz konkretnego użytkownika`
    }
    if (step.approverType === 'UserGroup' && !step.approverGroupId) {
      return `Krok ${step.stepOrder}: Wybierz grupę użytkowników`
    }
  }
  return null
}

const saveTemplate = async () => {
  const toast = useNotificationToast()

  try {
    saving.value = true

    // Validate approval steps
    if (form.value.requiresApproval && form.value.approvalStepTemplates.length > 0) {
      const validationError = validateApprovalSteps()
      if (validationError) {
        toast.error('Błąd walidacji', validationError)
        return
      }
    }

    // Prepare fields - add substitute field if required and serialize options
    const fieldsToSubmit = form.value.fields.map(field => ({
      ...field,
      options: field.options && field.options.length > 0 ? JSON.stringify(field.options) : undefined
    }))

    if (form.value.requiresSubstituteSelection) {
      fieldsToSubmit.push({
        label: 'Wybierz zastępcę',
        fieldType: 'UserSelect',
        placeholder: 'Wybierz osobę zastępującą',
        isRequired: true,
        order: 999
      })
    }

    // Prepare approval step templates with their quiz questions
    const approvalStepsToSubmit = form.value.approvalStepTemplates.map(step => ({
      ...step,
      quizQuestions: step.quizQuestions || []
    }))

    await createTemplate({
      name: form.value.name,
      description: form.value.description,
      icon: form.value.icon,
      category: form.value.category,
      departmentId: form.value.departmentId || undefined,
      requiresApproval: form.value.requiresApproval,
      requiresSubstituteSelection: form.value.requiresSubstituteSelection,
      estimatedProcessingDays: form.value.estimatedProcessingDays,
      fields: fieldsToSubmit,
      approvalStepTemplates: approvalStepsToSubmit
    })

    toast.success('Sukces!', 'Szablon utworzony pomyślnie')
    setTimeout(() => {
      navigateTo('/admin/request-templates')
    }, 1000)
  } catch (error: any) {
    console.error('Error creating template:', error)
    const errorMessage = error?.response?.data?.message || error?.message || 'Nieznany błąd'
    toast.error('Błąd podczas tworzenia szablonu', errorMessage)
  } finally {
    saving.value = false
  }
}
</script>

