<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import type { DepartmentTreeDto, CreateDepartmentDto, UpdateDepartmentDto } from '~/types/department'
import type { User } from '~/types/user'
import PositionAutocomplete from '~/components/common/PositionAutocomplete.vue'

definePageMeta({
  middleware: ['auth', 'verified', 'admin'],
  layout: 'default'
})

const config = useRuntimeConfig()
const apiUrl = config.public.apiUrl

// State
const departmentTree = ref<DepartmentTreeDto[]>([])
const allUsers = ref<User[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)
const activeTab = ref<'structure' | 'employees'>('structure')

// Modal states
const showDepartmentModal = ref(false)
const showDeleteModal = ref(false)
const showAssignModal = ref(false)
const showQuickEditModal = ref(false)
const editingDepartment = ref<DepartmentTreeDto | null>(null)
const deletingDepartmentId = ref<string | null>(null)
const parentDepartmentId = ref<string | null>(null)
const assigningUserId = ref<string | null>(null)
const editingUser = ref<User | null>(null)

// Form state
const departmentForm = ref<CreateDepartmentDto | UpdateDepartmentDto>({
  name: '',
  description: null,
  parentDepartmentId: null,
  departmentHeadId: null
})

// Selected department head (for UserAutocomplete)
const selectedDepartmentHead = ref<User | null>(null)

// Quick edit form state
const quickEditForm = ref({
  departmentId: '',
  departmentName: '',
  position: '',
  positionId: null as string | null
})

// Form validation errors
const formErrors = ref<Record<string, string>>({})

// Statistics
const stats = computed(() => {
  const totalDepartments = countDepartments(departmentTree.value)
  const totalEmployees = allUsers.value.length
  const assignedEmployees = allUsers.value.filter(u => u.departmentId).length
  const unassignedEmployees = totalEmployees - assignedEmployees

  return {
    totalDepartments,
    totalEmployees,
    assignedEmployees,
    unassignedEmployees
  }
})

// Unassigned users
const unassignedUsers = computed(() => {
  return allUsers.value.filter(u => !u.departmentId)
})

// Flatten departments tree for dropdown
const departmentsFlat = computed(() => {
  const flattenDepartments = (depts: DepartmentTreeDto[], level = 0): Array<DepartmentTreeDto & { level: number }> => {
    return depts.flatMap(dept => [
      { ...dept, level },
      ...flattenDepartments(dept.children || [], level + 1)
    ])
  }
  return flattenDepartments(departmentTree.value)
})

// Count departments recursively
function countDepartments(departments: DepartmentTreeDto[]): number {
  let count = departments.length
  for (const dept of departments) {
    if (dept.children.length > 0) {
      count += countDepartments(dept.children)
    }
  }
  return count
}

// Get auth headers
const { getAuthHeaders } = useAuth()

// Load department tree
const loadDepartmentTree = async () => {
  try {
    const response = await $fetch<DepartmentTreeDto[]>(`${apiUrl}/api/departments/tree`, {
      headers: getAuthHeaders()
    })
    departmentTree.value = response
  } catch (err: any) {
    error.value = err.message || 'Nie udało się pobrać struktury organizacyjnej'
    console.error('Error loading department tree:', err)
  }
}

// Load all users
const loadAllUsers = async () => {
  try {
    const response = await $fetch(`${apiUrl}/api/admin/users`, {
      headers: getAuthHeaders()
    }) as any

    if (response && response.users && Array.isArray(response.users)) {
      allUsers.value = response.users as User[]
    } else if (Array.isArray(response)) {
      allUsers.value = response as User[]
    } else {
      console.error('Unexpected response format:', response)
      allUsers.value = []
    }
  } catch (err: any) {
    error.value = err.message || 'Nie udało się pobrać listy pracowników'
    console.error('Error loading users:', err)
    allUsers.value = []
  }
}

// Load all data
const loadData = async () => {
  isLoading.value = true
  error.value = null

  try {
    await Promise.all([
      loadDepartmentTree(),
      loadAllUsers()
    ])
  } finally {
    isLoading.value = false
  }
}

// Open modal to add root department
const openAddRootModal = () => {
  editingDepartment.value = null
  parentDepartmentId.value = null
  selectedDepartmentHead.value = null
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
  selectedDepartmentHead.value = null
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
    let department = findDepartmentById(departmentTree.value, departmentId)

    if (!department) {
      department = await $fetch<DepartmentTreeDto>(`${apiUrl}/api/departments/${departmentId}`, {
        headers: getAuthHeaders()
      })
    }

    editingDepartment.value = department
    departmentForm.value = {
      name: department.name,
      description: department.description,
      parentDepartmentId: department.parentDepartmentId,
      departmentHeadId: department.departmentHeadId,
      isActive: department.isActive
    }

    // Find and set selected department head
    if (department.departmentHeadId) {
      const head = allUsers.value.find(u => u.id === department.departmentHeadId)
      selectedDepartmentHead.value = head || null
    } else {
      selectedDepartmentHead.value = null
    }

    formErrors.value = {}
    showDepartmentModal.value = true
  } catch (err: any) {
    error.value = 'Nie udało się pobrać danych działu'
    console.error('Error loading department:', err)
  }
}

// Handle department head selection
const handleDepartmentHeadSelected = (user: User | null) => {
  selectedDepartmentHead.value = user
  departmentForm.value.departmentHeadId = user?.id || null
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
      await $fetch(`${apiUrl}/api/departments/${editingDepartment.value.id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: departmentForm.value
      })
    } else {
      await $fetch(`${apiUrl}/api/departments`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: departmentForm.value
      })
    }

    await loadData()
    showDepartmentModal.value = false
  } catch (err: any) {
    if (err.data?.errors) {
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
    await $fetch(`${apiUrl}/api/departments/${deletingDepartmentId.value}`, {
      method: 'DELETE',
      headers: getAuthHeaders()
    })

    await loadData()
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
  selectedDepartmentHead.value = null
  formErrors.value = {}
}

// Open assign user to department modal
const openAssignModal = (userId: string) => {
  assigningUserId.value = userId
  showAssignModal.value = true
}

// Assign user to department
const assignUserToDepartment = async (departmentId: string) => {
  if (!assigningUserId.value) return

  isLoading.value = true
  error.value = null

  try {
    const user = allUsers.value.find(u => u.id === assigningUserId.value)
    if (!user) return

    await $fetch(`${apiUrl}/api/admin/users/${user.id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: {
        firstName: user.firstName,
        lastName: user.lastName,
        department: user.department || '',
        departmentId: departmentId,
        position: user.position || '',
        phoneNumber: user.phoneNumber,
        role: user.role,
        roleGroupIds: [],
        isActive: user.isActive,
        updatedBy: user.id
      }
    })

    await loadData()
    showAssignModal.value = false
    assigningUserId.value = null
  } catch (err: any) {
    error.value = err.message || 'Nie udało się przypisać pracownika do działu'
    console.error('Error assigning user to department:', err)
  } finally {
    isLoading.value = false
  }
}

// Cancel assign
const cancelAssign = () => {
  showAssignModal.value = false
  assigningUserId.value = null
}

// Get users for a department
const getUsersForDepartment = (departmentId: string) => {
  return allUsers.value.filter(u => u.departmentId === departmentId)
}

// Handle employee click for quick edit
const handleEmployeeClick = async (employeeId: string) => {
  const user = allUsers.value.find(u => u.id === employeeId)
  if (!user) return

  editingUser.value = user
  quickEditForm.value = {
    departmentId: user.departmentId || '',
    departmentName: user.department || '',
    position: user.position || '',
    positionId: null
  }
  showQuickEditModal.value = true
}

// Handle department change in quick edit
const handleQuickEditDepartmentChange = (event: Event) => {
  const target = event.target as HTMLSelectElement
  const selectedDeptName = target.value

  // Find the department to get its ID
  const findDeptByName = (depts: DepartmentTreeDto[], name: string): DepartmentTreeDto | null => {
    for (const dept of depts) {
      if (dept.name === name) return dept
      if (dept.children.length > 0) {
        const found = findDeptByName(dept.children, name)
        if (found) return found
      }
    }
    return null
  }

  const dept = findDeptByName(departmentTree.value, selectedDeptName)
  if (dept) {
    quickEditForm.value.departmentId = dept.id
    quickEditForm.value.departmentName = dept.name
  }
}

// Handle position updates from autocomplete
const handlePositionUpdate = (value: string | null) => {
  quickEditForm.value.positionId = value
}

const handlePositionNameUpdate = (name: string) => {
  quickEditForm.value.position = name
}

// Save quick edit changes
const saveQuickEdit = async () => {
  if (!editingUser.value) return

  isLoading.value = true
  error.value = null

  try {
    await $fetch(`${apiUrl}/api/admin/users/${editingUser.value.id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: {
        firstName: editingUser.value.firstName,
        lastName: editingUser.value.lastName,
        department: quickEditForm.value.departmentName,
        departmentId: quickEditForm.value.departmentId,
        position: quickEditForm.value.position,
        phoneNumber: editingUser.value.phoneNumber,
        role: editingUser.value.role,
        roleGroupIds: [],
        isActive: editingUser.value.isActive,
        updatedBy: editingUser.value.id
      }
    })

    await loadData()
    showQuickEditModal.value = false
    editingUser.value = null
  } catch (err: any) {
    error.value = err.message || 'Nie udało się zaktualizować pracownika'
    console.error('Error updating employee:', err)
  } finally {
    isLoading.value = false
  }
}

// Close quick edit modal
const closeQuickEditModal = () => {
  showQuickEditModal.value = false
  editingUser.value = null
}

// Load data on mount
onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="admin-structure-page min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Back Link -->
      <NuxtLink
        to="/admin"
        class="inline-flex items-center text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 mb-6"
      >
        <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        Powrót do panelu administracyjnego
      </NuxtLink>

      <!-- Page Header with Stats -->
      <div class="mb-8">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
              Struktura organizacyjna
            </h1>
            <p class="text-sm text-gray-600 dark:text-gray-400 mt-2">
              Zarządzaj działami i przypisuj pracowników do struktury firmy
            </p>
          </div>
        <button
          class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium flex items-center gap-2 transition-all shadow-md hover:shadow-lg"
          @click="openAddRootModal"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Dodaj dział główny
        </button>
      </div>

      <!-- Statistics Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="bg-gradient-to-br from-blue-500 to-blue-600 rounded-xl shadow-md p-5 text-white">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-blue-100 text-sm font-medium">Działy</p>
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

    <!-- Error Alert -->
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
      <button class="text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-200" @click="error = null">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>

    <!-- Tabs -->
    <div class="mb-6">
      <div class="border-b border-gray-200 dark:border-gray-700">
        <nav class="flex gap-4">
          <button
            class="px-4 py-3 text-sm font-medium border-b-2 transition-colors"
            :class="activeTab === 'structure'
              ? 'text-blue-600 dark:text-blue-400 border-blue-600 dark:border-blue-400'
              : 'text-gray-500 dark:text-gray-400 border-transparent hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
            @click="activeTab = 'structure'"
          >
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
              Struktura działów
            </div>
          </button>
          <button
            class="px-4 py-3 text-sm font-medium border-b-2 transition-colors"
            :class="activeTab === 'employees'
              ? 'text-blue-600 dark:text-blue-400 border-blue-600 dark:border-blue-400'
              : 'text-gray-500 dark:text-gray-400 border-transparent hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
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
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading && departmentTree.length === 0" class="flex items-center justify-center h-96">
      <div class="text-center">
        <div class="animate-spin rounded-full h-16 w-16 border-b-4 border-blue-600 mx-auto" />
        <p class="mt-4 text-gray-600 dark:text-gray-400 font-medium">Ładowanie struktury...</p>
      </div>
    </div>

    <!-- Department Structure Tab -->
    <div v-else-if="activeTab === 'structure'">
      <div v-if="departmentTree.length > 0" class="bg-white dark:bg-gray-800 rounded-xl shadow-md p-6">
        <AdminDepartmentTree
          v-for="department in departmentTree"
          :key="department.id"
          :department="department"
          @add-child="handleAddChild"
          @edit="handleEdit"
          @delete="handleDelete"
          @employee-click="handleEmployeeClick"
        />
      </div>

      <!-- Empty State -->
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
            Zacznij od utworzenia pierwszego działu w organizacji. Możesz później dodawać poddziały i przypisywać pracowników.
          </p>
          <button
            class="px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium inline-flex items-center gap-2 transition-all shadow-md hover:shadow-lg"
            @click="openAddRootModal"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            Dodaj pierwszy dział
          </button>
        </div>
      </div>
    </div>

    <!-- Unassigned Employees Tab -->
    <div v-else-if="activeTab === 'employees'">
      <div v-if="unassignedUsers.length > 0" class="bg-white dark:bg-gray-800 rounded-xl shadow-md overflow-hidden">
        <div class="p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white">
            Pracownicy bez przypisanego działu
          </h2>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            Kliknij "Przypisz do działu" aby dodać pracownika do struktury organizacyjnej
          </p>
        </div>
        <div class="divide-y divide-gray-200 dark:divide-gray-700">
          <div
            v-for="user in unassignedUsers"
            :key="user.id"
            class="p-6 hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-4">
                <div class="w-12 h-12 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white font-semibold text-lg">
                  {{ user.firstName[0] }}{{ user.lastName[0] }}
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
                @click="openAssignModal(user.id)"
              >
                Przypisz do działu
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="bg-white dark:bg-gray-800 rounded-xl shadow-md p-16 text-center">
        <div class="max-w-md mx-auto">
          <div class="w-20 h-20 bg-green-100 dark:bg-green-900/30 rounded-full flex items-center justify-center mx-auto mb-6">
            <svg class="w-10 h-10 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white mb-3">
            Świetnie! Wszyscy przypisani
          </h3>
          <p class="text-gray-600 dark:text-gray-400">
            Wszyscy pracownicy mają przypisane działy w strukturze organizacyjnej.
          </p>
        </div>
      </div>
    </div>

    <!-- Department Form Modal -->
    <div
      v-if="showDepartmentModal"
      class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
      @click.self="closeDepartmentModal"
    >
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-lg w-full max-h-[90vh] overflow-y-auto">
        <!-- Modal Header -->
        <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white flex items-center gap-2">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
            {{ editingDepartment ? 'Edytuj dział' : 'Dodaj nowy dział' }}
          </h2>
          <button
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
            @click="closeDepartmentModal"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Modal Body -->
        <div class="p-6 space-y-5">
          <!-- Name Field -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Nazwa działu <span class="text-red-500">*</span>
            </label>
            <input
              v-model="departmentForm.name"
              type="text"
              class="w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              :class="{
                'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white': !formErrors.name,
                'border-red-500 dark:border-red-500 bg-red-50 dark:bg-red-900/20': formErrors.name
              }"
              placeholder="np. Dział IT, Marketing, HR"
            >
            <p v-if="formErrors.name" class="mt-2 text-sm text-red-600 dark:text-red-400 flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {{ formErrors.name }}
            </p>
          </div>

          <!-- Description Field -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Opis działu
            </label>
            <textarea
              v-model="departmentForm.description"
              rows="3"
              class="w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              :class="{
                'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white': !formErrors.description,
                'border-red-500 dark:border-red-500 bg-red-50 dark:bg-red-900/20': formErrors.description
              }"
              placeholder="Opcjonalny opis działu i jego zadań"
            />
            <p v-if="formErrors.description" class="mt-2 text-sm text-red-600 dark:text-red-400 flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              {{ formErrors.description }}
            </p>
          </div>

          <!-- Department Head Field with UserAutocomplete -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Kierownik działu
            </label>
            <CommonUserAutocomplete
              :selected-user="selectedDepartmentHead"
              placeholder="Wyszukaj kierownika po imieniu, nazwisku lub email"
              @update:selected-user="handleDepartmentHeadSelected"
            />
            <p class="mt-2 text-xs text-gray-500 dark:text-gray-400 flex items-center gap-1">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Zacznij wpisywać imię lub nazwisko aby wyszukać pracownika
            </p>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
          <button
            class="px-5 py-2.5 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            @click="closeDepartmentModal"
          >
            Anuluj
          </button>
          <button
            class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
            :disabled="isLoading"
            @click="saveDepartment"
          >
            <span v-if="isLoading" class="flex items-center gap-2">
              <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              Zapisywanie...
            </span>
            <span v-else>{{ editingDepartment ? 'Zapisz zmiany' : 'Utwórz dział' }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Assign User to Department Modal -->
    <div
      v-if="showAssignModal"
      class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
      @click.self="cancelAssign"
    >
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-lg w-full max-h-[80vh] overflow-hidden flex flex-col">
        <!-- Modal Header -->
        <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white flex items-center gap-2">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            Wybierz dział
          </h2>
          <button
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
            @click="cancelAssign"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Modal Body -->
        <div class="p-6 overflow-y-auto flex-1">
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
            Wybierz dział, do którego chcesz przypisać pracownika
          </p>

          <!-- Department Tree for Selection -->
          <div class="space-y-2">
            <AdminDepartmentTree
              v-for="department in departmentTree"
              :key="department.id"
              :department="department"
              :selectable="true"
              @select="assignUserToDepartment"
            />
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
          <button
            class="px-5 py-2.5 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            @click="cancelAssign"
          >
            Anuluj
          </button>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div
      v-if="showDeleteModal"
      class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
      @click.self="cancelDelete"
    >
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-md w-full">
        <!-- Modal Header -->
        <div class="p-6">
          <div class="flex items-start gap-4 mb-4">
            <div class="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center flex-shrink-0">
              <svg class="w-6 h-6 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
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
        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
          <button
            class="px-5 py-2.5 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            @click="cancelDelete"
          >
            Anuluj
          </button>
          <button
            class="px-5 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
            :disabled="isLoading"
            @click="confirmDelete"
          >
            <span v-if="isLoading" class="flex items-center gap-2">
              <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              Usuwanie...
            </span>
            <span v-else>Usuń dział</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Quick Edit Employee Modal -->
    <div
      v-if="showQuickEditModal && editingUser"
      class="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
      @click.self="closeQuickEditModal"
    >
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-2xl max-w-lg w-full">
        <!-- Modal Header -->
        <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white flex items-center gap-2">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
            Szybka edycja pracownika
          </h2>
          <button
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
            @click="closeQuickEditModal"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Modal Body -->
        <div class="p-6 space-y-5">
          <!-- Employee Info -->
          <div class="flex items-center gap-4 pb-4 border-b border-gray-200 dark:border-gray-700">
            <div class="w-12 h-12 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white font-semibold text-lg">
              {{ editingUser.firstName[0] }}{{ editingUser.lastName[0] }}
            </div>
            <div>
              <h3 class="text-base font-semibold text-gray-900 dark:text-white">
                {{ editingUser.firstName }} {{ editingUser.lastName }}
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-400">{{ editingUser.email }}</p>
            </div>
          </div>

          <!-- Department Field -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Dział <span class="text-red-500">*</span>
            </label>
            <select
              v-model="quickEditForm.departmentName"
              @change="handleQuickEditDepartmentChange"
              required
              class="w-full px-4 py-2.5 border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
            >
              <option value="">Wybierz dział</option>
              <option
                v-for="dept in departmentsFlat"
                :key="dept.id"
                :value="dept.name"
              >
                {{ dept.level > 0 ? '└─'.repeat(dept.level) + ' ' : '' }}{{ dept.name }}
              </option>
            </select>
          </div>

          <!-- Position Field -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Stanowisko <span class="text-red-500">*</span>
            </label>
            <PositionAutocomplete
              :model-value="quickEditForm.positionId"
              @update:modelValue="handlePositionUpdate"
              @update:positionName="handlePositionNameUpdate"
              placeholder="Wpisz lub wybierz stanowisko..."
              required
            />
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
          <button
            class="px-5 py-2.5 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            @click="closeQuickEditModal"
          >
            Anuluj
          </button>
          <button
            class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
            :disabled="isLoading || !quickEditForm.departmentName || !quickEditForm.position"
            @click="saveQuickEdit"
          >
            <span v-if="isLoading" class="flex items-center gap-2">
              <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              Zapisywanie...
            </span>
            <span v-else>Zapisz zmiany</span>
          </button>
        </div>
      </div>
    </div>
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
