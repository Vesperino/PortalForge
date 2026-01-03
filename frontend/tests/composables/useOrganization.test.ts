import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useOrganization, type OrganizationEmployee, type OrganizationFilters } from '~/composables/useOrganization'
import { useAuthStore } from '~/stores/auth'
import type { DepartmentTreeDto } from '~/types/department'

declare const clearNuxtState: () => void

describe('useOrganization', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    clearNuxtState()

    const authStore = useAuthStore()
    authStore.accessToken = 'mock-token'
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  describe('loadDepartments', () => {
    it('should load departments successfully', async () => {
      const mockDepartments: DepartmentTreeDto[] = [
        {
          id: 'dept-1',
          name: 'IT Department',
          description: 'Information Technology',
          parentDepartmentId: null,
          departmentHeadId: 'user-1',
          departmentHeadName: 'John Head',
          departmentDirectorId: 'user-2',
          departmentDirectorName: 'Jane Director',
          isActive: true,
          level: 0,
          employeeCount: 10,
          children: [],
          employees: []
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue(mockDepartments)

      const { departments, loadDepartments } = useOrganization()
      await loadDepartments()

      expect(departments.value).toEqual(mockDepartments)
      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/departments/tree',
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: 'Bearer mock-token'
          })
        })
      )
    })

    it('should throw error on fetch failure', async () => {
      const error = new Error('Network error')
      global.$fetch = vi.fn().mockRejectedValue(error)

      const { loadDepartments } = useOrganization()

      await expect(loadDepartments()).rejects.toThrow('Network error')
    })
  })

  describe('loadAllUsers', () => {
    it('should load all users successfully', async () => {
      const mockUsers: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: '+1234567890',
          isActive: true
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue({ users: mockUsers })

      const { allEmployees, loadAllUsers } = useOrganization()
      await loadAllUsers()

      expect(allEmployees.value).toEqual(mockUsers)
      expect(global.$fetch).toHaveBeenCalledWith(
        'http://localhost:5155/api/admin/users',
        expect.any(Object)
      )
    })

    it('should handle empty users response', async () => {
      global.$fetch = vi.fn().mockResolvedValue({ users: null })

      const { allEmployees, loadAllUsers } = useOrganization()
      await loadAllUsers()

      expect(allEmployees.value).toEqual([])
    })
  })

  describe('loadData', () => {
    it('should load both departments and users successfully', async () => {
      const mockDepartments: DepartmentTreeDto[] = [
        {
          id: 'dept-1',
          name: 'IT',
          description: null,
          parentDepartmentId: null,
          departmentHeadId: null,
          departmentHeadName: null,
          departmentDirectorId: null,
          departmentDirectorName: null,
          isActive: true,
          level: 0,
          employeeCount: 5,
          children: [],
          employees: []
        }
      ]

      const mockUsers: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        }
      ]

      global.$fetch = vi.fn()
        .mockResolvedValueOnce(mockDepartments)
        .mockResolvedValueOnce({ users: mockUsers })

      const { departments, allEmployees, isLoading, error, loadData } = useOrganization()

      expect(isLoading.value).toBe(false)

      const loadPromise = loadData()
      expect(isLoading.value).toBe(true)

      await loadPromise

      expect(isLoading.value).toBe(false)
      expect(error.value).toBeNull()
      expect(departments.value).toEqual(mockDepartments)
      expect(allEmployees.value).toEqual(mockUsers)
    })

    it('should set error on failure', async () => {
      global.$fetch = vi.fn().mockRejectedValue(new Error('API Error'))

      const { isLoading, error, loadData } = useOrganization()

      await loadData()

      expect(isLoading.value).toBe(false)
      expect(error.value).toBe('API Error')
    })

    it('should set default error message for non-Error objects', async () => {
      global.$fetch = vi.fn().mockRejectedValue({ message: 'Some error' })

      const { error, loadData } = useOrganization()

      await loadData()

      expect(error.value).toBe('Nie udalo sie pobrac danych')
    })
  })

  describe('departmentsFlat', () => {
    it('should flatten nested departments', async () => {
      const mockDepartments: DepartmentTreeDto[] = [
        {
          id: 'dept-1',
          name: 'IT',
          description: null,
          parentDepartmentId: null,
          departmentHeadId: null,
          departmentHeadName: null,
          departmentDirectorId: null,
          departmentDirectorName: null,
          isActive: true,
          level: 0,
          employeeCount: 5,
          children: [
            {
              id: 'dept-2',
              name: 'Development',
              description: null,
              parentDepartmentId: 'dept-1',
              departmentHeadId: null,
              departmentHeadName: null,
              departmentDirectorId: null,
              departmentDirectorName: null,
              isActive: true,
              level: 1,
              employeeCount: 3,
              children: [],
              employees: []
            }
          ],
          employees: []
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue(mockDepartments)

      const { departmentsFlat, loadDepartments } = useOrganization()
      await loadDepartments()

      expect(departmentsFlat.value).toHaveLength(2)
      expect(departmentsFlat.value[0].id).toBe('dept-1')
      expect(departmentsFlat.value[1].id).toBe('dept-2')
    })
  })

  describe('filterEmployees', () => {
    it('should filter employees by department', () => {
      const employees: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        },
        {
          id: 'user-2',
          firstName: 'Jane',
          lastName: 'Smith',
          email: 'jane@example.com',
          position: 'Manager',
          profilePhotoUrl: null,
          departmentId: 'dept-2',
          department: 'HR',
          phoneNumber: null,
          isActive: true
        }
      ]

      const filters: OrganizationFilters = {
        searchQuery: '',
        departmentId: 'dept-1'
      }

      const { filterEmployees } = useOrganization()
      const result = filterEmployees(employees, filters)

      expect(result).toHaveLength(1)
      expect(result[0].id).toBe('user-1')
    })

    it('should filter employees by search query', () => {
      const employees: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        },
        {
          id: 'user-2',
          firstName: 'Jane',
          lastName: 'Smith',
          email: 'jane@example.com',
          position: 'Manager',
          profilePhotoUrl: null,
          departmentId: 'dept-2',
          department: 'HR',
          phoneNumber: null,
          isActive: true
        }
      ]

      const filters: OrganizationFilters = {
        searchQuery: 'john',
        departmentId: null
      }

      const { filterEmployees } = useOrganization()
      const result = filterEmployees(employees, filters)

      expect(result).toHaveLength(1)
      expect(result[0].firstName).toBe('John')
    })

    it('should filter employees by email', () => {
      const employees: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        }
      ]

      const filters: OrganizationFilters = {
        searchQuery: 'example.com',
        departmentId: null
      }

      const { filterEmployees } = useOrganization()
      const result = filterEmployees(employees, filters)

      expect(result).toHaveLength(1)
    })

    it('should filter employees by position', () => {
      const employees: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Senior Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        },
        {
          id: 'user-2',
          firstName: 'Jane',
          lastName: 'Smith',
          email: 'jane@example.com',
          position: 'Manager',
          profilePhotoUrl: null,
          departmentId: 'dept-2',
          department: 'HR',
          phoneNumber: null,
          isActive: true
        }
      ]

      const filters: OrganizationFilters = {
        searchQuery: 'developer',
        departmentId: null
      }

      const { filterEmployees } = useOrganization()
      const result = filterEmployees(employees, filters)

      expect(result).toHaveLength(1)
      expect(result[0].position).toBe('Senior Developer')
    })

    it('should combine department and search filters', () => {
      const employees: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        },
        {
          id: 'user-2',
          firstName: 'Jane',
          lastName: 'Doe',
          email: 'jane@example.com',
          position: 'Manager',
          profilePhotoUrl: null,
          departmentId: 'dept-2',
          department: 'HR',
          phoneNumber: null,
          isActive: true
        }
      ]

      const filters: OrganizationFilters = {
        searchQuery: 'doe',
        departmentId: 'dept-1'
      }

      const { filterEmployees } = useOrganization()
      const result = filterEmployees(employees, filters)

      expect(result).toHaveLength(1)
      expect(result[0].id).toBe('user-1')
    })

    it('should return all employees with no filters', () => {
      const employees: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        },
        {
          id: 'user-2',
          firstName: 'Jane',
          lastName: 'Smith',
          email: 'jane@example.com',
          position: 'Manager',
          profilePhotoUrl: null,
          departmentId: 'dept-2',
          department: 'HR',
          phoneNumber: null,
          isActive: true
        }
      ]

      const filters: OrganizationFilters = {
        searchQuery: '',
        departmentId: null
      }

      const { filterEmployees } = useOrganization()
      const result = filterEmployees(employees, filters)

      expect(result).toHaveLength(2)
    })
  })

  describe('getEmployeesByDepartment', () => {
    it('should return employees for a specific department', async () => {
      const mockUsers: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        },
        {
          id: 'user-2',
          firstName: 'Jane',
          lastName: 'Smith',
          email: 'jane@example.com',
          position: 'Manager',
          profilePhotoUrl: null,
          departmentId: 'dept-2',
          department: 'HR',
          phoneNumber: null,
          isActive: true
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue({ users: mockUsers })

      const { loadAllUsers, getEmployeesByDepartment } = useOrganization()
      await loadAllUsers()

      const result = getEmployeesByDepartment('dept-1')

      expect(result).toHaveLength(1)
      expect(result[0].id).toBe('user-1')
    })
  })

  describe('getManagerByDepartment', () => {
    it('should return manager for department', async () => {
      const mockUsers: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Manager',
          email: 'john@example.com',
          position: 'Department Head',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue({ users: mockUsers })

      const { loadAllUsers, getManagerByDepartment } = useOrganization()
      await loadAllUsers()

      const dept: DepartmentTreeDto = {
        id: 'dept-1',
        name: 'IT',
        description: null,
        parentDepartmentId: null,
        departmentHeadId: 'user-1',
        departmentHeadName: 'John Manager',
        departmentDirectorId: null,
        departmentDirectorName: null,
        isActive: true,
        level: 0,
        employeeCount: 1,
        children: [],
        employees: []
      }

      const result = getManagerByDepartment(dept)

      expect(result).not.toBeNull()
      expect(result?.firstName).toBe('John')
    })

    it('should return null if department has no head', async () => {
      global.$fetch = vi.fn().mockResolvedValue({ users: [] })

      const { loadAllUsers, getManagerByDepartment } = useOrganization()
      await loadAllUsers()

      const dept: DepartmentTreeDto = {
        id: 'dept-1',
        name: 'IT',
        description: null,
        parentDepartmentId: null,
        departmentHeadId: null,
        departmentHeadName: null,
        departmentDirectorId: null,
        departmentDirectorName: null,
        isActive: true,
        level: 0,
        employeeCount: 0,
        children: [],
        employees: []
      }

      const result = getManagerByDepartment(dept)

      expect(result).toBeNull()
    })
  })

  describe('getDirectorByDepartment', () => {
    it('should return director for department', async () => {
      const mockUsers: OrganizationEmployee[] = [
        {
          id: 'user-2',
          firstName: 'Jane',
          lastName: 'Director',
          email: 'jane@example.com',
          position: 'Director',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue({ users: mockUsers })

      const { loadAllUsers, getDirectorByDepartment } = useOrganization()
      await loadAllUsers()

      const dept: DepartmentTreeDto = {
        id: 'dept-1',
        name: 'IT',
        description: null,
        parentDepartmentId: null,
        departmentHeadId: null,
        departmentHeadName: null,
        departmentDirectorId: 'user-2',
        departmentDirectorName: 'Jane Director',
        isActive: true,
        level: 0,
        employeeCount: 1,
        children: [],
        employees: []
      }

      const result = getDirectorByDepartment(dept)

      expect(result).not.toBeNull()
      expect(result?.firstName).toBe('Jane')
    })
  })

  describe('getInitials', () => {
    it('should return initials for employee', () => {
      const employee: OrganizationEmployee = {
        id: 'user-1',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        position: 'Developer',
        profilePhotoUrl: null,
        departmentId: 'dept-1',
        department: 'IT',
        phoneNumber: null,
        isActive: true
      }

      const { getInitials } = useOrganization()
      const result = getInitials(employee)

      expect(result).toBe('JD')
    })

    it('should return empty string for null employee', () => {
      const { getInitials } = useOrganization()
      const result = getInitials(null)

      expect(result).toBe('')
    })

    it('should return empty string for undefined employee', () => {
      const { getInitials } = useOrganization()
      const result = getInitials(undefined)

      expect(result).toBe('')
    })

    it('should handle employee with missing name parts', () => {
      const employee: OrganizationEmployee = {
        id: 'user-1',
        firstName: '',
        lastName: 'Doe',
        email: 'john@example.com',
        position: 'Developer',
        profilePhotoUrl: null,
        departmentId: 'dept-1',
        department: 'IT',
        phoneNumber: null,
        isActive: true
      }

      const { getInitials } = useOrganization()
      const result = getInitials(employee)

      expect(result).toBe('D')
    })
  })

  describe('findEmployeeById', () => {
    it('should find employee by ID', async () => {
      const mockUsers: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          position: 'Developer',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        },
        {
          id: 'user-2',
          firstName: 'Jane',
          lastName: 'Smith',
          email: 'jane@example.com',
          position: 'Manager',
          profilePhotoUrl: null,
          departmentId: 'dept-2',
          department: 'HR',
          phoneNumber: null,
          isActive: true
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue({ users: mockUsers })

      const { loadAllUsers, findEmployeeById } = useOrganization()
      await loadAllUsers()

      const result = findEmployeeById('user-2')

      expect(result).not.toBeUndefined()
      expect(result?.firstName).toBe('Jane')
    })

    it('should return undefined for non-existent ID', async () => {
      global.$fetch = vi.fn().mockResolvedValue({ users: [] })

      const { loadAllUsers, findEmployeeById } = useOrganization()
      await loadAllUsers()

      const result = findEmployeeById('non-existent')

      expect(result).toBeUndefined()
    })
  })

  describe('buildOrgChartData', () => {
    it('should build organization chart data from departments', async () => {
      const mockDepartments: DepartmentTreeDto[] = [
        {
          id: 'dept-1',
          name: 'IT',
          description: 'IT Department',
          parentDepartmentId: null,
          departmentHeadId: 'user-1',
          departmentHeadName: 'John Head',
          departmentDirectorId: null,
          departmentDirectorName: null,
          isActive: true,
          level: 0,
          employeeCount: 2,
          children: [],
          employees: [
            {
              id: 'user-1',
              firstName: 'John',
              lastName: 'Head',
              email: 'john@example.com',
              position: 'Head',
              profilePhotoUrl: null,
              departmentId: 'dept-1',
              isActive: true
            }
          ]
        }
      ]

      const mockUsers: OrganizationEmployee[] = [
        {
          id: 'user-1',
          firstName: 'John',
          lastName: 'Head',
          email: 'john@example.com',
          position: 'Head',
          profilePhotoUrl: null,
          departmentId: 'dept-1',
          department: 'IT',
          phoneNumber: null,
          isActive: true
        }
      ]

      global.$fetch = vi.fn()
        .mockResolvedValueOnce(mockDepartments)
        .mockResolvedValueOnce({ users: mockUsers })

      const { loadData, buildOrgChartData } = useOrganization()
      await loadData()

      const result = buildOrgChartData()

      expect(result).toHaveLength(1)
      expect(result[0].key).toBe('dept-dept-1')
      expect(result[0].type).toBe('department')
      expect(result[0].data.name).toBe('IT')
    })

    it('should return empty array when no departments loaded', () => {
      const { buildOrgChartData } = useOrganization()
      const result = buildOrgChartData()

      expect(result).toEqual([])
    })
  })

  describe('getDepartmentFromLookup', () => {
    it('should get department from lookup after building org chart', async () => {
      const mockDepartments: DepartmentTreeDto[] = [
        {
          id: 'dept-1',
          name: 'IT',
          description: null,
          parentDepartmentId: null,
          departmentHeadId: null,
          departmentHeadName: null,
          departmentDirectorId: null,
          departmentDirectorName: null,
          isActive: true,
          level: 0,
          employeeCount: 0,
          children: [],
          employees: []
        }
      ]

      global.$fetch = vi.fn().mockResolvedValue(mockDepartments)

      const { loadDepartments, buildOrgChartData, getDepartmentFromLookup } = useOrganization()
      await loadDepartments()
      buildOrgChartData()

      const result = getDepartmentFromLookup('dept-dept-1')

      expect(result).not.toBeUndefined()
      expect(result?.name).toBe('IT')
    })
  })
})
