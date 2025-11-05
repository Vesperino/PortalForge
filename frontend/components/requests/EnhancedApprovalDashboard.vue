<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { 
  Search, Filter, CheckSquare, X, Clock, User, 
  MoreHorizontal, Check, AlertCircle, Calendar,
  ArrowUpDown, Download, Settings, UserCheck
} from 'lucide-vue-next'
import type { Request, RequestStatus, ApprovalStepStatus } from '~/types/requests'

interface ApprovalRequest extends Request {
  daysWaiting: number
  isOverdue: boolean
  canDelegate: boolean
  serviceCategory?: string
  serviceStatus?: string
}

const requests = ref<ApprovalRequest[]>([])
const selectedRequests = ref<Set<string>>(new Set())
const searchQuery = ref('')
const showFilters = ref(false)
const showBulkActions = ref(false)
const showDelegationModal = ref(false)
const isLoading = ref(false)

// Filter states
const filters = ref({
  status: [] as ApprovalStepStatus[],
  priority: [] as string[],
  category: [] as string[],
  dateRange: { start: '', end: '' },
  overdue: false,
  serviceRequests: false
})

// Sorting
const sortBy = ref<'submittedAt' | 'priority' | 'daysWaiting' | 'requestTemplateName'>('submittedAt')
const sortOrder = ref<'asc' | 'desc'>('desc')

// Bulk actions
const bulkAction = ref<'approve' | 'reject' | 'delegate' | null>(null)
const bulkComment = ref('')
const delegateToUser = ref('')

// Mock data
onMounted(async () => {
  await loadRequests()
})

const loadRequests = async () => {
  isLoading.value = true
  
  // Simulate API call
  await new Promise(resolve => setTimeout(resolve, 1000))
  
  requests.value = [
    {
      id: '1',
      requestNumber: 'REQ-2024-001',
      requestTemplateId: 'template1',
      requestTemplateName: 'Wniosek urlopowy',
      requestTemplateIcon: '🏖️',
      submittedById: 'user1',
      submittedByName: 'Jan Kowalski',
      submittedAt: '2024-01-15T10:00:00Z',
      priority: 'Standard',
      formData: '{}',
      status: 'InReview',
      approvalSteps: [],
      daysWaiting: 3,
      isOverdue: false,
      canDelegate: true
    },
    {
      id: '2',
      requestNumber: 'REQ-2024-002',
      requestTemplateId: 'template2',
      requestTemplateName: 'Wniosek IT',
      requestTemplateIcon: '💻',
      submittedById: 'user2',
      submittedByName: 'Anna Nowak',
      submittedAt: '2024-01-10T14:30:00Z',
      priority: 'Urgent',
      formData: '{}',
      status: 'InReview',
      approvalSteps: [],
      daysWaiting: 8,
      isOverdue: true,
      canDelegate: true,
      serviceCategory: 'IT Support',
      serviceStatus: 'In Progress'
    },
    {
      id: '3',
      requestNumber: 'REQ-2024-003',
      requestTemplateId: 'template3',
      requestTemplateName: 'Wniosek finansowy',
      requestTemplateIcon: '💰',
      submittedById: 'user3',
      submittedByName: 'Piotr Wiśniewski',
      submittedAt: '2024-01-18T09:15:00Z',
      priority: 'Standard',
      formData: '{}',
      status: 'InReview',
      approvalSteps: [],
      daysWaiting: 1,
      isOverdue: false,
      canDelegate: false
    }
  ]
  
  isLoading.value = false
}

const filteredRequests = computed(() => {
  let filtered = [...requests.value]
  
  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(request => 
      request.requestNumber.toLowerCase().includes(query) ||
      request.requestTemplateName.toLowerCase().includes(query) ||
      request.submittedByName.toLowerCase().includes(query)
    )
  }
  
  // Status filter
  if (filters.value.status.length > 0) {
    filtered = filtered.filter(request => 
      filters.value.status.includes(request.status as ApprovalStepStatus)
    )
  }
  
  // Priority filter
  if (filters.value.priority.length > 0) {
    filtered = filtered.filter(request => 
      filters.value.priority.includes(request.priority)
    )
  }
  
  // Overdue filter
  if (filters.value.overdue) {
    filtered = filtered.filter(request => request.isOverdue)
  }
  
  // Service requests filter
  if (filters.value.serviceRequests) {
    filtered = filtered.filter(request => !!request.serviceCategory)
  }
  
  // Sort
  filtered.sort((a, b) => {
    let aValue: any = a[sortBy.value]
    let bValue: any = b[sortBy.value]
    
    if (sortBy.value === 'submittedAt') {
      aValue = new Date(aValue).getTime()
      bValue = new Date(bValue).getTime()
    }
    
    if (sortOrder.value === 'asc') {
      return aValue > bValue ? 1 : -1
    } else {
      return aValue < bValue ? 1 : -1
    }
  })
  
  return filtered
})

const selectedCount = computed(() => selectedRequests.value.size)

const allSelected = computed(() => 
  filteredRequests.value.length > 0 && 
  filteredRequests.value.every(request => selectedRequests.value.has(request.id))
)

const toggleSelectAll = () => {
  if (allSelected.value) {
    selectedRequests.value.clear()
  } else {
    filteredRequests.value.forEach(request => {
      selectedRequests.value.add(request.id)
    })
  }
}

const toggleSelect = (requestId: string) => {
  if (selectedRequests.value.has(requestId)) {
    selectedRequests.value.delete(requestId)
  } else {
    selectedRequests.value.add(requestId)
  }
}

const clearFilters = () => {
  filters.value = {
    status: [],
    priority: [],
    category: [],
    dateRange: { start: '', end: '' },
    overdue: false,
    serviceRequests: false
  }
  searchQuery.value = ''
}

const executeBulkAction = async () => {
  if (!bulkAction.value || selectedCount.value === 0) return
  
  isLoading.value = true
  
  try {
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1500))
    
    // Update requests based on action
    const selectedIds = Array.from(selectedRequests.value)
    
    if (bulkAction.value === 'approve') {
      requests.value = requests.value.map(request => 
        selectedIds.includes(request.id) 
          ? { ...request, status: 'Approved' as RequestStatus }
          : request
      )
    } else if (bulkAction.value === 'reject') {
      requests.value = requests.value.map(request => 
        selectedIds.includes(request.id) 
          ? { ...request, status: 'Rejected' as RequestStatus }
          : request
      )
    }
    
    // Clear selections and close modal
    selectedRequests.value.clear()
    showBulkActions.value = false
    bulkAction.value = null
    bulkComment.value = ''
    
  } catch (error) {
    console.error('Bulk action failed:', error)
  } finally {
    isLoading.value = false
  }
}

const openDelegation = () => {
  showDelegationModal.value = true
}

const delegateRequests = async () => {
  if (!delegateToUser.value || selectedCount.value === 0) return
  
  isLoading.value = true
  
  try {
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    // Clear selections and close modal
    selectedRequests.value.clear()
    showDelegationModal.value = false
    delegateToUser.value = ''
    
  } catch (error) {
    console.error('Delegation failed:', error)
  } finally {
    isLoading.value = false
  }
}

const exportRequests = () => {
  // Simulate export functionality
  const data = filteredRequests.value.map(request => ({
    'Numer wniosku': request.requestNumber,
    'Typ wniosku': request.requestTemplateName,
    'Wnioskodawca': request.submittedByName,
    'Data złożenia': new Date(request.submittedAt).toLocaleDateString('pl-PL'),
    'Priorytet': request.priority,
    'Status': request.status,
    'Dni oczekiwania': request.daysWaiting
  }))
  
  console.log('Exporting data:', data)
  // In real app, this would generate and download a CSV/Excel file
}

const getStatusColor = (status: RequestStatus) => {
  switch (status) {
    case 'InReview': return 'text-yellow-600 bg-yellow-100 dark:bg-yellow-900/20'
    case 'Approved': return 'text-green-600 bg-green-100 dark:bg-green-900/20'
    case 'Rejected': return 'text-red-600 bg-red-100 dark:bg-red-900/20'
    default: return 'text-gray-600 bg-gray-100 dark:bg-gray-900/20'
  }
}

const getPriorityColor = (priority: string) => {
  return priority === 'Urgent' 
    ? 'text-red-600 bg-red-100 dark:bg-red-900/20'
    : 'text-blue-600 bg-blue-100 dark:bg-blue-900/20'
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Panel zatwierdzeń
        </h1>
        <p class="text-gray-600 dark:text-gray-400 mt-1">
          Zarządzaj wnioskami oczekującymi na zatwierdzenie
        </p>
      </div>
      
      <div class="flex items-center gap-3">
        <button
          @click="exportRequests"
          class="flex items-center gap-2 px-4 py-2 text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition"
        >
          <Download class="w-4 h-4" />
          Eksportuj
        </button>
        
        <button
          @click="openDelegation"
          :disabled="selectedCount === 0"
          class="flex items-center gap-2 px-4 py-2 text-blue-700 dark:text-blue-300 bg-blue-100 dark:bg-blue-900/20 border border-blue-300 dark:border-blue-600 rounded-lg hover:bg-blue-200 dark:hover:bg-blue-900/40 transition disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <UserCheck class="w-4 h-4" />
          Deleguj ({{ selectedCount }})
        </button>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600 dark:text-gray-400">Oczekujące</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ requests.filter(r => r.status === 'InReview').length }}
            </p>
          </div>
          <Clock class="w-8 h-8 text-yellow-500" />
        </div>
      </div>
      
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600 dark:text-gray-400">Przeterminowane</p>
            <p class="text-2xl font-bold text-red-600 dark:text-red-400">
              {{ requests.filter(r => r.isOverdue).length }}
            </p>
          </div>
          <AlertCircle class="w-8 h-8 text-red-500" />
        </div>
      </div>
      
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600 dark:text-gray-400">Serwisowe</p>
            <p class="text-2xl font-bold text-blue-600 dark:text-blue-400">
              {{ requests.filter(r => r.serviceCategory).length }}
            </p>
          </div>
          <Settings class="w-8 h-8 text-blue-500" />
        </div>
      </div>
      
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600 dark:text-gray-400">Pilne</p>
            <p class="text-2xl font-bold text-orange-600 dark:text-orange-400">
              {{ requests.filter(r => r.priority === 'Urgent').length }}
            </p>
          </div>
          <AlertCircle class="w-8 h-8 text-orange-500" />
        </div>
      </div>
    </div>

    <!-- Filters and Search -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
      <div class="flex items-center justify-between mb-4">
        <div class="flex items-center gap-4 flex-1">
          <!-- Search -->
          <div class="relative flex-1 max-w-md">
            <Search class="w-5 h-5 absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Szukaj wniosków..."
              class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
          
          <!-- Filter Toggle -->
          <button
            @click="showFilters = !showFilters"
            :class="[
              'flex items-center gap-2 px-4 py-2 border rounded-lg transition',
              showFilters
                ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-300 dark:border-blue-600 text-blue-700 dark:text-blue-300'
                : 'bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-600'
            ]"
          >
            <Filter class="w-4 h-4" />
            Filtry
          </button>
          
          <!-- Sort -->
          <select
            v-model="sortBy"
            class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="submittedAt">Data złożenia</option>
            <option value="priority">Priorytet</option>
            <option value="daysWaiting">Dni oczekiwania</option>
            <option value="requestTemplateName">Typ wniosku</option>
          </select>
          
          <button
            @click="sortOrder = sortOrder === 'asc' ? 'desc' : 'asc'"
            class="p-2 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 transition"
          >
            <ArrowUpDown class="w-4 h-4 text-gray-600 dark:text-gray-400" />
          </button>
        </div>
        
        <!-- Bulk Actions -->
        <div v-if="selectedCount > 0" class="flex items-center gap-2">
          <span class="text-sm text-gray-600 dark:text-gray-400">
            Wybrano: {{ selectedCount }}
          </span>
          <button
            @click="showBulkActions = true"
            class="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
          >
            <CheckSquare class="w-4 h-4" />
            Akcje grupowe
          </button>
        </div>
      </div>

      <!-- Advanced Filters -->
      <div v-if="showFilters" class="border-t border-gray-200 dark:border-gray-700 pt-4 mt-4">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Status
            </label>
            <div class="space-y-2">
              <label class="flex items-center">
                <input
                  v-model="filters.status"
                  type="checkbox"
                  value="InReview"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">W trakcie</span>
              </label>
              <label class="flex items-center">
                <input
                  v-model="filters.overdue"
                  type="checkbox"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Przeterminowane</span>
              </label>
            </div>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Priorytet
            </label>
            <div class="space-y-2">
              <label class="flex items-center">
                <input
                  v-model="filters.priority"
                  type="checkbox"
                  value="Urgent"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Pilne</span>
              </label>
              <label class="flex items-center">
                <input
                  v-model="filters.priority"
                  type="checkbox"
                  value="Standard"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Standardowe</span>
              </label>
            </div>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Typ
            </label>
            <div class="space-y-2">
              <label class="flex items-center">
                <input
                  v-model="filters.serviceRequests"
                  type="checkbox"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Wnioski serwisowe</span>
              </label>
            </div>
          </div>
        </div>
        
        <div class="flex items-center gap-2 mt-4">
          <button
            @click="clearFilters"
            class="px-4 py-2 text-gray-600 dark:text-gray-400 hover:text-gray-800 dark:hover:text-gray-200 transition"
          >
            Wyczyść filtry
          </button>
        </div>
      </div>
    </div>

    <!-- Requests Table -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full">
          <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th class="px-6 py-3 text-left">
                <input
                  type="checkbox"
                  :checked="allSelected"
                  @change="toggleSelectAll"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Wniosek
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Wnioskodawca
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Priorytet
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Oczekuje
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                Akcje
              </th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
            <tr
              v-for="request in filteredRequests"
              :key="request.id"
              :class="[
                'hover:bg-gray-50 dark:hover:bg-gray-700 transition',
                request.isOverdue && 'bg-red-50 dark:bg-red-900/10'
              ]"
            >
              <td class="px-6 py-4">
                <input
                  type="checkbox"
                  :checked="selectedRequests.has(request.id)"
                  @change="toggleSelect(request.id)"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
              </td>
              
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <span class="text-2xl">{{ request.requestTemplateIcon }}</span>
                  <div>
                    <div class="font-medium text-gray-900 dark:text-white">
                      {{ request.requestNumber }}
                    </div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">
                      {{ request.requestTemplateName }}
                    </div>
                    <div v-if="request.serviceCategory" class="text-xs text-blue-600 dark:text-blue-400">
                      {{ request.serviceCategory }}
                      <span v-if="request.serviceStatus" class="ml-1">
                        ({{ request.serviceStatus }})
                      </span>
                    </div>
                  </div>
                </div>
              </td>
              
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <User class="w-4 h-4 text-gray-400" />
                  <div>
                    <div class="font-medium text-gray-900 dark:text-white">
                      {{ request.submittedByName }}
                    </div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">
                      {{ new Date(request.submittedAt).toLocaleDateString('pl-PL') }}
                    </div>
                  </div>
                </div>
              </td>
              
              <td class="px-6 py-4">
                <span
                  :class="[
                    'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
                    getStatusColor(request.status)
                  ]"
                >
                  {{ request.status === 'InReview' ? 'W trakcie' : request.status }}
                </span>
              </td>
              
              <td class="px-6 py-4">
                <span
                  :class="[
                    'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
                    getPriorityColor(request.priority)
                  ]"
                >
                  {{ request.priority === 'Urgent' ? 'Pilne' : 'Standardowe' }}
                </span>
              </td>
              
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <Calendar class="w-4 h-4 text-gray-400" />
                  <span
                    :class="[
                      'text-sm font-medium',
                      request.isOverdue
                        ? 'text-red-600 dark:text-red-400'
                        : 'text-gray-900 dark:text-white'
                    ]"
                  >
                    {{ request.daysWaiting }} dni
                  </span>
                </div>
              </td>
              
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <button
                    class="p-2 text-green-600 hover:text-green-800 hover:bg-green-100 dark:hover:bg-green-900/20 rounded-lg transition"
                    title="Zatwierdź"
                  >
                    <Check class="w-4 h-4" />
                  </button>
                  <button
                    class="p-2 text-red-600 hover:text-red-800 hover:bg-red-100 dark:hover:bg-red-900/20 rounded-lg transition"
                    title="Odrzuć"
                  >
                    <X class="w-4 h-4" />
                  </button>
                  <button
                    class="p-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition"
                    title="Więcej opcji"
                  >
                    <MoreHorizontal class="w-4 h-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Empty State -->
      <div v-if="filteredRequests.length === 0 && !isLoading" class="text-center py-12">
        <CheckSquare class="w-12 h-12 mx-auto text-gray-400 mb-4" />
        <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
          Brak wniosków do zatwierdzenia
        </h3>
        <p class="text-gray-500 dark:text-gray-400">
          {{ searchQuery || Object.values(filters).some(f => f && (Array.isArray(f) ? f.length > 0 : true))
            ? 'Nie znaleziono wniosków spełniających kryteria wyszukiwania'
            : 'Wszystkie wnioski zostały przetworzone'
          }}
        </p>
      </div>
      
      <!-- Loading State -->
      <div v-if="isLoading" class="text-center py-12">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
        <p class="text-gray-500 dark:text-gray-400">Ładowanie wniosków...</p>
      </div>
    </div>

    <!-- Bulk Actions Modal -->
    <div
      v-if="showBulkActions"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
      @click.self="showBulkActions = false"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Akcje grupowe ({{ selectedCount }} wniosków)
        </h3>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Wybierz akcję
            </label>
            <select
              v-model="bulkAction"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            >
              <option value="">Wybierz akcję...</option>
              <option value="approve">Zatwierdź wszystkie</option>
              <option value="reject">Odrzuć wszystkie</option>
              <option value="delegate">Deleguj wszystkie</option>
            </select>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Komentarz (opcjonalny)
            </label>
            <textarea
              v-model="bulkComment"
              rows="3"
              placeholder="Dodaj komentarz do akcji grupowej..."
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>
        
        <div class="flex items-center justify-end gap-3 mt-6">
          <button
            @click="showBulkActions = false"
            class="px-4 py-2 text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-md transition"
          >
            Anuluj
          </button>
          <button
            @click="executeBulkAction"
            :disabled="!bulkAction || isLoading"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ isLoading ? 'Wykonywanie...' : 'Wykonaj' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Delegation Modal -->
    <div
      v-if="showDelegationModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
      @click.self="showDelegationModal = false"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Deleguj wnioski ({{ selectedCount }})
        </h3>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Deleguj do użytkownika
            </label>
            <select
              v-model="delegateToUser"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            >
              <option value="">Wybierz użytkownika...</option>
              <option value="user1">Jan Kowalski</option>
              <option value="user2">Anna Nowak</option>
              <option value="user3">Piotr Wiśniewski</option>
            </select>
          </div>
          
          <div class="text-sm text-gray-600 dark:text-gray-400">
            Wybrane wnioski zostaną przekazane do zatwierdzenia wskazanemu użytkownikowi.
          </div>
        </div>
        
        <div class="flex items-center justify-end gap-3 mt-6">
          <button
            @click="showDelegationModal = false"
            class="px-4 py-2 text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-md transition"
          >
            Anuluj
          </button>
          <button
            @click="delegateRequests"
            :disabled="!delegateToUser || isLoading"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ isLoading ? 'Delegowanie...' : 'Deleguj' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>