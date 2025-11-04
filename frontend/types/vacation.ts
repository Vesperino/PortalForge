// Vacation Schedule System Types

export interface VacationSchedule {
  id: string
  userId: string
  user: User
  startDate: string // ISO date string
  endDate: string // ISO date string
  substituteUserId?: string | null
  substitute?: User | null
  sourceRequestId: string
  status: VacationStatus
  createdAt: string
}

export interface User {
  id: string
  firstName: string
  lastName: string
  email: string
  position?: string
  departmentId?: string
  isActive: boolean
}

export enum VacationStatus {
  Scheduled = 'Scheduled',
  Active = 'Active',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export interface VacationCalendar {
  vacations: VacationSchedule[]
  teamSize: number
  alerts: VacationAlert[]
  statistics: VacationStatistics
}

export interface VacationAlert {
  date: string
  type: AlertType
  affectedEmployees: User[]
  coveragePercent: number
  message: string
}

export enum AlertType {
  COVERAGE_LOW = 'COVERAGE_LOW', // 30-49%
  COVERAGE_CRITICAL = 'COVERAGE_CRITICAL' // 50%+
}

export interface VacationStatistics {
  currentlyOnVacation: number
  scheduledVacations: number
  totalVacationDays: number
  averageVacationDays: number
  teamSize: number
  coveragePercent: number
}

// UI-specific types
export type ViewMode = 'timeline' | 'calendar' | 'list'

export interface CalendarDay {
  date: Date
  isCurrentMonth: boolean
  isToday: boolean
  vacations: VacationSchedule[]
}

export interface TimelineRow {
  employee: User
  vacations: VacationSchedule[]
}

export interface VacationBarStyle {
  left: string
  width: string
  backgroundColor: string
}
