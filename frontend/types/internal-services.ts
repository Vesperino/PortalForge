export interface InternalService {
  id: string
  name: string
  description: string
  url: string
  icon?: string
  iconType: 'emoji' | 'image' | 'font'
  categoryId?: string
  categoryName?: string
  displayOrder: number
  isActive: boolean
  isGlobal: boolean
  isPinned: boolean
  createdAt: string
  updatedAt?: string
  departmentIds: string[]
}

export interface InternalServiceCategory {
  id: string
  name: string
  description: string
  icon?: string
  displayOrder: number
  services: InternalService[]
}

export interface CreateInternalServiceRequest {
  name: string
  description: string
  url: string
  icon?: string
  iconType: 'emoji' | 'image' | 'font'
  categoryId?: string
  displayOrder: number
  isActive: boolean
  isGlobal: boolean
  isPinned: boolean
  departmentIds: string[]
}

export interface UpdateInternalServiceRequest {
  id: string
  name: string
  description: string
  url: string
  icon?: string
  iconType: 'emoji' | 'image' | 'font'
  categoryId?: string
  displayOrder: number
  isActive: boolean
  isGlobal: boolean
  isPinned: boolean
  departmentIds: string[]
}

export interface CreateCategoryRequest {
  name: string
  description: string
  icon?: string
  displayOrder: number
}

export interface UpdateCategoryRequest {
  id: string
  name: string
  description: string
  icon?: string
  displayOrder: number
}
