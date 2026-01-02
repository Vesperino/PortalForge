import { ref, computed } from 'vue'
import type { DepartmentTreeDto, CreateDepartmentDto, UpdateDepartmentDto } from '~/types/department'
import type { User } from '~/types/auth'

interface DepartmentFormData {
  name: string
  description: string | null
  parentDepartmentId: string | null
  departmentHeadId: string | null
  departmentDirectorId: string | null
  isActive?: boolean
}

interface QuickEditFormData {
  departmentId: string
  departmentName: string
  position: string
  positionId: string | null
}

interface FlatDepartment extends DepartmentTreeDto {
  level: number
}

export function useDepartmentStructure() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl
  const { getAuthHeaders } = useAuth()

  const departmentTree = ref<DepartmentTreeDto[]>([])
  const allUsers = ref<User[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

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

  const unassignedUsers = computed(() =>
    allUsers.value.filter(u => !u.departmentId)
  )

  const departmentsFlat = computed<FlatDepartment[]>(() => {
    const flattenDepartments = (
      depts: DepartmentTreeDto[],
      level = 0
    ): FlatDepartment[] => {
      return depts.flatMap(dept => [
        { ...dept, level },
        ...flattenDepartments(dept.children || [], level + 1)
      ])
    }
    return flattenDepartments(departmentTree.value)
  })

  function countDepartments(departments: DepartmentTreeDto[]): number {
    let count = departments.length
    for (const dept of departments) {
      if (dept.children.length > 0) {
        count += countDepartments(dept.children)
      }
    }
    return count
  }

  function findDepartmentById(
    departments: DepartmentTreeDto[],
    id: string
  ): DepartmentTreeDto | null {
    for (const dept of departments) {
      if (dept.id === id) return dept
      if (dept.children.length > 0) {
        const found = findDepartmentById(dept.children, id)
        if (found) return found
      }
    }
    return null
  }

  function findDepartmentByName(
    departments: DepartmentTreeDto[],
    name: string
  ): DepartmentTreeDto | null {
    for (const dept of departments) {
      if (dept.name === name) return dept
      if (dept.children.length > 0) {
        const found = findDepartmentByName(dept.children, name)
        if (found) return found
      }
    }
    return null
  }

  async function loadDepartmentTree(): Promise<void> {
    try {
      const response = await $fetch<DepartmentTreeDto[]>(
        `${apiUrl}/api/departments/tree`,
        { headers: getAuthHeaders() }
      )
      departmentTree.value = response
    } catch (err: unknown) {
      error.value =
        err instanceof Error
          ? err.message
          : 'Nie udalo sie pobrac struktury organizacyjnej'
      console.error('Error loading department tree:', err)
    }
  }

  async function loadAllUsers(): Promise<void> {
    try {
      const response = (await $fetch(`${apiUrl}/api/admin/users`, {
        headers: getAuthHeaders()
      })) as { users?: User[] } | User[]

      if (response && 'users' in response && Array.isArray(response.users)) {
        allUsers.value = response.users
      } else if (Array.isArray(response)) {
        allUsers.value = response
      } else {
        console.error('Unexpected response format:', response)
        allUsers.value = []
      }
    } catch (err: unknown) {
      error.value =
        err instanceof Error
          ? err.message
          : 'Nie udalo sie pobrac listy pracownikow'
      console.error('Error loading users:', err)
      allUsers.value = []
    }
  }

  async function loadData(): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      await Promise.all([loadDepartmentTree(), loadAllUsers()])
    } finally {
      isLoading.value = false
    }
  }

  async function getDepartmentById(
    departmentId: string
  ): Promise<DepartmentTreeDto | null> {
    let department = findDepartmentById(departmentTree.value, departmentId)

    if (!department) {
      try {
        department = await $fetch<DepartmentTreeDto>(
          `${apiUrl}/api/departments/${departmentId}`,
          { headers: getAuthHeaders() }
        )
      } catch (err: unknown) {
        error.value = 'Nie udalo sie pobrac danych dzialu'
        console.error('Error loading department:', err)
        return null
      }
    }

    return department
  }

  async function createDepartment(
    data: CreateDepartmentDto
  ): Promise<boolean> {
    isLoading.value = true
    error.value = null

    try {
      await $fetch(`${apiUrl}/api/departments`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: data
      })
      await loadData()
      return true
    } catch (err: unknown) {
      const fetchError = err as { data?: { errors?: Record<string, string[]>; message?: string }; message?: string }
      if (fetchError.data?.errors) {
        const errorMessages = Object.values(fetchError.data.errors)
          .flat()
          .join(', ')
        error.value = errorMessages
      } else {
        error.value =
          fetchError.data?.message ||
          fetchError.message ||
          'Nie udalo sie utworzyc dzialu'
      }
      console.error('Error creating department:', err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function updateDepartment(
    departmentId: string,
    data: UpdateDepartmentDto
  ): Promise<boolean> {
    isLoading.value = true
    error.value = null

    try {
      await $fetch(`${apiUrl}/api/departments/${departmentId}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: data
      })
      await loadData()
      return true
    } catch (err: unknown) {
      const fetchError = err as { data?: { errors?: Record<string, string[]>; message?: string }; message?: string }
      if (fetchError.data?.errors) {
        const errorMessages = Object.values(fetchError.data.errors)
          .flat()
          .join(', ')
        error.value = errorMessages
      } else {
        error.value =
          fetchError.data?.message ||
          fetchError.message ||
          'Nie udalo sie zaktualizowac dzialu'
      }
      console.error('Error updating department:', err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function deleteDepartment(departmentId: string): Promise<boolean> {
    isLoading.value = true
    error.value = null

    try {
      await $fetch(`${apiUrl}/api/departments/${departmentId}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
      })
      await loadData()
      return true
    } catch (err: unknown) {
      const fetchError = err as { message?: string }
      error.value = fetchError.message || 'Nie udalo sie usunac dzialu'
      console.error('Error deleting department:', err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function assignUserToDepartment(
    userId: string,
    departmentId: string
  ): Promise<boolean> {
    isLoading.value = true
    error.value = null

    try {
      const user = allUsers.value.find(u => u.id === userId)
      if (!user) {
        error.value = 'Nie znaleziono uzytkownika'
        return false
      }

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
      return true
    } catch (err: unknown) {
      const fetchError = err as { message?: string }
      error.value =
        fetchError.message || 'Nie udalo sie przypisac pracownika do dzialu'
      console.error('Error assigning user to department:', err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function updateUserQuickEdit(
    user: User,
    formData: QuickEditFormData
  ): Promise<boolean> {
    if (!formData.departmentId || !formData.position) {
      error.value = 'Dzial i stanowisko sa wymagane'
      return false
    }

    isLoading.value = true
    error.value = null

    try {
      await $fetch(`${apiUrl}/api/admin/users/${user.id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: {
          firstName: user.firstName,
          lastName: user.lastName,
          department: formData.departmentName,
          departmentId: formData.departmentId,
          position: formData.position,
          positionId: formData.positionId || null,
          phoneNumber: user.phoneNumber,
          role: user.role,
          roleGroupIds: [],
          isActive: user.isActive ?? true,
          updatedBy: user.id
        }
      })

      await loadData()
      return true
    } catch (err: unknown) {
      const fetchError = err as { data?: { message?: string }; message?: string }
      error.value =
        fetchError.data?.message ||
        fetchError.message ||
        'Nie udalo sie zaktualizowac pracownika'
      console.error('Error updating employee:', err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  function createEmptyFormData(): DepartmentFormData {
    return {
      name: '',
      description: null,
      parentDepartmentId: null,
      departmentHeadId: null,
      departmentDirectorId: null
    }
  }

  function createFormDataFromDepartment(
    department: DepartmentTreeDto
  ): DepartmentFormData {
    return {
      name: department.name,
      description: department.description,
      parentDepartmentId: department.parentDepartmentId,
      departmentHeadId: department.departmentHeadId,
      departmentDirectorId: department.departmentDirectorId ?? null,
      isActive: department.isActive
    }
  }

  function validateDepartmentForm(
    formData: DepartmentFormData
  ): Record<string, string> {
    const errors: Record<string, string> = {}

    if (!formData.name || formData.name.trim().length === 0) {
      errors.name = 'Nazwa jest wymagana'
    } else if (formData.name.length > 100) {
      errors.name = 'Nazwa nie moze przekraczac 100 znakow'
    }

    if (formData.description && formData.description.length > 500) {
      errors.description = 'Opis nie moze przekraczac 500 znakow'
    }

    return errors
  }

  function clearError(): void {
    error.value = null
  }

  return {
    departmentTree,
    allUsers,
    isLoading,
    error,
    stats,
    unassignedUsers,
    departmentsFlat,
    loadData,
    loadDepartmentTree,
    loadAllUsers,
    getDepartmentById,
    findDepartmentById,
    findDepartmentByName,
    createDepartment,
    updateDepartment,
    deleteDepartment,
    assignUserToDepartment,
    updateUserQuickEdit,
    createEmptyFormData,
    createFormDataFromDepartment,
    validateDepartmentForm,
    clearError
  }
}

export type { DepartmentFormData, QuickEditFormData, FlatDepartment }
