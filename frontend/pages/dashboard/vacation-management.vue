<script setup lang="ts">
import { ref, computed } from 'vue'
import { Calendar, BarChart3, AlertTriangle, Plus, X } from 'lucide-vue-next'
import EnhancedVacationRequest from '~/components/vacation/EnhancedVacationRequest.vue'
import VacationAnalyticsDashboard from '~/components/vacation/VacationAnalyticsDashboard.vue'
import VacationConflictVisualization from '~/components/vacation/VacationConflictVisualization.vue'
import { useVacations, type VacationSummary } from '~/composables/useVacations'

definePageMeta({
  middleware: ['auth', 'verified'],
  layout: 'default'
})

useHead({
  title: 'Zarządzanie urlopami - PortalForge'
})

// Composables
const { getUserVacationSummary } = useVacations()
const { submitRequest } = useRequestsApi()
const authStore = useAuthStore()
const toast = useNotificationToast()
const router = useRouter()

// State
const activeTab = ref<'overview' | 'request' | 'analytics' | 'conflicts'>('overview')
const vacationSummary = ref<VacationSummary | null>(null)
const isLoading = ref(true)
const showNewRequestModal = ref(false)

// Test conflict data for demonstration
const testConflictData = ref({
  startDate: '',
  endDate: ''
})

// Methods
const loadVacationSummary = async () => {
  if (!authStore.user?.id) return
  
  try {
    vacationSummary.value = await getUserVacationSummary(authStore.user.id)
  } catch (error) {
    console.error('Error loading vacation summary:', error)
    toast.error('Błąd podczas ładowania danych urlopowych')
  } finally {
    isLoading.value = false
  }
}

const handleNewVacationRequest = async (formData: Record<string, any>) => {
  try {
    // This would need to be updated with the actual vacation template ID
    const vacationTemplateId = 'vacation-template-id'
    
    const requestData = {
      requestTemplateId: vacationTemplateId,
      priority: 'Standard' as const,
      formData: formData
    }
    
    const result = await submitRequest(requestData)
    
    toast.success(`Wniosek urlopowy został złożony. Numer: ${result.requestNumber}`)
    showNewRequestModal.value = false
    
    // Refresh vacation summary
    await loadVacationSummary()
  } catch (error) {
    console.error('Error submitting vacation request:', error)
    toast.error('Błąd podczas składania wniosku urlopowego')
  }
}

const setTestConflictDates = () => {
  const today = new Date()
  const nextWeek = new Date(today.getTime() + 7 * 24 * 60 * 60 * 1000)
  
  testConflictData.value.startDate = today.toISOString().split('T')[0]
  testConflictData.value.endDate = nextWeek.toISOString().split('T')[0]
}

// Computed
const quotaCards = computed(() => {
  if (!vacationSummary.value) return []
  
  return [
    {
      title: 'Urlop wypoczynkowy',
      used: vacationSummary.value.vacationDaysUsed,
      total: vacationSummary.value.annualVacationDays,
      remaining: vacationSummary.value.vacationDaysRemaining,
      color: 'blue'
    },
    {
      title: 'Urlop na żądanie',
      used: vacationSummary.value.onDemandVacationDaysUsed,
      total: 4,
      remaining: vacationSummary.value.onDemandVacationDaysRemaining,
      color: 'green'
    },
    {
      title: 'Urlop okolicznościowy',
      used: vacationSummary.value.circumstantialLeaveDaysUsed,
      total: 0,
      remaining: 0,
      color: 'purple'
    }
  ]
})

// Lifecycle
onMounted(() => {
  loadVacationSummary()
  setTestConflictDates()
})
</script>

<template>
  <div class="vacation-management-page">
    <div class="max-w-7xl mx-auto p-6">
      <!-- Page Header -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
            Zarządzanie urlopami
          </h1>
          <p class="mt-1 text-sm text-gray-600 dark:text-gray-400">
            Kompleksowe zarządzanie wnioskami urlopowymi z analizą konfliktów i statystykami
          </p>
        </div>
        
        <button 
          @click="showNewRequestModal = true"
          class="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          <Plus class="w-4 h-4 mr-2" />
          Nowy wniosek
        </button>
      </div>

      <!-- Tab Navigation -->
      <div class="border-b border-gray-200 dark:border-gray-700 mb-6">
        <nav class="-mb-px flex space-x-8">
          <button 
            @click="activeTab = 'overview'"
            :class="{
              'border-blue-500 text-blue-600': activeTab === 'overview',
              'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300': activeTab !== 'overview'
            }"
            class="whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm"
          >
            <Calendar class="w-4 h-4 mr-2 inline" />
            Przegląd
          </button>
          
          <button 
            @click="activeTab = 'analytics'"
            :class="{
              'border-blue-500 text-blue-600': activeTab === 'analytics',
              'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300': activeTab !== 'analytics'
            }"
            class="whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm"
          >
            <BarChart3 class="w-4 h-4 mr-2 inline" />
            Analityka
          </button>
          
          <button 
            @click="activeTab = 'conflicts'"
            :class="{
              'border-blue-500 text-blue-600': activeTab === 'conflicts',
              'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300': activeTab !== 'conflicts'
            }"
            class="whitespace-nowrap py-2 px-1 border-b-2 font-medium text-sm"
          >
            <AlertTriangle class="w-4 h-4 mr-2 inline" />
            Konflikty
          </button>
        </nav>
      </div>

      <!-- Tab Content -->
      <div class="tab-content">
        <!-- Overview Tab -->
        <div v-if="activeTab === 'overview'" class="space-y-6">
          <div v-if="isLoading" class="flex items-center justify-center py-12">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>
          
          <div v-else class="space-y-6">
            <!-- Quota Cards -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div 
                v-for="quota in quotaCards" 
                :key="quota.title"
                class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6"
              >
                <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">
                  {{ quota.title }}
                </h3>
                
                <div class="space-y-2">
                  <div class="flex justify-between text-sm">
                    <span class="text-gray-600 dark:text-gray-400">Wykorzystane</span>
                    <span class="font-medium">{{ quota.used }}/{{ quota.total || '∞' }}</span>
                  </div>
                  
                  <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                    <div 
                      :class="`bg-${quota.color}-500`"
                      class="h-2 rounded-full transition-all duration-300"
                      :style="{ width: quota.total > 0 ? `${(quota.used / quota.total) * 100}%` : '0%' }"
                    ></div>
                  </div>
                  
                  <div class="text-sm text-gray-600 dark:text-gray-400">
                    <span v-if="quota.total > 0">
                      Pozostało: <strong>{{ quota.remaining }}</strong> dni
                    </span>
                    <span v-else>
                      Wykorzystano: <strong>{{ quota.used }}</strong> dni
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Quick Actions -->
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Szybkie akcje
              </h3>
              
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <button 
                  @click="showNewRequestModal = true"
                  class="p-4 border border-gray-200 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                >
                  <Plus class="w-6 h-6 text-blue-600 mx-auto mb-2" />
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    Nowy wniosek
                  </div>
                </button>
                
                <button 
                  @click="activeTab = 'analytics'"
                  class="p-4 border border-gray-200 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                >
                  <BarChart3 class="w-6 h-6 text-green-600 mx-auto mb-2" />
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    Zobacz statystyki
                  </div>
                </button>
                
                <button 
                  @click="activeTab = 'conflicts'"
                  class="p-4 border border-gray-200 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                >
                  <AlertTriangle class="w-6 h-6 text-yellow-600 mx-auto mb-2" />
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    Sprawdź konflikty
                  </div>
                </button>
                
                <button 
                  @click="router.push('/dashboard/team/vacation-calendar')"
                  class="p-4 border border-gray-200 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                >
                  <Calendar class="w-6 h-6 text-purple-600 mx-auto mb-2" />
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    Kalendarz zespołu
                  </div>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Analytics Tab -->
        <div v-if="activeTab === 'analytics'">
          <VacationAnalyticsDashboard :user-id="authStore.user?.id" />
        </div>

        <!-- Conflicts Tab -->
        <div v-if="activeTab === 'conflicts'" class="space-y-6">
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Sprawdź konflikty urlopowe
            </h3>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Data rozpoczęcia
                </label>
                <input 
                  v-model="testConflictData.startDate"
                  type="date" 
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                />
              </div>
              
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Data zakończenia
                </label>
                <input 
                  v-model="testConflictData.endDate"
                  type="date" 
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                />
              </div>
            </div>
          </div>
          
          <VacationConflictVisualization 
            :start-date="testConflictData.startDate"
            :end-date="testConflictData.endDate"
            :user-id="authStore.user?.id"
          />
        </div>
      </div>

      <!-- New Request Modal -->
      <div v-if="showNewRequestModal" class="fixed inset-0 z-50 overflow-y-auto">
        <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
          <div class="fixed inset-0 transition-opacity" aria-hidden="true">
            <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
          </div>

          <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-4xl sm:w-full">
            <div class="bg-white dark:bg-gray-800 px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div class="flex items-center justify-between mb-4">
                <h3 class="text-lg font-medium text-gray-900 dark:text-white">
                  Nowy wniosek urlopowy
                </h3>
                <button 
                  @click="showNewRequestModal = false"
                  class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                >
                  <X class="w-6 h-6" />
                </button>
              </div>
              
              <EnhancedVacationRequest 
                @submit="handleNewVacationRequest"
                @cancel="showNewRequestModal = false"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vacation-management-page {
  min-height: calc(100vh - 4rem);
  background-color: #f9fafb;
}

.dark .vacation-management-page {
  background-color: #111827;
}
</style>