import { defineStore } from 'pinia'
import type { PermissionDto } from './roleGroups'

export interface GetPermissionsResult {
  permissions: PermissionDto[]
  permissionsByCategory: Record<string, PermissionDto[]>
}

export const usePermissionsStore = defineStore('permissions', {
  state: () => ({
    permissions: [] as PermissionDto[],
    permissionsByCategory: {} as Record<string, PermissionDto[]>,
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async fetchPermissions(category?: string) {
      this.loading = true
      this.error = null

      try {
        const config = useRuntimeConfig()
        const authStore = useAuthStore()

        const url = category
          ? `${config.public.apiUrl}/api/admin/permissions?category=${category}`
          : `${config.public.apiUrl}/api/admin/permissions`

        const response = await $fetch(url, {
          headers: {
            Authorization: `Bearer ${authStore.accessToken}`,
          },
        }) as GetPermissionsResult

        this.permissions = response.permissions
        this.permissionsByCategory = response.permissionsByCategory
      } catch (err: any) {
        this.error = err.message || 'Failed to fetch permissions'
        console.error('Error fetching permissions:', err)
      } finally {
        this.loading = false
      }
    },
  },
})

