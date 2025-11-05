export interface DepartmentDto {
  id: string
  name: string
  description: string | null
  parentDepartmentId: string | null
  departmentHeadId: string | null
  departmentDirectorId: string | null
  isActive: boolean
  employeeCount: number
}

export const useDepartmentsApi = () => {
  const config = useRuntimeConfig()
  const { getAuthHeaders } = useAuth()

  const getDepartments = async (): Promise<DepartmentDto[]> => {
    try {
      const headers = await getAuthHeaders()
      const response = await $fetch<DepartmentDto[]>(`${config.public.apiBase}/departments`, {
        headers,
        credentials: 'include'
      })
      return response
    } catch (error: any) {
      console.error('Error fetching departments:', error)
      throw new Error(error?.data?.message || 'Failed to fetch departments')
    }
  }

  return {
    getDepartments
  }
}

