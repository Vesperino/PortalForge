// Organizational Permission Types

export interface OrganizationalPermissionDto {
  userId: string
  canViewAllDepartments: boolean
  visibleDepartmentIds: string[]
}

export interface UpdateOrganizationalPermissionRequest {
  canViewAllDepartments: boolean
  visibleDepartmentIds: string[]
}
