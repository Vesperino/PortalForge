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
              <component
                :is="getIconComponent(template.icon)"
                class="w-12 h-12 text-blue-600 dark:text-blue-400 group-hover:scale-110 transition-transform"
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
          <div
            v-for="request in filteredRequests"
            :key="request.id"
            class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 hover:shadow-md transition-shadow"
          >
            <div class="flex items-start justify-between mb-4">
              <div class="flex items-start gap-4">
                <component
                  :is="getIconComponent(request.requestTemplateIcon)"
                  class="w-10 h-10 text-blue-600 dark:text-blue-400 flex-shrink-0"
                />
                <div>
                  <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
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
                  {{ request.priority === 'Urgent' ? 'Pilne' : 'Standard' }}
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

            <button
              @click="viewRequestDetails(request)"
              class="text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-medium text-sm"
            >
              Zobacz szczegóły →
            </button>
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Plus, List, Clock, FileText, ClipboardList, X } from 'lucide-vue-next'
import * as LucideIcons from 'lucide-vue-next'
import type { RequestTemplate, Request } from '~/types/requests'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const { getAvailableTemplates, getMyRequests } = useRequestsApi()

const activeTab = ref<'new' | 'my-requests'>('new')
const templates = ref<RequestTemplate[]>([])
const myRequests = ref<Request[]>([])
const loadingTemplates = ref(true)
const loadingRequests = ref(true)
const templateSearch = ref('')
const requestSearch = ref('')
const statusFilter = ref('')
const selectedRequest = ref<Request | null>(null)
const showQuizModal = ref(false)
const selectedQuizStep = ref<any>(null)
const quizQuestions = ref<any[]>([])
const quizPassingScore = ref(80)

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

const getIconComponent = (iconName: string) => {
  const IconComponent = (LucideIcons as any)[iconName]
  return IconComponent || LucideIcons.FileText
}

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

const viewRequestDetails = (request: Request) => {
  selectedRequest.value = request
}

const closeRequestDetails = () => {
  selectedRequest.value = null
}

const openQuizModal = (step: any, questions: any[], passingScore: number) => {
  selectedQuizStep.value = step
  quizQuestions.value = questions
  quizPassingScore.value = passingScore
  showQuizModal.value = true
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

onMounted(() => {
  loadTemplates()
  loadMyRequests()
})
</script>

