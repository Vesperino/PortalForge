<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
          Wnioski
        </h1>
        <p class="text-gray-600 dark:text-gray-400">
          Składaj nowe wnioski i śledź status swoich zgłoszeń
        </p>
      </div>

      <!-- Tabs -->
      <div class="mb-6 border-b border-gray-200 dark:border-gray-700">
        <nav class="-mb-px flex space-x-8">
          <button
            @click="activeTab = 'new'"
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'new'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
            ]"
          >
            <Plus class="w-4 h-4 inline mr-2" />
            Nowy wniosek
          </button>
          <button
            @click="activeTab = 'my-requests'"
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'my-requests'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
            ]"
          >
            <List class="w-4 h-4 inline mr-2" />
            Moje wnioski
            <span v-if="myRequests.length > 0" class="ml-2 px-2 py-0.5 text-xs bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 rounded-full">
              {{ myRequests.length }}
            </span>
          </button>
          <button
            v-if="canApproveRequests"
            @click="activeTab = 'to-approve'"
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'to-approve'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
            ]"
          >
            <ClipboardList class="w-4 h-4 inline mr-2" />
            Do zatwierdzenia
            <span v-if="pendingApprovals.length > 0" class="ml-2 px-2 py-0.5 text-xs bg-orange-100 dark:bg-orange-900 text-orange-800 dark:text-orange-200 rounded-full">
              {{ pendingApprovals.length }}
            </span>
          </button>

          <button
            v-if="canApproveRequests"
            @click="activeTab = 'approved-by-me'"
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'approved-by-me'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
            ]"
          >
            Zatwierdzone przeze mnie
          </button>
        </nav>
      </div>

      <!-- Tab: Nowy wniosek -->
      <div v-if="activeTab === 'new'" class="space-y-6">
        <!-- Search & Filters -->
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1">
            <input
              v-model="templateSearch"
              type="text"
              placeholder="Szukaj szablonów..."
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-800 dark:text-white"
            >
          </div>
        </div>

        <!-- Templates Grid -->
        <div v-if="loadingTemplates" class="text-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        </div>

        <div v-else-if="filteredTemplates.length === 0" class="text-center py-12">
          <FileText class="w-16 h-16 mx-auto text-gray-400 mb-4" />
          <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
            Brak dostępnych szablonów
          </h3>
          <p class="text-gray-600 dark:text-gray-400">
            Nie znaleziono szablonów spełniających kryteria wyszukiwania
          </p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <button
            v-for="template in filteredTemplates"
            :key="template.id"
            @click="selectTemplate(template)"
            class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 hover:shadow-md hover:border-blue-500 transition-all text-left p-6 group"
          >
            <div class="flex items-start justify-between mb-4">
              <Icon
                :name="getIconifyName(template.icon)"
                class="w-12 h-12 group-hover:scale-110 transition-transform"
              />
              <span class="px-2 py-1 text-xs font-medium bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 rounded-full">
                {{ template.category }}
              </span>
            </div>

            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              {{ template.name }}
            </h3>

            <p class="text-sm text-gray-600 dark:text-gray-400 mb-4 line-clamp-2">
              {{ template.description }}
            </p>

            <div class="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
              <span v-if="template.estimatedProcessingDays">
                <Clock class="w-3 h-3 inline mr-1" />
                {{ template.estimatedProcessingDays }} dni
              </span>
              <span>
                {{ template.fields.length }} pól
              </span>
            </div>
          </button>
        </div>
      </div>

      <!-- Tab: Moje wnioski -->
      <div v-else-if="activeTab === 'my-requests'" class="space-y-6">
        <!-- Filters -->
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1">
            <input
              v-model="requestSearch"
              type="text"
              placeholder="Szukaj wniosków..."
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-800 dark:text-white"
            >
          </div>
          <select
            v-model="statusFilter"
            class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-800 dark:text-white"
          >
            <option value="">Wszystkie statusy</option>
            <option value="InReview">W trakcie</option>
            <option value="Approved">Zatwierdzone</option>
            <option value="Rejected">Odrzucone</option>
            <option value="AwaitingSurvey">Wymaga quizu</option>
          </select>
        </div>

        <!-- Requests List -->
        <div v-if="loadingRequests" class="text-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        </div>

        <div v-else-if="filteredRequests.length === 0" class="text-center py-12">
          <ClipboardList class="w-16 h-16 mx-auto text-gray-400 mb-4" />
          <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
            Brak wniosków
          </h3>
          <p class="text-gray-600 dark:text-gray-400">
            Nie masz jeszcze żadnych wniosków. Złóż pierwszy wniosek klikając w zakładkę "Nowy wniosek"
          </p>
        </div>

        <div v-else class="space-y-4">
          <NuxtLink
            v-for="request in filteredRequests"
            :key="request.id"
            :to="`/dashboard/requests/${request.id}`"
            class="block w-full bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 hover:shadow-lg hover:border-blue-500 dark:hover:border-blue-400 transition-all group"
          >
            <div class="flex items-start justify-between mb-4">
              <div class="flex items-start gap-4">
                <div class="relative">
                  <Icon
                    :name="getIconifyName(request.requestTemplateIcon)"
                    class="w-12 h-12 flex-shrink-0 transition-transform group-hover:scale-110"
                  />
                </div>
                <div>
                  <h3 class="text-lg font-semibold text-gray-900 dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                    {{ request.requestTemplateName }}
                  </h3>
                  <p class="text-sm text-gray-600 dark:text-gray-400">
                    {{ request.requestNumber }}
                  </p>
                </div>
              </div>

              <span
                :class="[
                  'px-3 py-1 text-sm font-medium rounded-full',
                  getStatusBadgeClass(request.status)
                ]"
              >
                {{ getStatusLabel(request.status) }}
              </span>
            </div>

            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4 text-sm">
              <div>
                <p class="text-gray-500 dark:text-gray-400">Data złożenia</p>
                <p class="font-medium text-gray-900 dark:text-white">
                  {{ formatDate(request.submittedAt) }}
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
              <div>
                <p class="text-gray-500 dark:text-gray-400">Etap</p>
                <p class="font-medium text-gray-900 dark:text-white">
                  {{ getCurrentStepLabel(request) }}
                </p>
              </div>
              <div>
                <p class="text-gray-500 dark:text-gray-400">Postęp</p>
                <p class="font-medium text-gray-900 dark:text-white">
                  {{ getApprovalProgress(request) }}
                </p>
              </div>
            </div>

            <div class="flex items-center text-blue-600 dark:text-blue-400 group-hover:text-blue-700 dark:group-hover:text-blue-300 font-medium text-sm">
              <span>Zobacz szczegóły</span>
              <svg class="w-4 h-4 ml-2 transition-transform group-hover:translate-x-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </div>
          </NuxtLink>
        </div>
      </div>

      <!-- Tab: Do zatwierdzenia -->
      <div v-else-if="activeTab === 'to-approve'" class="space-y-6">
        <!-- Search -->
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1">
            <input
              v-model="approvalSearch"
              type="text"
              placeholder="Szukaj wniosków..."
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-800 dark:text-white"
            >
          </div>
        </div>

        <!-- Loading State -->
        <div v-if="loadingApprovals" class="text-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        </div>

        <!-- Empty State -->
        <div v-else-if="filteredApprovals.length === 0" class="text-center py-12">
          <Icon name="heroicons:check-circle" class="w-16 h-16 mx-auto text-green-500 mb-4" />
          <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
            Brak wniosków do zatwierdzenia
          </h3>
          <p class="text-gray-600 dark:text-gray-400">
            Wszystkie wnioski zostały przetworzone
          </p>
        </div>

        <!-- Approvals List -->
        <div v-else class="space-y-4">
          <div
            v-for="request in filteredApprovals"
            :key="request.id"
            class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 hover:shadow-md transition-shadow"
          >
            <div class="flex items-start justify-between gap-4">
              <!-- Request Info -->
              <div class="flex-1">
                <div class="flex items-center gap-3 mb-2">
                  <Icon
                    :name="getIconifyName(request.requestTemplateIcon)"
                    class="w-6 h-6 text-blue-600"
                  />
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

                <div class="space-y-1 text-sm text-gray-600 dark:text-gray-300 mb-4">
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
                <div class="p-3 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg border border-yellow-200 dark:border-yellow-800">
                  <p class="text-sm font-medium text-yellow-900 dark:text-yellow-100">
                    Oczekuje na Twoją decyzję
                  </p>
                  <p class="text-xs text-yellow-700 dark:text-yellow-300 mt-1">
                    {{ getCurrentStepLabel(request) }}
                  </p>
                </div>
              </div>

              <!-- Actions -->
              <div class="flex flex-col gap-2">
                <button
                  @click="openApproveModal(request)"
                  class="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
                >
                  <Icon name="heroicons:check" class="w-5 h-5" />
                  Zatwierdź
                </button>
                <button
                  @click="openRejectModal(request)"
                  class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-colors flex items-center gap-2"
                >
                  <X class="w-5 h-5" />
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
      </div>

      <!-- Tab: Zatwierdzone przeze mnie -->
      <div v-else-if="activeTab === 'approved-by-me'" class="space-y-6">
        <div v-if="loadingApprovals" class="text-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        </div>

        <div v-else-if="approvalsHistory.length === 0" class="text-center py-12 text-gray-500 dark:text-gray-400">
          Brak historii zatwierdzonych/odrzuconych wniosków.
        </div>

        <div v-else class="space-y-4">
          <div
            v-for="item in approvalsHistory"
            :key="item.id"
            class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-4"
          >
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-500 dark:text-gray-400">{{ item.requestNumber }}</p>
                <p class="font-medium text-gray-900 dark:text-white">{{ item.templateName }}</p>
              </div>
              <div class="text-sm" :class="item.decision === 'Approved' ? 'text-green-600' : 'text-red-600'">
                {{ item.decision === 'Approved' ? 'Zatwierdzono' : 'Odrzucono' }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Quiz Modal -->
    <QuizModal
      v-if="showQuizModal && selectedQuizStep"
      :questions="quizQuestions"
      :passing-score="quizPassingScore"
      @close="closeQuizModal"
      @submit="handleQuizSubmit"
    />

    <!-- Request Details Modal -->
    <div
      v-if="selectedRequest"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
      @click.self="closeRequestDetails"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div class="p-6 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between sticky top-0 bg-white dark:bg-gray-800">
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
            Szczegóły wniosku
          </h2>
          <button
            @click="closeRequestDetails"
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
          >
            <X class="w-6 h-6" />
          </button>
        </div>

        <div class="p-6 space-y-6">
          <!-- Request Info -->
          <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-4">
            <h3 class="font-semibold text-gray-900 dark:text-white mb-3">Informacje o wniosku</h3>
            <div class="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p class="text-gray-500 dark:text-gray-400">Numer wniosku</p>
                <p class="font-medium text-gray-900 dark:text-white">{{ selectedRequest.requestNumber }}</p>
              </div>
              <div>
                <p class="text-gray-500 dark:text-gray-400">Status</p>
                <p class="font-medium text-gray-900 dark:text-white">{{ getStatusLabel(selectedRequest.status) }}</p>
              </div>
            </div>
          </div>

          <!-- Timeline -->
          <RequestTimeline :steps="selectedRequest.approvalSteps" />

          <!-- Form Data -->
          <div>
            <h3 class="font-semibold text-gray-900 dark:text-white mb-3">Wypełniony formularz</h3>
            <pre class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-4 text-sm overflow-x-auto">{{ JSON.parse(selectedRequest.formData) }}</pre>
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
            ></textarea>
          </div>

          <div class="flex gap-3">
            <button
              @click="handleApprove"
              :disabled="approving"
              class="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white rounded-lg font-medium transition-colors"
            >
              {{ approving ? 'Zatwierdzanie...' : 'Zatwierdź' }}
            </button>
            <button
              @click="closeApproveModal"
              :disabled="approving"
              class="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
            >
              Anuluj
            </button>
          </div>
        </div>
      </div>
    </Teleport>

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
            ></textarea>
          </div>

          <div class="flex gap-3">
            <button
              @click="handleReject"
              :disabled="rejecting || !rejectReason.trim()"
              class="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-gray-400 text-white rounded-lg font-medium transition-colors"
            >
              {{ rejecting ? 'Odrzucanie...' : 'Odrzuć' }}
            </button>
            <button
              @click="closeRejectModal"
              :disabled="rejecting"
              class="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
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
import { ref, computed, onMounted } from 'vue'
import { Plus, List, Clock, FileText, ClipboardList, X } from 'lucide-vue-next'
import type { RequestTemplate, Request } from '~/types/requests'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const { getAvailableTemplates, getMyRequests, getPendingApprovals, approveRequestStep, rejectRequestStep } = useRequestsApi()
const authStore = useAuthStore()

const activeTab = ref<'new' | 'my-requests' | 'to-approve' | 'approved-by-me'>('new')
const templates = ref<RequestTemplate[]>([])
const myRequests = ref<Request[]>([])
const pendingApprovals = ref<Request[]>([])
const approvalsHistory = ref<Request[]>([])
const loadingTemplates = ref(true)
const loadingRequests = ref(true)
const loadingApprovals = ref(true)
const templateSearch = ref('')
const requestSearch = ref('')
const approvalSearch = ref('')
const statusFilter = ref('')
const selectedRequest = ref<Request | null>(null)
const showQuizModal = ref(false)
const selectedQuizStep = ref<any>(null)
const quizQuestions = ref<any[]>([])
const quizPassingScore = ref(80)

// Approve/Reject modal state
const showApproveModal = ref(false)
const showRejectModal = ref(false)
const approveComment = ref('')
const rejectReason = ref('')
const approving = ref(false)
const rejecting = ref(false)

const filteredTemplates = computed(() => {
  if (!templateSearch.value) return templates.value
  
  const query = templateSearch.value.toLowerCase()
  return templates.value.filter(t => 
    t.name.toLowerCase().includes(query) ||
    t.description.toLowerCase().includes(query) ||
    t.category.toLowerCase().includes(query)
  )
})

const filteredRequests = computed(() => {
  let result = myRequests.value

  if (statusFilter.value) {
    result = result.filter(r => r.status === statusFilter.value)
  }

  if (requestSearch.value) {
    const query = requestSearch.value.toLowerCase()
    result = result.filter(r =>
      r.requestNumber.toLowerCase().includes(query) ||
      r.requestTemplateName.toLowerCase().includes(query)
    )
  }

  return result
})

const filteredApprovals = computed(() => {
  if (!approvalSearch.value) return pendingApprovals.value

  const query = approvalSearch.value.toLowerCase()
  return pendingApprovals.value.filter(r =>
    r.requestNumber.toLowerCase().includes(query) ||
    r.requestTemplateName.toLowerCase().includes(query) ||
    r.submittedByName?.toLowerCase().includes(query)
  )
})

const canApproveRequests = computed(() => {
  const user = authStore.user
  if (!user || !user.role) return false

  // Check if user has admin, hr, or manager role
  // These roles have permissions to approve requests
  return ['admin', 'hr', 'manager'].includes(user.role)
})

// Icon mapping
const { getIconifyName } = useIconMapping()

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

const getCurrentStepLabel = (request: Request) => {
  const currentStep = request.approvalSteps.find(s => s.status === 'InReview')
  return currentStep ? `U ${currentStep.approverName}` : '-'
}

const getApprovalProgress = (request: Request) => {
  const approved = request.approvalSteps.filter(s => s.status === 'Approved').length
  const total = request.approvalSteps.length
  return `${approved}/${total}`
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pl-PL', { year: 'numeric', month: 'long', day: 'numeric' })
}

const selectTemplate = (template: RequestTemplate) => {
  // Navigate to request submission page
  navigateTo(`/dashboard/requests/submit/${template.id}`)
}

const closeRequestDetails = () => {
  selectedRequest.value = null
}

const closeQuizModal = () => {
  showQuizModal.value = false
  selectedQuizStep.value = null
}

const handleQuizSubmit = async (score: number, passed: boolean, answers: Record<string, string>) => {
  // Handle quiz submission here
  console.log('Quiz submitted:', { score, passed, answers })
  closeQuizModal()
  await loadMyRequests() // Reload requests
}

const loadTemplates = async () => {
  try {
    loadingTemplates.value = true
    templates.value = await getAvailableTemplates()
  } catch (error) {
    console.error('Error loading templates:', error)
  } finally {
    loadingTemplates.value = false
  }
}

const loadMyRequests = async () => {
  try {
    loadingRequests.value = true
    myRequests.value = await getMyRequests()
  } catch (error) {
    console.error('Error loading requests:', error)
  } finally {
    loadingRequests.value = false
  }
}

const loadPendingApprovals = async () => {
  try {
    loadingApprovals.value = true
    pendingApprovals.value = await getPendingApprovals()
  } catch (error) {
    console.error('Error loading pending approvals:', error)
  } finally {
    loadingApprovals.value = false
  }
}

const loadApprovalsHistory = async () => {
  try {
    loadingApprovals.value = true
    const { getApprovalsHistory } = useRequestsApi()
    approvalsHistory.value = await getApprovalsHistory()
  } catch (error) {
    console.error('Error loading approvals history:', error)
  } finally {
    loadingApprovals.value = false
  }
}

const getCurrentStep = (request: Request) => {
  return request.approvalSteps.find(step => step.status === 'InReview')
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
    await loadPendingApprovals()
  } catch (err: any) {
    console.error('Error approving request:', err)
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
    await loadPendingApprovals()
  } catch (err: any) {
    console.error('Error rejecting request:', err)
  } finally {
    rejecting.value = false
  }
}

const getPriorityClass = (priority: string) => {
  return priority === 'Urgent'
    ? 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
    : 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300'
}

onMounted(() => {
  loadTemplates()
  loadMyRequests()
  if (canApproveRequests.value) {
    loadPendingApprovals()
    loadApprovalsHistory()
  }
})
</script>

