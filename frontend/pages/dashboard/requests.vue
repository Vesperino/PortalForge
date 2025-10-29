<script setup lang="ts">
import { computed, ref, watchEffect } from 'vue'
import type { Employee } from '~/types'
import { useAuthStore } from '~/stores/auth'
interface RequestFieldOption {
  value: string
  label: string
}

interface RequestField {
  id: string
  label: string
  type: 'text' | 'textarea' | 'number' | 'select' | 'date' | 'checkbox'
  placeholder?: string
  required?: boolean
  options?: RequestFieldOption[]
  min?: number
  max?: number
  helpText?: string
}

interface RequestType {
  id: string
  name: string
  description: string
  icon: string
  category: 'hardware' | 'software' | 'access' | 'training' | 'other'
  color: string
  requiresApproval: boolean
  estimatedProcessingDays?: number
  fields: RequestField[]
}

const requestTypes: RequestType[] = [
  {
    id: 'hardware-basic',
    name: 'Sprzęt komputerowy',
    description: 'Laptop, komputer stacjonarny, monitory i akcesoria',
    icon: '💻',
    category: 'hardware',
    color: 'blue',
    requiresApproval: true,
    estimatedProcessingDays: 7,
    fields: [
      {
        id: 'device',
        label: 'Rodzaj sprzętu',
        type: 'select',
        required: true,
        options: [
          { value: 'laptop', label: 'Laptop' },
          { value: 'desktop', label: 'Komputer stacjonarny' },
          { value: 'monitor', label: 'Monitor' },
          { value: 'peripherals', label: 'Akcesoria (klawiatura, mysz, itp.)' }
        ]
      },
      {
        id: 'specification',
        label: 'Specyfikacja / wymagania',
        type: 'textarea',
        required: true,
        placeholder: 'Opisz oczekiwane parametry, np. CPU, RAM, dodatkowe akcesoria.'
      },
      {
        id: 'quantity',
        label: 'Ilość',
        type: 'number',
        required: true,
        min: 1,
        max: 20
      },
      {
        id: 'justification',
        label: 'Uzasadnienie biznesowe',
        type: 'textarea',
        required: true,
        placeholder: 'Opisz, dlaczego sprzęt jest potrzebny.'
      }
    ]
  },
  {
    id: 'software-license',
    name: 'Oprogramowanie / licencje',
    description: 'Licencje, subskrypcje i narzędzia programistyczne',
    icon: '🧩',
    category: 'software',
    color: 'purple',
    requiresApproval: true,
    estimatedProcessingDays: 5,
    fields: [
      {
        id: 'software-name',
        label: 'Nazwa oprogramowania',
        type: 'text',
        required: true
      },
      {
        id: 'license-type',
        label: 'Typ licencji',
        type: 'select',
        required: true,
        options: [
          { value: 'subscription', label: 'Subskrypcja' },
          { value: 'perpetual', label: 'Licencja bezterminowa' },
          { value: 'trial', label: 'Wersja trial' }
        ]
      },
      {
        id: 'seats',
        label: 'Liczba stanowisk',
        type: 'number',
        required: true,
        min: 1,
        max: 50
      },
      {
        id: 'justification',
        label: 'Uzasadnienie',
        type: 'textarea',
        required: true,
        placeholder: 'Jakie procesy wymagają tej licencji?'
      }
    ]
  },
  {
    id: 'access-request',
    name: 'Uprawnienia i dostęp',
    description: 'Dostęp do systemów, repozytoriów i narzędzi firmowych',
    icon: '🔐',
    category: 'access',
    color: 'teal',
    requiresApproval: true,
    estimatedProcessingDays: 2,
    fields: [
      {
        id: 'system-name',
        label: 'System / narzędzie',
        type: 'text',
        required: true
      },
      {
        id: 'access-scope',
        label: 'Zakres dostępu',
        type: 'select',
        required: true,
        options: [
          { value: 'read', label: 'Tylko odczyt' },
          { value: 'write', label: 'Odczyt i zapis' },
          { value: 'admin', label: 'Administrator' }
        ]
      },
      {
        id: 'temporary',
        label: 'Dostęp tymczasowy',
        type: 'checkbox'
      },
      {
        id: 'end-date',
        label: 'Data zakończenia (jeśli tymczasowy)',
        type: 'date'
      },
      {
        id: 'justification',
        label: 'Uzasadnienie',
        type: 'textarea',
        required: true
      }
    ]
  },
  {
    id: 'training',
    name: 'Szkolenie / konferencja',
    description: 'Wniosek o udział w szkoleniu lub konferencji',
    icon: '🎓',
    category: 'training',
    color: 'amber',
    requiresApproval: true,
    estimatedProcessingDays: 10,
    fields: [
      {
        id: 'training-name',
        label: 'Nazwa szkolenia',
        type: 'text',
        required: true
      },
      {
        id: 'training-type',
        label: 'Forma',
        type: 'select',
        required: true,
        options: [
          { value: 'online', label: 'Online' },
          { value: 'onsite', label: 'Stacjonarne' },
          { value: 'hybrid', label: 'Hybrydowe' }
        ]
      },
      {
        id: 'training-date',
        label: 'Data rozpoczęcia',
        type: 'date',
        required: true
      },
      {
        id: 'cost',
        label: 'Koszt (PLN)',
        type: 'number',
        required: true,
        min: 0
      },
      {
        id: 'justification',
        label: 'Uzasadnienie biznesowe',
        type: 'textarea',
        required: true,
        placeholder: 'Jak szkolenie wpłynie na Twoją pracę / zespół?'
      }
    ]
  },
  {
    id: 'leave',
    name: 'Wniosek urlopowy',
    description: 'Wniosek o udzielenie urlopu wypoczynkowego',
    icon: '🏖️',
    category: 'other',
    color: 'green',
    requiresApproval: true,
    estimatedProcessingDays: 3,
    fields: [
      {
        id: 'leave-type',
        label: 'Rodzaj urlopu',
        type: 'select',
        required: true,
        options: [
          { value: 'annual', label: 'Urlop wypoczynkowy' },
          { value: 'on-demand', label: 'Urlop na żądanie' },
          { value: 'unpaid', label: 'Urlop bezpłatny' },
          { value: 'sick', label: 'Urlop okolicznościowy' }
        ]
      },
      {
        id: 'start-date',
        label: 'Data rozpoczęcia',
        type: 'date',
        required: true
      },
      {
        id: 'end-date',
        label: 'Data zakończenia',
        type: 'date',
        required: true
      },
      {
        id: 'days-count',
        label: 'Liczba dni',
        type: 'number',
        required: true,
        min: 1,
        max: 26,
        helpText: 'Liczba dni roboczych urlopu'
      },
      {
        id: 'replacement',
        label: 'Osoba zastępująca',
        type: 'text',
        required: false,
        placeholder: 'Imię i nazwisko osoby zastępującej'
      },
      {
        id: 'notes',
        label: 'Uwagi',
        type: 'textarea',
        required: false,
        placeholder: 'Dodatkowe informacje dotyczące urlopu'
      }
    ]
  },
  {
    id: 'other',
    name: 'Inny wniosek',
    description: 'Dowolny wniosek niestandardowy',
    icon: '📝',
    category: 'other',
    color: 'gray',
    requiresApproval: true,
    estimatedProcessingDays: 4,
    fields: [
      {
        id: 'title',
        label: 'Tytuł wniosku',
        type: 'text',
        required: true
      },
      {
        id: 'details',
        label: 'Szczegóły',
        type: 'textarea',
        required: true,
        placeholder: 'Opisz, czego dotyczy wniosek.'
      },
      {
        id: 'justification',
        label: 'Uzasadnienie',
        type: 'textarea',
        required: true
      }
    ]
  }
]

const getRequestTypeById = (id: string) => requestTypes.find(type => type.id === id)

definePageMeta({
  layout: 'default'
  // middleware: ['auth'] // Disabled for testing
})

type RequestPriority = 'standard' | 'pilne'
type StepStatus = 'pending' | 'in_review' | 'approved' | 'requires_survey' | 'survey_failed'
type RequestStatus = 'draft' | 'in_review' | 'approved' | 'rejected' | 'awaiting_survey'

interface SurveyQuestion {
  id: string
  question: string
  options: Array<{ value: string; label: string; isCorrect: boolean }>
}

interface SurveyAnswer {
  questionId: string
  selectedAnswer: string
}

interface ApprovalStep {
  id: number
  approver: Employee
  status: StepStatus
  startedAt?: Date
  finishedAt?: Date
  comment?: string
  requiresSurvey?: boolean
  surveyQuestions?: SurveyQuestion[]
  surveyAnswers?: SurveyAnswer[]
  surveyScore?: number
  surveyPassed?: boolean
}

interface RequestRecord {
  id: number
  requestNumber: string
  requestTypeId: string
  requestTypeName: string
  createdAt: Date
  priority: RequestPriority
  formData: Record<string, unknown>
  approvals: ApprovalStep[]
  status: RequestStatus
}

const { getEmployees, getEmployeeById } = useMockData()
const authStore = useAuthStore()
const employees = getEmployees()

const currentUser = computed<Employee | null>(() => {
  if (authStore.user?.userId) {
    const employee = getEmployeeById(authStore.user.userId)
    if (employee) {
      return employee
    }
  }
  return employees[0] ?? null
})

const primaryApprover = computed<Employee | null>(() => currentUser.value?.supervisor ?? null)
const secondaryApprover = computed<Employee | null>(() => primaryApprover.value?.supervisor ?? null)

// Tabs
const activeTab = ref<'new' | 'my-requests'>('new')

// New request wizard
const wizardStep = ref<'select-type' | 'fill-form' | 'review'>('select-type')
const selectedRequestType = ref<string | null>(null)
const formData = ref<Record<string, unknown>>({})
const priority = ref<RequestPriority>('standard')
const escalateToSecondary = ref(true)

// Request type selection with search
const requestTypeSearch = ref('')
const _showRequestTypeDropdown = ref(false)

// Requests list
const requests = ref<RequestRecord[]>([])
const seeded = ref(false)
let requestSeed = 1000

// Search and filters for "My Requests"
const searchQuery = ref('')
const statusFilter = ref<RequestStatus | 'all'>('all')
const selectedRequest = ref<RequestRecord | null>(null)

// Survey modal
const showSurveyModal = ref(false)
const surveyRequest = ref<RequestRecord | null>(null)
const surveyStepIndex = ref<number>(-1)
const surveyAnswers = ref<SurveyAnswer[]>([])

const errorMessage = ref<string | null>(null)
const successMessage = ref<string | null>(null)


// Computed
const currentRequestType = computed(() => {
  if (!selectedRequestType.value) return null
  return getRequestTypeById(selectedRequestType.value)
})

const filteredRequestTypes = computed(() => {
  if (!requestTypeSearch.value.trim()) return requestTypes
  const query = requestTypeSearch.value.toLowerCase()
  return requestTypes.filter(type =>
    type.name.toLowerCase().includes(query) ||
    type.description.toLowerCase().includes(query) ||
    type.category.toLowerCase().includes(query)
  )
})



const approvalSteps = computed(() => {
  const steps: Array<{ id: number; approver: Employee }> = []
  if (primaryApprover.value) {
    steps.push({ id: 1, approver: primaryApprover.value })
  }
  if (escalateToSecondary.value && secondaryApprover.value) {
    steps.push({ id: steps.length + 1, approver: secondaryApprover.value })
  }
  return steps
})

const filteredRequests = computed(() => {
  let filtered = requests.value

  // Filter by status
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(r => r.status === statusFilter.value)
  }

  // Filter by search query
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(r =>
      r.requestNumber.toLowerCase().includes(query) ||
      r.requestTypeName.toLowerCase().includes(query)
    )
  }

  return filtered
})

const currentSurveyStep = computed(() => {
  if (!surveyRequest.value || surveyStepIndex.value < 0) return null
  return surveyRequest.value.approvals[surveyStepIndex.value]
})

// Helper functions
const formatDate = (date: Date) =>
  new Intl.DateTimeFormat('pl-PL', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)

const getStatusBadgeClass = (status: RequestStatus) => {
  switch (status) {
    case 'approved':
      return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
    case 'in_review':
      return 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300'
    case 'awaiting_survey':
      return 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300'
    case 'rejected':
      return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
    case 'draft':
      return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
  }
}

const getStatusLabel = (status: RequestStatus) => {
  switch (status) {
    case 'approved': return 'Zatwierdzony'
    case 'in_review': return 'W akceptacji'
    case 'awaiting_survey': return 'Wymaga ankiety'
    case 'rejected': return 'Odrzucony'
    case 'draft': return 'Szkic'
  }
}

const getStepBadgeClass = (status: StepStatus) => {
  switch (status) {
    case 'approved':
      return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
    case 'in_review':
      return 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300'
    case 'requires_survey':
      return 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300'
    case 'survey_failed':
      return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
    case 'pending':
      return 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300'
  }
}

const _getStepStatusLabel = (status: StepStatus) => {
  switch (status) {
    case 'approved': return 'Zatwierdzony'
    case 'in_review': return 'W trakcie'
    case 'requires_survey': return 'Wymaga ankiety'
    case 'survey_failed': return 'Test niezaliczony'
    case 'pending': return 'Oczekuje'
  }
}



// Wizard actions
const selectRequestType = (typeId: string) => {
  selectedRequestType.value = typeId
  formData.value = {}
  wizardStep.value = 'fill-form'
}

const goBackToTypeSelection = () => {
  wizardStep.value = 'select-type'
  selectedRequestType.value = null
  formData.value = {}
}

const goToReview = () => {
  errorMessage.value = null

  // Validate required fields
  const reqType = currentRequestType.value
  if (!reqType) return

  for (const field of reqType.fields) {
    if (field.required && !formData.value[field.id]) {
      errorMessage.value = `Pole "${field.label}" jest wymagane.`
      return
    }
  }

  wizardStep.value = 'review'
}

const goBackToForm = () => {
  wizardStep.value = 'fill-form'
}

const submitRequest = () => {
  errorMessage.value = null
  successMessage.value = null

  if (!currentUser.value || !currentRequestType.value) {
    errorMessage.value = 'Brak danych użytkownika lub typu wniosku.'
    return
  }

  const approvals: ApprovalStep[] = []
  if (primaryApprover.value) {
    approvals.push({
      id: 1,
      approver: primaryApprover.value,
      status: 'in_review',
      startedAt: new Date()
    })
  }
  if (escalateToSecondary.value && secondaryApprover.value) {
    approvals.push({
      id: approvals.length + 1,
      approver: secondaryApprover.value,
      status: approvals.length ? 'pending' : 'in_review'
    })
  }

  const request: RequestRecord = {
    id: ++requestSeed,
    requestNumber: `ZAP-${new Date().getFullYear()}-${String(requestSeed).padStart(4, '0')}`,
    requestTypeId: currentRequestType.value.id,
    requestTypeName: currentRequestType.value.name,
    createdAt: new Date(),
    priority: priority.value,
    formData: { ...formData.value },
    approvals,
    status: approvals.length ? 'in_review' : 'approved'
  }

  requests.value = [request, ...requests.value]

  const active = request.approvals.find(step => step.status === 'in_review')
  successMessage.value = active
    ? `✓ Wniosek ${request.requestNumber} został złożony i przekazany do: ${active.approver.firstName} ${active.approver.lastName}.`
    : `✓ Wniosek ${request.requestNumber} został zatwierdzony automatycznie.`

  // Reset wizard
  wizardStep.value = 'select-type'
  selectedRequestType.value = null
  formData.value = {}
  priority.value = 'standard'
  escalateToSecondary.value = Boolean(secondaryApprover.value)

  // Auto-hide success message after 5 seconds
  setTimeout(() => {
    successMessage.value = null
  }, 5000)
}

const cancelWizard = () => {
  wizardStep.value = 'select-type'
  selectedRequestType.value = null
  formData.value = {}
  errorMessage.value = null
}

// My Requests actions
const viewRequestDetails = (request: RequestRecord) => {
  selectedRequest.value = request
}

const closeRequestDetails = () => {
  selectedRequest.value = null
}

const advanceRequest = (id: number) => {
  const request = requests.value.find(r => r.id === id)
  if (!request) return

  const current = request.approvals.find(step => step.status === 'in_review')
  if (!current) return

  // Check if this step requires survey
  if (current.requiresSurvey && !current.surveyPassed) {
    current.status = 'requires_survey'
    request.status = 'awaiting_survey'
    requests.value = [...requests.value]
    return
  }

  current.status = 'approved'
  current.finishedAt = new Date()

  const next = request.approvals.find(step => step.status === 'pending')
  if (next) {
    next.status = 'in_review'
    next.startedAt = new Date()
    request.status = 'in_review'
  } else {
    request.status = 'approved'
  }

  requests.value = [...requests.value]
}

// Survey functions
const openSurvey = (request: RequestRecord, stepIndex: number) => {
  surveyRequest.value = request
  surveyStepIndex.value = stepIndex
  surveyAnswers.value = []
  showSurveyModal.value = true
}

const closeSurvey = () => {
  showSurveyModal.value = false
  surveyRequest.value = null
  surveyStepIndex.value = -1
  surveyAnswers.value = []
}

const submitSurvey = () => {
  if (!surveyRequest.value || surveyStepIndex.value < 0) return

  const step = surveyRequest.value.approvals[surveyStepIndex.value]
  if (!step || !step.surveyQuestions) return

  // Calculate score
  let correctAnswers = 0
  const totalQuestions = step.surveyQuestions.length

  step.surveyQuestions.forEach(question => {
    const userAnswer = surveyAnswers.value.find(a => a.questionId === question.id)
    if (userAnswer) {
      const correctOption = question.options.find(o => o.isCorrect)
      if (correctOption && userAnswer.selectedAnswer === correctOption.value) {
        correctAnswers++
      }
    }
  })

  const score = (correctAnswers / totalQuestions) * 100
  step.surveyScore = score
  step.surveyAnswers = [...surveyAnswers.value]

  // Check if passed (80% threshold)
  const passed = score >= 80

  if (passed) {
    // Auto-approve
    step.surveyPassed = true
    step.status = 'approved'
    step.finishedAt = new Date()
    step.comment = `Test zaliczony (${score.toFixed(0)}% poprawnych odpowiedzi)`

    // Move to next step or complete
    const next = surveyRequest.value.approvals.find(s => s.status === 'pending')
    if (next) {
      next.status = 'in_review'
      next.startedAt = new Date()
      surveyRequest.value.status = 'in_review'
    } else {
      surveyRequest.value.status = 'approved'
    }

    successMessage.value = `✓ Test zaliczony! Wynik: ${score.toFixed(0)}%. Wniosek został zatwierdzony.`
  } else {
    // Failed - send back to approver
    step.surveyPassed = false
    step.status = 'survey_failed'
    step.comment = `Test niezaliczony (${score.toFixed(0)}% poprawnych odpowiedzi). Wymaga decyzji przełożonego.`
    surveyRequest.value.status = 'in_review'

    errorMessage.value = `Test niezaliczony. Wynik: ${score.toFixed(0)}%. Wniosek został przekazany do przełożonego.`
  }

  requests.value = [...requests.value]
  closeSurvey()

  // Auto-hide messages
  setTimeout(() => {
    successMessage.value = null
    errorMessage.value = null
  }, 5000)
}

// Seed mock data
watchEffect(() => {
  if (seeded.value || !currentUser.value) {
    return
  }

  const mockSupervisor = employees.find(e => e.id === 2) || employees[1]
  const mockSecondaryApprover = employees.find(e => e.id === 1) || employees[0]

  if (!mockSupervisor || !mockSecondaryApprover) {
    seeded.value = true
    return
  }

  // Mock 1: Laptop request - in first approval stage
  const approvals1: ApprovalStep[] = []
  if (primaryApprover.value) {
    approvals1.push({
      id: 1,
      approver: primaryApprover.value,
      status: 'in_review',
      startedAt: new Date(Date.now() - 1000 * 60 * 60 * 6)
    })
  }
  if (secondaryApprover.value) {
    approvals1.push({
      id: approvals1.length + 1,
      approver: secondaryApprover.value,
      status: approvals1.length ? 'pending' : 'in_review'
    })
  }

  const sample1: RequestRecord = {
    id: ++requestSeed,
    requestNumber: 'ZAP-2025-0001',
    requestTypeId: 'hardware-laptop',
    requestTypeName: 'Laptop / Komputer',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24),
    priority: 'standard',
    formData: {
      'device-type': 'laptop',
      'specifications': 'Laptop Lenovo ThinkPad X1 Carbon, i7-1365U, 16GB RAM, 512GB SSD',
      'quantity': 1,
      'justification': 'Wyposażenie nowego członka zespołu backend.',
      'deadline': new Date(Date.now() + 1000 * 60 * 60 * 24 * 14).toISOString().split('T')[0]
    },
    approvals: approvals1,
    status: approvals1.length ? 'in_review' : 'approved'
  }

  // Mock 2: Software license - in second approval stage (urgent)
  const approvals2: ApprovalStep[] = [
    {
      id: 1,
      approver: mockSupervisor,
      status: 'approved',
      startedAt: new Date(Date.now() - 1000 * 60 * 60 * 48),
      finishedAt: new Date(Date.now() - 1000 * 60 * 60 * 36),
      comment: 'Zatwierdzam - potrzebne dla projektu'
    },
    {
      id: 2,
      approver: mockSecondaryApprover,
      status: 'in_review',
      startedAt: new Date(Date.now() - 1000 * 60 * 60 * 36)
    }
  ]

  const sample2: RequestRecord = {
    id: ++requestSeed,
    requestNumber: 'ZAP-2025-0002',
    requestTypeId: 'software-license',
    requestTypeName: 'Licencja oprogramowania',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 48),
    priority: 'pilne',
    formData: {
      'software-name': 'JetBrains All Products Pack',
      'license-type': 'new',
      'duration': 'annual',
      'seats': 5,
      'justification': 'Rozbudowa zespołu deweloperskiego - potrzebne IDE dla nowych programistów'
    },
    approvals: approvals2,
    status: 'in_review'
  }

  // Mock 3: Training request - approved
  const approvals3: ApprovalStep[] = [
    {
      id: 1,
      approver: mockSupervisor,
      status: 'approved',
      startedAt: new Date(Date.now() - 1000 * 60 * 60 * 120),
      finishedAt: new Date(Date.now() - 1000 * 60 * 60 * 96),
      comment: 'Zatwierdzam'
    }
  ]

  const sample3: RequestRecord = {
    id: ++requestSeed,
    requestNumber: 'ZAP-2025-0003',
    requestTypeId: 'training-course',
    requestTypeName: 'Szkolenie / Konferencja',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 120),
    priority: 'standard',
    formData: {
      'training-name': 'AWS Solutions Architect - Professional',
      'organizer': 'AWS Training',
      'training-type': 'online',
      'date': new Date(Date.now() + 1000 * 60 * 60 * 24 * 30).toISOString().split('T')[0],
      'cost': 2500,
      'justification': 'Certyfikacja potrzebna do pracy z infrastrukturą chmurową klienta'
    },
    approvals: approvals3,
    status: 'approved'
  }

  // Mock 4: AI Agent request - requires survey after approval
  const aiSurveyQuestions: SurveyQuestion[] = [
    {
      id: 'q1',
      question: 'Jakie są główne zastosowania agenta AI w pracy programisty?',
      options: [
        { value: 'a', label: 'Tylko pisanie kodu', isCorrect: false },
        { value: 'b', label: 'Generowanie kodu, refaktoryzacja, dokumentacja i pomoc w debugowaniu', isCorrect: true },
        { value: 'c', label: 'Zastępowanie programistów', isCorrect: false }
      ]
    },
    {
      id: 'q2',
      question: 'Czy kod wygenerowany przez AI zawsze wymaga weryfikacji?',
      options: [
        { value: 'a', label: 'Nie, kod AI jest zawsze poprawny', isCorrect: false },
        { value: 'b', label: 'Tak, zawsze należy sprawdzić i przetestować kod', isCorrect: true },
        { value: 'c', label: 'Tylko w przypadku skomplikowanych funkcji', isCorrect: false }
      ]
    },
    {
      id: 'q3',
      question: 'Jakie dane można udostępniać agentowi AI?',
      options: [
        { value: 'a', label: 'Wszystkie dane firmowe bez ograniczeń', isCorrect: false },
        { value: 'b', label: 'Tylko dane publiczne i niezawierające informacji poufnych', isCorrect: true },
        { value: 'c', label: 'Hasła i klucze API', isCorrect: false }
      ]
    },
    {
      id: 'q4',
      question: 'Jak często należy aktualizować wiedzę o narzędziach AI?',
      options: [
        { value: 'a', label: 'Raz na kilka lat', isCorrect: false },
        { value: 'b', label: 'Regularnie, technologia szybko się rozwija', isCorrect: true },
        { value: 'c', label: 'Nigdy, wystarczy początkowe szkolenie', isCorrect: false }
      ]
    }
  ]

  const approvals4: ApprovalStep[] = [
    {
      id: 1,
      approver: mockSupervisor,
      status: 'approved',
      startedAt: new Date(Date.now() - 1000 * 60 * 60 * 24),
      finishedAt: new Date(Date.now() - 1000 * 60 * 60 * 12),
      comment: 'Zatwierdzam - dobry pomysł'
    },
    {
      id: 2,
      approver: mockSecondaryApprover,
      status: 'requires_survey',
      startedAt: new Date(Date.now() - 1000 * 60 * 60 * 12),
      requiresSurvey: true,
      surveyQuestions: aiSurveyQuestions,
      comment: 'Zatwierdzam pod warunkiem zaliczenia testu wiedzy o AI'
    }
  ]

  const sample4: RequestRecord = {
    id: ++requestSeed,
    requestNumber: 'ZAP-2025-0004',
    requestTypeId: 'software-license',
    requestTypeName: 'Agent AI do pracy',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24),
    priority: 'standard',
    formData: {
      'software-name': 'GitHub Copilot Enterprise',
      'license-type': 'subscription',
      'seats': 1,
      'justification': 'Zwiększenie produktywności w pisaniu kodu i automatyzacji zadań programistycznych'
    },
    approvals: approvals4,
    status: 'awaiting_survey'
  }

  requests.value = [sample4, sample1, sample2, sample3]
  seeded.value = true
})
</script>

<template>
  <div class="min-h-screen space-y-6">
    <!-- Header -->
    <header class="space-y-2">
      <h1 class="text-4xl font-bold text-gray-900 dark:text-white">Wnioski</h1>
      <p class="text-base text-gray-600 dark:text-gray-400">
        Składaj wnioski i śledź ich status w czasie rzeczywistym
      </p>
    </header>

    <!-- Global Messages -->
    <div v-if="errorMessage" class="rounded-lg border-2 border-red-300 dark:border-red-800 bg-red-50 dark:bg-red-900/30 px-4 py-3 text-sm text-red-700 dark:text-red-200 flex items-start gap-3">
      <svg class="h-5 w-5 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
      </svg>
      <span>{{ errorMessage }}</span>
    </div>

    <div v-if="successMessage" class="rounded-lg border-2 border-green-300 dark:border-green-800 bg-green-50 dark:bg-green-900/30 px-4 py-3 text-sm text-green-700 dark:text-green-200 flex items-start gap-3">
      <svg class="h-5 w-5 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
      </svg>
      <span>{{ successMessage }}</span>
    </div>

    <!-- Tabs -->
    <div class="border-b border-gray-200 dark:border-gray-700">
      <nav class="-mb-px flex space-x-8">
        <button
          :class="[
            'whitespace-nowrap border-b-2 py-4 px-1 text-sm font-semibold transition-colors',
            activeTab === 'new'
              ? 'border-blue-500 text-blue-600 dark:text-blue-400'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          ]"
          @click="activeTab = 'new'"
        >
          <span class="flex items-center gap-2">
            <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            Nowy wniosek
          </span>
        </button>
        <button
          :class="[
            'whitespace-nowrap border-b-2 py-4 px-1 text-sm font-semibold transition-colors',
            activeTab === 'my-requests'
              ? 'border-blue-500 text-blue-600 dark:text-blue-400'
              : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
          ]"
          @click="activeTab = 'my-requests'"
        >
          <span class="flex items-center gap-2">
            <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            Moje wnioski
            <span v-if="requests.length" class="inline-flex items-center justify-center rounded-full bg-blue-100 dark:bg-blue-900 px-2 py-0.5 text-xs font-bold text-blue-800 dark:text-blue-200">
              {{ requests.length }}
            </span>
          </span>
        </button>
      </nav>
    </div>

    <!-- Tab Content: New Request -->
    <div v-if="activeTab === 'new'" class="space-y-6">
      <!-- Step 1: Select Request Type -->
      <div v-if="wizardStep === 'select-type'" class="space-y-6">
        <div>
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">Wybierz typ wniosku</h2>
          <p class="text-gray-600 dark:text-gray-400">Wyszukaj i wybierz typ wniosku z listy dostępnych</p>
        </div>

        <!-- Search box -->
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input
            v-model="requestTypeSearch"
            type="text"
            placeholder="Szukaj typu wniosku..."
            class="w-full pl-10 pr-4 py-3 rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
          >
        </div>

        <!-- Request types table -->
        <div class="bg-white dark:bg-gray-800 rounded-xl border-2 border-gray-200 dark:border-gray-700 overflow-hidden">
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead class="bg-gray-50 dark:bg-gray-900/50 border-b border-gray-200 dark:border-gray-700">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Typ wniosku
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Kategoria
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Czas realizacji
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Akcja
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
                <tr
                  v-for="reqType in filteredRequestTypes"
                  :key="reqType.id"
                  class="hover:bg-gray-50 dark:hover:bg-gray-900/30 transition-colors"
                >
                  <td class="px-6 py-4">
                    <div class="flex items-center gap-3">
                      <span class="text-3xl">{{ reqType.icon }}</span>
                      <div>
                        <div class="text-sm font-semibold text-gray-900 dark:text-white">
                          {{ reqType.name }}
                        </div>
                        <div class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                          {{ reqType.description }}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <span
:class="[
                      'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
                      reqType.category === 'hardware' ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300' :
                      reqType.category === 'software' ? 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-300' :
                      reqType.category === 'access' ? 'bg-teal-100 text-teal-800 dark:bg-teal-900/30 dark:text-teal-300' :
                      reqType.category === 'training' ? 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300' :
                      'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                    ]">
                      {{ reqType.category }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">
                    <span v-if="reqType.estimatedProcessingDays">
                      ~{{ reqType.estimatedProcessingDays }} dni
                    </span>
                    <span v-else class="text-gray-400">-</span>
                  </td>
                  <td class="px-6 py-4 text-right">
                    <button
                      class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium transition-colors"
                      @click="selectRequestType(reqType.id)"
                    >
                      Wybierz
                      <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7l5 5m0 0l-5 5m5-5H6" />
                      </svg>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Empty state -->
          <div v-if="filteredRequestTypes.length === 0" class="px-6 py-12 text-center">
            <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Nie znaleziono typów wniosków</h3>
            <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Spróbuj zmienić kryteria wyszukiwania</p>
          </div>
        </div>
      </div>

      <!-- Step 2: Fill Form -->
      <div v-if="wizardStep === 'fill-form' && currentRequestType" class="space-y-6">
        <div class="flex items-center justify-between">
          <div>
            <button
              class="text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white flex items-center gap-1 mb-2"
              @click="goBackToTypeSelection"
            >
              <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
              Zmień typ wniosku
            </button>
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
              <span class="text-3xl">{{ currentRequestType.icon }}</span>
              {{ currentRequestType.name }}
            </h2>
            <p class="text-gray-600 dark:text-gray-400 mt-1">{{ currentRequestType.description }}</p>
          </div>
        </div>

        <div class="rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-8">
          <form class="space-y-6" @submit.prevent="goToReview">
            <!-- Dynamic form fields -->
            <div v-for="field in currentRequestType.fields" :key="field.id" class="space-y-2">
              <label class="block text-sm font-semibold text-gray-900 dark:text-white">
                {{ field.label }}
                <span v-if="field.required" class="text-red-500">*</span>
              </label>
              <p v-if="field.helpText" class="text-xs text-gray-500 dark:text-gray-400">{{ field.helpText }}</p>

              <!-- Text input -->
              <input
                v-if="field.type === 'text'"
                v-model="(formData[field.id] as string)"
                type="text"
                :placeholder="field.placeholder"
                :required="field.required"
                class="w-full rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-4 py-3 text-base text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
              >

              <!-- Textarea -->
              <textarea
                v-if="field.type === 'textarea'"
                v-model="(formData[field.id] as string)"
                :placeholder="field.placeholder"
                :required="field.required"
                rows="4"
                class="w-full rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-4 py-3 text-base text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
              />

              <!-- Number input -->
              <input
                v-if="field.type === 'number'"
                v-model.number="(formData[field.id] as number)"
                type="number"
                :placeholder="field.placeholder"
                :required="field.required"
                :min="field.min"
                :max="field.max"
                class="w-full rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-4 py-3 text-base text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
              >

              <!-- Select -->
              <select
                v-if="field.type === 'select'"
                v-model="(formData[field.id] as string)"
                :required="field.required"
                class="w-full rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-4 py-3 text-base text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
              >
                <option value="">Wybierz...</option>
                <option v-for="option in field.options" :key="option.value" :value="option.value">
                  {{ option.label }}
                </option>
              </select>

              <!-- Date input -->
              <input
                v-if="field.type === 'date'"
                v-model="(formData[field.id] as string)"
                type="date"
                :required="field.required"
                class="w-full rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-4 py-3 text-base text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
              >

              <!-- Checkbox -->
              <label v-if="field.type === 'checkbox'" class="flex items-center gap-3 cursor-pointer">
                <input
                  v-model="(formData[field.id] as boolean)"
                  type="checkbox"
                  class="h-5 w-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                >
                <span class="text-sm text-gray-700 dark:text-gray-300">{{ field.label }}</span>
              </label>
            </div>

            <!-- Priority and Escalation -->
            <div class="pt-4 border-t border-gray-200 dark:border-gray-700 space-y-4">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-semibold text-gray-900 dark:text-white mb-2">Priorytet</label>
                  <select
                    v-model="priority"
                    class="w-full rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-4 py-3 text-base text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                  >
                    <option value="standard">Standardowy</option>
                    <option value="pilne">Pilne</option>
                  </select>
                </div>
              </div>
            </div>

            <!-- Form Actions -->
            <div class="flex justify-end gap-3 pt-4">
              <button
                type="button"
                class="px-6 py-3 rounded-lg border-2 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 font-semibold hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                @click="cancelWizard"
              >
                Anuluj
              </button>
              <button
                type="submit"
                class="px-6 py-3 rounded-lg bg-blue-600 text-white font-semibold hover:bg-blue-700 transition-colors shadow-lg"
              >
                Dalej: Przegląd
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Step 3: Review -->
      <div v-if="wizardStep === 'review' && currentRequestType" class="space-y-6">
        <div>
          <button
            class="text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white flex items-center gap-1 mb-2"
            @click="goBackToForm"
          >
            <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
            Wróć do edycji
          </button>
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Przegląd wniosku</h2>
          <p class="text-gray-600 dark:text-gray-400 mt-1">Sprawdź dane przed wysłaniem</p>
        </div>

        <div class="rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-8 space-y-6">
          <!-- Request Type -->
          <div>
            <h3 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">Typ wniosku</h3>
            <div class="flex items-center gap-3">
              <span class="text-3xl">{{ currentRequestType.icon }}</span>
              <div>
                <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ currentRequestType.name }}</p>
                <p class="text-sm text-gray-600 dark:text-gray-400">{{ currentRequestType.description }}</p>
              </div>
            </div>
          </div>

          <!-- Form Data -->
          <div class="border-t border-gray-200 dark:border-gray-700 pt-6">
            <h3 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-4">Szczegóły</h3>
            <dl class="grid grid-cols-1 gap-4">
              <div v-for="field in currentRequestType.fields" :key="field.id">
                <dt class="text-sm font-medium text-gray-500 dark:text-gray-400">{{ field.label }}</dt>
                <dd class="mt-1 text-base text-gray-900 dark:text-white">
                  <template v-if="field.type === 'checkbox'">
                    {{ formData[field.id] ? 'Tak' : 'Nie' }}
                  </template>
                  <template v-else-if="field.type === 'select' && field.options">
                    {{ field.options.find((opt) => opt.value === formData[field.id])?.label || formData[field.id] }}
                  </template>
                  <template v-else>
                    {{ formData[field.id] || '—' }}
                  </template>
                </dd>
              </div>
            </dl>
          </div>

          <!-- Priority -->
          <div class="border-t border-gray-200 dark:border-gray-700 pt-6">
            <h3 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">Priorytet</h3>
            <span
:class="[
              'inline-flex items-center px-3 py-1 rounded-full text-sm font-semibold',
              priority === 'pilne' ? 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300' : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
            ]">
              {{ priority === 'pilne' ? '🔴 Pilne' : '⚪ Standardowy' }}
            </span>
          </div>

          <!-- Approval Path -->
          <div v-if="approvalSteps.length" class="border-t border-gray-200 dark:border-gray-700 pt-6">
            <h3 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-4">Ścieżka akceptacji</h3>
            <div class="space-y-3">
              <div v-for="step in approvalSteps" :key="step.id" class="flex items-center gap-3">
                <div class="flex-shrink-0 w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-300 flex items-center justify-center font-semibold text-sm">
                  {{ step.id }}
                </div>
                <div>
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">
                    {{ step.approver.firstName }} {{ step.approver.lastName }}
                  </p>
                  <p class="text-xs text-gray-500 dark:text-gray-400">{{ step.approver.position }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex justify-end gap-3">
          <button
            type="button"
            class="px-6 py-3 rounded-lg border-2 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 font-semibold hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
            @click="goBackToForm"
          >
            Wróć do edycji
          </button>
          <button
            type="button"
            class="px-6 py-3 rounded-lg bg-green-600 text-white font-semibold hover:bg-green-700 transition-colors shadow-lg"
            @click="submitRequest"
          >
            ✓ Złóż wniosek
          </button>
        </div>
      </div>
    </div>

    <!-- Tab Content: My Requests -->
    <div v-if="activeTab === 'my-requests'" class="space-y-6">
      <!-- Filters and Search -->
      <div class="rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- Search -->
          <div>
            <label class="block text-sm font-semibold text-gray-900 dark:text-white mb-2">Szukaj</label>
            <div class="relative">
              <input
                v-model="searchQuery"
                type="text"
                placeholder="Numer wniosku lub typ..."
                class="w-full rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 pl-10 pr-4 py-3 text-base text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
              >
              <svg class="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>

          <!-- Status Filter -->
          <div>
            <label class="block text-sm font-semibold text-gray-900 dark:text-white mb-2">Status</label>
            <select
              v-model="statusFilter"
              class="w-full rounded-lg border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-4 py-3 text-base text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
            >
              <option value="all">Wszystkie</option>
              <option value="draft">Szkice</option>
              <option value="in_review">W akceptacji</option>
              <option value="awaiting_survey">Wymaga ankiety</option>
              <option value="approved">Zatwierdzone</option>
              <option value="rejected">Odrzucone</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Requests List -->
      <div v-if="filteredRequests.length === 0" class="rounded-xl border-2 border-dashed border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50 p-12 text-center">
        <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <h3 class="mt-4 text-lg font-semibold text-gray-900 dark:text-white">Brak wniosków</h3>
        <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
          {{ searchQuery || statusFilter !== 'all' ? 'Nie znaleziono wniosków spełniających kryteria.' : 'Nie masz jeszcze żadnych wniosków.' }}
        </p>
      </div>

      <div v-else class="grid grid-cols-1 gap-4">
        <button
          v-for="request in filteredRequests"
          :key="request.id"
          class="rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6 text-left transition-all hover:border-blue-500 hover:shadow-lg"
          :class="{ 'border-red-300 dark:border-red-800': request.priority === 'pilne' }"
          @click="viewRequestDetails(request)"
        >
          <div class="flex items-start justify-between gap-4">
            <div class="flex-1 space-y-3">
              <!-- Header -->
              <div class="flex items-center gap-3 flex-wrap">
                <h3 class="text-lg font-bold text-gray-900 dark:text-white">
                  {{ request.requestNumber }}
                </h3>
                <span :class="getStatusBadgeClass(request.status)" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold">
                  {{ getStatusLabel(request.status) }}
                </span>
                <span v-if="request.priority === 'pilne'" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300">
                  🔴 Pilne
                </span>
              </div>

              <!-- Type and Date -->
              <div class="flex items-center gap-4 text-sm text-gray-600 dark:text-gray-400">
                <span class="flex items-center gap-1">
                  <span class="text-lg">{{ getRequestTypeById(request.requestTypeId)?.icon }}</span>
                  {{ request.requestTypeName }}
                </span>
                <span>•</span>
                <span>{{ formatDate(request.createdAt) }}</span>
              </div>

              <!-- Approval Progress -->
              <div v-if="request.approvals.length" class="flex items-center gap-2">
                <div v-for="approval in request.approvals" :key="approval.id" class="flex items-center gap-2">
                  <span :class="getStepBadgeClass(approval.status)" class="inline-flex items-center px-2 py-1 rounded text-xs font-medium">
                    {{ approval.approver.firstName }} {{ approval.approver.lastName }}
                  </span>
                  <svg v-if="approval.id < request.approvals.length" class="h-4 w-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                  </svg>
                </div>
              </div>
            </div>

            <!-- Arrow -->
            <svg class="h-6 w-6 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </div>
        </button>
      </div>
    </div>

    <!-- Request Details Modal -->
    <div
      v-if="selectedRequest"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      @click.self="closeRequestDetails"
    >
      <div class="max-w-4xl w-full max-h-[90vh] overflow-y-auto rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 shadow-2xl">
        <!-- Modal Header -->
        <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-8 py-6 flex items-center justify-between">
          <div>
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">{{ selectedRequest.requestNumber }}</h2>
            <div class="flex items-center gap-2 mt-2">
              <span :class="getStatusBadgeClass(selectedRequest.status)" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold">
                {{ getStatusLabel(selectedRequest.status) }}
              </span>
              <span v-if="selectedRequest.priority === 'pilne'" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300">
                🔴 Pilne
              </span>
            </div>
          </div>
          <button
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
            @click="closeRequestDetails"
          >
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Modal Content -->
        <div class="px-8 py-6 space-y-6">
          <!-- Request Type -->
          <div>
            <h3 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">Typ wniosku</h3>
            <div class="flex items-center gap-3">
              <span class="text-3xl">{{ getRequestTypeById(selectedRequest.requestTypeId)?.icon }}</span>
              <div>
                <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ selectedRequest.requestTypeName }}</p>
                <p class="text-sm text-gray-600 dark:text-gray-400">Utworzono: {{ formatDate(selectedRequest.createdAt) }}</p>
              </div>
            </div>
          </div>

          <!-- Form Data -->
          <div class="border-t border-gray-200 dark:border-gray-700 pt-6">
            <h3 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-4">Szczegóły wniosku</h3>
            <dl class="grid grid-cols-1 gap-4">
              <div v-for="(value, key) in selectedRequest.formData" :key="key">
                <dt class="text-sm font-medium text-gray-500 dark:text-gray-400 capitalize">
                  {{ String(key).replace(/-/g, ' ') }}
                </dt>
                <dd class="mt-1 text-base text-gray-900 dark:text-white">
                  {{ value || '—' }}
                </dd>
              </div>
            </dl>
          </div>

          <!-- Approval Path -->
          <div v-if="selectedRequest.approvals.length" class="border-t border-gray-200 dark:border-gray-700 pt-6">
            <h3 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-4">Ścieżka akceptacji</h3>
            <div class="space-y-4">
              <div
                v-for="approval in selectedRequest.approvals"
                :key="approval.id"
                class="flex items-start gap-4 p-4 rounded-lg border-2"
                :class="[
                  approval.status === 'approved' ? 'border-green-200 dark:border-green-800 bg-green-50 dark:bg-green-900/20' :
                  approval.status === 'in_review' ? 'border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-900/20' :
                  approval.status === 'requires_survey' ? 'border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-900/20' :
                  approval.status === 'survey_failed' ? 'border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/20' :
                  'border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/20'
                ]"
              >
                <div
class="flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center font-bold text-lg"
                  :class="[
                    approval.status === 'approved' ? 'bg-green-100 dark:bg-green-900 text-green-600 dark:text-green-300' :
                    approval.status === 'in_review' ? 'bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-300' :
                    approval.status === 'requires_survey' ? 'bg-amber-100 dark:bg-amber-900 text-amber-600 dark:text-amber-300' :
                    approval.status === 'survey_failed' ? 'bg-red-100 dark:bg-red-900 text-red-600 dark:text-red-300' :
                    'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300'
                  ]"
                >
                  {{ approval.id }}
                </div>
                <div class="flex-1">
                  <div class="flex items-center justify-between">
                    <div>
                      <p class="font-semibold text-gray-900 dark:text-white">
                        {{ approval.approver.firstName }} {{ approval.approver.lastName }}
                      </p>
                      <p class="text-sm text-gray-600 dark:text-gray-400">{{ approval.approver.position }}</p>
                    </div>
                    <span :class="getStepBadgeClass(approval.status)" class="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold">
                      {{
                        approval.status === 'approved' ? '✓ Zatwierdzony' :
                        approval.status === 'in_review' ? '⏳ W trakcie' :
                        approval.status === 'requires_survey' ? '📝 Wymaga ankiety' :
                        approval.status === 'survey_failed' ? '❌ Test niezaliczony' :
                        '⏸ Oczekuje'
                      }}
                    </span>
                  </div>
                  <div v-if="approval.startedAt" class="mt-2 text-xs text-gray-500 dark:text-gray-400">
                    Rozpoczęto: {{ formatDate(approval.startedAt) }}
                  </div>
                  <div v-if="approval.finishedAt" class="text-xs text-gray-500 dark:text-gray-400">
                    Zakończono: {{ formatDate(approval.finishedAt) }}
                  </div>
                  <div v-if="approval.comment" class="mt-2 text-sm text-gray-700 dark:text-gray-300 italic">
                    "{{ approval.comment }}"
                  </div>

                  <!-- Survey score display -->
                  <div v-if="approval.surveyScore !== undefined" class="mt-3 p-3 rounded-lg bg-gray-100 dark:bg-gray-800">
                    <div class="flex items-center justify-between">
                      <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Wynik testu:</span>
                      <span
:class="[
                        'text-sm font-bold',
                        approval.surveyPassed ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
                      ]">
                        {{ approval.surveyScore.toFixed(0) }}%
                      </span>
                    </div>
                  </div>

                  <!-- Survey button -->
                  <button
                    v-if="approval.status === 'requires_survey' && approval.requiresSurvey"
                    class="mt-3 px-4 py-2 rounded-lg bg-amber-600 text-white text-sm font-semibold hover:bg-amber-700 transition-colors flex items-center gap-2"
                    @click="openSurvey(selectedRequest, selectedRequest.approvals.indexOf(approval))"
                  >
                    <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    📝 Wypełnij ankietę
                  </button>

                  <!-- Demo: Advance button -->
                  <button
                    v-if="approval.status === 'in_review'"
                    class="mt-3 px-4 py-2 rounded-lg bg-green-600 text-white text-sm font-semibold hover:bg-green-700 transition-colors"
                    @click="advanceRequest(selectedRequest.id)"
                  >
                    ✓ Zatwierdź (Demo)
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 border-t border-gray-200 dark:border-gray-700 px-8 py-4 flex justify-end">
          <button
            class="px-6 py-2 rounded-lg border-2 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 font-semibold hover:bg-white dark:hover:bg-gray-800 transition-colors"
            @click="closeRequestDetails"
          >
            Zamknij
          </button>
        </div>
      </div>
    </div>

    <!-- Survey Modal -->
    <div
      v-if="showSurveyModal && surveyRequest && currentSurveyStep"
      class="fixed inset-0 z-50 overflow-y-auto bg-black/50 backdrop-blur-sm flex items-center justify-center p-4"
      @click.self="closeSurvey"
    >
      <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl max-w-3xl w-full max-h-[90vh] overflow-hidden flex flex-col">
        <!-- Survey Header -->
        <div class="sticky top-0 bg-gradient-to-r from-amber-500 to-orange-500 px-8 py-6 flex items-center justify-between">
          <div>
            <h2 class="text-2xl font-bold text-white">Test wiedzy</h2>
            <p class="text-amber-100 mt-1">
              Wniosek: {{ surveyRequest.requestNumber }} - {{ surveyRequest.requestTypeName }}
            </p>
            <p class="text-amber-100 text-sm mt-1">
              Wymagane minimum: 80% poprawnych odpowiedzi
            </p>
          </div>
          <button
            class="text-white hover:text-amber-100 transition-colors"
            @click="closeSurvey"
          >
            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Survey Content -->
        <div class="px-8 py-6 space-y-6 overflow-y-auto flex-1">
          <div
            v-for="(question, qIndex) in currentSurveyStep.surveyQuestions"
            :key="question.id"
            class="p-6 rounded-xl border-2 border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50"
          >
            <div class="flex items-start gap-3 mb-4">
              <span class="flex-shrink-0 w-8 h-8 rounded-full bg-amber-100 dark:bg-amber-900 text-amber-600 dark:text-amber-300 flex items-center justify-center font-bold text-sm">
                {{ qIndex + 1 }}
              </span>
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white flex-1">
                {{ question.question }}
              </h3>
            </div>

            <div class="space-y-3 ml-11">
              <label
                v-for="option in question.options"
                :key="option.value"
                class="flex items-start gap-3 p-4 rounded-lg border-2 cursor-pointer transition-all hover:border-amber-500 hover:bg-amber-50 dark:hover:bg-amber-900/20"
                :class="[
                  surveyAnswers.find(a => a.questionId === question.id)?.selectedAnswer === option.value
                    ? 'border-amber-500 bg-amber-50 dark:bg-amber-900/20'
                    : 'border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800'
                ]"
              >
                <input
                  type="radio"
                  :name="`question-${question.id}`"
                  :value="option.value"
                  :checked="surveyAnswers.find(a => a.questionId === question.id)?.selectedAnswer === option.value"
                  class="mt-1 h-4 w-4 text-amber-600 focus:ring-amber-500"
                  @change="() => {
                    const existingIndex = surveyAnswers.findIndex(a => a.questionId === question.id)
                    if (existingIndex >= 0 && surveyAnswers[existingIndex]) {
                      surveyAnswers[existingIndex].selectedAnswer = option.value
                    } else {
                      surveyAnswers.push({ questionId: question.id, selectedAnswer: option.value })
                    }
                  }"
                >
                <span class="text-sm text-gray-900 dark:text-white flex-1">
                  {{ option.label }}
                </span>
              </label>
            </div>
          </div>
        </div>

        <!-- Survey Footer -->
        <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 border-t border-gray-200 dark:border-gray-700 px-8 py-4 flex items-center justify-between">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Odpowiedzi: {{ surveyAnswers.length }} / {{ currentSurveyStep.surveyQuestions?.length || 0 }}
          </p>
          <div class="flex gap-3">
            <button
              class="px-6 py-2 rounded-lg border-2 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 font-semibold hover:bg-white dark:hover:bg-gray-800 transition-colors"
              @click="closeSurvey"
            >
              Anuluj
            </button>
            <button
              :disabled="surveyAnswers.length < (currentSurveyStep.surveyQuestions?.length || 0)"
              :class="[
                'px-6 py-2 rounded-lg font-semibold transition-colors',
                surveyAnswers.length < (currentSurveyStep.surveyQuestions?.length || 0)
                  ? 'bg-gray-300 dark:bg-gray-700 text-gray-500 dark:text-gray-400 cursor-not-allowed'
                  : 'bg-amber-600 hover:bg-amber-700 text-white'
              ]"
              @click="submitSurvey"
            >
              Wyślij odpowiedzi
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
