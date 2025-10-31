<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { DepartmentTreeDto, CreateDepartmentDto, UpdateDepartmentDto } from '~/types/department'

definePageMeta({
  middleware: ['auth', 'admin'],
  layout: 'dashboard'
})

// State
const departmentTree = ref<DepartmentTreeDto[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

// Modal states
const showDepartmentModal = ref(false)
const showDeleteModal = ref(false)
const editingDepartment = ref<DepartmentTreeDto | null>(null)
const deletingDepartmentId = ref<string | null>(null)
const parentDepartmentId = ref<string | null>(null)

// Form state
const departmentForm = ref<CreateDepartmentDto | UpdateDepartmentDto>({
  name: '',
  description: null,
  parentDepartmentId: null,
  departmentHeadId: null
})

// Form validation errors
const formErrors = ref<Record<string, string>>({})

// Load department tree
const loadDepartmentTree = async () => {
  isLoading.value = true
  error.value = null

  try {
    const data = await $fetch<DepartmentTreeDto[]>('/api/departments/tree')
    departmentTree.value = data
  } catch (err: any) {
    error.value = err.message || 'Nie udało się pobrać struktury organizacyjnej'
    console.error('Error loading department tree:', err)
  } finally {
    isLoading.value = false
  }
}

// Open modal to add root department
const openAddRootModal = () => {
  editingDepartment.value = null
  parentDepartmentId.value = null
  departmentForm.value = {
    name: '',
    description: null,
    parentDepartmentId: null,
    departmentHeadId: null
  }
  formErrors.value = {}
  showDepartmentModal.value = true
}

// Open modal to add child department
const handleAddChild = (departmentId: string) => {
  editingDepartment.value = null
  parentDepartmentId.value = departmentId
  departmentForm.value = {
    name: '',
    description: null,
    parentDepartmentId: departmentId,
    departmentHeadId: null
  }
  formErrors.value = {}
  showDepartmentModal.value = true
}

// Find department in tree by ID (recursive)
const findDepartmentById = (
  departments: DepartmentTreeDto[],
  id: string
): DepartmentTreeDto | null => {
  for (const dept of departments) {
    if (dept.id === id) return dept
    if (dept.children.length > 0) {
      const found = findDepartmentById(dept.children, id)
      if (found) return found
    }
  }
  return null
}

// Open modal to edit department
const handleEdit = async (departmentId: string) => {
  try {
    // Find department in tree or fetch from API
    let department = findDepartmentById(departmentTree.value, departmentId)

    if (!department) {
      // Fallback: fetch from API
      department = await $fetch<DepartmentTreeDto>(`/api/departments/${departmentId}`)
    }

    editingDepartment.value = department
    departmentForm.value = {
      name: department.name,
      description: department.description,
      parentDepartmentId: department.parentDepartmentId,
      departmentHeadId: department.departmentHeadId
    }
    formErrors.value = {}
    showDepartmentModal.value = true
  } catch (err: any) {
    error.value = 'Nie udało się pobrać danych działu'
    console.error('Error loading department:', err)
  }
}

// Validate form
const validateForm = (): boolean => {
  formErrors.value = {}

  if (!departmentForm.value.name || departmentForm.value.name.trim().length === 0) {
    formErrors.value.name = 'Nazwa jest wymagana'
  } else if (departmentForm.value.name.length > 100) {
    formErrors.value.name = 'Nazwa nie może przekraczać 100 znaków'
  }

  if (departmentForm.value.description && departmentForm.value.description.length > 500) {
    formErrors.value.description = 'Opis nie może przekraczać 500 znaków'
  }

  return Object.keys(formErrors.value).length === 0
}

// Save department (create or update)
const saveDepartment = async () => {
  if (!validateForm()) return

  isLoading.value = true
  error.value = null

  try {
    if (editingDepartment.value) {
      // Update existing department
      await $fetch(`/api/departments/${editingDepartment.value.id}`, {
        method: 'PUT',
        body: departmentForm.value
      })
    } else {
      // Create new department
      await $fetch('/api/departments', {
        method: 'POST',
        body: departmentForm.value
      })
    }

    // Reload tree
    await loadDepartmentTree()

    // Close modal
    showDepartmentModal.value = false
  } catch (err: any) {
    if (err.data?.errors) {
      // Handle validation errors from API
      const apiErrors = err.data.errors
      formErrors.value = {}
      for (const key in apiErrors) {
        formErrors.value[key.toLowerCase()] = apiErrors[key][0]
      }
    } else {
      error.value = err.message || 'Nie udało się zapisać działu'
    }
    console.error('Error saving department:', err)
  } finally {
    isLoading.value = false
  }
}

// Open delete confirmation modal
const handleDelete = (departmentId: string) => {
  deletingDepartmentId.value = departmentId
  showDeleteModal.value = true
}

// Delete department
const confirmDelete = async () => {
  if (!deletingDepartmentId.value) return

  isLoading.value = true
  error.value = null

  try {
    await $fetch(`/api/departments/${deletingDepartmentId.value}`, {
      method: 'DELETE'
    })

    // Reload tree
    await loadDepartmentTree()

    // Close modal
    showDeleteModal.value = false
    deletingDepartmentId.value = null
  } catch (err: any) {
    error.value = err.message || 'Nie udało się usunąć działu'
    console.error('Error deleting department:', err)
  } finally {
    isLoading.value = false
  }
}

// Cancel delete
const cancelDelete = () => {
  showDeleteModal.value = false
  deletingDepartmentId.value = null
}

// Close department modal
const closeDepartmentModal = () => {
  showDepartmentModal.value = false
  editingDepartment.value = null
  parentDepartmentId.value = null
  formErrors.value = {}
}

// Load data on mount
onMounted(() => {
  loadDepartmentTree()
})
</script>

<template>
  <div class="admin-structure-page">
    <!-- Page Header -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Struktura organizacyjna
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
          Zarządzaj działami i strukturą firmy
        </p>
      </div>
      <button
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium flex items-center gap-2 transition-colors"
        @click="openAddRootModal"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M12 4v16m8-8H4"
          />
        </svg>
        Dodaj dział główny
      </button>
    </div>

    <!-- Error Alert -->
    <div
      v-if="error"
      class="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg flex items-start gap-3"
    >
      <svg class="w-5 h-5 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
        />
      </svg>
      <div class="flex-1">
        <p class="text-sm font-medium text-red-800 dark:text-red-300">{{ error }}</p>
      </div>
      <button
        class="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-200"
        @click="error = null"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading && departmentTree.length === 0" class="flex items-center justify-center h-64">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
        <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie struktury...</p>
      </div>
    </div>

    <!-- Department Tree -->
    <div v-else-if="departmentTree.length > 0" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <DepartmentTree
        v-for="department in departmentTree"
        :key="department.id"
        :department="department"
        @add-child="handleAddChild"
        @edit="handleEdit"
        @delete="handleDelete"
      />
    </div>

    <!-- Empty State -->
    <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
      <svg
        class="w-16 h-16 text-gray-400 mx-auto mb-4"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
        />
      </svg>
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
        Brak działów
      </h3>
      <p class="text-gray-600 dark:text-gray-400 mb-6">
        Zacznij od dodania głównego działu w organizacji
      </p>
      <button
        class="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium inline-flex items-center gap-2 transition-colors"
        @click="openAddRootModal"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M12 4v16m8-8H4"
          />
        </svg>
        Dodaj pierwszy dział
      </button>
    </div>

    <!-- Department Form Modal -->
    <div
      v-if="showDepartmentModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="closeDepartmentModal"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-lg w-full max-h-[90vh] overflow-y-auto">
        <!-- Modal Header -->
        <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
            {{ editingDepartment ? 'Edytuj dział' : 'Dodaj dział' }}
          </h2>
          <button
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
            @click="closeDepartmentModal"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Modal Body -->
        <div class="p-6 space-y-4">
          <!-- Name Field -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Nazwa <span class="text-red-500">*</span>
            </label>
            <input
              v-model="departmentForm.name"
              type="text"
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              :class="{
                'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white': !formErrors.name,
                'border-red-500 dark:border-red-500': formErrors.name
              }"
              placeholder="np. Dział IT"
            >
            <p v-if="formErrors.name" class="mt-1 text-sm text-red-600 dark:text-red-400">
              {{ formErrors.name }}
            </p>
          </div>

          <!-- Description Field -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Opis
            </label>
            <textarea
              v-model="departmentForm.description"
              rows="3"
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              :class="{
                'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white': !formErrors.description,
                'border-red-500 dark:border-red-500': formErrors.description
              }"
              placeholder="Opcjonalny opis działu"
            />
            <p v-if="formErrors.description" class="mt-1 text-sm text-red-600 dark:text-red-400">
              {{ formErrors.description }}
            </p>
          </div>

          <!-- Department Head Field -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Kierownik działu
            </label>
            <input
              v-model="departmentForm.departmentHeadId"
              type="text"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
              placeholder="UUID kierownika (opcjonalne)"
            >
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              W przyszłości zostanie zastąpione autocomplete
            </p>
          </div>

          <!-- Parent Department (shown only when editing) -->
          <div v-if="editingDepartment">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Dział nadrzędny
            </label>
            <input
              v-model="departmentForm.parentDepartmentId"
              type="text"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
              placeholder="UUID działu nadrzędnego (opcjonalne)"
            >
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Pozostaw puste dla działu głównego
            </p>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700">
          <button
            class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            @click="closeDepartmentModal"
          >
            Anuluj
          </button>
          <button
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="isLoading"
            @click="saveDepartment"
          >
            {{ isLoading ? 'Zapisywanie...' : 'Zapisz' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div
      v-if="showDeleteModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="cancelDelete"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full">
        <!-- Modal Header -->
        <div class="p-6">
          <div class="flex items-center gap-4 mb-4">
            <div class="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center flex-shrink-0">
              <svg class="w-6 h-6 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                />
              </svg>
            </div>
            <div>
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                Usuń dział
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Czy na pewno chcesz usunąć ten dział?
              </p>
            </div>
          </div>

          <div class="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg p-4">
            <p class="text-sm text-yellow-800 dark:text-yellow-300">
              To działanie spowoduje dezaktywację działu (soft delete). Poddziały i pracownicy pozostaną w systemie.
            </p>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700">
          <button
            class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            @click="cancelDelete"
          >
            Anuluj
          </button>
          <button
            class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="isLoading"
            @click="confirmDelete"
          >
            {{ isLoading ? 'Usuwanie...' : 'Usuń' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-structure-page {
  /* Additional styles if needed */
}
</style>
