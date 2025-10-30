import type { UserDto, GetUsersResult } from '~/stores/admin'

export interface GetUsersParams {
  searchTerm?: string
  department?: string
  position?: string
  role?: string
  isActive?: boolean
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDescending?: boolean
}

export function useUsersApi() {
  const config = useRuntimeConfig()
  const apiUrl = config.public.apiUrl || 'http://localhost:5155'

  const authStore = useAuthStore()

  const getAuthHeaders = (): Record<string, string> | undefined => {
    const token = authStore.accessToken

    if (token) {
      return { Authorization: `Bearer ${token}` }
    }
    return undefined
  }

  async function getUsers(params?: GetUsersParams): Promise<GetUsersResult> {
    const headers = getAuthHeaders()
    const queryParams = new URLSearchParams()

    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm)
    if (params?.department) queryParams.append('department', params.department)
    if (params?.position) queryParams.append('position', params.position)
    if (params?.role) queryParams.append('role', params.role)
    if (params?.isActive !== undefined) queryParams.append('isActive', params.isActive.toString())
    if (params?.pageNumber) queryParams.append('pageNumber', params.pageNumber.toString())
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
    if (params?.sortBy) queryParams.append('sortBy', params.sortBy)
    if (params?.sortDescending !== undefined) queryParams.append('sortDescending', params.sortDescending.toString())

    const url = `${apiUrl}/api/admin/users${queryParams.toString() ? '?' + queryParams.toString() : ''}`

    return await $fetch(url, { headers }) as GetUsersResult
  }

  async function getUserById(id: string): Promise<UserDto> {
    const headers = getAuthHeaders()
    return await $fetch(`${apiUrl}/api/admin/users/${id}`, { headers }) as UserDto
  }

  return {
    getUsers,
    getUserById
  }
}

