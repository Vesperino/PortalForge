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
            @change="(e) => { const newType = (e.target as HTMLSelectElement).value as any; onApproverTypeChange(newType); updateStep({ approverType: newType }); }"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
          >
            <option value="Role">Rola hierarchiczna</option>
            <option value="SpecificUser">Konkretny użytkownik</option>
            <option value="UserGroup">Grupa użytkowników</option>
            <option value="Submitter">Wnioskodawca (self-approval)</option>
          </select>
        </div>

        <!-- Role Selection (when ApproverType = Role) -->
        <div v-if="step.approverType === 'Role'">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Rola zatwierdzającego
          </label>
          <select
            :value="step.approverRole || ''"
            @change="(e) => updateStep({ approverRole: (e.target as HTMLSelectElement).value || undefined as any })"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
          >
            <option value="">Dowolny przełożony (pierwszy dostępny w hierarchii)</option>
            <option value="Manager">Kierownik (bezpośredni przełożony)</option>
            <option value="Director">Dyrektor (przełożony przełożonego)</option>
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
              @input="searchUsers"
              @focus="showUserDropdown = true"
              type="text"
              placeholder="Szukaj użytkownika..."
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
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
                @click="clearUserSelection"
                class="text-red-600 hover:text-red-700 p-1"
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
                @click="selectUser(user)"
                class="p-3 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer flex items-center gap-3"
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
            @change="(e) => updateStep({ approverGroupId: (e.target as HTMLSelectElement).value })"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg dark:bg-gray-700 dark:text-white"
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
        <div class="flex items-center">
          <label class="flex items-center">
            <input
              :checked="step.requiresQuiz"
              @change="(e) => updateStep({ requiresQuiz: (e.target as HTMLInputElement).checked })"
              type="checkbox"
              class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
            >
            <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">
              Wymaga quizu
            </span>
          </label>
        </div>
      </div>

      <button
        @click="$emit('remove')"
        class="text-red-600 hover:text-red-700 p-2"
      >
        <Icon name="heroicons:trash" class="w-5 h-5" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { RequestApprovalStepTemplate } from '~/types/requests'
import type { UserDto } from '~/stores/admin'
import type { RoleGroupDto } from '~/stores/roleGroups'

const props = defineProps<{
  step: RequestApprovalStepTemplate
  roleGroups: RoleGroupDto[]
  users: UserDto[]
}>()

const emit = defineEmits<{
  remove: []
  'update:step': [value: RequestApprovalStepTemplate]
}>()

const userSearchTerm = ref('')
const showUserDropdown = ref(false)
const selectedUser = ref<UserDto | null>(null)
const dropdownRef = ref<HTMLElement | null>(null)

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
  if (newType !== 'Role') {
    updates.approverRole = undefined
  }
  if (newType !== 'SpecificUser') {
    updates.specificUserId = undefined
    selectedUser.value = null
    userSearchTerm.value = ''
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

