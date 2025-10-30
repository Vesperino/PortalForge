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
          <button
            v-for="request in filteredRequests"
            :key="request.id"
            @click="viewRequestDetails(request)"
            class="w-full bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 hover:shadow-lg hover:border-blue-500 dark:hover:border-blue-400 transition-all text-left group cursor-pointer"
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
          </button>
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

// Icon mapping - maps icon names to Iconify icon names
const iconMapping: Record<string, string> = {
  'beach-umbrella': 'fluent-emoji-flat:beach-with-umbrella',
  'palm-tree': 'fluent-emoji-flat:palm-tree',
  'sun': 'fluent-emoji-flat:sun',
  'airplane': 'fluent-emoji-flat:airplane',
  'luggage': 'fluent-emoji-flat:luggage',
  'island': 'fluent-emoji-flat:desert-island',
  'camping': 'fluent-emoji-flat:camping',
  'mountain': 'fluent-emoji-flat:mountain',
  'briefcase': 'fluent-emoji-flat:briefcase',
  'office-building': 'fluent-emoji-flat:office-building',
  'chart-increasing': 'fluent-emoji-flat:chart-increasing',
  'clipboard': 'fluent-emoji-flat:clipboard',
  'calendar': 'fluent-emoji-flat:calendar',
  'pushpin': 'fluent-emoji-flat:pushpin',
  'memo': 'fluent-emoji-flat:memo',
  'laptop': 'fluent-emoji-flat:laptop',
  'desktop-computer': 'fluent-emoji-flat:desktop-computer',
  'keyboard': 'fluent-emoji-flat:keyboard',
  'computer-mouse': 'fluent-emoji-flat:computer-mouse',
  'printer': 'fluent-emoji-flat:printer',
  'mobile-phone': 'fluent-emoji-flat:mobile-phone',
  'battery': 'fluent-emoji-flat:battery',
  'electric-plug': 'fluent-emoji-flat:electric-plug',
  'page-facing-up': 'fluent-emoji-flat:page-facing-up',
  'page-with-curl': 'fluent-emoji-flat:page-with-curl',
  'bookmark-tabs': 'fluent-emoji-flat:bookmark-tabs',
  'file-folder': 'fluent-emoji-flat:file-folder',
  'open-file-folder': 'fluent-emoji-flat:open-file-folder',
  'card-index-dividers': 'fluent-emoji-flat:card-index-dividers',
  'spiral-notepad': 'fluent-emoji-flat:spiral-notepad',
  'bust-in-silhouette': 'fluent-emoji-flat:bust-in-silhouette',
  'busts-in-silhouette': 'fluent-emoji-flat:busts-in-silhouette',
  'man-office-worker': 'fluent-emoji-flat:man-office-worker',
  'woman-office-worker': 'fluent-emoji-flat:woman-office-worker',
  'technologist': 'fluent-emoji-flat:technologist',
  'man-teacher': 'fluent-emoji-flat:man-teacher',
  'alarm-clock': 'fluent-emoji-flat:alarm-clock',
  'hourglass': 'fluent-emoji-flat:hourglass-done',
  'stopwatch': 'fluent-emoji-flat:stopwatch',
  'envelope': 'fluent-emoji-flat:envelope',
  'incoming-envelope': 'fluent-emoji-flat:incoming-envelope',
  'outbox-tray': 'fluent-emoji-flat:outbox-tray',
  'inbox-tray': 'fluent-emoji-flat:inbox-tray',
  'telephone': 'fluent-emoji-flat:telephone',
  'speech-balloon': 'fluent-emoji-flat:speech-balloon',
  'megaphone': 'fluent-emoji-flat:megaphone',
  'money-bag': 'fluent-emoji-flat:money-bag',
  'dollar-banknote': 'fluent-emoji-flat:dollar-banknote',
  'credit-card': 'fluent-emoji-flat:credit-card',
  'receipt': 'fluent-emoji-flat:receipt',
  'chart-increasing-with-yen': 'fluent-emoji-flat:chart-increasing-with-yen',
  'hospital': 'fluent-emoji-flat:hospital',
  'pill': 'fluent-emoji-flat:pill',
  'syringe': 'fluent-emoji-flat:syringe',
  'stethoscope': 'fluent-emoji-flat:stethoscope',
  'thermometer': 'fluent-emoji-flat:thermometer',
  'adhesive-bandage': 'fluent-emoji-flat:adhesive-bandage',
  'automobile': 'fluent-emoji-flat:automobile',
  'bus': 'fluent-emoji-flat:bus',
  'train': 'fluent-emoji-flat:train',
  'bicycle': 'fluent-emoji-flat:bicycle',
  'fuel-pump': 'fluent-emoji-flat:fuel-pump',
  'parking': 'fluent-emoji-flat:p-button',
  'hamburger': 'fluent-emoji-flat:hamburger',
  'pizza': 'fluent-emoji-flat:pizza',
  'coffee': 'fluent-emoji-flat:hot-beverage',
  'birthday-cake': 'fluent-emoji-flat:birthday-cake',
  'fork-and-knife': 'fluent-emoji-flat:fork-and-knife',
  'clinking-beer-mugs': 'fluent-emoji-flat:clinking-beer-mugs',
  'party-popper': 'fluent-emoji-flat:party-popper',
  'wrapped-gift': 'fluent-emoji-flat:wrapped-gift',
  'balloon': 'fluent-emoji-flat:balloon',
  'christmas-tree': 'fluent-emoji-flat:christmas-tree',
  'fireworks': 'fluent-emoji-flat:fireworks',
  'trophy': 'fluent-emoji-flat:trophy',
  'medal': 'fluent-emoji-flat:1st-place-medal',
  'hammer': 'fluent-emoji-flat:hammer',
  'wrench': 'fluent-emoji-flat:wrench',
  'hammer-and-wrench': 'fluent-emoji-flat:hammer-and-wrench',
  'gear': 'fluent-emoji-flat:gear',
  'toolbox': 'fluent-emoji-flat:toolbox',
  'magnet': 'fluent-emoji-flat:magnet',
  'key': 'fluent-emoji-flat:key',
  'locked': 'fluent-emoji-flat:locked',
  'unlocked': 'fluent-emoji-flat:unlocked',
  'deciduous-tree': 'fluent-emoji-flat:deciduous-tree',
  'evergreen-tree': 'fluent-emoji-flat:evergreen-tree',
  'four-leaf-clover': 'fluent-emoji-flat:four-leaf-clover',
  'seedling': 'fluent-emoji-flat:seedling',
  'herb': 'fluent-emoji-flat:herb',
  'globe-showing-europe-africa': 'fluent-emoji-flat:globe-showing-europe-africa',
  'recycling-symbol': 'fluent-emoji-flat:recycling-symbol',
  'soccer-ball': 'fluent-emoji-flat:soccer-ball',
  'basketball': 'fluent-emoji-flat:basketball',
  'tennis': 'fluent-emoji-flat:tennis',
  'running-shoe': 'fluent-emoji-flat:running-shoe',
  'trophy-sports': 'fluent-emoji-flat:trophy',
  'medal-sports': 'fluent-emoji-flat:sports-medal',
  'person-biking': 'fluent-emoji-flat:person-biking'
}

const getIconifyName = (iconName: string) => {
  return iconMapping[iconName] || 'fluent-emoji-flat:question-mark'
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

