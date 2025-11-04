<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading -->
      <div v-if="loading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
      </div>

      <!-- Error -->
      <div v-else-if="error || !template" class="text-center py-12">
        <p class="text-red-600">{{ error || 'Szablon nie został znaleziony' }}</p>
        <NuxtLink to="/dashboard/requests" class="text-blue-600 hover:underline mt-4 inline-block">
          Powrót do wniosków
        </NuxtLink>
      </div>

      <!-- Form -->
      <div v-else>
        <!-- Header -->
        <div class="mb-8">
          <NuxtLink
            to="/dashboard/requests"
            class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-700 mb-4"
          >
            <ArrowLeft class="w-4 h-4 mr-2" />
            Powrót do listy
          </NuxtLink>

          <div class="flex items-center gap-4 mb-4">
            <Icon
              :name="getIconifyName(template.icon)"
              class="w-12 h-12"
            />
            <div>
              <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
                {{ template.name }}
              </h1>
              <p class="text-gray-600 dark:text-gray-400">
                {{ template.description }}
              </p>
            </div>
          </div>

          <div v-if="template.estimatedProcessingDays" class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
            <Clock class="w-4 h-4" />
            <span>Szacowany czas procesowania: {{ template.estimatedProcessingDays}} dni</span>
          </div>
        </div>

        <!-- Vacation Summary Panel (Only for vacation requests) -->
        <div v-if="template.isVacationRequest" class="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-900/20 dark:to-indigo-900/20 rounded-lg shadow-sm border border-blue-200 dark:border-blue-800 p-6 mb-6">
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
            <svg class="w-6 h-6 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            Twoja dostępność urlopowa
          </h2>

          <!-- Loading vacation data -->
          <div v-if="vacationSummaryLoading" class="text-center py-4">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
          </div>

          <!-- Vacation summary -->
          <div v-else-if="vacationSummary" class="space-y-4">
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div class="text-center p-3 bg-white dark:bg-gray-800 rounded-lg">
                <p class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                  {{ vacationSummary.totalAvailableVacationDays }}
                </p>
                <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">Dostępne dni</p>
              </div>
              <div class="text-center p-3 bg-white dark:bg-gray-800 rounded-lg">
                <p class="text-2xl font-bold text-orange-600 dark:text-orange-400">
                  {{ vacationSummary.vacationDaysUsed }}
                </p>
                <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">Wykorzystane</p>
              </div>
              <div class="text-center p-3 bg-white dark:bg-gray-800 rounded-lg">
                <p class="text-2xl font-bold text-purple-600 dark:text-purple-400">
                  {{ vacationSummary.onDemandVacationDaysRemaining }}
                </p>
                <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">Na żądanie</p>
              </div>
              <div class="text-center p-3 bg-white dark:bg-gray-800 rounded-lg">
                <p class="text-2xl font-bold text-green-600 dark:text-green-400">
                  {{ vacationSummary.vacationDaysRemaining }}
                </p>
                <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">Pozostały urlop wypoczynkowy</p>
              </div>
            </div>

            <!-- Validation feedback -->
            <div v-if="validationResult" class="mt-4">
              <!-- Success -->
              <div v-if="validationResult.canTake" class="flex items-start gap-3 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg">
                <svg class="w-6 h-6 text-green-600 dark:text-green-400 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <div>
                  <p class="font-semibold text-green-800 dark:text-green-200">
                    Możesz wziąć ten urlop
                  </p>
                  <p class="text-sm text-green-600 dark:text-green-400 mt-1">
                    Wnioskowane dni: {{ validationResult.requestedDays }} dni roboczych
                  </p>
                  <p class="text-sm text-green-600 dark:text-green-400">
                    Po zatwierdzeniu pozostanie: {{ vacationSummary.vacationDaysRemaining - validationResult.requestedDays }} dni
                  </p>
                </div>
              </div>

              <!-- Error -->
              <div v-else class="flex items-start gap-3 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
                <svg class="w-6 h-6 text-red-600 dark:text-red-400 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <div>
                  <p class="font-semibold text-red-800 dark:text-red-200">
                    Nie możesz wziąć tego urlopu
                  </p>
                  <p class="text-sm text-red-600 dark:text-red-400 mt-1">
                    {{ validationResult.errorMessage }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Validation in progress -->
            <div v-else-if="isValidating" class="flex items-center gap-2 p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
              <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"></div>
              <span class="text-sm text-gray-600 dark:text-gray-400">Sprawdzanie dostępności...</span>
            </div>
          </div>
        </div>

        <!-- Circumstantial attachments (photos/scans) -->
        <div v-if="template.isVacationRequest && currentLeaveType === 'Circumstantial'" class="mb-6">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Załączniki (zdjęcia/skan zaświadczenia)
          </label>
          <input type="file" multiple accept="image/*" @change="onAttachmentsChange" class="block w-full text-sm text-gray-700 dark:text-gray-300" />
          <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">Do 5 plików, zostaną zapisane wraz z wnioskiem.</p>
        </div>

        <!-- Form Fields -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 mb-6">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-6">
            Wypełnij formularz
          </h2>

        <div class="space-y-6">
          <!-- Substitute picker for vacation requests -->
          <div v-if="template.isVacationRequest" class="relative">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Zastępca (opcjonalnie)
            </label>
            <input
              v-model="subSearch"
              type="text"
              placeholder="Wpisz min. 2 znaki i wybierz z listy"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              @input="searchUsers"
            />
            <ul v-if="subResults.length > 0" class="absolute z-10 mt-1 w-full bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded shadow">
              <li
                v-for="u in subResults"
                :key="u.id"
                class="px-3 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer"
                @click="pickSubstitute(u)"
              >
                {{ u.firstName }} {{ u.lastName }} <span class="text-xs text-gray-500">{{ u.email }}</span>
              </li>
            </ul>
          </div>
            <div v-for="field in sortedFields" :key="field.id">
              <!-- Text -->
              <div v-if="field.fieldType === 'Text'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <input
                  v-model="formData[field.id!]"
                  type="text"
                  :placeholder="field.placeholder"
                  :required="field.isRequired"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                >
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Textarea -->
              <div v-else-if="field.fieldType === 'Textarea'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <textarea
                  v-model="formData[field.id!]"
                  rows="4"
                  :placeholder="field.placeholder"
                  :required="field.isRequired"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                />
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Number -->
              <div v-else-if="field.fieldType === 'Number'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <input
                  v-model.number="formData[field.id!]"
                  type="number"
                  :placeholder="field.placeholder"
                  :required="field.isRequired"
                  :min="field.minValue"
                  :max="field.maxValue"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                >
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Select -->
              <div v-else-if="field.fieldType === 'Select'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <select
                  v-model="formData[field.id!]"
                  :required="field.isRequired"
                  @change="handleFieldChange"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                >
                  <option value="">Wybierz...</option>
                  <option
                    v-for="option in parseOptions(field.options)"
                    :key="option.value"
                    :value="option.value"
                  >
                    {{ option.label }}
                  </option>
                </select>
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Date -->
              <div v-else-if="field.fieldType === 'Date'">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {{ field.label }}
                  <span v-if="field.isRequired" class="text-red-500">*</span>
                </label>
                <input
                  v-model="formData[field.id!]"
                  type="date"
                  :required="field.isRequired"
                  @change="handleFieldChange"
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
                >
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>

              <!-- Checkbox -->
              <div v-else-if="field.fieldType === 'Checkbox'">
                <label class="flex items-center">
                  <input
                    v-model="formData[field.id!]"
                    type="checkbox"
                    :required="field.isRequired"
                    class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  >
                  <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                    {{ field.label }}
                    <span v-if="field.isRequired" class="text-red-500">*</span>
                  </span>
                </label>
                <p v-if="field.helpText" class="mt-1 text-sm text-gray-500">{{ field.helpText }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Priority -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 mb-6">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            Priorytet
          </h2>
          <div class="flex gap-4">
            <label class="flex items-center">
              <input
                v-model="priority"
                type="radio"
                value="Standard"
                class="w-4 h-4 text-blue-600 border-gray-300 focus:ring-blue-500"
              >
              <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                Standard
              </span>
            </label>
            <label class="flex items-center">
              <input
                v-model="priority"
                type="radio"
                value="Urgent"
                class="w-4 h-4 text-red-600 border-gray-300 focus:ring-red-500"
              >
              <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
                Pilne
              </span>
            </label>
          </div>
        </div>

        <!-- Submit -->
        <div class="flex items-center justify-between">
          <NuxtLink
            to="/dashboard/requests"
            class="px-6 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700"
          >
            Anuluj
          </NuxtLink>

          <button
            @click="submitRequest"
            :disabled="submitting || (template.isVacationRequest && validationResult && !validationResult.canTake)"
            class="px-6 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg font-medium transition-colors"
          >
            {{ submitting ? 'Wysyłanie...' : 'Złóż wniosek' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { ArrowLeft, Clock } from 'lucide-vue-next'
import type { RequestTemplate, RequestPriority } from '~/types/requests'
import { type VacationSummary, type ValidateVacationResponse, LeaveType } from '~/composables/useVacations'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const route = useRoute()
const authStore = useAuthStore()
const { getTemplateById, submitRequest: submitRequestApi } = useRequestsApi()
const { getUserVacationSummary, validateVacation } = useVacations()
const toast = useNotificationToast()

const templateId = route.params.id as string
const template = ref<RequestTemplate | null>(null)
const loading = ref(true)
const error = ref('')
const formData = ref<Record<string, any>>({})
const priority = ref<RequestPriority>('Standard')
const submitting = ref(false)

// Vacation-specific state
const vacationSummary = ref<VacationSummary | null>(null)
const vacationSummaryLoading = ref(false)
  const validationResult = ref<ValidateVacationResponse | null>(null)
  const isValidating = ref(false)
  // Substitute picker state
  const subSearch = ref('')
  const subResults = ref<Array<{ id: string; firstName: string; lastName: string; email: string }>>([])
  const subSelected = ref<{ id: string; firstName: string; lastName: string } | null>(null)
  const searchUsers = async () => {
    if (!template.value?.isVacationRequest) return
    const q = subSearch.value.trim()
    if (q.length < 2) { subResults.value = []; return }
    try {
      const headers = useAuth().getAuthHeaders()
      const config = useRuntimeConfig()
      const res = await $fetch(`${config.public.apiUrl}/api/users/search?q=${encodeURIComponent(q)}`, { headers }) as Array<{ id: string; firstName: string; lastName: string; email: string }>
      subResults.value = res
    } catch (e) {
      subResults.value = []
    }
  }
  const pickSubstitute = (u: { id: string; firstName: string; lastName: string }) => {
    subSelected.value = u
    formData.value['substituteUserId'] = u.id
    subResults.value = []
    subSearch.value = `${u.firstName} ${u.lastName}`
  }

const sortedFields = computed(() => {
  if (!template.value) return []
  return [...template.value.fields].sort((a, b) => a.order - b.order)
})

// Icon mapping for curated icon set
const iconMapping: Record<string, string> = {
  'beach-umbrella': 'fluent-emoji-flat:beach-with-umbrella',
  plane: 'fluent-emoji-flat:airplane',
  calendar: 'heroicons:calendar-days',
  laptop: 'heroicons:computer-desktop',
  toolbox: 'heroicons:wrench-screwdriver',
  document: 'heroicons:document-text',
  folder: 'heroicons:folder',
  clipboard: 'heroicons:clipboard-document-list',
  shield: 'heroicons:shield-check',
  warning: 'heroicons:exclamation-triangle',
  graduation: 'heroicons:academic-cap',
  book: 'heroicons:book-open',
  users: 'heroicons:user-group',
  bell: 'heroicons:bell',
  check: 'heroicons:check-circle',
  'medical-bag': 'heroicons:briefcase'
}

const getIconifyName = (iconName: string) => {
  return iconMapping[iconName] || 'heroicons:question-mark-circle'
}

const parseOptions = (optionsJson: string | undefined) => {
  if (!optionsJson) return []
  try {
    return JSON.parse(optionsJson)
  } catch {
    return []
  }
}

const loadTemplate = async () => {
  try {
    loading.value = true
    template.value = await getTemplateById(templateId)

    // Load vacation summary if this is a vacation request
    if (template.value?.isVacationRequest && authStore.user?.id) {
      await loadVacationSummary()
    }
  } catch (err) {
    console.error('Error loading template:', err)
    error.value = 'Nie udało się załadować szablonu'
  } finally {
    loading.value = false
  }
}

const loadVacationSummary = async () => {
  if (!authStore.user?.id) return

  try {
    vacationSummaryLoading.value = true
    vacationSummary.value = await getUserVacationSummary(authStore.user.id)
  } catch (err) {
    console.error('Error loading vacation summary:', err)
  } finally {
    vacationSummaryLoading.value = false
  }
}

const handleFieldChange = async () => {
  // Only validate if this is a vacation request
  if (!template.value?.isVacationRequest) return

  // Extract vacation fields from formData
  let leaveType: string | null = null
  let startDate: string | null = null
  let endDate: string | null = null

  for (const [, value] of Object.entries(formData.value)) {
    if (typeof value === 'string') {
      // Check if it's a leave type
      if (value === 'Annual' || value === 'OnDemand' || value === 'Circumstantial') {
        leaveType = value
      }
      // Check if it's a date (ISO format yyyy-MM-dd)
      else if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
        if (!startDate) {
          startDate = value
        } else if (!endDate) {
          endDate = value
        }
      }
    }
  }

  // Only validate if we have all required fields
  if (leaveType && startDate && endDate) {
    await performValidation(startDate, endDate, leaveType as LeaveType)
  } else {
    validationResult.value = null
  }
}

const performValidation = async (startDate: string, endDate: string, leaveType: LeaveType) => {
  try {
    isValidating.value = true
    validationResult.value = await validateVacation({
      startDate,
      endDate,
      leaveType
    })
  } catch (err) {
    console.error('Error validating vacation:', err)
  } finally {
    isValidating.value = false
  }
}

// Extract current leave type from form data
const currentLeaveType = computed(() => {
  for (const [, value] of Object.entries(formData.value)) {
    if (value === 'Annual' || value === 'OnDemand' || value === 'Circumstantial' || value === 'Sick') {
      return value as LeaveType
    }
  }
  return null
})

// Handle attachments selection (base64 inline for backend to persist)
const onAttachmentsChange = async (e: Event) => {
  const input = e.target as HTMLInputElement
  if (!input.files) return
  const files = Array.from(input.files).slice(0, 5)
  const encoded: string[] = []
  for (const file of files) {
    const dataUrl = await new Promise<string>((resolve, reject) => {
      const reader = new FileReader()
      reader.onload = () => resolve(reader.result as string)
      reader.onerror = reject
      reader.readAsDataURL(file)
    })
    encoded.push(dataUrl)
  }
  // Persist under a well-known key so backend can pick it up
  formData.value.attachments = encoded
}

const submitRequest = async () => {
  if (!template.value) return

  // Block submission if vacation validation failed
  if (template.value.isVacationRequest && validationResult.value && !validationResult.value.canTake) {
    toast.error('Nie możesz złożyć tego wniosku', validationResult.value.errorMessage || undefined)
    return
  }

  try {
    submitting.value = true

    const result = await submitRequestApi({
      requestTemplateId: template.value.id,
      priority: priority.value,
      formData: formData.value
    })

    toast.success('Wniosek został złożony pomyślnie!', `Numer wniosku: ${result.requestNumber}`)
    navigateTo('/dashboard/requests')
  } catch (err: any) {
    console.error('Error submitting request:', err)

    // Show specific error message if available
    const errorMessage = err?.data?.message || err?.message || 'Błąd podczas składania wniosku'
    toast.error('Błąd podczas składania wniosku', errorMessage)
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  loadTemplate()
})
</script>
