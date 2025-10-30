export type UserRole = 'Admin' | 'Manager' | 'HR' | 'Marketing' | 'Employee'

export type PositionLevel = 'Junior' | 'Mid' | 'Senior' | 'Lead' | 'Manager' | 'Director' | 'C-Level'

export type EventTag = 'szkolenie' | 'impreza' | 'spotkanie' | 'meeting' | 'all-hands' | 'urodziny' | 'święto'

export type NewsCategory = 'announcement' | 'product' | 'hr' | 'tech' | 'event'

export type DocumentCategory = 'policy' | 'procedure' | 'template' | 'report' | 'presentation' | 'manual'

export type FileType = 'pdf' | 'docx' | 'xlsx' | 'pptx' | 'txt'

export type LeaveType = 'vacation' | 'sick' | 'personal' | 'parental' | 'unpaid' | 'remote'

export type LeaveStatus = 'pending' | 'approved' | 'rejected' | 'cancelled'

export type TaskStatus = 'todo' | 'in-progress' | 'review' | 'done' | 'blocked'

export type TaskPriority = 'low' | 'medium' | 'high' | 'urgent'

export type ProjectStatus = 'planning' | 'active' | 'on-hold' | 'completed' | 'cancelled'

export interface Department {
  id: number
  name: string
  description?: string
  managerId?: number
  manager?: Employee
  color?: string // Kolor dla wizualizacji
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
  yearsOfService?: number
  hireDate?: Date
  birthDate?: Date
  address?: string
  city?: string
  country?: string
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
  authorName?: string
  author?: Employee
  createdAt: Date
  updatedAt?: Date
  eventId?: number
  event?: Event
  views: number
  category: NewsCategory
  // Event-specific fields
  isEvent: boolean
  eventHashtag?: string
  eventDateTime?: Date
  eventLocation?: string
  eventPlaceId?: string
  // Department visibility
  departmentId?: number
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

export interface LeaveRequest {
  id: number
  employeeId: number
  employee?: Employee
  type: LeaveType
  startDate: Date
  endDate: Date
  days: number
  reason?: string
  status: LeaveStatus
  requestedAt: Date
  reviewedBy?: number
  reviewer?: Employee
  reviewedAt?: Date
  reviewComment?: string
}

export interface Task {
  id: number
  title: string
  description?: string
  assignedTo: number
  assignee?: Employee
  createdBy: number
  creator?: Employee
  projectId?: number
  project?: Project
  status: TaskStatus
  priority: TaskPriority
  dueDate?: Date
  createdAt: Date
  updatedAt?: Date
  completedAt?: Date
  estimatedHours?: number
  actualHours?: number
  tags?: string[]
}

export interface Project {
  id: number
  name: string
  description?: string
  status: ProjectStatus
  startDate: Date
  endDate?: Date
  managerId: number
  manager?: Employee
  teamMembers: number[]
  team?: Employee[]
  departmentId?: number
  department?: Department
  progress: number // 0-100
  budget?: number
  createdAt: Date
  updatedAt?: Date
}

export interface TimeOff {
  id: number
  employeeId: number
  employee?: Employee
  date: Date
  hours: number
  type: LeaveType
  approved: boolean
}

export interface Announcement {
  id: number
  title: string
  content: string
  priority: 'low' | 'medium' | 'high'
  publishedBy: number
  publisher?: Employee
  publishedAt: Date
  expiresAt?: Date
  targetDepartments?: number[]
  isActive: boolean
}
