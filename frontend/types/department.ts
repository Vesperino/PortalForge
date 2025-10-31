// Department Types

export interface DepartmentDto {
  id: string
  name: string
  description: string | null
  parentDepartmentId: string | null
  departmentHeadId: string | null
  departmentHeadName: string | null
  isActive: boolean
  level: number
  employeeCount: number
  createdAt: string
}

export interface DepartmentTreeDto {
  id: string
  name: string
  description: string | null
  parentDepartmentId: string | null
  departmentHeadId: string | null
  departmentHeadName: string | null
  isActive: boolean
  level: number
  employeeCount: number
  children: DepartmentTreeDto[]
}

export interface CreateDepartmentDto {
  name: string
  description: string | null
  parentDepartmentId: string | null
  departmentHeadId: string | null
}

export interface UpdateDepartmentDto {
  name: string
  description: string | null
  parentDepartmentId: string | null
  departmentHeadId: string | null
  isActive: boolean
}

export interface EmployeeDto {
  id: string
  firstName: string
  lastName: string
  email: string
  position: string | null
  departmentId: string
  isActive: boolean
}
