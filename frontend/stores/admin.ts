import { defineStore } from 'pinia'

export interface UserDto {
  id: string
  email: string
  firstName: string
  lastName: string
  department: string
  departmentId?: string | null
  position: string
  positionId?: string | null
  role: string
  isActive: boolean
  isEmailVerified: boolean
  mustChangePassword: boolean
  phoneNumber?: string
  profilePhotoUrl?: string
  createdAt: string
  lastLoginAt?: string
  roleGroups: string[]
}

export interface GetUsersResult {
  users: UserDto[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateUserRequest {
  email: string
  password: string
  firstName: string
  lastName: string
  department: string
  position: string
  phoneNumber?: string
  role: string
  roleGroupIds: string[]
  mustChangePassword: boolean
}

export interface UpdateUserRequest {
  firstName: string
  lastName: string
  department: string
  departmentId?: string | null
  position: string
  positionId?: string | null
  phoneNumber?: string
  role: string
  roleGroupIds: string[]
  isActive: boolean
}

export const useAdminStore = defineStore('admin', {
  state: () => ({
    users: [] as UserDto[],
    currentUser: null as UserDto | null,
    totalCount: 0,
    pageNumber: 1,
    pageSize: 20,
    totalPages: 0,
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async fetchUsers(params?: {
      searchTerm?: string
      department?: string
      position?: string
      role?: string
      isActive?: boolean
      pageNumber?: number
      pageSize?: number
      sortBy?: string
      sortDescending?: boolean
    }) {
      this.loading = true
      this.error = null

      try {
        const config = useRuntimeConfig()
        const authStore = useAuthStore()
        
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

        const response = await $fetch(
          `${config.public.apiUrl}/api/admin/users?${queryParams.toString()}`,
          {
            headers: {
              Authorization: `Bearer ${authStore.accessToken}`,
            },
          }
        ) as GetUsersResult

        this.users = response.users
        this.totalCount = response.totalCount
        this.pageNumber = response.pageNumber
        this.pageSize = response.pageSize
        this.totalPages = response.totalPages
      } catch (err: any) {
        this.error = err.message || 'Failed to fetch users'
        console.error('Error fetching users:', err)
      } finally {
        this.loading = false
      }
    },

    async fetchUserById(userId: string) {
      this.loading = true
      this.error = null

      try {
        const config = useRuntimeConfig()
        const authStore = useAuthStore()

        const user = await $fetch(
          `${config.public.apiUrl}/api/admin/users/${userId}`,
          {
            headers: {
              Authorization: `Bearer ${authStore.accessToken}`,
            },
          }
        ) as UserDto

        this.currentUser = user
        return user
      } catch (err: any) {
        this.error = err.message || 'Failed to fetch user'
        console.error('Error fetching user:', err)
        throw err
      } finally {
        this.loading = false
      }
    },

    async createUser(request: CreateUserRequest) {
      this.loading = true
      this.error = null

      try {
        const config = useRuntimeConfig()
        const authStore = useAuthStore()

        const response = await $fetch(
          `${config.public.apiUrl}/api/admin/users`,
          {
            method: 'POST',
            headers: {
              Authorization: `Bearer ${authStore.accessToken}`,
              'Content-Type': 'application/json',
            },
            body: request,
          }
        )

        return response
      } catch (err: any) {
        this.error = err.message || 'Failed to create user'
        console.error('Error creating user:', err)
        throw err
      } finally {
        this.loading = false
      }
    },

    async updateUser(userId: string, request: UpdateUserRequest) {
      this.loading = true
      this.error = null

      try {
        const config = useRuntimeConfig()
        const authStore = useAuthStore()

        const response = await $fetch(
          `${config.public.apiUrl}/api/admin/users/${userId}`,
          {
            method: 'PUT',
            headers: {
              Authorization: `Bearer ${authStore.accessToken}`,
              'Content-Type': 'application/json',
            },
            body: request,
          }
        )

        return response
      } catch (err: any) {
        this.error = err.message || 'Failed to update user'
        console.error('Error updating user:', err)
        throw err
      } finally {
        this.loading = false
      }
    },

    async deleteUser(userId: string) {
      this.loading = true
      this.error = null

      try {
        const config = useRuntimeConfig()
        const authStore = useAuthStore()

        await $fetch(
          `${config.public.apiUrl}/api/admin/users/${userId}`,
          {
            method: 'DELETE',
            headers: {
              Authorization: `Bearer ${authStore.accessToken}`,
            },
          }
        )

        // Remove from local state
        this.users = this.users.filter(u => u.id !== userId)
      } catch (err: any) {
        this.error = err.message || 'Failed to delete user'
        console.error('Error deleting user:', err)
        throw err
      } finally {
        this.loading = false
      }
    },
  },
})

