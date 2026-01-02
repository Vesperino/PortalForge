<script setup lang="ts">
import { ref, computed } from 'vue'
import type { User } from '~/types/auth'
import type { DepartmentTreeDto } from '~/types/department'

interface Props {
  users: User[]
  departmentTree: DepartmentTreeDto[]
  isLoading?: boolean
  showModal: boolean
  assigningUserId: string | null
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false
})

const emit = defineEmits<{
  'assign': [userId: string, departmentId: string]
  'open-assign-modal': [userId: string]
  'close-assign-modal': []
}>()

const searchQuery = ref('')

const filteredUsers = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return props.users
  return props.users.filter(u =>
    `${u.firstName} ${u.lastName}`.toLowerCase().includes(q) ||
    (u.email || '').toLowerCase().includes(q) ||
    (u.position || '').toLowerCase().includes(q)
  )
})

function handleOpenAssignModal(userId: string): void {
  emit('open-assign-modal', userId)
}

function handleAssign(departmentId: string): void {
  if (props.assigningUserId) {
    emit('assign', props.assigningUserId, departmentId)
  }
}

function handleCloseModal(): void {
  emit('close-assign-modal')
}

function getInitials(firstName: string, lastName: string): string {
  return `${firstName[0] || ''}${lastName[0] || ''}`.toUpperCase()
}
</script>

<template>
  <div class="user-assignment-panel">
    <div v-if="users.length > 0" class="bg-white dark:bg-gray-800 rounded-xl shadow-md overflow-hidden">
      <div class="p-6 border-b border-gray-200 dark:border-gray-700">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white">
          Pracownicy bez przypisanego dzialu
        </h2>
        <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
          Kliknij "Przypisz do dzialu" aby dodac pracownika do struktury organizacyjnej
        </p>
        <div class="mt-4">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Wyszukaj po imieniu, nazwisku, emailu lub stanowisku..."
            class="w-full px-4 py-2.5 border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
            data-testid="user-search-input"
          >
        </div>
      </div>
      <div class="divide-y divide-gray-200 dark:divide-gray-700">
        <div
          v-for="user in filteredUsers"
          :key="user.id"
          class="p-6 hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
          data-testid="unassigned-user-item"
        >
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-4">
              <div class="w-12 h-12 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white font-semibold text-lg">
                {{ getInitials(user.firstName || '', user.lastName || '') }}
              </div>
              <div>
                <h3 class="text-base font-semibold text-gray-900 dark:text-white">
                  {{ user.firstName }} {{ user.lastName }}
                </h3>
                <p class="text-sm text-gray-600 dark:text-gray-400">{{ user.email }}</p>
                <p v-if="user.position" class="text-sm text-gray-500 dark:text-gray-500 mt-0.5">
                  {{ user.position }}
                </p>
              </div>
            </div>
            <button
              class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium text-sm transition-colors"
              data-testid="assign-button"
              @click="handleOpenAssignModal(user.id)"
            >
              Przypisz do dzialu
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="bg-white dark:bg-gray-800 rounded-xl shadow-md p-16 text-center">
      <div class="max-w-md mx-auto">
        <div class="w-20 h-20 bg-green-100 dark:bg-green-900/30 rounded-full flex items-center justify-center mx-auto mb-6">
          <svg class="w-10 h-10 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h3 class="text-xl font-semibold text-gray-900 dark:text-white mb-3">
          Swietnie! Wszyscy przypisani
        </h3>
        <p class="text-gray-600 dark:text-gray-400">
          Wszyscy pracownicy maja przypisane dzialy w strukturze organizacyjnej.
        </p>
      </div>
    </div>

    <div
      v-if="showModal"
      class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
    >
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-lg w-full max-h-[80vh] overflow-hidden flex flex-col">
        <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white flex items-center gap-2">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            Wybierz dzial
          </h2>
          <button
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
            data-testid="close-assign-modal"
            @click="handleCloseModal"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div class="p-6 overflow-y-auto flex-1">
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
            Wybierz dzial, do ktorego chcesz przypisac pracownika
          </p>

          <div class="space-y-2">
            <AdminStructureDepartmentTree
              v-for="department in departmentTree"
              :key="department.id"
              :department="department"
              :selectable="true"
              @select="handleAssign"
            />
          </div>
        </div>

        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
          <button
            class="px-5 py-2.5 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            @click="handleCloseModal"
          >
            Anuluj
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
