import type { RoleGroupDto } from '~/stores/roleGroups'

export interface CreateRoleGroupRequest {
  name: string
  description: string
  permissionIds: string[]
}

export interface UpdateRoleGroupRequest {
  name: string
  description: string
  permissionIds: string[]
}

export interface CreateRoleGroupResult {
  roleGroupId: string
  message: string
}

export interface UpdateRoleGroupResult {
  message: string
}

export interface DeleteRoleGroupResult {
  message: string
}

export interface GetRoleGroupByIdResult {
  roleGroup: RoleGroupDto
}

export function useRoleGroupApi() {
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

  async function fetchRoleGroupById(id: string): Promise<RoleGroupDto> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/admin/rolegroups/${id}`, {
      headers
    }) as GetRoleGroupByIdResult
    return response.roleGroup
  }

  async function createRoleGroup(request: CreateRoleGroupRequest): Promise<CreateRoleGroupResult> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/admin/rolegroups`, {
      method: 'POST',
      headers,
      body: request
    }) as CreateRoleGroupResult
    return response
  }

  async function updateRoleGroup(id: string, request: UpdateRoleGroupRequest): Promise<UpdateRoleGroupResult> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/admin/rolegroups/${id}`, {
      method: 'PUT',
      headers,
      body: request
    }) as UpdateRoleGroupResult
    return response
  }

  async function deleteRoleGroup(id: string): Promise<DeleteRoleGroupResult> {
    const headers = getAuthHeaders()
    const response = await $fetch(`${apiUrl}/api/admin/rolegroups/${id}`, {
      method: 'DELETE',
      headers
    }) as DeleteRoleGroupResult
    return response
  }

  return {
    fetchRoleGroupById,
    createRoleGroup,
    updateRoleGroup,
    deleteRoleGroup
  }
}

