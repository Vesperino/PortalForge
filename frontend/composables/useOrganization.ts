import type { DepartmentTreeDto } from '~/types/department'
import type { Employee } from '~/types'
import type { OrganizationChartNode } from 'primevue/organizationchart'

export interface OrganizationEmployee {
  id: string
  firstName: string
  lastName: string
  email: string
  position: string | null
  profilePhotoUrl: string | null
  departmentId: string
  department: string | null
  phoneNumber: string | null
  isActive: boolean
  supervisor?: OrganizationEmployee | null
  subordinates?: OrganizationEmployee[]
}

export interface OrganizationFilters {
  searchQuery: string
  departmentId: string | null
}

export function useOrganization() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl
  const { getAuthHeaders } = useAuth()
  const { handleError } = useApiError()

  const departments = ref<DepartmentTreeDto[]>([])
  const allEmployees = ref<OrganizationEmployee[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const loadDepartments = async (): Promise<void> => {
    try {
      const response = await $fetch<DepartmentTreeDto[]>(`${apiUrl}/api/departments/tree`, {
        headers: getAuthHeaders()
      })
      departments.value = response
    } catch (err: unknown) {
      handleError(err, { customMessage: 'Nie udalo sie pobrac struktury dzialow' })
      throw err
    }
  }

  const loadAllUsers = async (): Promise<void> => {
    try {
      const response = await $fetch<{ users: OrganizationEmployee[] }>(`${apiUrl}/api/admin/users`, {
        headers: getAuthHeaders()
      })
      allEmployees.value = response.users || []
    } catch (err: unknown) {
      handleError(err, { customMessage: 'Nie udalo sie pobrac listy pracownikow' })
      throw err
    }
  }

  const loadData = async (): Promise<void> => {
    isLoading.value = true
    error.value = null

    try {
      await Promise.all([
        loadDepartments(),
        loadAllUsers()
      ])
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'Nie udalo sie pobrac danych'
    } finally {
      isLoading.value = false
    }
  }

  const getAllDepartmentsFlat = (depts: DepartmentTreeDto[]): DepartmentTreeDto[] => {
    const result: DepartmentTreeDto[] = []
    depts.forEach(dept => {
      result.push(dept)
      if (dept.children && dept.children.length > 0) {
        result.push(...getAllDepartmentsFlat(dept.children))
      }
    })
    return result
  }

  const departmentsFlat = computed(() => getAllDepartmentsFlat(departments.value))

  const filterEmployees = (
    employees: OrganizationEmployee[],
    filters: OrganizationFilters
  ): OrganizationEmployee[] => {
    let filtered = employees

    if (filters.departmentId) {
      filtered = filtered.filter(e => e.departmentId === filters.departmentId)
    }

    if (filters.searchQuery) {
      const query = filters.searchQuery.toLowerCase()
      filtered = filtered.filter(e =>
        e.firstName?.toLowerCase().includes(query) ||
        e.lastName?.toLowerCase().includes(query) ||
        e.email?.toLowerCase().includes(query) ||
        e.position?.toLowerCase().includes(query) ||
        e.department?.toLowerCase().includes(query)
      )
    }

    return filtered
  }

  const getEmployeesByDepartment = (departmentId: string): OrganizationEmployee[] => {
    return allEmployees.value.filter(e => e.departmentId === departmentId)
  }

  const getManagerByDepartment = (dept: DepartmentTreeDto): OrganizationEmployee | null => {
    if (!dept.departmentHeadId) return null
    return allEmployees.value.find(e => e.id === dept.departmentHeadId) || null
  }

  const getDirectorByDepartment = (dept: DepartmentTreeDto): OrganizationEmployee | null => {
    if (!dept.departmentDirectorId) return null
    return allEmployees.value.find(e => e.id === dept.departmentDirectorId) || null
  }

  const getInitials = (employee: Employee | OrganizationEmployee | null | undefined): string => {
    if (!employee) return ''
    return `${employee.firstName?.[0] || ''}${employee.lastName?.[0] || ''}`
  }

  const departmentLookup = new Map<string, DepartmentTreeDto>()

  const convertDepartmentToOrgChart = (dept: DepartmentTreeDto): OrganizationChartNode => {
    const nodeKey = `dept-${dept.id}`
    departmentLookup.set(nodeKey, dept)

    const manager = getManagerByDepartment(dept)
    const director = getDirectorByDepartment(dept)
    const employees = dept.employees || []

    const node: OrganizationChartNode = {
      key: nodeKey,
      type: 'department',
      data: {
        id: dept.id,
        name: dept.name,
        description: dept.description,
        manager: manager ? `${manager.firstName} ${manager.lastName}` : 'Brak kierownika',
        director: director ? `${director.firstName} ${director.lastName}` : 'Brak dyrektora',
        employeeCount: employees.length,
        level: dept.level
      },
      children: []
    }

    employees.forEach(emp => {
      node.children?.push({
        key: `emp-${emp.id}`,
        type: 'employee',
        data: {
          id: emp.id,
          firstName: emp.firstName,
          lastName: emp.lastName,
          position: emp.position,
          email: emp.email,
          profilePhotoUrl: emp.profilePhotoUrl,
          isHead: dept.departmentHeadId === emp.id
        },
        children: []
      })
    })

    if (dept.children && dept.children.length > 0) {
      dept.children.forEach(child => {
        node.children?.push(convertDepartmentToOrgChart(child))
      })
    }

    return node
  }

  const buildOrgChartData = (): OrganizationChartNode[] => {
    if (departments.value.length === 0) return []
    departmentLookup.clear()
    return departments.value.map(dept => convertDepartmentToOrgChart(dept))
  }

  const getDepartmentFromLookup = (nodeKey: string): DepartmentTreeDto | undefined => {
    return departmentLookup.get(nodeKey)
  }

  const findEmployeeById = (id: string): OrganizationEmployee | undefined => {
    return allEmployees.value.find(e => e.id === id)
  }

  return {
    departments,
    allEmployees,
    isLoading,
    error,
    departmentsFlat,
    loadData,
    loadDepartments,
    loadAllUsers,
    filterEmployees,
    getEmployeesByDepartment,
    getManagerByDepartment,
    getDirectorByDepartment,
    getInitials,
    buildOrgChartData,
    getDepartmentFromLookup,
    findEmployeeById
  }
}
