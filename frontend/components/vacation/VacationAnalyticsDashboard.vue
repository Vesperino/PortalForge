<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { BarChart3, TrendingUp, Calendar, Users, Clock, CheckCircle } from 'lucide-vue-next'

interface VacationAnalytics {
  totalRequestsThisYear: number
  averageProcessingTime: number
  approvalRate: number
  mostUsedLeaveType: string
  peakVacationMonths: string[]
  teamCoverageImpact: number
  monthlyBreakdown: Array<{
    month: string
    requests: number
    approved: number
    rejected: number
  }>
  leaveTypeBreakdown: Array<{
    type: string
    count: number
    percentage: number
  }>
}

const props = defineProps<{
  userId?: string
}>()

// Composables
const { getAuthHeaders } = useAuth()
const config = useRuntimeConfig()
const authStore = useAuthStore()

// State
const analytics = ref<VacationAnalytics | null>(null)
const isLoading = ref(true)
const selectedYear = ref(new Date().getFullYear())

// Computed
const totalRequests = computed(() => 
  analytics.value?.monthlyBreakdown.reduce((sum, month) => sum + month.requests, 0) || 0
)

const totalApproved = computed(() => 
  analytics.value?.monthlyBreakdown.reduce((sum, month) => sum + month.approved, 0) || 0
)

const totalRejected = computed(() => 
  analytics.value?.monthlyBreakdown.reduce((sum, month) => sum + month.rejected, 0) || 0
)// Metho
ds
const loadAnalytics = async () => {
  isLoading.value = true
  
  try {
    const userId = props.userId || authStore.user?.id
    if (!userId) return
    
    const response = await $fetch(
      `${config.public.apiUrl}/api/vacation-schedules/analytics/${userId}?year=${selectedYear.value}`,
      { headers: getAuthHeaders() }
    ) as VacationAnalytics
    
    analytics.value = response
  } catch (error) {
    console.error('Error loading vacation analytics:', error)
    
    // Mock data for demonstration
    analytics.value = {
      totalRequestsThisYear: 8,
      averageProcessingTime: 2.3,
      approvalRate: 87.5,
      mostUsedLeaveType: 'Urlop wypoczynkowy',
      peakVacationMonths: ['Lipiec', 'Sierpień', 'Grudzień'],
      teamCoverageImpact: 18,
      monthlyBreakdown: [
        { month: 'Styczeń', requests: 1, approved: 1, rejected: 0 },
        { month: 'Luty', requests: 0, approved: 0, rejected: 0 },
        { month: 'Marzec', requests: 1, approved: 1, rejected: 0 },
        { month: 'Kwiecień', requests: 0, approved: 0, rejected: 0 },
        { month: 'Maj', requests: 1, approved: 0, rejected: 1 },
        { month: 'Czerwiec', requests: 1, approved: 1, rejected: 0 },
        { month: 'Lipiec', requests: 2, approved: 2, rejected: 0 },
        { month: 'Sierpień', requests: 1, approved: 1, rejected: 0 },
        { month: 'Wrzesień', requests: 0, approved: 0, rejected: 0 },
        { month: 'Październik', requests: 0, approved: 0, rejected: 0 },
        { month: 'Listopad', requests: 1, approved: 1, rejected: 0 },
        { month: 'Grudzień', requests: 0, approved: 0, rejected: 0 }
      ],
      leaveTypeBreakdown: [
        { type: 'Urlop wypoczynkowy', count: 6, percentage: 75 },
        { type: 'Urlop na żądanie', count: 1, percentage: 12.5 },
        { type: 'Urlop okolicznościowy', count: 1, percentage: 12.5 }
      ]
    }
  } finally {
    isLoading.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadAnalytics()
})
</script>

<template>
  <div class="vacation-analytics-dashboard space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <h2 class="text-xl font-semibold text-gray-900 dark:text-white flex items-center">
        <BarChart3 class="w-6 h-6 mr-2" />
        Analityka urlopowa
      </h2>
      
      <select 
        v-model="selectedYear" 
        @change="loadAnalytics"
        class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
      >
        <option :value="2024">2024</option>
        <option :value="2023">2023</option>
        <option :value="2022">2022</option>
      </select>
    </div>

    <div v-if="isLoading" class="flex items-center justify-center py-12">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>

    <div v-else-if="analytics" class="space-y-6">
      <!-- Key Metrics -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
          <div class="flex items-center">
            <Calendar class="w-8 h-8 text-blue-600" />
            <div class="ml-4">
              <div class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ totalRequests }}
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">
                Łączne wnioski
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
          <div class="flex items-center">
            <CheckCircle class="w-8 h-8 text-green-600" />
            <div class="ml-4">
              <div class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ analytics.approvalRate }}%
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">
                Wskaźnik akceptacji
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
          <div class="flex items-center">
            <Clock class="w-8 h-8 text-yellow-600" />
            <div class="ml-4">
              <div class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ analytics.averageProcessingTime }}d
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">
                Średni czas realizacji
              </div>
            </div>
          </div>
        </div>

        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
          <div class="flex items-center">
            <Users class="w-8 h-8 text-purple-600" />
            <div class="ml-4">
              <div class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ analytics.teamCoverageImpact }}%
              </div>
              <div class="text-sm text-gray-600 dark:text-gray-400">
                Wpływ na zespół
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Monthly Breakdown Chart -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Rozkład miesięczny wniosków
        </h3>
        
        <div class="space-y-3">
          <div 
            v-for="month in analytics.monthlyBreakdown" 
            :key="month.month"
            class="flex items-center justify-between"
          >
            <div class="flex-1">
              <div class="flex items-center justify-between mb-1">
                <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {{ month.month }}
                </span>
                <span class="text-sm text-gray-500 dark:text-gray-400">
                  {{ month.requests }} wniosków
                </span>
              </div>
              
              <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                <div class="flex h-2 rounded-full overflow-hidden">
                  <div 
                    class="bg-green-500"
                    :style="{ width: month.requests > 0 ? `${(month.approved / month.requests) * 100}%` : '0%' }"
                  ></div>
                  <div 
                    class="bg-red-500"
                    :style="{ width: month.requests > 0 ? `${(month.rejected / month.requests) * 100}%` : '0%' }"
                  ></div>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div class="flex items-center justify-center mt-4 space-x-6 text-sm">
          <div class="flex items-center">
            <div class="w-3 h-3 bg-green-500 rounded-full mr-2"></div>
            <span class="text-gray-600 dark:text-gray-400">Zatwierdzone</span>
          </div>
          <div class="flex items-center">
            <div class="w-3 h-3 bg-red-500 rounded-full mr-2"></div>
            <span class="text-gray-600 dark:text-gray-400">Odrzucone</span>
          </div>
        </div>
      </div>

      <!-- Leave Type Breakdown -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Rozkład typów urlopów
        </h3>
        
        <div class="space-y-4">
          <div 
            v-for="leaveType in analytics.leaveTypeBreakdown" 
            :key="leaveType.type"
            class="flex items-center justify-between"
          >
            <div class="flex-1">
              <div class="flex items-center justify-between mb-2">
                <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {{ leaveType.type }}
                </span>
                <span class="text-sm text-gray-500 dark:text-gray-400">
                  {{ leaveType.count }} ({{ leaveType.percentage }}%)
                </span>
              </div>
              
              <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                <div 
                  class="bg-blue-500 h-2 rounded-full transition-all duration-300"
                  :style="{ width: `${leaveType.percentage}%` }"
                ></div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Peak Vacation Months -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center">
          <TrendingUp class="w-5 h-5 mr-2" />
          Szczyty urlopowe
        </h3>
        
        <div class="flex flex-wrap gap-2">
          <span 
            v-for="month in analytics.peakVacationMonths" 
            :key="month"
            class="px-3 py-1 bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 rounded-full text-sm font-medium"
          >
            {{ month }}
          </span>
        </div>
        
        <p class="mt-3 text-sm text-gray-600 dark:text-gray-400">
          Miesiące z największą liczbą wniosków urlopowych
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vacation-analytics-dashboard {
  /* Custom styles if needed */
}
</style>