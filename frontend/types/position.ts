export interface Position {
  id: string
  name: string
  description: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string | null
}

export interface CreatePositionDto {
  name: string
  description?: string | null
  isActive?: boolean
}

export interface UpdatePositionDto {
  name: string
  description?: string | null
  isActive: boolean
}
