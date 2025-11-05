<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { AlertTriangle, Users, Calendar, Info, CheckCircle } from 'lucide-vue-next'

interface VacationConflict {
  date: string
  conflictingUsers: Array<{
    id: string
    name: string
    department: string
    position?: string
  }>
  severity: 'low' | 'medium' | 'high'
  coveragePercentage: number
  affectedProjects?: string[]
}

interface ConflictSummary {
  totalConflicts: number
  highSeverityConflicts: number
  affectedDepartments: string[]
  worstCoverageDay: {
    date: string
    percentage: number
  }
}

const props = defineProps<{
  startDate: string
  endDate: string
  userId?: string
  departmentId?: string
}>()

// Composables
const { getAuthHeaders } = useAuth()
const config = useRuntimeConfig()

// State
const conflicts = ref<VacationConflict[]>([])
const summary = ref<ConflictSummary | null>(null)
const isLoading = ref(false)
const showDetails = ref(true)

// Computed
const sortedConflicts = computed(() => {
  return [...conflicts.value].sort((a, b) => {
    const severityOrder = { high: 3, medium: 2, low: 1 }
    return severityOrder[b.severity] - severityOrder[a.severity]
  })
})

const severityStats = computed(() => {
  const stats = { high: 0, medium: 0, low: 0 }
  conflicts.value.forEach(conflict => {
    stats[conflict.severity]++
  })
  return stats
})// Methods

const loadConflicts = async () => {
  if (!props.startDate || !props.endDate) return
  
  isLoading.value = true
  
  try {
    const response = await $fetch(
      `${config.public.apiUrl}/api/vacation-schedules/conflicts`,
      {
        method: 'POST',
        headers: getAuthHeaders(),
        body: {
          startDate: props.startDate,
          endDate: props.endDate,
          userId: props.userId,
          departmentId: props.departmentId
        }
      }
    ) as { conflicts: VacationConflict[], summary: ConflictSummary }
    
    conflicts.value = response.conflicts
    summary.value = response.summary
  } catch (error) {
    console.error('Error loading vacation conflicts:', error)
    
    // Mock data for demonstration
    conflicts.value = [
      {
        date: props.startDate,
        conflictingUsers: [
          { id: '1', name: 'Jan Kowalski', department: 'IT', position: 'Senior Developer' },
          { id: '2', name: 'Anna Nowak', department: 'IT', position: 'Project Manager' }
        ],
        severity: 'high',
        coveragePercentage: 40,
        affectedProjects: ['Portal Klienta', 'System CRM']
      },
      {
        date: new Date(new Date(props.startDate).getTime() + 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        conflictingUsers: [
          { id: '3', name: 'Piotr Wiśniewski', department: 'Marketing', position: 'Marketing Specialist' }
        ],
        severity: 'medium',
        coveragePercentage: 75,
        affectedProjects: ['Kampania Q4']
      }
    ]
    
    summary.value = {
      totalConflicts: 2,
      highSeverityConflicts: 1,
      affectedDepartments: ['IT', 'Marketing'],
      worstCoverageDay: {
        date: props.startDate,
        percentage: 40
      }
    }
  } finally {
    isLoading.value = false
  }
}

const getSeverityColor = (severity: string) => {
  switch (severity) {
    case 'high':
      return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
    case 'medium':
      return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
    case 'low':
      return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
    default:
      return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
  }
}

const getSeverityIcon = (severity: string) => {
  switch (severity) {
    case 'high':
      return 'text-red-600'
    case 'medium':
      return 'text-yellow-600'
    case 'low':
      return 'text-green-600'
    default:
      return 'text-gray-600'
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('pl-PL', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

// Watch for prop changes
watch([() => props.startDate, () => props.endDate], () => {
  if (props.startDate && props.endDate) {
    loadConflicts()
  }
})

// Lifecycle
onMounted(() => {
  if (props.startDate && props.endDate) {
    loadConflicts()
  }
})
</script>

<template>
  <div class="vacation-conflict-visualization">
    <div v-if="isLoading" class="flex items-center justify-center py-8">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      <span class="ml-3 text-gray-600 dark:text-gray-400">Sprawdzanie konfliktów...</span>
    </div>

    <div v-else-if="conflicts.length === 0" class="text-center py-8">
      <div class="flex items-center justify-center mb-4">
        <div class="w-12 h-12 bg-green-100 dark:bg-green-900 rounded-full flex items-center justify-center">
          <CheckCircle class="w-6 h-6 text-green-600 dark:text-green-400" />
        </div>
      </div>
      <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
        Brak konfliktów urlopowych
      </h3>
      <p class="text-gray-600 dark:text-gray-400">
        W wybranym terminie nie wykryto konfliktów z innymi urlopami
      </p>
    </div>

    <div v-else class="space-y-6">
      <!-- Summary Statistics -->
      <div v-if="summary" class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center">
          <AlertTriangle class="w-5 h-5 mr-2" />
          Podsumowanie konfliktów
        </h3>
        
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div class="text-center">
            <div class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ summary.totalConflicts }}
            </div>
            <div class="text-sm text-gray-600 dark:text-gray-400">
              Łączne konflikty
            </div>
          </div>
          
          <div class="text-center">
            <div class="text-2xl font-bold text-red-600">
              {{ summary.highSeverityConflicts }}
            </div>
            <div class="text-sm text-gray-600 dark:text-gray-400">
              Wysokie ryzyko
            </div>
          </div>
          
          <div class="text-center">
            <div class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ summary.affectedDepartments.length }}
            </div>
            <div class="text-sm text-gray-600 dark:text-gray-400">
              Dotknięte działy
            </div>
          </div>
          
          <div class="text-center">
            <div class="text-2xl font-bold text-orange-600">
              {{ summary.worstCoverageDay.percentage }}%
            </div>
            <div class="text-sm text-gray-600 dark:text-gray-400">
              Najgorsze pokrycie
            </div>
          </div>
        </div>
      </div>

      <!-- Severity Overview -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
            Poziomy ryzyka
          </h3>
          <button 
            @click="showDetails = !showDetails"
            class="text-sm text-blue-600 hover:text-blue-800 dark:text-blue-400"
          >
            {{ showDetails ? 'Ukryj szczegóły' : 'Pokaż szczegóły' }}
          </button>
        </div>
        
        <div class="flex space-x-4">
          <div class="flex items-center">
            <div class="w-4 h-4 bg-red-500 rounded-full mr-2"></div>
            <span class="text-sm text-gray-600 dark:text-gray-400">
              Wysokie ({{ severityStats.high }})
            </span>
          </div>
          <div class="flex items-center">
            <div class="w-4 h-4 bg-yellow-500 rounded-full mr-2"></div>
            <span class="text-sm text-gray-600 dark:text-gray-400">
              Średnie ({{ severityStats.medium }})
            </span>
          </div>
          <div class="flex items-center">
            <div class="w-4 h-4 bg-green-500 rounded-full mr-2"></div>
            <span class="text-sm text-gray-600 dark:text-gray-400">
              Niskie ({{ severityStats.low }})
            </span>
          </div>
        </div>
      </div>

      <!-- Detailed Conflicts -->
      <div v-if="showDetails" class="space-y-4">
        <div 
          v-for="conflict in sortedConflicts" 
          :key="`${conflict.date}-${conflict.conflictingUsers.length}`"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6"
        >
          <div class="flex items-start justify-between mb-4">
            <div>
              <h4 class="text-lg font-medium text-gray-900 dark:text-white flex items-center">
                <Calendar class="w-5 h-5 mr-2" />
                {{ formatDate(conflict.date) }}
              </h4>
              <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Pokrycie zespołu: {{ conflict.coveragePercentage }}%
              </p>
            </div>
            
            <div class="flex items-center space-x-2">
              <AlertTriangle :class="getSeverityIcon(conflict.severity)" class="w-5 h-5" />
              <span 
                :class="getSeverityColor(conflict.severity)"
                class="px-2 py-1 rounded-full text-xs font-medium"
              >
                {{ conflict.severity === 'high' ? 'Wysokie' : conflict.severity === 'medium' ? 'Średnie' : 'Niskie' }}
              </span>
            </div>
          </div>
          
          <!-- Conflicting Users -->
          <div class="mb-4">
            <h5 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 flex items-center">
              <Users class="w-4 h-4 mr-1" />
              Osoby na urlopie ({{ conflict.conflictingUsers.length }})
            </h5>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div 
                v-for="user in conflict.conflictingUsers" 
                :key="user.id"
                class="flex items-center p-3 bg-gray-50 dark:bg-gray-700 rounded-lg"
              >
                <div class="w-8 h-8 bg-blue-100 dark:bg-blue-900 rounded-full flex items-center justify-center mr-3">
                  <span class="text-sm font-medium text-blue-600 dark:text-blue-400">
                    {{ user.name.split(' ').map(n => n[0]).join('') }}
                  </span>
                </div>
                
                <div>
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    {{ user.name }}
                  </div>
                  <div class="text-xs text-gray-600 dark:text-gray-400">
                    {{ user.position }} • {{ user.department }}
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Affected Projects -->
          <div v-if="conflict.affectedProjects && conflict.affectedProjects.length > 0">
            <h5 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 flex items-center">
              <Info class="w-4 h-4 mr-1" />
              Dotknięte projekty
            </h5>
            
            <div class="flex flex-wrap gap-2">
              <span 
                v-for="project in conflict.affectedProjects" 
                :key="project"
                class="px-2 py-1 bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 rounded text-xs"
              >
                {{ project }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vacation-conflict-visualization {
  /* Custom styles if needed */
}
</style>