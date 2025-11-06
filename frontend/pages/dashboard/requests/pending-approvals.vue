<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
          Wnioski do zatwierdzenia
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-300">
          Wnioski oczekujące na Twoją decyzję
        </p>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <Icon name="svg-spinners:ring-resize" class="w-12 h-12 text-blue-600" />
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
      <div class="flex items-center gap-3">
        <Icon name="heroicons:exclamation-circle" class="w-6 h-6 text-red-600 dark:text-red-400" />
        <div>
          <h3 class="font-semibold text-red-900 dark:text-red-100">Błąd</h3>
          <p class="text-sm text-red-700 dark:text-red-300">{{ error }}</p>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="requests.length === 0" class="text-center py-12">
      <Icon name="heroicons:check-circle" class="w-16 h-16 mx-auto text-green-500 mb-4" />
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
        Brak wniosków do zatwierdzenia
      </h3>
      <p class="text-gray-600 dark:text-gray-300">
        Wszystkie wnioski zostały przetworzone
      </p>
    </div>

    <!-- Requests List -->
    <div v-else class="space-y-4">
      <div
        v-for="request in requests"
        :key="request.id"
        class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 hover:shadow-md transition-shadow"
      >
        <div class="flex items-start justify-between gap-4">
          <!-- Request Info -->
          <div class="flex-1">
            <div class="flex items-center gap-3 mb-2">
              <Icon :name="request.requestTemplateIcon" class="w-6 h-6 text-blue-600" />
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                {{ request.requestTemplateName }}
              </h3>
              <span
                class="px-2 py-1 text-xs font-medium rounded-full"
                :class="getPriorityClass(request.priority)"
              >
                {{ request.priority === 'Urgent' ? 'Pilne' : 'Standardowy' }}
              </span>
            </div>

            <div class="space-y-1 text-sm text-gray-600 dark:text-gray-300">
              <p>
                <span class="font-medium">Numer:</span> {{ request.requestNumber }}
              </p>
              <p>
                <span class="font-medium">Wnioskodawca:</span> {{ request.submittedByName }}
              </p>
              <p>
                <span class="font-medium">Data złożenia:</span> {{ formatDate(request.submittedAt) }}
              </p>
            </div>

            <!-- Current Step Info -->
            <div class="mt-4 p-3 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg border border-yellow-200 dark:border-yellow-800">
              <p class="text-sm font-medium text-yellow-900 dark:text-yellow-100">
                Oczekuje na Twoją decyzję
              </p>
              <p class="text-xs text-yellow-700 dark:text-yellow-300 mt-1">
                Krok {{ getCurrentStep(request)?.stepOrder }} z {{ request.approvalSteps.length }}
              </p>
            </div>

            <!-- Quiz Result (if completed) -->
            <div
              v-if="getQuizResult(request)"
              class="mt-3 rounded-lg border overflow-hidden"
              :class="[
                getQuizResult(request)?.passed
                  ? 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800'
                  : 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800'
              ]"
            >
              <button
                class="w-full p-3 text-left flex items-center justify-between hover:opacity-80 transition-opacity"
                @click="toggleQuizDetails(request.id)"
              >
                <div>
                  <p class="text-xs font-medium" :class="[
                    getQuizResult(request)?.passed
                      ? 'text-green-900 dark:text-green-100'
                      : 'text-red-900 dark:text-red-100'
                  ]">
                    📋 Quiz {{ getQuizResult(request)?.passed ? 'zaliczony' : 'niezaliczony' }}
                  </p>
                  <p class="text-xs mt-1" :class="[
                    getQuizResult(request)?.passed
                      ? 'text-green-700 dark:text-green-300'
                      : 'text-red-700 dark:text-red-300'
                  ]">
                    Wynik: {{ getQuizResult(request)?.score }}% (wymagane: {{ getQuizResult(request)?.requiredScore }}%)
                  </p>
                </div>
                <Icon
                  :name="expandedQuizzes.has(request.id) ? 'heroicons:chevron-up' : 'heroicons:chevron-down'"
                  class="w-4 h-4"
                  :class="[
                    getQuizResult(request)?.passed
                      ? 'text-green-700 dark:text-green-300'
                      : 'text-red-700 dark:text-red-300'
                  ]"
                />
              </button>

              <!-- Quiz Details (expandable) -->
              <div
                v-if="expandedQuizzes.has(request.id)"
                class="border-t px-3 py-2 space-y-2"
                :class="[
                  getQuizResult(request)?.passed
                    ? 'border-green-200 dark:border-green-800 bg-white dark:bg-gray-800'
                    : 'border-red-200 dark:border-red-800 bg-white dark:bg-gray-800'
                ]"
              >
                <div
                  v-for="(question, index) in getQuizQuestions(request)"
                  :key="question.id"
                  class="text-xs"
                >
                  <p class="font-medium text-gray-900 dark:text-white mb-1">
                    {{ index + 1 }}. {{ question.question }}
                  </p>
                  <div class="ml-4 space-y-1">
                    <div
                      v-for="option in getQuizOptions(question)"
                      :key="option.value"
                      class="flex items-center gap-2"
                    >
                      <span
                        v-if="isSelectedAnswer(request, question.id, option.value)"
                        :class="[
                          'font-medium',
                          isCorrectOption(option) ? 'text-green-700 dark:text-green-400' : 'text-red-700 dark:text-red-400'
                        ]"
                      >
                        {{ isCorrectOption(option) ? '✓' : '✗' }}
                      </span>
                      <span
                        v-else-if="isCorrectOption(option)"
                        class="text-green-700 dark:text-green-400 font-medium"
                      >
                        ✓
                      </span>
                      <span v-else class="w-4"></span>
                      <span
                        :class="[
                          isSelectedAnswer(request, question.id, option.value)
                            ? 'font-medium ' + (isCorrectOption(option) ? 'text-green-900 dark:text-green-100' : 'text-red-900 dark:text-red-100')
                            : isCorrectOption(option)
                            ? 'text-gray-600 dark:text-gray-400 italic'
                            : 'text-gray-600 dark:text-gray-400'
                        ]"
                      >
                        {{ option.label }}
                        <span v-if="isSelectedAnswer(request, question.id, option.value)" class="font-medium">
                          (wybrana)
                        </span>
                        <span v-else-if="isCorrectOption(option)" class="text-xs">
                          (poprawna)
                        </span>
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex flex-col gap-2">
            <!-- Quiz Button (if required and not yet completed) -->
            <button
              v-if="needsQuiz(request)"
              class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
              @click="openQuizModal(request)"
            >
              <Icon name="heroicons:clipboard-document-check" class="w-5 h-5" />
              Wypełnij quiz
            </button>

            <!-- Approve Button (always enabled - approver decides) -->
            <button
              class="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
              @click="openApproveModal(request)"
            >
              <Icon name="heroicons:check" class="w-5 h-5" />
              Zatwierdź
            </button>

            <button
              class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
              @click="openRejectModal(request)"
            >
              <Icon name="heroicons:x-mark" class="w-5 h-5" />
              Odrzuć
            </button>
            <NuxtLink
              :to="`/dashboard/requests/${request.id}`"
              class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors text-center"
            >
              Szczegóły
            </NuxtLink>
          </div>
        </div>
      </div>
    </div>

    <!-- Approve Modal -->
    <Teleport to="body">
      <div
        v-if="showApproveModal"
        class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
        @click.self="closeApproveModal"
      >
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full p-6">
          <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-4">
            Zatwierdź wniosek
          </h3>
          <p class="text-gray-600 dark:text-gray-300 mb-4">
            Czy na pewno chcesz zatwierdzić wniosek <strong>{{ selectedRequest?.requestNumber }}</strong>?
          </p>
          
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Komentarz (opcjonalny)
            </label>
            <textarea
              v-model="approveComment"
              rows="3"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
              placeholder="Dodaj komentarz..."
            />
          </div>

          <div class="flex gap-3">
            <button
              :disabled="approving"
              class="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white rounded-lg font-medium transition-colors"
              @click="handleApprove"
            >
              {{ approving ? 'Zatwierdzanie...' : 'Zatwierdź' }}
            </button>
            <button
              :disabled="approving"
              class="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
              @click="closeApproveModal"
            >
              Anuluj
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Quiz Modal -->
    <QuizModal
      v-if="selectedQuizRequest"
      :show="showQuizModal"
      :questions="getQuizQuestions(selectedQuizRequest)"
      :passing-score="getPassingScore(selectedQuizRequest)"
      :request-id="selectedQuizRequest.id"
      :step-id="getCurrentStep(selectedQuizRequest)?.id || ''"
      @close="closeQuizModal"
      @quiz-completed="handleQuizCompleted"
    />

    <!-- Reject Modal -->
    <Teleport to="body">
      <div
        v-if="showRejectModal"
        class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
        @click.self="closeRejectModal"
      >
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full p-6">
          <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-4">
            Odrzuć wniosek
          </h3>
          <p class="text-gray-600 dark:text-gray-300 mb-4">
            Podaj powód odrzucenia wniosku <strong>{{ selectedRequest?.requestNumber }}</strong>:
          </p>
          
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Powód odrzucenia *
            </label>
            <textarea
              v-model="rejectReason"
              rows="4"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-red-500 dark:bg-gray-700 dark:text-white"
              placeholder="Opisz powód odrzucenia..."
              required
            />
          </div>

          <div class="flex gap-3">
            <button
              :disabled="rejecting || !rejectReason.trim()"
              class="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-gray-400 text-white rounded-lg font-medium transition-colors"
              @click="handleReject"
            >
              {{ rejecting ? 'Odrzucanie...' : 'Odrzuć' }}
            </button>
            <button
              :disabled="rejecting"
              class="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
              @click="closeRejectModal"
            >
              Anuluj
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import type { Request, RequestApprovalStep } from '~/types/requests'

definePageMeta({
  layout: 'default',
  middleware: 'auth'
})

const { getPendingApprovals, approveRequestStep, rejectRequestStep } = useRequestsApi()

const requests = ref<Request[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const showApproveModal = ref(false)
const showRejectModal = ref(false)
const showQuizModal = ref(false)
const selectedRequest = ref<Request | null>(null)
const selectedQuizRequest = ref<Request | null>(null)
const approveComment = ref('')
const rejectReason = ref('')
const approving = ref(false)
const rejecting = ref(false)
const expandedQuizzes = ref<Set<string>>(new Set())

const fetchRequests = async () => {
  loading.value = true
  error.value = null
  try {
    requests.value = await getPendingApprovals()
  } catch (err: any) {
    error.value = err.message || 'Nie udało się pobrać wniosków'
  } finally {
    loading.value = false
  }
}

const getCurrentStep = (request: Request): RequestApprovalStep | undefined => {
  return request.approvalSteps.find(step => step.status === 'InReview')
}

const getPriorityClass = (priority: string) => {
  return priority === 'Urgent'
    ? 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
    : 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300'
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const openApproveModal = (request: Request) => {
  selectedRequest.value = request
  approveComment.value = ''
  showApproveModal.value = true
}

const closeApproveModal = () => {
  showApproveModal.value = false
  selectedRequest.value = null
  approveComment.value = ''
}

const openRejectModal = (request: Request) => {
  selectedRequest.value = request
  rejectReason.value = ''
  showRejectModal.value = true
}

const closeRejectModal = () => {
  showRejectModal.value = false
  selectedRequest.value = null
  rejectReason.value = ''
}

const handleApprove = async () => {
  if (!selectedRequest.value) return
  
  const currentStep = getCurrentStep(selectedRequest.value)
  if (!currentStep) return

  approving.value = true
  try {
    await approveRequestStep(
      selectedRequest.value.id,
      currentStep.id,
      { comment: approveComment.value || undefined }
    )
    
    closeApproveModal()
    await fetchRequests()
  } catch (err: any) {
    error.value = err.message || 'Nie udało się zatwierdzić wniosku'
  } finally {
    approving.value = false
  }
}

const handleReject = async () => {
  if (!selectedRequest.value || !rejectReason.value.trim()) return

  const currentStep = getCurrentStep(selectedRequest.value)
  if (!currentStep) return

  rejecting.value = true
  try {
    await rejectRequestStep(
      selectedRequest.value.id,
      currentStep.id,
      { reason: rejectReason.value }
    )

    closeRejectModal()
    await fetchRequests()
  } catch (err: any) {
    error.value = err.message || 'Nie udało się odrzucić wniosku'
  } finally {
    rejecting.value = false
  }
}

const needsQuiz = (request: Request): boolean => {
  const currentStep = getCurrentStep(request)
  if (!currentStep) return false

  // Show quiz button only if quiz is required and hasn't been attempted yet
  return currentStep.requiresQuiz && currentStep.quizScore === null
}

const getQuizResult = (request: Request) => {
  const currentStep = getCurrentStep(request)
  if (!currentStep || !currentStep.requiresQuiz || currentStep.quizScore === null) {
    return null
  }

  return {
    score: currentStep.quizScore,
    passed: currentStep.quizPassed || false,
    requiredScore: currentStep.passingScore || 70
  }
}

const getQuizQuestions = (request: Request) => {
  const currentStep = getCurrentStep(request)
  return currentStep?.quizQuestions || []
}

const getPassingScore = (request: Request): number => {
  const currentStep = getCurrentStep(request)
  return currentStep?.passingScore || 70
}

const openQuizModal = (request: Request) => {
  selectedQuizRequest.value = request
  showQuizModal.value = true
}

const closeQuizModal = () => {
  showQuizModal.value = false
  selectedQuizRequest.value = null
}

const handleQuizCompleted = async () => {
  closeQuizModal()
  await fetchRequests()
}

const toggleQuizDetails = (requestId: string) => {
  if (expandedQuizzes.value.has(requestId)) {
    expandedQuizzes.value.delete(requestId)
  } else {
    expandedQuizzes.value.add(requestId)
  }
  // Trigger reactivity by creating new Set
  expandedQuizzes.value = new Set(expandedQuizzes.value)
}

const getQuizOptions = (question: any) => {
  try {
    return JSON.parse(question.options)
  } catch {
    return []
  }
}

const isSelectedAnswer = (request: Request, questionId: string, optionValue: string): boolean => {
  const currentStep = getCurrentStep(request)
  if (!currentStep?.quizAnswers) return false

  const answer = currentStep.quizAnswers.find(a => a.questionId === questionId)
  return answer?.selectedAnswer === optionValue
}

const isCorrectOption = (option: any): boolean => {
  return option.isCorrect === true
}

onMounted(() => {
  fetchRequests()
})
</script>

