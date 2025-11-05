<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ArrowLeft, Send, Save, AlertCircle, CheckCircle } from 'lucide-vue-next'
import type { RequestTemplate, SubmitRequestDto } from '~/types/requests'

interface Props {
  templateId: string
}

const props = defineProps<Props>()

const template = ref<RequestTemplate | null>(null)
const formData = ref<Record<string, any>>({})
const isFormValid = ref(false)
const isSubmitting = ref(false)
const isDraft = ref(false)
const submitError = ref<string | null>(null)
const submitSuccess = ref(false)

// Mock template data - in real app, this would come from API
onMounted(async () => {
  // Simulate API call to fetch template
  template.value = {
    id: props.templateId,
    name: 'Wniosek urlopowy',
    description: 'Formularz składania wniosku o urlop',
    icon: '🏖️',
    category: 'HR',
    isActive: true,
    requiresApproval: true,
    estimatedProcessingDays: 3,
    createdById: 'user1',
    createdByName: 'Admin',
    createdAt: new Date().toISOString(),
    fields: [
      {
        id: 'leave_type',
        label: 'Typ urlopu',
        fieldType: 'Select',
        isRequired: true,
        order: 0,
        options: JSON.stringify([
          { value: 'annual', label: 'Urlop wypoczynkowy' },
          { value: 'sick', label: 'Zwolnienie lekarskie' },
          { value: 'circumstantial', label: 'Urlop okolicznościowy' },
          { value: 'on_demand', label: 'Urlop na żądanie' }
        ]),
        helpText: 'Wybierz odpowiedni typ urlopu'
      },
      {
        id: 'date_range',
        label: 'Okres urlopu',
        fieldType: 'DateRange',
        isRequired: true,
        order: 1,
        helpText: 'Wybierz daty rozpoczęcia i zakończenia urlopu'
      },
      {
        id: 'reason',
        label: 'Powód/Uzasadnienie',
        fieldType: 'Textarea',
        isRequired: false,
        order: 2,
        placeholder: 'Opisz powód urlopu (opcjonalne)',
        helpText: 'Dodatkowe informacje o powodzie urlopu'
      },
      {
        id: 'documentation',
        label: 'Dokumentacja',
        fieldType: 'FileUpload',
        isRequired: false,
        order: 3,
        isConditional: true,
        conditionalLogic: JSON.stringify({
          dependsOn: 'leave_type',
          condition: 'equals',
          value: 'circumstantial'
        }),
        fileMaxSize: 10,
        allowedFileTypes: JSON.stringify(['pdf', 'doc', 'docx', 'jpg', 'png']),
        helpText: 'Wymagane dla urlopu okolicznościowego'
      },
      {
        id: 'coverage_user',
        label: 'Osoba zastępująca',
        fieldType: 'UserPicker',
        isRequired: false,
        order: 4,
        helpText: 'Wybierz osobę, która będzie Cię zastępować podczas nieobecności'
      },
      {
        id: 'urgent_contact',
        label: 'Kontakt w nagłych przypadkach',
        fieldType: 'Checkbox',
        isRequired: false,
        order: 5,
        placeholder: 'Jestem dostępny/a w nagłych przypadkach'
      }
    ],
    approvalStepTemplates: [],
    quizQuestions: []
  }
})

const onFormDataUpdate = (data: Record<string, any>) => {
  formData.value = data
}

const onValidationChange = (valid: boolean) => {
  isFormValid.value = valid
}

const saveDraft = async () => {
  isDraft.value = true
  submitError.value = null
  
  try {
    // Simulate API call to save draft
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    // Show success message
    submitSuccess.value = true
    setTimeout(() => {
      submitSuccess.value = false
    }, 3000)
  } catch (error) {
    submitError.value = 'Nie udało się zapisać wersji roboczej'
  } finally {
    isDraft.value = false
  }
}

const submitRequest = async () => {
  if (!isFormValid.value || !template.value) return
  
  isSubmitting.value = true
  submitError.value = null
  
  try {
    const submitData: SubmitRequestDto = {
      requestTemplateId: template.value.id,
      priority: 'Standard',
      formData: formData.value
    }
    
    // Simulate API call to submit request
    await new Promise(resolve => setTimeout(resolve, 2000))
    
    // Show success and redirect
    submitSuccess.value = true
    
    // In real app, redirect to request details or dashboard
    setTimeout(() => {
      // navigateTo('/dashboard/requests')
    }, 2000)
  } catch (error) {
    submitError.value = 'Nie udało się wysłać wniosku. Spróbuj ponownie.'
  } finally {
    isSubmitting.value = false
  }
}

const goBack = () => {
  // In real app, navigate back to templates list
  // navigateTo('/dashboard/requests/templates')
}

const estimatedCompletionDate = computed(() => {
  if (!template.value?.estimatedProcessingDays) return null
  
  const date = new Date()
  date.setDate(date.getDate() + template.value.estimatedProcessingDays)
  return date.toLocaleDateString('pl-PL', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-4">
            <button
              @click="goBack"
              class="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition"
            >
              <ArrowLeft class="w-5 h-5" />
            </button>
            
            <div v-if="template" class="flex items-center gap-3">
              <span class="text-2xl">{{ template.icon }}</span>
              <div>
                <h1 class="text-xl font-semibold text-gray-900 dark:text-white">
                  {{ template.name }}
                </h1>
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  {{ template.description }}
                </p>
              </div>
            </div>
          </div>
          
          <div class="flex items-center gap-3">
            <button
              @click="saveDraft"
              :disabled="isDraft"
              class="flex items-center gap-2 px-4 py-2 text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition disabled:opacity-50"
            >
              <Save class="w-4 h-4" />
              {{ isDraft ? 'Zapisywanie...' : 'Zapisz wersję roboczą' }}
            </button>
            
            <button
              @click="submitRequest"
              :disabled="!isFormValid || isSubmitting"
              :class="[
                'flex items-center gap-2 px-6 py-2 rounded-lg font-medium transition',
                isFormValid && !isSubmitting
                  ? 'bg-blue-600 hover:bg-blue-700 text-white'
                  : 'bg-gray-300 dark:bg-gray-600 text-gray-500 dark:text-gray-400 cursor-not-allowed'
              ]"
            >
              <Send class="w-4 h-4" />
              {{ isSubmitting ? 'Wysyłanie...' : 'Wyślij wniosek' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Content -->
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Main Form -->
        <div class="lg:col-span-2">
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
            <!-- Success Message -->
            <div
              v-if="submitSuccess"
              class="mb-6 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg flex items-center gap-3"
            >
              <CheckCircle class="w-5 h-5 text-green-600 dark:text-green-400" />
              <div class="text-green-800 dark:text-green-200">
                <div class="font-medium">Wniosek został wysłany pomyślnie!</div>
                <div class="text-sm">Zostaniesz przekierowany do panelu wniosków...</div>
              </div>
            </div>

            <!-- Error Message -->
            <div
              v-if="submitError"
              class="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg flex items-center gap-3"
            >
              <AlertCircle class="w-5 h-5 text-red-600 dark:text-red-400" />
              <div class="text-red-800 dark:text-red-200">
                <div class="font-medium">Wystąpił błąd</div>
                <div class="text-sm">{{ submitError }}</div>
              </div>
            </div>

            <!-- Form -->
            <DynamicFormRenderer
              v-if="template"
              :template="template"
              @update:form-data="onFormDataUpdate"
              @validation-change="onValidationChange"
            />
          </div>
        </div>

        <!-- Sidebar -->
        <div class="space-y-6">
          <!-- Processing Info -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
            <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Informacje o procesie
            </h3>
            
            <div class="space-y-4">
              <div v-if="template?.requiresApproval" class="flex items-start gap-3">
                <div class="w-2 h-2 bg-blue-500 rounded-full mt-2"></div>
                <div>
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    Wymaga zatwierdzenia
                  </div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">
                    Wniosek zostanie przekazany do odpowiednich osób
                  </div>
                </div>
              </div>
              
              <div v-if="template?.estimatedProcessingDays" class="flex items-start gap-3">
                <div class="w-2 h-2 bg-green-500 rounded-full mt-2"></div>
                <div>
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    Szacowany czas realizacji
                  </div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">
                    {{ template.estimatedProcessingDays }} dni roboczych
                  </div>
                  <div v-if="estimatedCompletionDate" class="text-xs text-gray-400 dark:text-gray-500">
                    Przewidywane zakończenie: {{ estimatedCompletionDate }}
                  </div>
                </div>
              </div>
              
              <div class="flex items-start gap-3">
                <div class="w-2 h-2 bg-purple-500 rounded-full mt-2"></div>
                <div>
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    Powiadomienia
                  </div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">
                    Otrzymasz powiadomienie o każdej zmianie statusu
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Form Validation Status -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
            <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">
              Status formularza
            </h3>
            
            <div class="flex items-center gap-3">
              <div
                :class="[
                  'w-3 h-3 rounded-full',
                  isFormValid ? 'bg-green-500' : 'bg-red-500'
                ]"
              ></div>
              <span
                :class="[
                  'text-sm font-medium',
                  isFormValid
                    ? 'text-green-700 dark:text-green-400'
                    : 'text-red-700 dark:text-red-400'
                ]"
              >
                {{ isFormValid ? 'Formularz jest poprawny' : 'Formularz zawiera błędy' }}
              </span>
            </div>
            
            <div class="mt-3 text-sm text-gray-500 dark:text-gray-400">
              {{ isFormValid 
                ? 'Możesz wysłać wniosek lub zapisać jako wersję roboczą' 
                : 'Uzupełnij wymagane pola, aby móc wysłać wniosek'
              }}
            </div>
          </div>

          <!-- Help -->
          <div class="bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800 p-6">
            <h3 class="text-lg font-medium text-blue-900 dark:text-blue-100 mb-2">
              Potrzebujesz pomocy?
            </h3>
            <p class="text-sm text-blue-700 dark:text-blue-300 mb-4">
              Jeśli masz pytania dotyczące wypełniania wniosku, skontaktuj się z działem HR.
            </p>
            <button class="text-sm text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-200 font-medium">
              Skontaktuj się z HR →
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>