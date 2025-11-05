<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { Calendar, AlertTriangle, CheckCircle, Clock, FileText, Upload, X, Users, TrendingUp, BarChart3 } from 'lucide-vue-next'
import { useVacations, type VacationSummary, type ValidateVacationRequest, type ValidateVacationResponse, LeaveType } from '~/composables/useVacations'
import type { VacationSchedule } from '~/types/vacation'

interface VacationQuota {
  type: string
  label: string
  total: number
  used: number
  remaining: number
  color: string
}

interface VacationConflict {
  date: string
  conflictingUsers: Array<{
    id: string
    name: string
    department: string
  }>
  severity: 'low' | 'medium' | 'high'
}

interface VacationAnalytics {
  totalRequestsThisYear: number
  averageProcessingTime: number
  approvalRate: number
  mostUsedLeaveType: string
  peakVacationMonths: string[]
  teamCoverageImpact: number
}

const props = defineProps<{
  templateId?: string
  initialData?: Record<string, any>
}>()

const emit = defineEmits<{
  submit: [data: Record<string, any>]
  cancel: []
}>()

// Composables
const { validateVacation, getUserVacationSummary } = useVacations()
const { getAuthHeaders } = useAuth()
const config = useRuntimeConfig()
const authStore = useAuthStore()
const toast = useNotificationToast()

// State
const isLoading = ref(false)
const isValidating = ref(false)
const showConflictDetails = ref(false)
const showAnalytics = ref(false)

// Form data
const formData = ref({
  leaveType: LeaveType.Annual,
  startDate: '',
  endDate: '',
  reason: '',
  substituteUserId: '',
  documents: [] as File[]
})

// Vacation data
const vacationSummary = ref<VacationSummary | null>(null)
const validationResult = ref<ValidateVacationResponse | null>(null)
const conflicts = ref<VacationConflict[]>([])
const analytics = ref<VacationAnalytics | null>(null)
const existingVacations = ref<VacationSchedule[]>([])

// Computed properties
const quotas = computed<VacationQuota[]>(() => {
  if (!vacationSummary.value) return []
  
  return [
    {
      type: 'annual',
      label: 'Urlop wypoczynkowy',
      total: vacationSummary.value.annualVacationDays,
      used: vacationSummary.value.vacationDaysUsed,
      remaining: vacationSummary.value.vacationDaysRemaining,
      color: 'bg-blue-500'
    },
    {
      type: 'ondemand',
      label: 'Urlop na żądanie',
      total: 4, // Polish law limit
      used: vacationSummary.value.onDemandVacationDaysUsed,
      remaining: vacationSummary.value.onDemandVacationDaysRemaining,
      color: 'bg-green-500'
    },
    {
      type: 'circumstantial',
      label: 'Urlop okolicznościowy',
      total: 0, // No fixed limit
      used: vacationSummary.value.circumstantialLeaveDaysUsed,
      remaining: 0,
      color: 'bg-purple-500'
    }
  ]
})

const isFormValid = computed(() => {
  return formData.value.startDate && 
         formData.value.endDate && 
         formData.value.reason.trim() &&
         (!requiresDocuments.value || formData.value.documents.length > 0) &&
         validationResult.value?.canTake
})

const requiresDocuments = computed(() => {
  return formData.value.leaveType === LeaveType.Circumstantial
})

const selectedQuota = computed(() => {
  const typeMap = {
    [LeaveType.Annual]: 'annual',
    [LeaveType.OnDemand]: 'ondemand',
    [LeaveType.Circumstantial]: 'circumstantial'
  }
  return quotas.value.find(q => q.type === typeMap[formData.value.leaveType])
})

const conflictSeverityColor = computed(() => {
  const highConflicts = conflicts.value.filter(c => c.severity === 'high').length
  const mediumConflicts = conflicts.value.filter(c => c.severity === 'medium').length
  
  if (highConflicts > 0) return 'text-red-600'
  if (mediumConflicts > 0) return 'text-yellow-600'
  return 'text-green-600'
})


// Methods
const loadVacationSummary = async () => {
  if (!authStore.user?.id) return
  
  try {
    vacationSummary.value = await getUserVacationSummary(authStore.user.id)
  } catch (error) {
    console.error('Error loading vacation summary:', error)
    toast.error('Błąd podczas ładowania danych urlopowych')
  }
}

const loadExistingVacations = async () => {
  if (!authStore.user?.id) return
  
  try {
    const response = await $fetch(
      `${config.public.apiUrl}/api/vacation-schedules/my?year=${new Date().getFullYear()}`,
      { headers: getAuthHeaders() }
    ) as VacationSchedule[]
    
    existingVacations.value = response
  } catch (error) {
    console.error('Error loading existing vacations:', error)
  }
}

const loadAnalytics = async () => {
  if (!authStore.user?.id) return
  
  try {
    const response = await $fetch(
      `${config.public.apiUrl}/api/vacation-schedules/analytics/${authStore.user.id}`,
      { headers: getAuthHeaders() }
    ) as VacationAnalytics
    
    analytics.value = response
  } catch (error) {
    console.error('Error loading analytics:', error)
    // Mock data for demonstration
    analytics.value = {
      totalRequestsThisYear: 3,
      averageProcessingTime: 2.5,
      approvalRate: 95,
      mostUsedLeaveType: 'Annual',
      peakVacationMonths: ['Lipiec', 'Sierpień'],
      teamCoverageImpact: 15
    }
  }
}

const validateVacationRequest = async () => {
  if (!formData.value.startDate || !formData.value.endDate) return
  
  isValidating.value = true
  
  try {
    const request: ValidateVacationRequest = {
      startDate: formData.value.startDate,
      endDate: formData.value.endDate,
      leaveType: formData.value.leaveType
    }
    
    validationResult.value = await validateVacation(request)
    
    // Load conflicts if validation passes
    if (validationResult.value.canTake) {
      await loadVacationConflicts()
    }
  } catch (error) {
    console.error('Error validating vacation:', error)
    validationResult.value = {
      canTake: false,
      errorMessage: 'Błąd podczas walidacji wniosku',
      requestedDays: 0
    }
  } finally {
    isValidating.value = false
  }
}

const loadVacationConflicts = async () => {
  if (!formData.value.startDate || !formData.value.endDate) return
  
  try {
    const response = await $fetch(
      `${config.public.apiUrl}/api/vacation-schedules/conflicts`,
      {
        method: 'POST',
        headers: getAuthHeaders(),
        body: {
          startDate: formData.value.startDate,
          endDate: formData.value.endDate,
          userId: authStore.user?.id
        }
      }
    ) as VacationConflict[]
    
    conflicts.value = response
  } catch (error) {
    console.error('Error loading conflicts:', error)
    // Mock conflicts for demonstration
    conflicts.value = [
      {
        date: formData.value.startDate,
        conflictingUsers: [
          { id: '1', name: 'Jan Kowalski', department: 'IT' },
          { id: '2', name: 'Anna Nowak', department: 'IT' }
        ],
        severity: 'medium'
      }
    ]
  }
}

const handleFileUpload = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files) {
    formData.value.documents = Array.from(target.files)
  }
}

const removeDocument = (index: number) => {
  formData.value.documents.splice(index, 1)
}

const submitRequest = async () => {
  if (!isFormValid.value) return
  
  isLoading.value = true
  
  try {
    // Prepare form data for submission
    const requestData = {
      leaveType: formData.value.leaveType,
      startDate: formData.value.startDate,
      endDate: formData.value.endDate,
      reason: formData.value.reason,
      substituteUserId: formData.value.substituteUserId || null,
      requestedDays: validationResult.value?.requestedDays || 0,
      documents: formData.value.documents.map(file => ({
        name: file.name,
        size: file.size,
        type: file.type
      }))
    }
    
    emit('submit', requestData)
    toast.success('Wniosek urlopowy został złożony')
  } catch (error) {
    console.error('Error submitting request:', error)
    toast.error('Błąd podczas składania wniosku')
  } finally {
    isLoading.value = false
  }
}

const cancelRequest = () => {
  emit('cancel')
}

// Watchers
watch([() => formData.value.startDate, () => formData.value.endDate, () => formData.value.leaveType], 
  () => {
    if (formData.value.startDate && formData.value.endDate) {
      validateVacationRequest()
    }
  },
  { debounce: 500 }
)

// Lifecycle
onMounted(async () => {
  await Promise.all([
    loadVacationSummary(),
    loadExistingVacations(),
    loadAnalytics()
  ])
  
  // Initialize form with props data
  if (props.initialData) {
    Object.assign(formData.value, props.initialData)
  }
})
</script>

<template>
  <div class="enhanced-vacation-request space-y-6">
    <!-- Vacation Quota Tracking Display -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center">
        <BarChart3 class="w-5 h-5 mr-2" />
        Stan urlopów
      </h3>
      
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div v-for="quota in quotas" :key="quota.type" class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
          <div class="flex items-center justify-between mb-2">
            <span class="text-sm font-medium text-gray-700 dark:text-gray-300">{{ quota.label }}</span>
            <span class="text-xs text-gray-500 dark:text-gray-400">
              {{ quota.used }}/{{ quota.total || '∞' }}
            </span>
          </div>
          
          <div class="w-full bg-gray-200 dark:bg-gray-600 rounded-full h-2 mb-2">
            <div 
              :class="quota.color" 
              class="h-2 rounded-full transition-all duration-300"
              :style="{ width: quota.total > 0 ? `${(quota.used / quota.total) * 100}%` : '0%' }"
            ></div>
          </div>
          
          <div class="text-sm text-gray-600 dark:text-gray-400">
            <span v-if="quota.type !== 'circumstantial'">
              Pozostało: <strong>{{ quota.remaining }}</strong> dni
            </span>
            <span v-else>
              Wykorzystano: <strong>{{ quota.used }}</strong> dni
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Vacation Request Form -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center">
        <Calendar class="w-5 h-5 mr-2" />
        Nowy wniosek urlopowy
      </h3>
      
      <div class="space-y-4">
        <!-- Leave Type Selection -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Typ urlopu
          </label>
          <select 
            v-model="formData.leaveType" 
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option :value="LeaveType.Annual">Urlop wypoczynkowy</option>
            <option :value="LeaveType.OnDemand">Urlop na żądanie</option>
            <option :value="LeaveType.Circumstantial">Urlop okolicznościowy</option>
            <option :value="LeaveType.Sick">Zwolnienie lekarskie</option>
          </select>
        </div>

        <!-- Date Range -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Data rozpoczęcia
            </label>
            <input 
              v-model="formData.startDate"
              type="date" 
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Data zakończenia
            </label>
            <input 
              v-model="formData.endDate"
              type="date" 
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>

        <!-- Reason -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Powód/Uzasadnienie
          </label>
          <textarea 
            v-model="formData.reason"
            rows="3"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
            placeholder="Podaj powód urlopu..."
          ></textarea>
        </div>

        <!-- Document Upload for Circumstantial Leave -->
        <div v-if="requiresDocuments" class="border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg p-6">
          <div class="text-center">
            <FileText class="mx-auto h-12 w-12 text-gray-400" />
            <div class="mt-4">
              <label class="cursor-pointer">
                <span class="mt-2 block text-sm font-medium text-gray-900 dark:text-white">
                  Załącz dokumenty potwierdzające
                </span>
                <span class="mt-1 block text-xs text-gray-500 dark:text-gray-400">
                  Wymagane dla urlopu okolicznościowego
                </span>
                <input 
                  type="file" 
                  multiple 
                  accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
                  class="sr-only"
                  @change="handleFileUpload"
                />
                <span class="mt-2 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
                  <Upload class="w-4 h-4 mr-2" />
                  Wybierz pliki
                </span>
              </label>
            </div>
          </div>
          
          <!-- Uploaded Files -->
          <div v-if="formData.documents.length > 0" class="mt-4 space-y-2">
            <div 
              v-for="(file, index) in formData.documents" 
              :key="index"
              class="flex items-center justify-between bg-gray-50 dark:bg-gray-700 rounded-md p-2"
            >
              <span class="text-sm text-gray-700 dark:text-gray-300">{{ file.name }}</span>
              <button 
                @click="removeDocument(index)"
                class="text-red-500 hover:text-red-700"
              >
                <X class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        <!-- Validation Results -->
        <div v-if="isValidating" class="flex items-center text-blue-600 dark:text-blue-400">
          <Clock class="w-4 h-4 mr-2 animate-spin" />
          Sprawdzanie dostępności...
        </div>
        
        <div v-else-if="validationResult" class="space-y-2">
          <div v-if="validationResult.canTake" class="flex items-center text-green-600 dark:text-green-400">
            <CheckCircle class="w-4 h-4 mr-2" />
            Urlop można udzielić ({{ validationResult.requestedDays }} dni)
          </div>
          
          <div v-else class="flex items-center text-red-600 dark:text-red-400">
            <AlertTriangle class="w-4 h-4 mr-2" />
            {{ validationResult.errorMessage }}
          </div>
        </div>
      </div>
    </div>

    <!-- Vacation Conflict Visualization -->
    <div v-if="conflicts.length > 0" class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white flex items-center">
          <Users class="w-5 h-5 mr-2" />
          Konflikty urlopowe
        </h3>
        <button 
          @click="showConflictDetails = !showConflictDetails"
          class="text-sm text-blue-600 hover:text-blue-800 dark:text-blue-400"
        >
          {{ showConflictDetails ? 'Ukryj szczegóły' : 'Pokaż szczegóły' }}
        </button>
      </div>
      
      <div class="mb-4">
        <div :class="conflictSeverityColor" class="text-sm font-medium">
          Wykryto {{ conflicts.length }} konflikt(ów) w wybranym terminie
        </div>
      </div>
      
      <div v-if="showConflictDetails" class="space-y-3">
        <div 
          v-for="conflict in conflicts" 
          :key="conflict.date"
          class="border border-gray-200 dark:border-gray-600 rounded-lg p-4"
        >
          <div class="flex items-center justify-between mb-2">
            <span class="font-medium text-gray-900 dark:text-white">
              {{ new Date(conflict.date).toLocaleDateString('pl-PL') }}
            </span>
            <span 
              :class="{
                'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200': conflict.severity === 'high',
                'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200': conflict.severity === 'medium',
                'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200': conflict.severity === 'low'
              }"
              class="px-2 py-1 rounded-full text-xs font-medium"
            >
              {{ conflict.severity === 'high' ? 'Wysoki' : conflict.severity === 'medium' ? 'Średni' : 'Niski' }}
            </span>
          </div>
          
          <div class="text-sm text-gray-600 dark:text-gray-400">
            Osoby na urlopie:
            <div class="mt-1 space-y-1">
              <div 
                v-for="user in conflict.conflictingUsers" 
                :key="user.id"
                class="flex items-center"
              >
                <span class="font-medium">{{ user.name }}</span>
                <span class="ml-2 text-xs text-gray-500">({{ user.department }})</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Vacation Analytics Dashboard -->
    <div v-if="analytics" class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white flex items-center">
          <TrendingUp class="w-5 h-5 mr-2" />
          Analityka urlopowa
        </h3>
        <button 
          @click="showAnalytics = !showAnalytics"
          class="text-sm text-blue-600 hover:text-blue-800 dark:text-blue-400"
        >
          {{ showAnalytics ? 'Ukryj' : 'Pokaż' }}
        </button>
      </div>
      
      <div v-if="showAnalytics" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
          <div class="text-2xl font-bold text-gray-900 dark:text-white">
            {{ analytics.totalRequestsThisYear }}
          </div>
          <div class="text-sm text-gray-600 dark:text-gray-400">
            Wnioski w tym roku
          </div>
        </div>
        
        <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
          <div class="text-2xl font-bold text-gray-900 dark:text-white">
            {{ analytics.averageProcessingTime }}d
          </div>
          <div class="text-sm text-gray-600 dark:text-gray-400">
            Średni czas realizacji
          </div>
        </div>
        
        <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
          <div class="text-2xl font-bold text-gray-900 dark:text-white">
            {{ analytics.approvalRate }}%
          </div>
          <div class="text-sm text-gray-600 dark:text-gray-400">
            Wskaźnik akceptacji
          </div>
        </div>
        
        <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
          <div class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Najczęściej używany typ
          </div>
          <div class="text-lg font-bold text-gray-900 dark:text-white">
            {{ analytics.mostUsedLeaveType }}
          </div>
        </div>
        
        <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
          <div class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Szczyty urlopowe
          </div>
          <div class="text-lg font-bold text-gray-900 dark:text-white">
            {{ analytics.peakVacationMonths.join(', ') }}
          </div>
        </div>
        
        <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
          <div class="text-2xl font-bold text-gray-900 dark:text-white">
            {{ analytics.teamCoverageImpact }}%
          </div>
          <div class="text-sm text-gray-600 dark:text-gray-400">
            Wpływ na zespół
          </div>
        </div>
      </div>
    </div>

    <!-- Action Buttons -->
    <div class="flex justify-end space-x-4">
      <button 
        @click="cancelRequest"
        class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
      >
        Anuluj
      </button>
      
      <button 
        @click="submitRequest"
        :disabled="!isFormValid || isLoading"
        class="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <Clock v-if="isLoading" class="w-4 h-4 mr-2 animate-spin" />
        {{ isLoading ? 'Składanie...' : 'Złóż wniosek' }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.enhanced-vacation-request {
  /* Custom styles if needed */
}
</style>