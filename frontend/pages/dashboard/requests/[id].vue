<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ArrowLeft, CheckCircle, XCircle } from 'lucide-vue-next'
import RequestQuizForm from '~/components/requests/RequestQuizForm.vue'
import RequestQuizResult from '~/components/requests/RequestQuizResult.vue'
import RequestQuizAnswersDisplay from '~/components/requests/RequestQuizAnswersDisplay.vue'
import RequestAttachments from '~/components/requests/RequestAttachments.vue'
import RequestComments from '~/components/requests/RequestComments.vue'
import RequestEditHistory from '~/components/requests/RequestEditHistory.vue'
import type { Request, RequestTemplate, RequestApprovalStep } from '~/types/requests'

// Extended user type to support PascalCase Id
interface UserWithId {
  id?: string
  Id?: string
}

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { getRequestById, getTemplateById, addComment } = useRequestsApi()
const toast = useNotificationToast()

const requestId = route.params.id as string

// State
const request = ref<Request | null>(null)
const template = ref<RequestTemplate | null>(null)
const isLoading = ref(true)
const error = ref<string | null>(null)

// Approval actions
const showApproveModal = ref(false)
const showRejectModal = ref(false)
const approveComment = ref('')
const rejectComment = ref('')
const isSubmitting = ref(false)

// Quiz state
const showQuizForm = ref(false)

// Check if current user is the request submitter
const isRequestSubmitter = computed(() => {
  if (!request.value || !authStore.user) return false
  const submitterId = request.value.submittedById
  const user = authStore.user as UserWithId
  const userId = user.id || user.Id
  return submitterId === userId
})

// Check if current user is the approver for the current step
const isCurrentApprover = computed(() => {
  if (!request.value || !authStore.user) return false

  const currentStepItem = request.value.approvalSteps.find((s: RequestApprovalStep) =>
    s.status === 'InReview'
  )

  if (!currentStepItem) return false

  const approverId = currentStepItem.approverId
  const user = authStore.user as UserWithId
  const userId = user.id || user.Id

  return approverId === userId
})

// Get current approval step
const currentStep = computed((): RequestApprovalStep | undefined => {
  if (!request.value) return undefined
  return request.value.approvalSteps.find((s: RequestApprovalStep) =>
    s.status === 'InReview'
  )
})

// Check if user can add comments (request submitter or approvers)
const canAddComment = computed(() => {
  if (!request.value || !authStore.user) return false

  const user = authStore.user as UserWithId
  const userId = user.id || user.Id

  // Submitter can always comment
  if (request.value.submittedById === userId) return true

  // Approvers can comment
  const isApprover = request.value.approvalSteps.some((s: RequestApprovalStep) => s.approverId === userId)
  return isApprover
})

// Quiz-related computed properties
const requiresQuiz = computed(() => {
  const step = currentStep.value
  return step?.requiresQuiz === true
})

const quizPassed = computed(() => {
  const step = currentStep.value
  return step?.quizPassed === true
})

const quizScore = computed(() => {
  const step = currentStep.value
  return step?.quizScore
})

const hasQuizQuestions = computed(() => {
  const step = currentStep.value
  const questions = step?.quizQuestions
  return questions && questions.length > 0
})

const canApprove = computed(() => {
  // Approver can always make a decision, even if quiz was failed
  // This allows them to override the quiz requirement if needed
  return isCurrentApprover.value
})

// Format form data for display
const formattedFormData = computed(() => {
  if (!request.value?.formData) return []
  try {
    const data = JSON.parse(request.value.formData) as Record<string, unknown>
    const fields = template.value?.fields || []
    const labelById = new Map<string, string>()

    // Map field IDs to labels - handle both string and Guid types
    for (const f of fields) {
      // Handle both `id` and `Id` properties (backend uses `Id`, frontend uses `id`)
      const fieldId = (f.id || f.Id)?.toString().toLowerCase()
      if (fieldId) {
        labelById.set(fieldId, f.label)
      }
    }

    return Object.entries(data).map(([key, value]) => {
      // Normalize key to lowercase for matching
      const normalizedKey = key.toLowerCase()
      const label = labelById.get(normalizedKey) || formatFieldName(key)
      return { key: label, value: formatFieldValue(value) }
    })
  } catch (err) {
    console.error('Error parsing form data:', err)
    return []
  }
})

// Format field name (camelCase to readable)
const formatFieldName = (key: string): string => {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, str => str.toUpperCase())
    .trim()
}

// Format field value
const formatFieldValue = (value: unknown): string => {
  if (value === null || value === undefined) return '-'
  if (typeof value === 'boolean') return value ? 'Tak' : 'Nie'
  if (typeof value === 'object') return JSON.stringify(value, null, 2)

  // Try to format as date if it looks like an ISO date
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}/.test(value)) {
    try {
      const date = new Date(value)
      return date.toLocaleDateString('pl-PL', { year: 'numeric', month: 'long', day: 'numeric' })
    } catch {
      return value
    }
  }

  return String(value)
}

// Get status badge class
const getStatusBadgeClass = (status: string) => {
  const classes: Record<string, string> = {
    InReview: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    Approved: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    Rejected: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    AwaitingSurvey: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    Draft: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200'
  }
  return classes[status] || classes.Draft
}

// Get status label
const getStatusLabel = (status: string) => {
  const labels: Record<string, string> = {
    Draft: 'Szkic',
    InReview: 'W trakcie oceny',
    Approved: 'Zatwierdzony',
    Rejected: 'Odrzucony',
    AwaitingSurvey: 'Wymaga quizu'
  }
  return labels[status] || status
}

// Format date
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Load request data
const loadRequest = async () => {
  isLoading.value = true
  error.value = null

  try {
    const data = await getRequestById(requestId)
    request.value = data
    // Load template to map field IDs (GUID) to human labels
    if (request.value?.requestTemplateId) {
      try {
        template.value = (await getTemplateById(request.value.requestTemplateId))
      } catch (e) {
        // Non-fatal for details page; fallback to camelCase label formatting
        console.warn('Failed to load request template for details mapping', e)
      }
    }
  } catch (err: unknown) {
    const errorData = err as { statusCode?: number; message?: string }
    if (errorData.statusCode === 404) {
      error.value = 'Wniosek nie został znaleziony'
    } else {
      error.value = errorData.message || 'Nie udało się pobrać szczegółów wniosku'
    }
    console.error('Error loading request:', err)
  } finally {
    isLoading.value = false
  }
}

// Handle approve
const handleApprove = async () => {
  if (!currentStep.value) return

  isSubmitting.value = true

  try {
    const { approveRequestStep } = useRequestsApi()
    await approveRequestStep(requestId, currentStep.value.id, {
      comment: approveComment.value || undefined
    })

    // Reload request data
    await loadRequest()

    // Close modal and reset
    showApproveModal.value = false
    approveComment.value = ''
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : 'Nie udało się zatwierdzić wniosku'
    console.error('Error approving request:', err)
  } finally {
    isSubmitting.value = false
  }
}

// Handle reject
const handleReject = async () => {
  if (!currentStep.value) return

  if (!rejectComment.value.trim()) {
    toast.warning('Komentarz jest wymagany przy odrzuceniu wniosku')
    return
  }

  isSubmitting.value = true

  try {
    const { rejectRequestStep } = useRequestsApi()
    await rejectRequestStep(requestId, currentStep.value.id, {
      reason: rejectComment.value
    })

    // Reload request data
    await loadRequest()

    // Close modal and reset
    showRejectModal.value = false
    rejectComment.value = ''
  } catch (err: unknown) {
    error.value = err instanceof Error ? err.message : 'Nie udało się odrzucić wniosku'
    console.error('Error rejecting request:', err)
  } finally {
    isSubmitting.value = false
  }
}

// Handle quiz submission
const handleQuizSubmitted = async (result: { success: boolean; score: number; passed: boolean }) => {
  if (result.passed) {
    toast.success(`Quiz zaliczony! Wynik: ${result.score}%`)
    showQuizForm.value = false
    // Reload request to get updated quiz status
    await loadRequest()
  } else {
    toast.error(`Quiz niezaliczony. Wynik: ${result.score}%`)
  }
}

// Handle add comment
const handleAddComment = async (data: { comment: string; attachments: File[] }) => {
  try {
    await addComment(requestId, data.comment, data.attachments)

    toast.success('Komentarz został dodany')

    // Reload request to get new comment
    await loadRequest()
  } catch (err: unknown) {
    console.error('Error adding comment:', err)
    toast.error('Nie udało się dodać komentarza')
  }
}

// Navigate back
const goBack = () => {
  router.push('/dashboard/requests')
}

// Load data on mount
onMounted(() => {
  loadRequest()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Back Button -->
      <button
        class="mb-6 flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors"
        @click="goBack"
      >
        <ArrowLeft class="w-5 h-5" />
        <span>Powrót do wniosków</span>
      </button>

      <!-- Loading State -->
      <div v-if="isLoading" class="flex items-center justify-center h-64">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
          <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie wniosku...</p>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
        <svg
          class="w-16 h-16 text-red-400 mx-auto mb-4"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
          Błąd
        </h3>
        <p class="text-gray-600 dark:text-gray-400">
          {{ error }}
        </p>
      </div>

      <!-- Request Details -->
      <div v-else-if="request" class="space-y-6">
        <!-- Header -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <div class="flex items-start justify-between mb-4">
            <div>
              <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                {{ request.requestTemplateName }}
              </h1>
              <p class="text-gray-600 dark:text-gray-400">
                {{ request.requestNumber }}
              </p>
            </div>
            <span
              :class="['px-3 py-1 text-sm font-medium rounded-full', getStatusBadgeClass(request.status)]"
            >
              {{ getStatusLabel(request.status) }}
            </span>
          </div>

          <!-- Basic Info -->
          <div class="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
            <div>
              <p class="text-gray-500 dark:text-gray-400">Data złożenia</p>
              <p class="font-medium text-gray-900 dark:text-white">
                {{ formatDate(request.submittedAt) }}
              </p>
            </div>
            <div>
              <p class="text-gray-500 dark:text-gray-400">Wnioskodawca</p>
              <p class="font-medium text-gray-900 dark:text-white">
                {{ request.submittedByName }}
              </p>
            </div>
            <div>
              <p class="text-gray-500 dark:text-gray-400">Priorytet</p>
              <p class="font-medium text-gray-900 dark:text-white">
                <span :class="request.priority === 'Urgent' ? 'text-red-600 dark:text-red-400' : ''">
                  {{ request.priority === 'Urgent' ? '🔴 Pilne' : '🔵 Standard' }}
                </span>
              </p>
            </div>
          </div>
        </div>

        <!-- Timeline -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <RequestTimeline :steps="request.approvalSteps" />
        </div>

        <!-- Form Data -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Wypełniony formularz
          </h3>
          <div class="space-y-3">
            <div
              v-for="field in formattedFormData"
              :key="field.key"
              class="flex flex-col sm:flex-row sm:items-start gap-2 py-3 border-b border-gray-200 dark:border-gray-700 last:border-b-0"
            >
              <dt class="text-sm font-medium text-gray-500 dark:text-gray-400 sm:w-1/3">
                {{ field.key }}
              </dt>
              <dd class="text-sm text-gray-900 dark:text-white sm:w-2/3">
                {{ field.value }}
              </dd>
            </div>
          </div>
        </div>

        <!-- Attachments -->
        <div v-if="request.attachments && request.attachments.length > 0" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <RequestAttachments :attachments="request.attachments" />
        </div>

        <!-- Comments -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <RequestComments
            :comments="request.comments || []"
            :can-add-comment="canAddComment"
            @add-comment="handleAddComment"
          />
        </div>

        <!-- Edit History -->
        <div v-if="request.editHistory && request.editHistory.length > 0" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <RequestEditHistory :edit-history="request.editHistory" />
        </div>

        <!-- Quiz Section for REQUEST SUBMITTER -->
        <div v-if="isRequestSubmitter && requiresQuiz && hasQuizQuestions">
          <!-- Show quiz result if already completed -->
          <RequestQuizResult
            v-if="quizScore !== null && quizScore !== undefined"
            :score="quizScore"
            :passed="quizPassed"
            :passing-score="(currentStep?.passingScore || currentStep?.PassingScore || 70)"
          />

          <!-- Show quiz form if not yet completed or failed -->
          <div v-else-if="showQuizForm" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
            <RequestQuizForm
              :questions="(currentStep?.quizQuestions || currentStep?.QuizQuestions || [])"
              :request-id="requestId"
              :step-id="(currentStep?.id || currentStep?.Id)"
              :passing-score="(currentStep?.passingScore || currentStep?.PassingScore)"
              @submitted="handleQuizSubmitted"
              @cancel="showQuizForm = false"
            />
          </div>

          <!-- Show button to start quiz if not started yet -->
          <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              Quiz wymagany
            </h3>
            <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
              Ten etap wymaga zaliczenia quizu. Proszę wypełnić quiz poniżej.
            </p>
            <button
              type="button"
              class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition"
              @click="showQuizForm = true"
            >
              Rozpocznij quiz
            </button>
          </div>
        </div>

        <!-- Quiz Results for APPROVER (read-only view) -->
        <div v-if="isCurrentApprover && requiresQuiz && hasQuizQuestions" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Wyniki quizu wnioskodawcy
          </h3>

          <!-- Show quiz result if completed -->
          <div v-if="quizScore !== null && quizScore !== undefined">
            <RequestQuizResult
              :score="quizScore"
              :passed="quizPassed"
              :passing-score="(currentStep?.passingScore || currentStep?.PassingScore || 70)"
            />

            <!-- Display quiz questions and answers (read-only) -->
            <div class="mt-6">
              <h4 class="text-sm font-semibold text-gray-900 dark:text-white mb-3">
                Szczegółowe odpowiedzi
              </h4>
              <RequestQuizAnswersDisplay
                :questions="(currentStep?.quizQuestions || currentStep?.QuizQuestions || [])"
                :answers="(currentStep?.quizAnswers || currentStep?.QuizAnswers || [])"
              />
            </div>
          </div>

          <!-- Show waiting message if quiz not completed yet -->
          <div v-else class="p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
            <div class="flex items-start gap-3">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div>
                <p class="text-sm font-medium text-blue-900 dark:text-blue-100 mb-1">
                  Oczekiwanie na wypełnienie quizu przez wnioskodawcę
                </p>
                <p class="text-sm text-blue-800 dark:text-blue-200">
                  Wnioskodawca musi najpierw wypełnić wymagany quiz. Po wypełnieniu quizu przez wnioskodawcę, wyniki pojawią się tutaj.
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Action Buttons (only for current approver) -->
        <div v-if="isCurrentApprover" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Akcje
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
            Jesteś aktualnym opiniującym tego wniosku. Możesz go zatwierdzić lub odrzucić.
          </p>

          <!-- Warning if quiz is required but not completed/passed -->
          <div
            v-if="requiresQuiz && !quizPassed"
            class="mb-4 p-4 border rounded-lg"
            :class="{
              'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800':
                quizScore === null || quizScore === undefined,
              'bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-800':
                quizScore !== null && quizScore !== undefined
            }"
          >
            <div class="flex items-start gap-3">
              <svg
                class="w-5 h-5 mt-0.5 flex-shrink-0"
                :class="{
                  'text-blue-600 dark:text-blue-400': quizScore === null || quizScore === undefined,
                  'text-amber-600 dark:text-amber-400': quizScore !== null && quizScore !== undefined
                }"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
              <div>
                <p
                  class="text-sm font-medium mb-1"
                  :class="{
                    'text-blue-900 dark:text-blue-100': quizScore === null || quizScore === undefined,
                    'text-amber-900 dark:text-amber-100': quizScore !== null && quizScore !== undefined
                  }"
                >
                  {{ quizScore === null || quizScore === undefined ? 'Oczekiwanie na quiz' : 'Quiz niezaliczony' }}
                </p>
                <p
                  class="text-sm"
                  :class="{
                    'text-blue-800 dark:text-blue-200': quizScore === null || quizScore === undefined,
                    'text-amber-800 dark:text-amber-200': quizScore !== null && quizScore !== undefined
                  }"
                >
                  {{ quizScore === null || quizScore === undefined
                    ? 'Wnioskodawca musi najpierw wypełnić wymagany quiz. Możesz jednak podjąć decyzję o zatwierdzeniu lub odrzuceniu mimo to.'
                    : `Wnioskodawca nie zdał quizu (wynik: ${quizScore}%). Mimo to, jako opiniujący możesz podjąć decyzję o zatwierdzeniu lub odrzuceniu wniosku według własnego uznania.`
                  }}
                </p>
              </div>
            </div>
          </div>

          <div class="flex gap-3">
            <button
              :disabled="!canApprove"
              class="px-6 py-3 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium flex items-center gap-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              @click="showApproveModal = true"
            >
              <CheckCircle class="w-5 h-5" />
              Zatwierdź
            </button>
            <button
              class="px-6 py-3 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium flex items-center gap-2 transition-colors"
              @click="showRejectModal = true"
            >
              <XCircle class="w-5 h-5" />
              Odrzuć
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Approve Modal -->
    <div
      v-if="showApproveModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="showApproveModal = false"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full">
        <div class="p-6 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
            Zatwierdź wniosek
          </h3>
        </div>

        <div class="p-6 space-y-4">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Czy na pewno chcesz zatwierdzić ten wniosek?
          </p>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Komentarz (opcjonalnie)
            </label>
            <textarea
              v-model="approveComment"
              rows="3"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
              placeholder="Dodaj komentarz..."
            />
          </div>
        </div>

        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700">
          <button
            class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            :disabled="isSubmitting"
            @click="showApproveModal = false"
          >
            Anuluj
          </button>
          <button
            class="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="isSubmitting"
            @click="handleApprove"
          >
            {{ isSubmitting ? 'Zatwierdzanie...' : 'Zatwierdź' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Reject Modal -->
    <div
      v-if="showRejectModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="showRejectModal = false"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full">
        <div class="p-6 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
            Odrzuć wniosek
          </h3>
        </div>

        <div class="p-6 space-y-4">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Czy na pewno chcesz odrzucić ten wniosek? Podaj powód odrzucenia.
          </p>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Powód odrzucenia <span class="text-red-500">*</span>
            </label>
            <textarea
              v-model="rejectComment"
              rows="4"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
              placeholder="Wpisz powód odrzucenia wniosku..."
              required
            />
          </div>
        </div>

        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700">
          <button
            class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            :disabled="isSubmitting"
            @click="showRejectModal = false"
          >
            Anuluj
          </button>
          <button
            class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="isSubmitting || !rejectComment.trim()"
            @click="handleReject"
          >
            {{ isSubmitting ? 'Odrzucanie...' : 'Odrzuć' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
