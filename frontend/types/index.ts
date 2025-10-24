export type UserRole = 'Admin' | 'Manager' | 'HR' | 'Marketing' | 'Employee'

export type PositionLevel = 'Junior' | 'Mid' | 'Senior' | 'Lead' | 'Manager' | 'Director' | 'C-Level'

export type EventTag = 'szkolenie' | 'impreza' | 'spotkanie' | 'meeting' | 'all-hands' | 'urodziny' | 'święto'

export type NewsCategory = 'announcement' | 'product' | 'hr' | 'tech' | 'event'

export type DocumentCategory = 'policy' | 'procedure' | 'template' | 'report' | 'presentation' | 'manual'

export type FileType = 'pdf' | 'docx' | 'xlsx' | 'pptx' | 'txt'

export interface Department {
  id: number
  name: string
  description?: string
  managerId?: number
  manager?: Employee
}

export interface Position {
  id: number
  name: string
  level: PositionLevel
}

export interface Employee {
  id: number
  firstName: string
  lastName: string
  email: string
  phone?: string
  departmentId: number
  department?: Department
  positionId: number
  position?: Position
  supervisorId?: number
  supervisor?: Employee
  avatar?: string
  role: UserRole
  subordinates?: Employee[]
}

export interface Event {
  id: number
  title: string
  description: string
  startDate: Date
  endDate?: Date
  location?: string
  tags: EventTag[]
  targetDepartments: number[]
  createdBy: number
  createdAt: Date
  newsId?: number
  attendees?: number
}

export interface News {
  id: number
  title: string
  content: string
  excerpt: string
  imageUrl?: string
  authorId: number
  author?: Employee
  createdAt: Date
  updatedAt?: Date
  eventId?: number
  event?: Event
  views: number
  category: NewsCategory
}

export interface Document {
  id: number
  name: string
  category: DocumentCategory
  fileType: FileType
  size: number
  uploadedBy: number
  uploader?: Employee
  uploadedAt: Date
  url?: string
  description?: string
}
