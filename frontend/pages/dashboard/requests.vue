<script setup lang="ts">
import { computed, reactive, ref, watchEffect } from 'vue'
import type { Employee } from '~/types'
import { useAuthStore } from '~/stores/auth'

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

type RequestPriority = 'standard' | 'pilne'
type StepStatus = 'pending' | 'in_review' | 'approved'
type LicenseDuration = 'perpetual' | 'annual' | 'monthly'

interface HardwareItem {
  id: number
  name: string
  quantity: number
  justification: string
}

interface SoftwareItem {
  id: number
  name: string
  seats: number
  justification: string
  duration: LicenseDuration
}

interface ApprovalStep {
  id: number
  approver: Employee
  status: StepStatus
  startedAt?: Date
  finishedAt?: Date
}

interface RequestRecord {
  id: number
  requestNumber: string
  createdAt: Date
  priority: RequestPriority
  justification: string
  hardwareItems: HardwareItem[]
  softwareItems: SoftwareItem[]
  approvals: ApprovalStep[]
  status: 'in_review' | 'approved'
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

const licenseOptions: Array<{ value: LicenseDuration; label: string }> = [
  { value: 'annual', label: 'Roczna' },
  { value: 'perpetual', label: 'Bezterminowa' },
  { value: 'monthly', label: 'Miesięczna' }
]

const priorityOptions: Array<{ value: RequestPriority; label: string }> = [
  { value: 'standard', label: 'Standardowy' },
  { value: 'pilne', label: 'Pilne' }
]

let hardwareSeed = 0
let softwareSeed = 0
let requestSeed = 1000

const makeHardware = (): HardwareItem => ({
  id: ++hardwareSeed,
  name: '',
  quantity: 1,
  justification: ''
})

const makeSoftware = (): SoftwareItem => ({
  id: ++softwareSeed,
  name: '',
  seats: 1,
  justification: '',
  duration: 'annual'
})

const form = reactive({
  includeHardware: true,
  includeSoftware: false,
  hardwareItems: [makeHardware()],
  softwareItems: [makeSoftware()],
  justification: '',
  priority: 'standard' as RequestPriority,
  escalate: true
})

watchEffect(() => {
  if (!secondaryApprover.value) {
    form.escalate = false
  }
})

const errorMessage = ref<string | null>(null)
const infoMessage = ref<string | null>(null)

const requests = ref<RequestRecord[]>([])
const seeded = ref(false)

const previewSteps = computed(() => {
  const steps: Array<{ id: number; approver: Employee }> = []
  if (primaryApprover.value) {
    steps.push({ id: 1, approver: primaryApprover.value })
  }
  if (form.escalate && secondaryApprover.value) {
    steps.push({ id: steps.length + 1, approver: secondaryApprover.value })
  }
  return steps
})

const formatDate = (date: Date) =>
  new Intl.DateTimeFormat('pl-PL', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)

const statusClass = (status: RequestRecord['status']) =>
  status === 'approved'
    ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
    : 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200'

const stepClass = (status: StepStatus) => {
  if (status === 'approved') {
    return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
  }
  if (status === 'in_review') {
    return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
  }
  return 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300'
}

const addHardwareItem = () => {
  form.hardwareItems.push(makeHardware())
}

const removeHardwareItem = (index: number) => {
  form.hardwareItems.splice(index, 1)
  if (!form.hardwareItems.length) {
    form.hardwareItems.push(makeHardware())
  }
}

const addSoftwareItem = () => {
  form.softwareItems.push(makeSoftware())
}

const removeSoftwareItem = (index: number) => {
  form.softwareItems.splice(index, 1)
  if (!form.softwareItems.length) {
    form.softwareItems.push(makeSoftware())
  }
}

const resetForm = () => {
  hardwareSeed = 0
  softwareSeed = 0
  form.includeHardware = true
  form.includeSoftware = false
  form.hardwareItems.splice(0, form.hardwareItems.length, makeHardware())
  form.softwareItems.splice(0, form.softwareItems.length, makeSoftware())
  form.justification = ''
  form.priority = 'standard'
  form.escalate = Boolean(secondaryApprover.value)
}

const validateForm = () => {
  const hardwareValid = form.includeHardware && form.hardwareItems.some(item => item.name.trim())
  const softwareValid = form.includeSoftware && form.softwareItems.some(item => item.name.trim())
  if (!hardwareValid && !softwareValid) {
    return 'Dodaj co najmniej jedną pozycję sprzętową lub licencyjną.'
  }
  if (!form.justification.trim()) {
    return 'Uzupełnij uzasadnienie wniosku.'
  }
  return null
}

const buildApprovals = (): ApprovalStep[] => {
  const steps: ApprovalStep[] = []
  if (primaryApprover.value) {
    steps.push({
      id: 1,
      approver: primaryApprover.value,
      status: 'in_review',
      startedAt: new Date()
    })
  }
  if (form.escalate && secondaryApprover.value) {
    steps.push({
      id: steps.length + 1,
      approver: secondaryApprover.value,
      status: steps.length ? 'pending' : 'in_review'
    })
  }
  return steps
}

const submitRequest = () => {
  errorMessage.value = null
  infoMessage.value = null

  if (!currentUser.value) {
    errorMessage.value = 'Brak danych użytkownika.'
    return
  }

  const validation = validateForm()
  if (validation) {
    errorMessage.value = validation
    return
  }

  const hardwareItems = form.includeHardware
    ? form.hardwareItems
        .filter(item => item.name.trim())
        .map(item => ({
          id: item.id,
          name: item.name.trim(),
          quantity: item.quantity,
          justification: item.justification.trim()
        }))
    : []

  const softwareItems = form.includeSoftware
    ? form.softwareItems
        .filter(item => item.name.trim())
        .map(item => ({
          id: item.id,
          name: item.name.trim(),
          seats: item.seats,
          justification: item.justification.trim(),
          duration: item.duration
        }))
    : []

  const approvals = buildApprovals()

  const request: RequestRecord = {
    id: ++requestSeed,
    requestNumber: `ZAP-${new Date().getFullYear()}-${String(requestSeed).slice(-3)}`,
    createdAt: new Date(),
    priority: form.priority,
    justification: form.justification.trim(),
    hardwareItems,
    softwareItems,
    approvals,
    status: approvals.length ? 'in_review' : 'approved'
  }

  requests.value = [request, ...requests.value]

  const active = request.approvals.find(step => step.status === 'in_review')
  infoMessage.value = active
    ? `Wniosek przekazano do: ${active.approver.firstName} ${active.approver.lastName}.`
    : 'Wniosek został zatwierdzony automatycznie.'

  resetForm()
}

const currentOwner = (request: RequestRecord) => {
  const step = request.approvals.find(s => s.status === 'in_review')
  if (step) {
    return `${step.approver.firstName} ${step.approver.lastName}`
  }
  return request.status === 'approved' ? 'Proces zakończony' : 'Przetwarzanie automatyczne'
}

const advanceRequest = (id: number) => {
  const request = requests.value.find(r => r.id === id)
  if (!request) return
  const current = request.approvals.find(step => step.status === 'in_review')
  if (!current) return
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

watchEffect(() => {
  if (seeded.value || !currentUser.value) {
    return
  }

  const approvals: ApprovalStep[] = []
  if (primaryApprover.value) {
    approvals.push({
      id: 1,
      approver: primaryApprover.value,
      status: 'in_review',
      startedAt: new Date(Date.now() - 1000 * 60 * 60 * 6)
    })
  }
  if (secondaryApprover.value) {
    approvals.push({
      id: approvals.length + 1,
      approver: secondaryApprover.value,
      status: approvals.length ? 'pending' : 'in_review'
    })
  }

  const sample: RequestRecord = {
    id: ++requestSeed,
    requestNumber: 'ZAP-2025-001',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24),
    priority: 'standard',
    justification: 'Wyposażenie nowego członka zespołu backend.',
    hardwareItems: [
      { id: ++hardwareSeed, name: 'Laptop Lenovo ThinkPad', quantity: 1, justification: 'Standard zespołu.' }
    ],
    softwareItems: [
      { id: ++softwareSeed, name: 'JetBrains All Products Pack', seats: 1, justification: 'IDE dla backendu.', duration: 'annual' }
    ],
    approvals,
    status: approvals.length ? 'in_review' : 'approved'
  }

  requests.value = [sample]
  seeded.value = true
})
</script>

<template>
  <div class="space-y-6">
    <header class="space-y-1">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Wnioski</h1>
      <p class="text-sm text-gray-600 dark:text-gray-400">
        Złóż zapotrzebowanie na sprzęt lub oprogramowanie i śledź, u kogo jest obecnie akceptowane.
      </p>
    </header>

    <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">
      <section class="space-y-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white">Nowy wniosek</h2>

        <p v-if="errorMessage" class="rounded-md border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/30 px-3 py-2 text-sm text-red-700 dark:text-red-200">
          {{ errorMessage }}
        </p>
        <p v-if="infoMessage" class="rounded-md border border-green-200 dark:border-green-800 bg-green-50 dark:bg-green-900/30 px-3 py-2 text-sm text-green-700 dark:text-green-200">
          {{ infoMessage }}
        </p>

        <form class="space-y-4" @submit.prevent="submitRequest">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <label class="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-200">
              <input v-model="form.includeHardware" type="checkbox" class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
              Sprzęt
            </label>
            <label class="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-200">
              <input v-model="form.includeSoftware" type="checkbox" class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
              Oprogramowanie
            </label>
          </div>

          <div v-if="form.includeHardware" class="space-y-3">
            <div
              v-for="(item, index) in form.hardwareItems"
              :key="item.id"
              class="space-y-2 rounded-md border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 p-3"
            >
              <div class="flex flex-col gap-2 sm:flex-row sm:items-center">
                <input
                  v-model="item.name"
                  type="text"
                  placeholder="Nazwa sprzętu"
                  class="flex-1 rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100 focus:border-blue-500 focus:ring-blue-500"
                />
                <input
                  v-model.number="item.quantity"
                  type="number"
                  min="1"
                  class="w-24 rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100 focus:border-blue-500 focus:ring-blue-500"
                />
                <button
                  type="button"
                  class="rounded-md border border-red-200 dark:border-red-800 px-2 py-1 text-xs font-medium text-red-600 dark:text-red-300 hover:bg-red-50 dark:hover:bg-red-900/30"
                  @click="removeHardwareItem(index)"
                >
                  Usuń
                </button>
              </div>
              <textarea
                v-model="item.justification"
                rows="2"
                placeholder="Uzasadnienie"
                class="w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100 focus:border-blue-500 focus:ring-blue-500"
              />
            </div>

            <button
              type="button"
              class="rounded-md border border-blue-200 dark:border-blue-800 px-3 py-1.5 text-xs font-medium text-blue-600 dark:text-blue-300 hover:bg-blue-50 dark:hover:bg-blue-900/30"
              @click="addHardwareItem"
            >
              Dodaj kolejną pozycję
            </button>
          </div>

          <div v-if="form.includeSoftware" class="space-y-3">
            <div
              v-for="(item, index) in form.softwareItems"
              :key="item.id"
              class="space-y-2 rounded-md border border-purple-200 dark:border-purple-800 bg-purple-50 dark:bg-purple-900/20 p-3"
            >
              <div class="grid gap-2 sm:grid-cols-[1fr_auto_auto] sm:items-center">
                <input
                  v-model="item.name"
                  type="text"
                  placeholder="Nazwa licencji"
                  class="rounded-md border-purple-200 dark:border-purple-700 dark:bg-purple-950/40 dark:text-purple-100 focus:border-purple-500 focus:ring-purple-500"
                />
                <input
                  v-model.number="item.seats"
                  type="number"
                  min="1"
                  class="rounded-md border-purple-200 dark:border-purple-700 dark:bg-purple-950/40 dark:text-purple-100 focus:border-purple-500 focus:ring-purple-500"
                />
                <select
                  v-model="item.duration"
                  class="rounded-md border-purple-200 dark:border-purple-700 dark:bg-purple-950/40 dark:text-purple-100 focus:border-purple-500 focus:ring-purple-500"
                >
                  <option v-for="option in licenseOptions" :key="option.value" :value="option.value">
                    {{ option.label }}
                  </option>
                </select>
              </div>
              <textarea
                v-model="item.justification"
                rows="2"
                placeholder="Uzasadnienie"
                class="w-full rounded-md border-purple-200 dark:border-purple-700 dark:bg-purple-950/40 dark:text-purple-100 focus:border-purple-500 focus:ring-purple-500"
              />
              <button
                type="button"
                class="rounded-md border border-purple-200 dark:border-purple-700 px-2 py-1 text-xs font-medium text-purple-700 dark:text-purple-200 hover:bg-purple-100/60 dark:hover:bg-purple-900/30"
                @click="removeSoftwareItem(index)"
              >
                Usuń
              </button>
            </div>

            <button
              type="button"
              class="rounded-md border border-purple-200 dark:border-purple-800 px-3 py-1.5 text-xs font-medium text-purple-700 dark:text-purple-200 hover:bg-purple-100/60 dark:hover:bg-purple-900/30"
              @click="addSoftwareItem"
            >
              Dodaj licencję
            </button>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-200">Uzasadnienie</label>
            <textarea
              v-model="form.justification"
              rows="3"
              placeholder="Cel zamówienia, wymagany termin, projekt."
              class="mt-1 w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100 focus:border-blue-500 focus:ring-blue-500"
            />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-200">Priorytet</label>
              <select
                v-model="form.priority"
                class="mt-1 w-full rounded-md border-gray-300 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100 focus:border-blue-500 focus:ring-blue-500"
              >
                <option v-for="option in priorityOptions" :key="option.value" :value="option.value">
                  {{ option.label }}
                </option>
              </select>
            </div>
            <div class="flex items-center gap-2 pt-6 sm:pt-0">
              <input
                v-model="form.escalate"
                type="checkbox"
                :disabled="!secondaryApprover"
                class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 disabled:cursor-not-allowed"
              />
              <span class="text-sm text-gray-700 dark:text-gray-200">
                Eskaluj do kolejnego poziomu
                <span v-if="secondaryApprover" class="block text-xs text-gray-500 dark:text-gray-400">
                  {{ secondaryApprover.firstName }} {{ secondaryApprover.lastName }}
                </span>
              </span>
            </div>
          </div>

          <div class="flex justify-end gap-3 pt-2">
            <button
              type="button"
              class="rounded-md border border-gray-300 dark:border-gray-600 bg-white dark:bg-transparent px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800"
              @click="resetForm"
            >
              Wyczyść
            </button>
            <button
              type="submit"
              class="rounded-md bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
            >
              Wyślij do akceptacji
            </button>
          </div>
        </form>

        <div class="space-y-2">
          <h3 class="text-sm font-semibold text-gray-900 dark:text-white">Ścieżka akceptacji</h3>
          <div
            v-if="!previewSteps.length"
            class="rounded-md border border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-900/30 px-3 py-2 text-xs text-amber-700 dark:text-amber-200"
          >
            Brak przełożonych — po wysłaniu wniosek będzie od razu zatwierdzony.
          </div>
          <ul v-else class="space-y-1 text-sm text-gray-700 dark:text-gray-300">
            <li v-for="step in previewSteps" :key="step.id" class="flex items-center gap-2">
              <span class="inline-flex h-6 w-6 items-center justify-center rounded-full bg-blue-600 text-xs font-semibold text-white">
                {{ step.id }}
              </span>
              <span>
                {{ step.approver.firstName }} {{ step.approver.lastName }} • {{ step.approver.position?.name || 'Brak danych' }}
              </span>
            </li>
          </ul>
        </div>
      </section>

      <section class="space-y-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white">Wnioski w toku</h2>
          <p class="text-sm text-gray-600 dark:text-gray-400">Stan akceptacji i aktualny właściciel procesu.</p>
        </div>

        <div
          v-if="!requests.length"
          class="rounded-md border border-dashed border-gray-300 dark:border-gray-600 p-6 text-center text-sm text-gray-500 dark:text-gray-400"
        >
          Brak złożonych wniosków.
        </div>

        <div
          v-for="request in requests"
          :key="request.id"
          class="space-y-3 rounded-md border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 p-4"
        >
          <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ request.requestNumber }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400">Złożono: {{ formatDate(request.createdAt) }}</p>
            </div>
            <span class="inline-flex rounded-full px-3 py-1 text-xs font-semibold" :class="statusClass(request.status)">
              {{ request.status === 'approved' ? 'Zatwierdzony' : 'W akceptacji' }}
            </span>
          </div>

          <p class="text-sm text-gray-700 dark:text-gray-300">
            Aktualny etap: {{ currentOwner(request) }}
          </p>

          <ul class="space-y-1 text-sm text-gray-700 dark:text-gray-300">
            <li
              v-for="step in request.approvals"
              :key="step.id"
              class="flex items-center justify-between rounded-md border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2"
            >
              <span>{{ step.approver.firstName }} {{ step.approver.lastName }}</span>
              <span class="rounded-full px-3 py-1 text-xs font-semibold" :class="stepClass(step.status)">
                {{ step.status === 'approved' ? 'OK' : step.status === 'in_review' ? 'W akceptacji' : 'Oczekuje' }}
              </span>
            </li>
          </ul>

          <button
            v-if="request.status !== 'approved' && request.approvals.some(step => step.status === 'in_review')"
            type="button"
            class="w-full rounded-md border border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-900/30 px-3 py-2 text-sm font-medium text-blue-700 dark:text-blue-200 hover:bg-blue-100 dark:hover:bg-blue-900/50"
            @click="advanceRequest(request.id)"
          >
            Symuluj akceptację bieżącego kroku
          </button>
        </div>
      </section>
    </div>
  </div>
</template>
