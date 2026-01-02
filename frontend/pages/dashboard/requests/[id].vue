<script setup lang="ts">
import { onMounted } from 'vue'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const route = useRoute()
const router = useRouter()
const toast = useNotificationToast()

const requestId = route.params.id as string

const {
  request,
  template,
  isLoading,
  error,
  isRequestSubmitter,
  isCurrentApprover,
  currentStep,
  canAddComment,
  requiresQuiz,
  quizPassed,
  quizScore,
  hasQuizQuestions,
  loadRequest,
  handleApprove,
  handleReject,
  handleAddComment
} = useRequestDetails(requestId)

const handleBack = (): void => {
  router.push('/dashboard/requests')
}

const handleQuizSubmitted = async (result: { success: boolean; score: number; passed: boolean }): Promise<void> => {
  if (result.passed) {
    toast.success(`Quiz zaliczony! Wynik: ${result.score}%`)
    await loadRequest()
  } else {
    toast.error(`Quiz niezaliczony. Wynik: ${result.score}%`)
  }
}

onMounted(() => {
  loadRequest()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div v-if="isLoading" class="flex items-center justify-center h-64">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
          <p class="mt-4 text-gray-600 dark:text-gray-400">Ladowanie wniosku...</p>
        </div>
      </div>

      <div
        v-else-if="error"
        class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center"
      >
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
          Blad
        </h3>
        <p class="text-gray-600 dark:text-gray-400">
          {{ error }}
        </p>
      </div>

      <div v-else-if="request" class="space-y-6">
        <RequestHeader
          :request="request"
          @back="handleBack"
        />

        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <RequestTimeline :steps="request.approvalSteps" />
        </div>

        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <RequestFormDataDisplay
            :form-data="request.formData"
            :fields="template?.fields"
          />
        </div>

        <div
          v-if="request.attachments && request.attachments.length > 0"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
        >
          <RequestAttachments :attachments="request.attachments" />
        </div>

        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
          <RequestComments
            :comments="request.comments || []"
            :can-add-comment="canAddComment"
            @add-comment="handleAddComment"
          />
        </div>

        <div
          v-if="request.editHistory && request.editHistory.length > 0"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
        >
          <RequestEditHistory :edit-history="request.editHistory" />
        </div>

        <QuizSection
          :request-id="requestId"
          :current-step="currentStep"
          :is-request-submitter="isRequestSubmitter"
          :requires-quiz="requiresQuiz"
          :quiz-passed="quizPassed"
          :quiz-score="quizScore"
          :has-quiz-questions="hasQuizQuestions"
          @quiz-submitted="handleQuizSubmitted"
        />

        <ApproverQuizView
          :current-step="currentStep"
          :is-current-approver="isCurrentApprover"
          :requires-quiz="requiresQuiz"
          :quiz-passed="quizPassed"
          :quiz-score="quizScore"
          :has-quiz-questions="hasQuizQuestions"
        />

        <ApprovalActions
          :request="request"
          :current-step="currentStep"
          :is-current-approver="isCurrentApprover"
          :requires-quiz="requiresQuiz"
          :quiz-passed="quizPassed"
          :quiz-score="quizScore"
          @approve="handleApprove"
          @reject="handleReject"
        />
      </div>
    </div>
  </div>
</template>
