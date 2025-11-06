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
            @change="(e) => { const newType = (e.target as HTMLSelectElement).value as any; onApproverTypeChange(newType); updateStep({ approverType: newType }); }"
          >
            <option value="DirectSupervisor">Kierownik działu (HeadOfDepartment)</option>
            <option value="DepartmentDirector">Dyrektor działu (Director)</option>
            <option value="SpecificUser">Konkretny użytkownik</option>
            <option value="SpecificDepartment">Szef konkretnego działu</option>
            <option value="UserGroup">Grupa użytkowników</option>
            <option value="Submitter">Wnioskodawca (self-approval)</option>
          </select>
        </div>

        <!-- Department Selection (when ApproverType = SpecificDepartment) -->
        <div v-if="step.approverType === 'SpecificDepartment'">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Wybierz dział
          </label>
          <select
            :value="step.specificDepartmentId"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
            @change="(e) => updateStep({ specificDepartmentId: (e.target as HTMLSelectElement).value })"
          >
            <option value="">-- Wybierz dział --</option>
            <option v-for="dept in departments" :key="dept.id" :value="dept.id">
              {{ dept.name }}
            </option>
          </select>
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

        <!-- Group Selection (when ApproverType = UserGroup) -->
        <div v-if="step.approverType === 'UserGroup'">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Wybierz grupę użytkowników
          </label>
          <select
            :value="step.approverGroupId"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
            @change="(e) => updateStep({ approverGroupId: (e.target as HTMLSelectElement).value })"
          >
            <option value="">-- Wybierz grupę --</option>
            <option v-for="group in roleGroups" :key="group.id" :value="group.id">
              {{ group.name }} ({{ group.userCount }} użytkowników)
            </option>
          </select>
        </div>

        <!-- Submitter Info (when ApproverType = Submitter) -->
        <div v-if="step.approverType === 'Submitter'" class="p-3 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg">
          <p class="text-sm text-yellow-800 dark:text-yellow-200">
            <Icon name="heroicons:information-circle" class="w-4 h-4 inline mr-1" />
            Ten krok będzie zatwierdzany przez osobę, która złożyła wniosek (self-approval/acknowledgment).
          </p>
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

const updateStep = (updates: Partial<RequestApprovalStepTemplate>) => {
  emit('update:step', { ...props.step, ...updates })
}

const onApproverTypeChange = (newType: string) => {
  // Clear selections when type changes
  const updates: Partial<RequestApprovalStepTemplate> = {}

  if (newType !== 'SpecificUser') {
    updates.specificUserId = undefined
    selectedUser.value = null
    userSearchTerm.value = ''
  }
  if (newType !== 'SpecificDepartment') {
    updates.specificDepartmentId = undefined
  }
  if (newType !== 'UserGroup') {
    updates.approverGroupId = undefined
  }

  if (Object.keys(updates).length > 0) {
    updateStep(updates)
  }
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

const handleQuizSave = (data: { questions: any[], passingScore: number }) => {
  updateStep({
    quizQuestions: data.questions,
    passingScore: data.passingScore
  })
  showQuizModal.value = false
}

// Initialize selected user if specificUserId is already set
onMounted(() => {
  if (props.step.specificUserId) {
    const user = props.users.find(u => u.id === props.step.specificUserId)
    if (user) {
      selectedUser.value = user
      userSearchTerm.value = `${user.firstName} ${user.lastName}`
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
}
</script>

