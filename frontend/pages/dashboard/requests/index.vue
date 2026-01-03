<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Wnioski
        </h1>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
          Skladaj nowe wnioski i sledz status swoich zgloszen
        </p>
      </div>

      <RequestTabs
        v-model="activeTab"
        :my-requests-count="myRequests.length"
        :pending-approvals-count="pendingApprovals.length"
        :approvals-history-count="approvalsHistory.length"
      />

      <div v-if="activeTab === 'new'" class="space-y-6">
        <TemplateSelector
          :templates="templates"
          :loading="loadingTemplates"
          :search-query="templateSearch"
          @select="handleSelectTemplate"
          @update:search-query="templateSearch = $event"
        />
      </div>

      <div v-else-if="activeTab === 'my-requests'" class="space-y-6">
        <RequestFilters
          v-model="requestSearch"
          :status-value="statusFilter"
          :show-status-filter="true"
          search-placeholder="Szukaj wniosków..."
          @update:status-value="statusFilter = $event"
        />

        <RequestList
          :requests="filteredRequests"
          :loading="loadingRequests"
          empty-title="Brak wniosków"
          empty-message="Nie masz jeszcze żadnych wniosków. Złóż pierwszy wniosek klikając w zakładkę 'Nowy wniosek'"
        />
      </div>

      <div v-else-if="activeTab === 'to-approve'" class="space-y-6">
        <RequestFilters
          v-model="approvalSearch"
          search-placeholder="Szukaj wniosków..."
        />

        <PendingApprovalList
          :requests="filteredApprovals"
          :loading="loadingApprovals"
          @approve="openApproveModal"
          @reject="openRejectModal"
        />
      </div>

      <div v-else-if="activeTab === 'approved-by-me'" class="space-y-6">
        <ApprovalHistoryList
          :items="approvalsHistory"
          :loading="loadingApprovals"
          @click="openRequestDetailsModal"
        />
      </div>
    </div>

    <QuizModal
      v-if="showQuizModal && selectedQuizStep"
      :questions="quizQuestions"
      :passing-score="quizPassingScore"
      @close="closeQuizModal"
      @submit="handleQuizSubmit"
    />

    <RequestDetailsModal
      v-if="showRequestDetailsModal"
      :request="requestDetails"
      :template="requestTemplate"
      @close="closeRequestDetailsModal"
    />

    <ApproveRequestModal
      v-if="showApproveModal"
      :request="selectedRequest"
      :loading="approving"
      @close="closeApproveModal"
      @confirm="handleApprove"
    />

    <RejectRequestModal
      v-if="showRejectModal"
      :request="selectedRequest"
      :loading="rejecting"
      @close="closeRejectModal"
      @confirm="handleReject"
    />
  </div>
</template>

<script setup lang="ts">
import type {
  RequestTemplate,
  Request,
  RequestApprovalStep,
  ApprovalHistoryItem,
  RequestTab,
  QuizQuestion,
  RequestWithDetails
} from '~/types/requests'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const {
  getAvailableTemplates,
  getMyRequests,
  getPendingApprovals,
  approveRequestStep,
  rejectRequestStep,
  getRequestById,
  getTemplateById,
  getApprovalsHistory
} = useRequestsApi()

const activeTab = ref<RequestTab>('new')
const templates = ref<RequestTemplate[]>([])
const myRequests = ref<Request[]>([])
const pendingApprovals = ref<Request[]>([])
const approvalsHistory = ref<ApprovalHistoryItem[]>([])
const loadingTemplates = ref(true)
const loadingRequests = ref(true)
const loadingApprovals = ref(true)
const templateSearch = ref('')
const requestSearch = ref('')
const approvalSearch = ref('')
const statusFilter = ref('')
const selectedRequest = ref<Request | null>(null)
const showQuizModal = ref(false)
const selectedQuizStep = ref<RequestApprovalStep | null>(null)
const quizQuestions = ref<QuizQuestion[]>([])
const quizPassingScore = ref(80)

const showRequestDetailsModal = ref(false)
const requestDetails = ref<RequestWithDetails | null>(null)
const requestTemplate = ref<RequestTemplate | null>(null)

const showApproveModal = ref(false)
const showRejectModal = ref(false)
const approving = ref(false)
const rejecting = ref(false)

const filteredRequests = computed((): Request[] => {
  let result = myRequests.value

  if (statusFilter.value) {
    result = result.filter((r: Request) => r.status === statusFilter.value)
  }

  if (requestSearch.value) {
    const query = requestSearch.value.toLowerCase()
    result = result.filter(
      (r: Request) =>
        r.requestNumber.toLowerCase().includes(query) ||
        r.requestTemplateName.toLowerCase().includes(query)
    )
  }

  return result
})

const filteredApprovals = computed((): Request[] => {
  if (!approvalSearch.value) return pendingApprovals.value

  const query = approvalSearch.value.toLowerCase()
  return pendingApprovals.value.filter(
    (r: Request) =>
      r.requestNumber.toLowerCase().includes(query) ||
      r.requestTemplateName.toLowerCase().includes(query) ||
      r.submittedByName?.toLowerCase().includes(query)
  )
})

const handleSelectTemplate = (template: RequestTemplate): void => {
  navigateTo(`/dashboard/requests/submit/${template.id}`)
}

const openRequestDetailsModal = async (requestId: string): Promise<void> => {
  try {
    showRequestDetailsModal.value = true
    requestDetails.value = null
    requestTemplate.value = null

    const details = await getRequestById(requestId)
    requestDetails.value = details

    if (details.requestTemplateId) {
      try {
        requestTemplate.value = await getTemplateById(details.requestTemplateId)
      } catch (e) {
        console.warn('Failed to load request template for details mapping', e)
      }
    }
  } catch (err) {
    console.error('Error loading request details:', err)
    showRequestDetailsModal.value = false
  }
}

const closeRequestDetailsModal = (): void => {
  showRequestDetailsModal.value = false
  requestDetails.value = null
  requestTemplate.value = null
}

const closeQuizModal = (): void => {
  showQuizModal.value = false
  selectedQuizStep.value = null
}

const handleQuizSubmit = async (
  _score: number,
  _passed: boolean,
  _answers: Record<string, string>
): Promise<void> => {
  closeQuizModal()
  await loadMyRequests()
}

const loadTemplates = async (): Promise<void> => {
  try {
    loadingTemplates.value = true
    templates.value = await getAvailableTemplates()
  } catch (error) {
    console.error('Error loading templates:', error)
  } finally {
    loadingTemplates.value = false
  }
}

const loadMyRequests = async (): Promise<void> => {
  try {
    loadingRequests.value = true
    myRequests.value = await getMyRequests()
  } catch (error) {
    console.error('Error loading requests:', error)
  } finally {
    loadingRequests.value = false
  }
}

const loadPendingApprovals = async (): Promise<void> => {
  try {
    loadingApprovals.value = true
    pendingApprovals.value = await getPendingApprovals()
  } catch (error) {
    console.error('Error loading pending approvals:', error)
  } finally {
    loadingApprovals.value = false
  }
}

const loadApprovalsHistory = async (): Promise<void> => {
  try {
    loadingApprovals.value = true
    approvalsHistory.value = await getApprovalsHistory()
  } catch (error) {
    console.error('Error loading approvals history:', error)
  } finally {
    loadingApprovals.value = false
  }
}

const getCurrentStep = (request: Request): RequestApprovalStep | undefined => {
  return request.approvalSteps.find(
    (step: RequestApprovalStep) => step.status === 'InReview'
  )
}

const openApproveModal = (request: Request): void => {
  selectedRequest.value = request
  showApproveModal.value = true
}

const closeApproveModal = (): void => {
  showApproveModal.value = false
  selectedRequest.value = null
}

const openRejectModal = (request: Request): void => {
  selectedRequest.value = request
  showRejectModal.value = true
}

const closeRejectModal = (): void => {
  showRejectModal.value = false
  selectedRequest.value = null
}

const handleApprove = async (comment: string): Promise<void> => {
  if (!selectedRequest.value) return

  const currentStep = getCurrentStep(selectedRequest.value)
  if (!currentStep) return

  approving.value = true
  try {
    await approveRequestStep(selectedRequest.value.id, currentStep.id, {
      comment: comment || undefined
    })

    closeApproveModal()
    await loadPendingApprovals()
  } catch (err) {
    console.error('Error approving request:', err)
  } finally {
    approving.value = false
  }
}

const handleReject = async (reason: string): Promise<void> => {
  if (!selectedRequest.value || !reason.trim()) return

  const currentStep = getCurrentStep(selectedRequest.value)
  if (!currentStep) return

  rejecting.value = true
  try {
    await rejectRequestStep(selectedRequest.value.id, currentStep.id, {
      reason
    })

    closeRejectModal()
    await loadPendingApprovals()
  } catch (err) {
    console.error('Error rejecting request:', err)
  } finally {
    rejecting.value = false
  }
}

onMounted(() => {
  loadTemplates()
  loadMyRequests()
  loadPendingApprovals()
  loadApprovalsHistory()
})
</script>
