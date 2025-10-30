<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading State -->
      <div v-if="loading" class="flex items-center justify-center py-12">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p class="text-gray-600 dark:text-gray-400">Ładowanie szablonu...</p>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-6">
        <h3 class="text-lg font-semibold text-red-900 dark:text-red-200 mb-2">Błąd</h3>
        <p class="text-red-700 dark:text-red-300">{{ error }}</p>
        <NuxtLink
          to="/admin/request-templates"
          class="inline-flex items-center mt-4 text-blue-600 dark:text-blue-400 hover:text-blue-700"
        >
          <ArrowLeft class="w-4 h-4 mr-2" />
          Powrót do listy
        </NuxtLink>
      </div>

      <!-- Form -->
      <div v-else>
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
            Edytuj szablon wniosku
          </h1>
          <p class="text-gray-600 dark:text-gray-400">
            Modyfikuj szablon wniosku: {{ form.name }}
          </p>
        </div>

        <!-- Progress Steps -->
        <div class="mb-8">
          <div class="flex items-center justify-between">
            <button
              v-for="(step, index) in steps"
              :key="index"
              @click="currentStep = index"
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
                v-model="form.requiresApproval"
                type="checkbox"
                id="requiresApproval"
                class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              >
              <label for="requiresApproval" class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                Wymaga zatwierdzenia
              </label>
            </div>

            <div class="flex items-center">
              <input
                v-model="form.isActive"
                type="checkbox"
                id="isActive"
                class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              >
              <label for="isActive" class="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                Szablon aktywny
              </label>
            </div>
          </div>

          <!-- Step 2: Form Fields -->
          <div v-else-if="currentStep === 1" class="space-y-6">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                Pola formularza
              </h3>
              <button
                @click="addField"
                class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
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

                      <div>
                        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Placeholder
                        </label>
                        <input
                          v-model="field.placeholder"
                          type="text"
                          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
                        >
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
                      @click="removeField(index)"
                      class="text-red-600 hover:text-red-700 p-2"
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
                @click="addApprovalStep"
                class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
              >
                <Plus class="w-4 h-4 mr-2" />
                Dodaj etap
              </button>
            </div>

            <div v-if="form.approvalStepTemplates.length === 0" class="text-center py-8 text-gray-500">
              Brak etapów. Dodaj etapy zatwierdzania.
            </div>

            <div v-if="loadingData" class="text-center py-8">
              <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-2"></div>
              <p class="mt-2 text-gray-600 dark:text-gray-400">Ładowanie danych...</p>
            </div>

            <div v-else class="space-y-4">
              <ApprovalStepEditor
                v-for="(step, index) in form.approvalStepTemplates"
                :key="index"
                :step="step"
                :users="users"
                :role-groups="roleGroups"
                @remove="removeApprovalStep(index)"
              />
            </div>
          </div>

          <!-- Step 4: Quiz (if any step requires it) -->
          <div v-else-if="currentStep === 3" class="space-y-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Próg zdawalności (%)
              </label>
              <input
                v-model.number="form.passingScore"
                type="number"
                min="0"
                max="100"
                placeholder="80"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              >
            </div>

            <div class="flex items-center justify-between mb-4">
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                Pytania quizu
              </h3>
              <button
                @click="addQuizQuestion"
                class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
              >
                <Plus class="w-4 h-4 mr-2" />
                Dodaj pytanie
              </button>
            </div>

            <div v-if="quizQuestions.length === 0" class="text-center py-8 text-gray-500">
              Brak pytań. Dodaj pytania do quizu.
            </div>

            <div class="space-y-6">
              <div
                v-for="(question, qIndex) in quizQuestions"
                :key="qIndex"
                class="bg-gray-50 dark:bg-gray-700/50 border border-gray-200 dark:border-gray-600 rounded-lg p-4"
              >
                <div class="flex items-start gap-4 mb-4">
                  <div class="flex-shrink-0 w-8 h-8 bg-purple-600 text-white rounded-full flex items-center justify-center font-bold">
                    {{ qIndex + 1 }}
                  </div>
                  
                  <div class="flex-1">
                    <input
                      v-model="question.question"
                      type="text"
                      placeholder="Treść pytania..."
                      class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
                    >
                  </div>

                  <button
                    @click="removeQuizQuestion(qIndex)"
                    class="text-red-600 hover:text-red-700 p-2"
                  >
                    <Trash2 class="w-5 h-5" />
                  </button>
                </div>

                <div class="ml-12 space-y-2">
                  <button
                    @click="addQuizOption(qIndex)"
                    class="text-sm text-blue-600 hover:text-blue-700"
                  >
                    + Dodaj odpowiedź
                  </button>
                  
                  <div
                    v-for="(option, oIndex) in question.options"
                    :key="oIndex"
                    class="flex items-center gap-2"
                  >
                    <input
                      v-model="option.isCorrect"
                      type="checkbox"
                      class="w-4 h-4 text-green-600 border-gray-300 rounded focus:ring-green-500"
                    >
                    <input
                      v-model="option.label"
                      type="text"
                      placeholder="Treść odpowiedzi..."
                      class="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
                    >
                    <button
                      @click="removeQuizOption(qIndex, oIndex)"
                      class="text-red-600 hover:text-red-700"
                    >
                      <X class="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Navigation Buttons -->
          <div class="flex items-center justify-between mt-8 pt-6 border-t border-gray-200 dark:border-gray-700">
            <button
              v-if="currentStep > 0"
              @click="currentStep--"
              class="inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700"
            >
              <ArrowLeft class="w-4 h-4 mr-2" />
              Wstecz
            </button>
            <div v-else />

            <button
              v-if="currentStep < steps.length - 1"
              @click="currentStep++"
              class="inline-flex items-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg"
            >
              Dalej
              <ArrowRight class="w-4 h-4 ml-2" />
            </button>
            <button
              v-else
              @click="saveTemplate"
              :disabled="saving"
              class="inline-flex items-center px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg disabled:opacity-50"
            >
              <Save class="w-4 h-4 mr-2" />
              {{ saving ? 'Zapisywanie...' : 'Zapisz zmiany' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ArrowLeft, ArrowRight, Plus, Trash2, X, Save, GripVertical } from 'lucide-vue-next'
import draggable from 'vuedraggable'
import type { RequestTemplateField, RequestApprovalStepTemplate, QuizOption } from '~/types/requests'
import type { UserDto } from '~/stores/admin'
import type { RoleGroupDto } from '~/stores/roleGroups'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'admin', 'request-templates-admin']
})

const route = useRoute()
const router = useRouter()
const { getTemplateById, updateTemplate } = useRequestsApi()
const { getUsers } = useUsersApi()
const { getAllRoleGroups } = useRoleGroupApi()

const templateId = computed(() => route.params.id as string)

// Load users and role groups
const users = ref<UserDto[]>([])
const roleGroups = ref<RoleGroupDto[]>([])
const loadingData = ref(true)

const loading = ref(true)
const saving = ref(false)
const error = ref<string | null>(null)

const steps = ['Podstawowe info', 'Pola formularza', 'Przepływ zatwierdzeń', 'Quiz']
const currentStep = ref(0)

const form = ref({
  name: '',
  description: '',
  icon: 'FileText',
  category: '',
  departmentId: '',
  requiresApproval: true,
  estimatedProcessingDays: undefined as number | undefined,
  passingScore: 80,
  isActive: true,
  fields: [] as RequestTemplateField[],
  approvalStepTemplates: [] as RequestApprovalStepTemplate[],
})

interface QuizQuestionLocal {
  question: string
  options: QuizOption[]
  order: number
}

const quizQuestions = ref<QuizQuestionLocal[]>([])

const loadTemplate = async () => {
  try {
    loading.value = true
    error.value = null
    
    const template = await getTemplateById(templateId.value)
    
    if (!template) {
      error.value = 'Szablon nie został znaleziony'
      return
    }

    // Populate form with template data
    form.value = {
      name: template.name,
      description: template.description,
      icon: template.icon || 'FileText',
      category: template.category,
      departmentId: template.departmentId || '',
      requiresApproval: template.requiresApproval ?? true,
      estimatedProcessingDays: template.estimatedProcessingDays,
      passingScore: template.passingScore || 80,
      isActive: template.isActive ?? true,
      fields: template.fields || [],
      approvalStepTemplates: template.approvalStepTemplates || [],
    }

    // Load quiz questions if they exist
    if (template.quizQuestions && Array.isArray(template.quizQuestions)) {
      quizQuestions.value = template.quizQuestions.map((q: any) => ({
        question: q.question,
        options: typeof q.options === 'string' ? JSON.parse(q.options) : q.options,
        order: q.order
      }))
    }

    // Load users and role groups in parallel
    const [usersResult, groupsResult] = await Promise.all([
      getUsers({ isActive: true, pageSize: 1000 }),
      getAllRoleGroups(false)
    ])
    users.value = usersResult.users
    roleGroups.value = groupsResult

  } catch (err: any) {
    console.error('Error loading template:', err)
    error.value = err.message || 'Nie udało się załadować szablonu'
  } finally {
    loading.value = false
    loadingData.value = false
  }
}

const addField = () => {
  form.value.fields.push({
    label: '',
    fieldType: 'Text',
    placeholder: '',
    isRequired: false,
    order: form.value.fields.length + 1
  })
}

const removeField = (index: number) => {
  form.value.fields.splice(index, 1)
  // Reorder
  form.value.fields.forEach((f, i) => f.order = i + 1)
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

const addQuizQuestion = () => {
  quizQuestions.value.push({
    question: '',
    options: [],
    order: quizQuestions.value.length + 1
  })
}

const removeQuizQuestion = (index: number) => {
  quizQuestions.value.splice(index, 1)
  quizQuestions.value.forEach((q, i) => q.order = i + 1)
}

const addQuizOption = (questionIndex: number) => {
  const question = quizQuestions.value[questionIndex]
  if (!question) return
  
  const option: QuizOption = {
    value: `option_${Date.now()}`,
    label: '',
    isCorrect: false
  }
  question.options.push(option)
}

const removeQuizOption = (questionIndex: number, optionIndex: number) => {
  const question = quizQuestions.value[questionIndex]
  if (!question) return
  
  question.options.splice(optionIndex, 1)
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

    // Prepare quiz questions
    const quizQuestionsFormatted = quizQuestions.value.map(q => ({
      question: q.question,
      options: JSON.stringify(q.options),
      order: q.order
    }))

    await updateTemplate(templateId.value, {
      name: form.value.name,
      description: form.value.description,
      icon: form.value.icon,
      category: form.value.category,
      departmentId: form.value.departmentId || undefined,
      requiresApproval: form.value.requiresApproval,
      estimatedProcessingDays: form.value.estimatedProcessingDays,
      passingScore: form.value.passingScore,
      isActive: form.value.isActive,
      fields: form.value.fields,
      approvalStepTemplates: form.value.approvalStepTemplates,
      quizQuestions: quizQuestionsFormatted
    })

    toast.success('Sukces!', 'Szablon zaktualizowany pomyślnie')
    setTimeout(() => {
      router.push('/admin/request-templates')
    }, 1000)
  } catch (error: any) {
    console.error('Error updating template:', error)
    const errorMessage = error?.response?.data?.message || error?.message || 'Nieznany błąd'
    toast.error('Błąd podczas aktualizacji szablonu', errorMessage)
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadTemplate()
})
</script>
