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

interface OrgVisibility {
  canViewAllDepartments: boolean
  visibleDepartmentIds: string[]
}

export function useOrganization() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl
  const { getAuthHeaders } = useAuth()
  const authStore = useAuthStore()
  const { handleError, isForbiddenError } = useApiError()

  const departments = useState<DepartmentTreeDto[]>('org-departments', () => [])
  const allEmployees = useState<OrganizationEmployee[]>('org-employees', () => [])
  const isLoading = useState<boolean>('org-loading', () => false)
  const error = useState<string | null>('org-error', () => null)

  const normalizeUsersResponse = (response: unknown): OrganizationEmployee[] => {
    if (Array.isArray(response)) return response as OrganizationEmployee[]

    if (response && typeof response === 'object') {
      const users = (response as { users?: OrganizationEmployee[]; items?: OrganizationEmployee[] }).users
      const items = (response as { users?: OrganizationEmployee[]; items?: OrganizationEmployee[] }).items
      if (Array.isArray(users)) return users
      if (Array.isArray(items)) return items
    }

    return []
  }

  const hasRole = (role?: string): boolean => {
    if (!role) return false
    const normalized = role.toLowerCase()
    return normalized === 'admin' || normalized === 'hr'
  }

  const canViewAllDepartments = (): boolean => {
    const userRole = authStore.user?.role
    const rolesArray = (authStore.user as { roles?: string[] } | null)?.roles
    return hasRole(userRole) || (rolesArray?.some(hasRole) ?? false)
  }

  const loadOrgVisibility = async (): Promise<OrgVisibility> => {
    if (canViewAllDepartments()) {
      return { canViewAllDepartments: true, visibleDepartmentIds: [] }
    }

    const userId = authStore.user?.id
    if (!userId) {
      return { canViewAllDepartments: false, visibleDepartmentIds: [] }
    }

    try {
      const response = await $fetch(
        `${apiUrl}/api/admin/permissions/organizational/${userId}`,
        { headers: getAuthHeaders() }
      ) as { canViewAllDepartments?: boolean; visibleDepartmentIds?: string[] }

      return {
        canViewAllDepartments: !!response.canViewAllDepartments,
        visibleDepartmentIds: Array.isArray(response.visibleDepartmentIds) ? response.visibleDepartmentIds : []
      }
    } catch (_err) {
      return { canViewAllDepartments: false, visibleDepartmentIds: [] }
    }
  }

  const getAllowedDepartmentIds = (visibility: OrgVisibility): Set<string> | null => {
    if (visibility.canViewAllDepartments) return null

    const ownDepartmentId = authStore.user?.departmentId
    const hasExplicit = visibility.visibleDepartmentIds.length > 0

    if (!ownDepartmentId && !hasExplicit) {
      return null
    }

    const allowed = new Set<string>(visibility.visibleDepartmentIds || [])
    if (ownDepartmentId) {
      allowed.add(ownDepartmentId)
    }

    return allowed
  }

  const filterDepartmentTree = (
    nodes: DepartmentTreeDto[],
    allowedIds: Set<string> | null
  ): DepartmentTreeDto[] => {
    if (!allowedIds) return nodes

    const result: DepartmentTreeDto[] = []

    for (const node of nodes) {
      const filteredChildren = filterDepartmentTree(node.children || [], allowedIds)
      const isVisible = allowedIds.has(node.id)

      if (!isVisible && filteredChildren.length === 0) {
        continue
      }

      const employees = isVisible ? (node.employees || []) : []

      result.push({
        ...node,
        children: filteredChildren,
        employees,
        employeeCount: employees.length
      })
    }

    return result
  }

  const buildEmployeesFromTree = (nodes: DepartmentTreeDto[]): OrganizationEmployee[] => {
    const result: OrganizationEmployee[] = []

    const traverse = (dept: DepartmentTreeDto) => {
      const employees = dept.employees || []
      for (const employee of employees) {
        result.push({
          id: employee.id,
          firstName: employee.firstName,
          lastName: employee.lastName,
          email: employee.email,
          position: employee.position ?? null,
          profilePhotoUrl: employee.profilePhotoUrl ?? null,
          departmentId: employee.departmentId || dept.id,
          department: dept.name,
          phoneNumber: null,
          isActive: employee.isActive
        })
      }

      if (dept.children && dept.children.length > 0) {
        dept.children.forEach(traverse)
      }
    }

    nodes.forEach(traverse)
    return result
  }

  const filterEmployeesByDepartments = (
    employees: OrganizationEmployee[],
    allowedIds: Set<string> | null
  ): OrganizationEmployee[] => {
    if (!allowedIds) return employees
    return employees.filter(emp => allowedIds.has(emp.departmentId))
  }

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

  const loadAllUsers = async (options: {
    fallbackDepartments?: DepartmentTreeDto[]
    allowedDepartmentIds?: Set<string> | null
  } = {}): Promise<void> => {
    try {
      const response = await $fetch(`${apiUrl}/api/admin/users`, {
        headers: getAuthHeaders()
      })
      const users = normalizeUsersResponse(response)
      allEmployees.value = filterEmployeesByDepartments(users, options.allowedDepartmentIds || null)
    } catch (err: unknown) {
      if (isForbiddenError(err)) {
        const fallback = buildEmployeesFromTree(options.fallbackDepartments || departments.value)
        allEmployees.value = filterEmployeesByDepartments(fallback, options.allowedDepartmentIds || null)
        return
      }
      handleError(err, { customMessage: 'Nie udalo sie pobrac listy pracownikow' })
      throw err
    }
  }

  const loadData = async (): Promise<void> => {
    isLoading.value = true
    error.value = null

    try {
      const visibility = await loadOrgVisibility()
      const allowedDepartmentIds = getAllowedDepartmentIds(visibility)

      await loadDepartments()

      if (allowedDepartmentIds) {
        departments.value = filterDepartmentTree(departments.value, allowedDepartmentIds)
      }

      await loadAllUsers({
        fallbackDepartments: departments.value,
        allowedDepartmentIds
      })
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
