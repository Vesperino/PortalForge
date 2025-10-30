export type DepartmentRole = 'Employee' | 'Manager' | 'Director'

export type RequestStatus = 'Draft' | 'InReview' | 'Approved' | 'Rejected' | 'AwaitingSurvey'

export type ApprovalStepStatus = 'Pending' | 'InReview' | 'Approved' | 'Rejected' | 'RequiresSurvey' | 'SurveyFailed'

export type RequestPriority = 'Standard' | 'Urgent'

export type FieldType = 'Text' | 'Textarea' | 'Number' | 'Select' | 'Date' | 'Checkbox'

export interface RequestTemplateField {
  id?: string
  label: string
  fieldType: FieldType
  placeholder?: string
  isRequired: boolean
  options?: string // JSON string
  minValue?: number
  maxValue?: number
  helpText?: string
  order: number
}

export interface RequestApprovalStepTemplate {
  id?: string
  stepOrder: number
  approverRole: DepartmentRole
  requiresQuiz: boolean
}

export interface QuizQuestion {
  id?: string
  question: string
  options: string // JSON: [{value, label, isCorrect}]
  order: number
}

export interface QuizOption {
  value: string
  label: string
  isCorrect: boolean
}

export interface RequestTemplate {
  id: string
  name: string
  description: string
  icon: string
  category: string
  departmentId?: string
  isActive: boolean
  requiresApproval: boolean
  estimatedProcessingDays?: number
  passingScore?: number
  createdById: string
  createdByName: string
  createdAt: string
  updatedAt?: string
  fields: RequestTemplateField[]
  approvalStepTemplates: RequestApprovalStepTemplate[]
  quizQuestions: QuizQuestion[]
}

export interface RequestApprovalStep {
  id: string
  stepOrder: number
  approverId: string
  approverName: string
  status: ApprovalStepStatus
  startedAt?: string
  finishedAt?: string
  comment?: string
  requiresQuiz: boolean
  quizScore?: number
  quizPassed?: boolean
}

export interface Request {
  id: string
  requestNumber: string
  requestTemplateId: string
  requestTemplateName: string
  requestTemplateIcon: string
  submittedById: string
  submittedByName: string
  submittedAt: string
  priority: RequestPriority
  formData: string // JSON
  status: RequestStatus
  completedAt?: string
  approvalSteps: RequestApprovalStep[]
}

export interface CreateRequestTemplateDto {
  name: string
  description: string
  icon: string
  category: string
  departmentId?: string
  requiresApproval: boolean
  estimatedProcessingDays?: number
  passingScore?: number
  fields: RequestTemplateField[]
  approvalStepTemplates: RequestApprovalStepTemplate[]
  quizQuestions: QuizQuestion[]
}

export interface SubmitRequestDto {
  requestTemplateId: string
  priority: RequestPriority
  formData: Record<string, any>
}

export interface ApproveStepDto {
  comment?: string
}

