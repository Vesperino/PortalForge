<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { DepartmentTreeDto } from '~/types/department'
import type { User } from '~/types/auth'
import type { DepartmentFormData, QuickEditFormData } from '~/composables/useDepartmentStructure'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default'
})

const {
  departmentTree,
  allUsers,
  isLoading,
  error,
  stats,
  unassignedUsers,
  departmentsFlat,
  loadData,
  getDepartmentById,
  createDepartment,
  updateDepartment,
  deleteDepartment,
  assignUserToDepartment,
  updateUserQuickEdit,
  createEmptyFormData,
  createFormDataFromDepartment,
  validateDepartmentForm,
  clearError
} = useDepartmentStructure()

const activeTab = ref<'structure' | 'employees'>('structure')

const showDepartmentModal = ref(false)
const showDeleteModal = ref(false)
const showAssignModal = ref(false)
const showQuickEditModal = ref(false)

const editingDepartment = ref<DepartmentTreeDto | null>(null)
const deletingDepartmentId = ref<string | null>(null)
const assigningUserId = ref<string | null>(null)
const editingUser = ref<User | null>(null)

const departmentForm = ref<DepartmentFormData>(createEmptyFormData())
const quickEditForm = ref<QuickEditFormData>({
  departmentId: '',
  departmentName: '',
  position: '',
  positionId: null
})

const selectedDepartmentHead = ref<User | null>(null)
const selectedDepartmentDirector = ref<User | null>(null)
const formErrors = ref<Record<string, string>>({})

function openAddRootModal(): void {
  editingDepartment.value = null
  selectedDepartmentHead.value = null
  selectedDepartmentDirector.value = null
  departmentForm.value = createEmptyFormData()
  formErrors.value = {}
  showDepartmentModal.value = true
}

function handleAddChild(departmentId: string): void {
  editingDepartment.value = null
  selectedDepartmentHead.value = null
  selectedDepartmentDirector.value = null
  departmentForm.value = {
    ...createEmptyFormData(),
    parentDepartmentId: departmentId
  }
  formErrors.value = {}
  showDepartmentModal.value = true
}

async function handleEdit(departmentId: string): Promise<void> {
  const department = await getDepartmentById(departmentId)
  if (!department) return

  editingDepartment.value = department
  departmentForm.value = createFormDataFromDepartment(department)

  if (department.departmentHeadId) {
    const head = allUsers.value.find(u => u.id === department.departmentHeadId)
    selectedDepartmentHead.value = head || null
  } else {
    selectedDepartmentHead.value = null
  }

  if (department.departmentDirectorId) {
    const director = allUsers.value.find(u => u.id === department.departmentDirectorId)
    selectedDepartmentDirector.value = director || null
  } else {
    selectedDepartmentDirector.value = null
  }

  formErrors.value = {}
  showDepartmentModal.value = true
}

function handleDelete(departmentId: string): void {
  deletingDepartmentId.value = departmentId
  showDeleteModal.value = true
}

async function saveDepartment(): Promise<void> {
  formErrors.value = validateDepartmentForm(departmentForm.value)
  if (Object.keys(formErrors.value).length > 0) return

  let success: boolean
  if (editingDepartment.value) {
    success = await updateDepartment(editingDepartment.value.id, {
      name: departmentForm.value.name,
      description: departmentForm.value.description,
      parentDepartmentId: departmentForm.value.parentDepartmentId,
      departmentHeadId: departmentForm.value.departmentHeadId,
      departmentDirectorId: departmentForm.value.departmentDirectorId,
      isActive: departmentForm.value.isActive ?? true
    })
  } else {
    success = await createDepartment({
      name: departmentForm.value.name,
      description: departmentForm.value.description,
      parentDepartmentId: departmentForm.value.parentDepartmentId,
      departmentHeadId: departmentForm.value.departmentHeadId,
      departmentDirectorId: departmentForm.value.departmentDirectorId
    })
  }

  if (success) {
    closeDepartmentModal()
  }
}

function closeDepartmentModal(): void {
  showDepartmentModal.value = false
  editingDepartment.value = null
  selectedDepartmentHead.value = null
  selectedDepartmentDirector.value = null
  formErrors.value = {}
}

async function confirmDelete(): Promise<void> {
  if (!deletingDepartmentId.value) return

  const success = await deleteDepartment(deletingDepartmentId.value)
  if (success) {
    showDeleteModal.value = false
    deletingDepartmentId.value = null
  }
}

function cancelDelete(): void {
  showDeleteModal.value = false
  deletingDepartmentId.value = null
}

function openAssignModal(userId: string): void {
  assigningUserId.value = userId
  showAssignModal.value = true
}

async function handleAssignUser(userId: string, departmentId: string): Promise<void> {
  const success = await assignUserToDepartment(userId, departmentId)
  if (success) {
    showAssignModal.value = false
    assigningUserId.value = null
  }
}

function closeAssignModal(): void {
  showAssignModal.value = false
  assigningUserId.value = null
}

function handleEmployeeClick(employeeId: string): void {
  const user = allUsers.value.find(u => u.id === employeeId)
  if (!user) return

  editingUser.value = user
  quickEditForm.value = {
    departmentId: user.departmentId || '',
    departmentName: user.department || '',
    position: user.position || '',
    positionId: user.positionId || null
  }
  showQuickEditModal.value = true
}

function onGlobalUserSelect(user: User | null): void {
  if (user?.id) {
    handleEmployeeClick(user.id)
  }
}

async function saveQuickEdit(): Promise<void> {
  if (!editingUser.value) return

  const success = await updateUserQuickEdit(editingUser.value, quickEditForm.value)
  if (success) {
    showQuickEditModal.value = false
    editingUser.value = null
  }
}

function closeQuickEditModal(): void {
  showQuickEditModal.value = false
  editingUser.value = null
}

function updateDepartmentForm(data: DepartmentFormData): void {
  departmentForm.value = data
}

function updateQuickEditForm(data: QuickEditFormData): void {
  quickEditForm.value = data
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="admin-structure-page min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <NuxtLink
        to="/admin"
        class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 mb-6"
      >
        <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        Powrot do panelu administracyjnego
      </NuxtLink>

      <div class="mb-8">
        <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
              Struktura organizacyjna
            </h1>
            <p class="text-sm text-gray-600 dark:text-gray-400 mt-2">
              Zarzadzaj dzialami i przypisuj pracownikow do struktury firmy
            </p>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div class="bg-gradient-to-br from-blue-500 to-blue-600 rounded-xl shadow-md p-5 text-white">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-blue-100 text-sm font-medium">Dzialy</p>
                <p class="text-3xl font-bold mt-1">{{ stats.totalDepartments }}</p>
              </div>
              <div class="w-12 h-12 bg-white/20 rounded-lg flex items-center justify-center">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-gradient-to-br from-green-500 to-green-600 rounded-xl shadow-md p-5 text-white">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-green-100 text-sm font-medium">Wszyscy pracownicy</p>
                <p class="text-3xl font-bold mt-1">{{ stats.totalEmployees }}</p>
              </div>
              <div class="w-12 h-12 bg-white/20 rounded-lg flex items-center justify-center">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-gradient-to-br from-purple-500 to-purple-600 rounded-xl shadow-md p-5 text-white">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-purple-100 text-sm font-medium">Przypisani</p>
                <p class="text-3xl font-bold mt-1">{{ stats.assignedEmployees }}</p>
              </div>
              <div class="w-12 h-12 bg-white/20 rounded-lg flex items-center justify-center">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-gradient-to-br from-orange-500 to-orange-600 rounded-xl shadow-md p-5 text-white">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-orange-100 text-sm font-medium">Nieprzypisani</p>
                <p class="text-3xl font-bold mt-1">{{ stats.unassignedEmployees }}</p>
              </div>
              <div class="w-12 h-12 bg-white/20 rounded-lg flex items-center justify-center">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div
        v-if="error"
        class="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg flex items-start gap-3"
      >
        <svg class="w-5 h-5 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div class="flex-1">
          <p class="text-sm font-medium text-red-800 dark:text-red-300">{{ error }}</p>
        </div>
        <button class="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-200" @click="clearError">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div class="mb-6">
        <div class="border-b border-gray-200 dark:border-gray-700">
          <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
            <nav class="flex flex-wrap gap-4">
              <button
                class="px-4 py-3 text-sm font-medium border-b-2 transition-colors"
                :class="activeTab === 'structure'
                  ? 'text-blue-600 dark:text-blue-400 border-blue-600 dark:border-blue-400'
                  : 'text-gray-500 dark:text-gray-400 border-transparent hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
                data-testid="structure-tab"
                @click="activeTab = 'structure'"
              >
                <div class="flex items-center gap-2">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                  </svg>
                  Struktura dzialow
                </div>
              </button>
              <button
                class="px-4 py-3 text-sm font-medium border-b-2 transition-colors"
                :class="activeTab === 'employees'
                  ? 'text-blue-600 dark:text-blue-400 border-blue-600 dark:border-blue-400'
                  : 'text-gray-500 dark:text-gray-400 border-transparent hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
                data-testid="employees-tab"
                @click="activeTab = 'employees'"
              >
                <div class="flex items-center gap-2">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                  </svg>
                  Nieprzypisani pracownicy
                  <span v-if="stats.unassignedEmployees > 0" class="px-2 py-0.5 text-xs font-semibold bg-orange-100 dark:bg-orange-900/30 text-orange-800 dark:text-orange-300 rounded-full">
                    {{ stats.unassignedEmployees }}
                  </span>
                </div>
              </button>
            </nav>

            <div class="flex items-center gap-2 w-full md:w-auto md:max-w-xl mt-3 md:mt-0">
              <CommonUserAutocomplete
                class="flex-1"
                placeholder="Szybkie wyszukiwanie pracownika..."
                @select="onGlobalUserSelect"
              />
              <button
                class="px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-all shadow-md hover:shadow-lg whitespace-nowrap"
                data-testid="add-root-department"
                @click="openAddRootModal"
              >
                Dodaj dzial glowny
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="isLoading && departmentTree.length === 0" class="flex items-center justify-center h-96">
        <div class="text-center">
          <div class="animate-spin rounded-full h-16 w-16 border-b-4 border-blue-600 mx-auto" />
          <p class="mt-4 text-gray-600 dark:text-gray-400 font-medium">Ladowanie struktury...</p>
        </div>
      </div>

      <div v-else-if="activeTab === 'structure'">
        <div v-if="departmentTree.length > 0" class="bg-white dark:bg-gray-800 rounded-xl shadow-md p-6">
          <AdminStructureDepartmentTree
            v-for="department in departmentTree"
            :key="department.id"
            :department="department"
            @add-child="handleAddChild"
            @edit="handleEdit"
            @delete="handleDelete"
            @employee-click="handleEmployeeClick"
          />
        </div>

        <div v-else class="bg-white dark:bg-gray-800 rounded-xl shadow-md p-16 text-center">
          <div class="max-w-md mx-auto">
            <div class="w-20 h-20 bg-blue-100 dark:bg-blue-900/30 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg class="w-10 h-10 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
            </div>
            <h3 class="text-xl font-semibold text-gray-900 dark:text-white mb-3">
              Brak struktury organizacyjnej
            </h3>
            <p class="text-gray-600 dark:text-gray-400 mb-8">
              Zacznij od utworzenia pierwszego dzialu w organizacji. Mozesz pozniej dodawac poddzialy i przypisywac pracownikow.
            </p>
            <button
              class="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium inline-flex items-center gap-2 transition-all shadow-md hover:shadow-lg"
              @click="openAddRootModal"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              Dodaj pierwszy dzial
            </button>
          </div>
        </div>
      </div>

      <div v-else-if="activeTab === 'employees'">
        <AdminStructureUserAssignmentPanel
          :users="unassignedUsers"
          :department-tree="departmentTree"
          :is-loading="isLoading"
          :show-modal="showAssignModal"
          :assigning-user-id="assigningUserId"
          @assign="handleAssignUser"
          @open-assign-modal="openAssignModal"
          @close-assign-modal="closeAssignModal"
        />
      </div>

      <AdminStructureDepartmentForm
        :is-open="showDepartmentModal"
        :is-loading="isLoading"
        :editing-department="editingDepartment"
        :form-data="departmentForm"
        :form-errors="formErrors"
        :selected-head="selectedDepartmentHead"
        :selected-director="selectedDepartmentDirector"
        @update:form-data="updateDepartmentForm"
        @update:selected-head="selectedDepartmentHead = $event"
        @update:selected-director="selectedDepartmentDirector = $event"
        @save="saveDepartment"
        @close="closeDepartmentModal"
      />

      <AdminStructureDeleteConfirmModal
        :is-open="showDeleteModal"
        :is-loading="isLoading"
        @confirm="confirmDelete"
        @cancel="cancelDelete"
      />

      <AdminStructureQuickEditModal
        :is-open="showQuickEditModal"
        :is-loading="isLoading"
        :user="editingUser"
        :form-data="quickEditForm"
        :departments="departmentsFlat"
        @update:form-data="updateQuickEditForm"
        @save="saveQuickEdit"
        @close="closeQuickEditModal"
      />
    </div>
  </div>
</template>

<style scoped>
.admin-structure-page {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
