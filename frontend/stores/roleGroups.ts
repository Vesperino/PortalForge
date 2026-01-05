import { defineStore } from 'pinia'

export interface PermissionDto {
  id: string
  name: string
  description: string
  category: string
}

export interface RoleGroupDto {
  id: string
  name: string
  description: string
  isSystemRole: boolean
  createdAt: string
  updatedAt?: string
  permissions: PermissionDto[]
  userCount: number
}

export interface GetRoleGroupsResult {
  roleGroups: RoleGroupDto[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export const useRoleGroupsStore = defineStore('roleGroups', {
  state: () => ({
    roleGroups: [] as RoleGroupDto[],
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async fetchRoleGroups(includePermissions: boolean = true) {
      this.loading = true
      this.error = null

      try {
        const config = useRuntimeConfig()
        const authStore = useAuthStore()

        const response = await $fetch(
          `${config.public.apiUrl}/api/admin/rolegroups?includePermissions=${includePermissions}`,
          {
            headers: {
              Authorization: `Bearer ${authStore.accessToken}`,
            },
          }
        ) as GetRoleGroupsResult

        this.roleGroups = response.roleGroups
      } catch (err: unknown) {
        this.error = err instanceof Error ? err.message : 'Failed to fetch role groups'
        console.error('Error fetching role groups:', err)
      } finally {
        this.loading = false
      }
    },
  },
})

