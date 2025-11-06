<template>
  <div class="bg-gray-50 dark:bg-gray-700/50 border border-gray-200 dark:border-gray-600 rounded-lg p-4">
    <div class="flex items-start gap-4">
      <div class="flex-shrink-0 w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center font-bold">
        {{ step.stepOrder }}
      </div>
      
      <div class="flex-1 space-y-4">
        <!-- Approver Type Selection -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Typ zatwierdzającego
          </label>
          <select
            :value="step.approverType"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
            @change="(e) => { const newType = (e.target as HTMLSelectElement).value; onApproverTypeChange(newType); }"
          >
            <option value="DirectSupervisor">Kierownik działu (HeadOfDepartment)</option>
            <option value="DepartmentDirector">Dyrektor działu (Director)</option>
            <option value="SpecificUser">Konkretny użytkownik</option>
            <option value="SpecificDepartment">Szef konkretnego działu</option>
          </select>
        </div>

        <!-- Department Selection (when ApproverType = SpecificDepartment) -->
        <div v-if="step.approverType === 'SpecificDepartment'">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Wybierz dział
          </label>
          <div ref="departmentDropdownRef" class="relative">
            <input
              v-model="departmentSearchTerm"
              type="text"
              placeholder="Szukaj działu..."
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
              @input="searchDepartments"
              @focus="showDepartmentDropdown = true"
            >

            <!-- Selected Department Display -->
            <div v-if="selectedDepartment && departmentSearchTerm && !showDepartmentDropdown" class="mt-2 p-2 bg-green-50 dark:bg-green-900/20 rounded-lg flex items-center justify-between">
              <div class="flex items-center gap-2">
                <div class="w-8 h-8 bg-green-600 text-white rounded-full flex items-center justify-center text-sm font-bold">
                  <Icon name="heroicons:building-office-2" class="w-5 h-5" />
                </div>
                <div>
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    {{ selectedDepartment.name }}
                  </div>
                </div>
              </div>
              <button
                class="text-red-600 hover:text-red-700 p-1"
                @click="clearDepartmentSelection"
              >
                <Icon name="heroicons:x-mark" class="w-5 h-5" />
              </button>
            </div>

            <!-- Department Dropdown -->
            <div
              v-if="showDepartmentDropdown && filteredDepartments.length > 0"
              class="absolute z-10 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg max-h-60 overflow-y-auto"
            >
              <div
                v-for="dept in filteredDepartments"
                :key="dept.id"
                class="p-3 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer flex items-center gap-3"
                @click="selectDepartment(dept)"
              >
                <div class="w-8 h-8 bg-green-600 text-white rounded-full flex items-center justify-center text-sm">
                  <Icon name="heroicons:building-office-2" class="w-5 h-5" />
                </div>
                <div>
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    {{ dept.name }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Department Role Selection -->
          <div class="mt-3">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Rola w dziale
            </label>
            <select
              :value="step.specificDepartmentRoleType || 'Head'"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
              @change="(e) => updateStep({ specificDepartmentRoleType: (e.target as HTMLSelectElement).value as any })"
            >
              <option value="Head">Kierownik działu (Head)</option>
              <option value="Director">Dyrektor działu (Director)</option>
            </select>
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Wybierz, czy wniosek ma być zatwierdzany przez kierownika czy dyrektora wybranego działu
            </p>
          </div>
        </div>

        <!-- User Selection (when ApproverType = SpecificUser) -->
        <div v-if="step.approverType === 'SpecificUser'">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Wybierz użytkownika
          </label>
          <div ref="dropdownRef" class="relative">
            <input
              v-model="userSearchTerm"
              type="text"
              placeholder="Szukaj użytkownika..."
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
              @input="searchUsers"
              @focus="showUserDropdown = true"
            >
            
            <!-- Selected User Display -->
            <div v-if="selectedUser && !showUserDropdown" class="mt-2 p-2 bg-blue-50 dark:bg-blue-900/20 rounded-lg flex items-center justify-between">
              <div class="flex items-center gap-2">
                <div class="w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-bold">
                  {{ selectedUser.firstName[0] }}{{ selectedUser.lastName[0] }}
                </div>
                <div>
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    {{ selectedUser.firstName }} {{ selectedUser.lastName }}
                  </div>
                  <div class="text-xs text-gray-600 dark:text-gray-400">
                    {{ selectedUser.position }} - {{ selectedUser.department }}
                  </div>
                </div>
              </div>
              <button
                class="text-red-600 hover:text-red-700 p-1"
                @click="clearUserSelection"
              >
                <Icon name="heroicons:x-mark" class="w-5 h-5" />
              </button>
            </div>

            <!-- User Dropdown -->
            <div
              v-if="showUserDropdown && filteredUsers.length > 0"
              class="absolute z-10 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg max-h-60 overflow-y-auto"
            >
              <div
                v-for="user in filteredUsers"
                :key="user.id"
                class="p-3 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer flex items-center gap-3"
                @click="selectUser(user)"
              >
                <div class="w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-bold">
                  {{ user.firstName[0] }}{{ user.lastName[0] }}
                </div>
                <div>
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    {{ user.firstName }} {{ user.lastName }}
                  </div>
                  <div class="text-xs text-gray-600 dark:text-gray-400">
                    {{ user.position }} - {{ user.department }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>


        <!-- Requires Quiz Checkbox -->
        <div class="space-y-2">
          <label class="flex items-center">
            <input
              :checked="step.requiresQuiz"
              type="checkbox"
              class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              @change="(e) => updateStep({ requiresQuiz: (e.target as HTMLInputElement).checked })"
            >
            <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
              Wymaga quizu
            </span>
          </label>

          <!-- Quiz Configuration Button -->
          <div v-if="step.requiresQuiz" class="flex items-center gap-3 p-3 bg-purple-50 dark:bg-purple-900/20 rounded-lg border border-purple-200 dark:border-purple-800">
            <div class="flex-1">
              <p class="text-sm font-medium text-purple-900 dark:text-purple-100">
                Quiz dla tego etapu
              </p>
              <p class="text-xs text-purple-700 dark:text-purple-300 mt-1">
                <template v-if="step.quizQuestions && step.quizQuestions.length > 0">
                  {{ step.quizQuestions.length }} pytań | Próg: {{ step.passingScore || 80 }}%
                </template>
                <template v-else>
                  Brak pytań - kliknij "Konfiguruj quiz" aby dodać
                </template>
              </p>
            </div>
            <button
              class="inline-flex items-center px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white text-sm font-medium rounded-lg transition-colors"
              @click="showQuizModal = true"
            >
              <Icon name="heroicons:cog-6-tooth" class="w-4 h-4 mr-2" />
              Konfiguruj quiz
            </button>
          </div>
        </div>
      </div>

      <button
        class="text-red-600 hover:text-red-700 p-2"
        @click="$emit('remove')"
      >
        <Icon name="heroicons:trash" class="w-5 h-5" />
      </button>
    </div>

    <!-- Quiz Configuration Modal -->
    <QuizConfigModal
      v-if="showQuizModal"
      :questions="step.quizQuestions || []"
      :passing-score="step.passingScore"
      :step-order="step.stepOrder"
      @close="showQuizModal = false"
      @save="handleQuizSave"
    />
  </div>
</template>

<script setup lang="ts">
import type { RequestApprovalStepTemplate } from '~/types/requests'
import type { UserDto } from '~/stores/admin'
import type { RoleGroupDto } from '~/stores/roleGroups'

interface DepartmentDto {
  id: string
  name: string
}

const props = defineProps<{
  step: RequestApprovalStepTemplate
  roleGroups: RoleGroupDto[]
  users: UserDto[]
  departments: DepartmentDto[]
}>()

const emit = defineEmits<{
  remove: []
  'update:step': [value: RequestApprovalStepTemplate]
}>()

const userSearchTerm = ref('')
const showUserDropdown = ref(false)
const selectedUser = ref<UserDto | null>(null)
const dropdownRef = ref<HTMLElement | null>(null)
const showQuizModal = ref(false)

const departmentSearchTerm = ref('')
const showDepartmentDropdown = ref(false)
const selectedDepartment = ref<DepartmentDto | null>(null)
const departmentDropdownRef = ref<HTMLElement | null>(null)

const filteredUsers = computed(() => {
  if (!userSearchTerm.value) return props.users.slice(0, 10)

  const search = userSearchTerm.value.toLowerCase()
  return props.users.filter(user =>
    user.firstName.toLowerCase().includes(search) ||
    user.lastName.toLowerCase().includes(search) ||
    user.email.toLowerCase().includes(search) ||
    user.position.toLowerCase().includes(search) ||
    user.department.toLowerCase().includes(search)
  ).slice(0, 10)
})

const filteredDepartments = computed(() => {
  if (!Array.isArray(props.departments)) return []
  if (!departmentSearchTerm.value) return props.departments.slice(0, 10)

  const search = departmentSearchTerm.value.toLowerCase()
  return props.departments.filter(dept =>
    dept.name.toLowerCase().includes(search)
  ).slice(0, 10)
})

const updateStep = (updates: Partial<RequestApprovalStepTemplate>) => {
  emit('update:step', { ...props.step, ...updates })
}

const onApproverTypeChange = (newType: string) => {
  // Clear selections when type changes
  const updates: Partial<RequestApprovalStepTemplate> = {
    approverType: newType as any
  }

  if (newType !== 'SpecificUser') {
    updates.specificUserId = undefined
    selectedUser.value = null
    userSearchTerm.value = ''
  }
  if (newType !== 'SpecificDepartment') {
    updates.specificDepartmentId = undefined
    updates.specificDepartmentRoleType = undefined
    selectedDepartment.value = null
    departmentSearchTerm.value = ''
  } else if (newType === 'SpecificDepartment' && !props.step.specificDepartmentRoleType) {
    // Set default role type when switching to SpecificDepartment
    updates.specificDepartmentRoleType = 'Head'
  }

  updateStep(updates)
}

const searchUsers = () => {
  showUserDropdown.value = true
}

const selectUser = (user: UserDto) => {
  selectedUser.value = user
  updateStep({ specificUserId: user.id })
  userSearchTerm.value = `${user.firstName} ${user.lastName}`
  showUserDropdown.value = false
}

const clearUserSelection = () => {
  selectedUser.value = null
  updateStep({ specificUserId: undefined })
  userSearchTerm.value = ''
}

const searchDepartments = () => {
  showDepartmentDropdown.value = true
}

const selectDepartment = (dept: DepartmentDto) => {
  selectedDepartment.value = dept
  updateStep({ specificDepartmentId: dept.id })
  departmentSearchTerm.value = dept.name
  showDepartmentDropdown.value = false
}

const clearDepartmentSelection = () => {
  selectedDepartment.value = null
  updateStep({ specificDepartmentId: undefined })
  departmentSearchTerm.value = ''
  showDepartmentDropdown.value = true
}

const handleQuizSave = (data: { questions: any[], passingScore: number }) => {
  updateStep({
    quizQuestions: data.questions,
    passingScore: data.passingScore
  })
  showQuizModal.value = false
}

// Initialize selected user and department if already set
onMounted(() => {
  if (props.step.specificUserId && Array.isArray(props.users)) {
    const user = props.users.find(u => u.id === props.step.specificUserId)
    if (user) {
      selectedUser.value = user
      userSearchTerm.value = `${user.firstName} ${user.lastName}`
    }
  }

  if (props.step.specificDepartmentId && Array.isArray(props.departments)) {
    const dept = props.departments.find(d => d.id === props.step.specificDepartmentId)
    if (dept) {
      selectedDepartment.value = dept
      departmentSearchTerm.value = dept.name
    }
  }

  // Close dropdown when clicking outside
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

const handleClickOutside = (event: MouseEvent) => {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target as Node)) {
    showUserDropdown.value = false
  }
  if (departmentDropdownRef.value && !departmentDropdownRef.value.contains(event.target as Node)) {
    showDepartmentDropdown.value = false
  }
}
</script>

