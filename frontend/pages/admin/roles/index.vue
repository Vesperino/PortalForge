<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Header -->
      <div class="mb-8">
        <NuxtLink
          to="/admin"
          class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 mb-4"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Powrót do panelu administracyjnego
        </NuxtLink>
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Zarządzanie Grupami Ról</h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">Przeglądaj grupy ról i przypisane uprawnienia</p>
          </div>
          <NuxtLink
            to="/admin/roles/create"
            class="inline-flex items-center px-4 py-2 bg-blue-600 dark:bg-blue-500 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 transition-colors"
          >
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            Utwórz Grupę Ról
          </NuxtLink>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="roleGroupsStore.loading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 dark:border-blue-400"/>
        <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie grup ról...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="roleGroupsStore.error" class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 rounded-lg p-4 mb-6">
        <p class="text-red-800 dark:text-red-300">{{ roleGroupsStore.error }}</p>
      </div>

      <!-- Role Groups Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="roleGroup in roleGroupsStore.roleGroups"
          :key="roleGroup.id"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
        >
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-xl font-semibold text-gray-900 dark:text-white">{{ roleGroup.name }}</h3>
            <span
              v-if="roleGroup.isSystemRole"
              class="px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 dark:bg-blue-900/30 text-blue-800 dark:text-blue-300"
            >
              System
            </span>
          </div>

          <p class="text-gray-600 dark:text-gray-400 mb-4">{{ roleGroup.description }}</p>

          <div class="mb-4">
            <div class="flex items-center text-sm text-gray-500 dark:text-gray-400">
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
              {{ roleGroup.userCount }} {{ roleGroup.userCount === 1 ? 'użytkownik' : 'użytkowników' }}
            </div>
          </div>

          <div class="border-t border-gray-200 dark:border-gray-700 pt-4">
            <h4 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Uprawnienia ({{ roleGroup.permissions.length }})</h4>
            <div class="space-y-1 max-h-48 overflow-y-auto mb-4">
              <div
                v-for="permission in roleGroup.permissions"
                :key="permission.id"
                class="text-xs text-gray-600 dark:text-gray-400 flex items-start"
              >
                <svg class="w-4 h-4 mr-1 text-green-500 dark:text-green-400 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
                </svg>
                <span>{{ permission.description }}</span>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex items-center justify-end space-x-2 pt-4 border-t border-gray-200 dark:border-gray-700">
              <NuxtLink
                :to="`/admin/roles/edit/${roleGroup.id}`"
                class="inline-flex items-center px-3 py-1.5 text-sm bg-blue-600 dark:bg-blue-500 text-white rounded hover:bg-blue-700 dark:hover:bg-blue-600 transition-colors"
              >
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                </svg>
                Edytuj
              </NuxtLink>
              <button
                v-if="!roleGroup.isSystemRole"
                class="inline-flex items-center px-3 py-1.5 text-sm bg-red-600 dark:bg-red-500 text-white rounded hover:bg-red-700 dark:hover:bg-red-600 transition-colors"
                @click="confirmDelete(roleGroup)"
              >
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
                Usuń
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Delete Confirmation Modal -->
      <div
        v-if="showDeleteModal"
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
        @click.self="cancelDelete"
      >
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full mx-4 p-6">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Potwierdź usunięcie</h3>
          <p class="text-gray-600 dark:text-gray-400 mb-6">
            Czy na pewno chcesz usunąć grupę ról <strong>{{ roleGroupToDelete?.name }}</strong>?
            <span v-if="roleGroupToDelete && roleGroupToDelete.userCount > 0" class="block mt-2 text-red-600 dark:text-red-400">
              Uwaga: Ta grupa ma przypisanych {{ roleGroupToDelete.userCount }} {{ roleGroupToDelete.userCount === 1 ? 'użytkownika' : 'użytkowników' }}.
            </span>
          </p>

          <!-- Error Message -->
          <div v-if="deleteError" class="mb-4 bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 rounded-lg p-3">
            <p class="text-sm text-red-800 dark:text-red-300">{{ deleteError }}</p>
          </div>

          <div class="flex items-center justify-end space-x-3">
            <button
              :disabled="isDeleting"
              class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              @click="cancelDelete"
            >
              Anuluj
            </button>
            <button
              :disabled="isDeleting"
              class="px-4 py-2 bg-red-600 dark:bg-red-500 text-white rounded-lg hover:bg-red-700 dark:hover:bg-red-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              @click="handleDelete"
            >
              {{ isDeleting ? 'Usuwanie...' : 'Usuń' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { RoleGroupDto } from '~/stores/roleGroups'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default',
})

useHead({
  title: 'Grupy Ról - Panel Administracyjny',
})

const roleGroupsStore = useRoleGroupsStore()
const { deleteRoleGroup } = useRoleGroupApi()

const showDeleteModal = ref(false)
const roleGroupToDelete = ref<RoleGroupDto | null>(null)
const isDeleting = ref(false)
const deleteError = ref<string | null>(null)

// Fetch role groups on mount
onMounted(() => {
  roleGroupsStore.fetchRoleGroups(true)
})

function confirmDelete(roleGroup: RoleGroupDto): void {
  roleGroupToDelete.value = roleGroup
  showDeleteModal.value = true
  deleteError.value = null
}

function cancelDelete(): void {
  if (!isDeleting.value) {
    showDeleteModal.value = false
    roleGroupToDelete.value = null
    deleteError.value = null
  }
}

async function handleDelete(): Promise<void> {
  if (!roleGroupToDelete.value) return

  isDeleting.value = true
  deleteError.value = null

  try {
    await deleteRoleGroup(roleGroupToDelete.value.id)

    // Refresh role groups list
    await roleGroupsStore.fetchRoleGroups(true)

    // Close modal
    showDeleteModal.value = false
    roleGroupToDelete.value = null
  } catch (error: unknown) {
    console.error('Error deleting role group:', error)
    const errorData = error as { data?: { message?: string } }
    deleteError.value = errorData?.data?.message || 'Nie udało się usunąć grupy ról'
  } finally {
    isDeleting.value = false
  }
}
</script>

