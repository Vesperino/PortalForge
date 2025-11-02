<script setup lang="ts">
import type { Employee } from '~/types'
import type { DepartmentTreeDto } from '~/types/department'
import type { OrganizationChartNode } from 'primevue/organizationchart'
import PositionAutocomplete from '~/components/common/PositionAutocomplete.vue'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const config = useRuntimeConfig()
const apiUrl = config.public.apiUrl

const authStore = useAuthStore()

const getAuthHeaders = (): Record<string, string> | undefined => {
  const token = authStore.accessToken
  if (token) {
    return { Authorization: `Bearer ${token}` }
  }
  return undefined
}

const viewMode = ref<'tree' | 'departments' | 'list'>('departments')
const selectedDepartment = ref<string | null>(null)
const searchQuery = ref<string>('')

const departments = ref<DepartmentTreeDto[]>([])
const allEmployees = ref<any[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

const selectedEmployee = ref<Employee | null>(null)
const showEmployeeModal = ref(false)
const selectedDepartmentNode = ref<DepartmentTreeDto | null>(null)
const showDepartmentModal = ref(false)

// Load data on mount - removed, will be in pan & zoom onMounted

// Load all data
const loadData = async () => {
  isLoading.value = true
  error.value = null

  try {
    await Promise.all([
      loadDepartments(),
      loadAllUsers()
    ])
  } catch (err: any) {
    error.value = err.message || 'Nie udało się pobrać danych'
    console.error('Error loading data:', err)
  } finally {
    isLoading.value = false
  }
}

// Load departments
const loadDepartments = async () => {
  try {
    const response = await $fetch<DepartmentTreeDto[]>(`${apiUrl}/api/departments/tree`, {
      headers: getAuthHeaders()
    })
    departments.value = response
  } catch (err: any) {
    console.error('Error loading departments:', err)
  }
}

// Load all users
const loadAllUsers = async () => {
  try {
    const response = await $fetch<any>(`${apiUrl}/api/admin/users`, {
      headers: getAuthHeaders()
    })
    allEmployees.value = response.users || []
  } catch (err: any) {
    console.error('Error loading users:', err)
  }
}

const filteredEmployees = computed(() => {
  let filtered = allEmployees.value

  if (selectedDepartment.value) {
    filtered = filtered.filter((e: any) => e.departmentId === selectedDepartment.value)
  }

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter((e: any) =>
      e.firstName?.toLowerCase().includes(query) ||
      e.lastName?.toLowerCase().includes(query) ||
      e.email?.toLowerCase().includes(query) ||
      e.position?.toLowerCase().includes(query) ||
      e.department?.toLowerCase().includes(query)
    )
  }

  return filtered
})

const selectEmployee = (employee: Employee) => {
  selectedEmployee.value = employee
  showEmployeeModal.value = true
}

const closeEmployeeModal = () => {
  showEmployeeModal.value = false
  // Nie czyścimy selectedEmployee, żeby można było ponownie otworzyć
}

const getInitials = (employee: Employee) => {
  return `${employee.firstName?.[0] || ''}${employee.lastName?.[0] || ''}`
}

const getEmployeesByDepartment = (departmentId: string) => {
  return allEmployees.value.filter((e: any) => e.departmentId === departmentId)
}

const getDepartmentEmployees = (dept: DepartmentTreeDto): any[] => {
  const employees = getEmployeesByDepartment(dept.id)
  // Recursively get employees from child departments
  if (dept.children && dept.children.length > 0) {
    dept.children.forEach(child => {
      employees.push(...getDepartmentEmployees(child))
    })
  }
  return employees
}

const getManagerByDepartment = (dept: DepartmentTreeDto): any | null => {
  if (!dept.departmentHeadId) return null
  return allEmployees.value.find((e: any) => e.id === dept.departmentHeadId) || null
}

const getAllDepartmentsFlat = (depts: DepartmentTreeDto[]): DepartmentTreeDto[] => {
  const result: DepartmentTreeDto[] = []
  depts.forEach(dept => {
    result.push(dept)
    if (dept.children && dept.children.length > 0) {
      result.push(...getAllDepartmentsFlat(dept.children))
    }
  })
  return result
}

const departmentsFlat = computed(() => getAllDepartmentsFlat(departments.value))

// Convert Department tree to PrimeVue OrganizationChart format
const departmentLookup = new Map<string, DepartmentTreeDto>()

const convertDepartmentToOrgChart = (dept: DepartmentTreeDto): OrganizationChartNode => {
  const nodeKey = `dept-${dept.id}`
  departmentLookup.set(nodeKey, dept)

  const manager = getManagerByDepartment(dept)
  const employees = dept.employees || []

  const node: OrganizationChartNode = {
    key: nodeKey,
    type: 'department',
    data: {
      id: dept.id,
      name: dept.name,
      description: dept.description,
      manager: manager ? `${manager.firstName} ${manager.lastName}` : 'Brak kierownika',
      employeeCount: employees.length,
      level: dept.level
    },
    children: []
  }

  // Add employees as children (excluding department head as separate node if they're in the department)
  employees.forEach(emp => {
    node.children?.push({
      key: `emp-${emp.id}`,
      type: 'employee',
      data: {
        id: emp.id,
        firstName: emp.firstName,
        lastName: emp.lastName,
        position: emp.position,
        email: emp.email,
        profilePhotoUrl: emp.profilePhotoUrl,
        isHead: dept.departmentHeadId === emp.id
      },
      children: []
    })
  })

  // Add child departments
  if (dept.children && dept.children.length > 0) {
    dept.children.forEach(child => {
      node.children?.push(convertDepartmentToOrgChart(child))
    })
  }

  return node
}

const departmentOrgChartData = computed(() => {
  if (departments.value.length === 0) return []
  departmentLookup.clear()
  return departments.value.map(dept => convertDepartmentToOrgChart(dept))
})

const handleDepartmentNodeClick = (event: any) => {
  const node = event.node || event
  const department = departmentLookup.get(node.key as string)
  if (department) {
    selectedDepartmentNode.value = department
    showDepartmentModal.value = true
  }
}

const handleEmployeeNodeClick = (node: any) => {
  // Find the employee in allEmployees array by ID
  const employee = allEmployees.value.find((e: any) => e.id === node.data.id)
  if (employee) {
    selectEmployee(employee)
  }
}

// Pan & Zoom functionality
const zoom = ref(1)
const panX = ref(0)
const panY = ref(0)
const wrapperRef = ref<HTMLElement | null>(null)
const containerRef = ref<HTMLElement | null>(null)
const isDragging = ref(false)
const hasDragged = ref(false)
const dragStart = ref({ x: 0, y: 0 })
const isSpacePressed = ref(false)

// Handle keyboard events for space key
const handleKeyDown = (e: KeyboardEvent) => {
  if (e.code === 'Space' && !isSpacePressed.value) {
    isSpacePressed.value = true
    e.preventDefault()
  }
}

const handleKeyUp = (e: KeyboardEvent) => {
  if (e.code === 'Space') {
    isSpacePressed.value = false
    isDragging.value = false
  }
}

// Fit the chart content to wrapper width/height and center it
const fitToWidth = () => {
  const wrapper = wrapperRef.value
  const container = containerRef.value
  if (!wrapper || !container) return

  // Natural content size (unscaled)
  const contentWidth = container.scrollWidth || container.offsetWidth
  const contentHeight = container.scrollHeight || container.offsetHeight
  const wrapperWidth = wrapper.clientWidth
  const wrapperHeight = wrapper.clientHeight

  if (!contentWidth || !wrapperWidth) return

  // Calculate scale to fit width and height, with small padding
  const widthScale = wrapperWidth / contentWidth
  const heightScale = contentHeight ? (wrapperHeight / contentHeight) : 1
  const targetScale = Math.min(widthScale, heightScale)
  const newZoom = Math.min(3, Math.max(0.3, targetScale * 0.98))

  zoom.value = newZoom

  // Center horizontally, and vertically if there's room
  const scaledWidth = contentWidth * newZoom
  const scaledHeight = contentHeight * newZoom
  panX.value = (wrapperWidth - scaledWidth) / 2
  panY.value = Math.max(20, (wrapperHeight - scaledHeight) / 2)
}

// Add/remove keyboard listeners
onMounted(async () => {
  // Load data first
  await loadData()

  // Add keyboard listeners
  window.addEventListener('keydown', handleKeyDown)
  window.addEventListener('keyup', handleKeyUp)

  // Fit to available width after content renders
  // Use a short timeout to ensure PrimeVue chart tables have mounted
  setTimeout(() => {
    fitToWidth()
  }, 100)

  // Also refit on window resize
  window.addEventListener('resize', fitToWidth)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown)
  window.removeEventListener('keyup', handleKeyUp)
  window.removeEventListener('resize', fitToWidth)
})

const handleWheel = (e: WheelEvent) => {
  e.preventDefault()

  // Get mouse position relative to wrapper
  const wrapper = e.currentTarget as HTMLElement
  const rect = wrapper.getBoundingClientRect()
  const mouseX = e.clientX - rect.left
  const mouseY = e.clientY - rect.top

  // Calculate new zoom
  const delta = e.deltaY * -0.001
  const oldZoom = zoom.value
  const newZoom = Math.min(Math.max(0.3, oldZoom + delta), 3)

  // Calculate the point in the content that's under the mouse
  const pointX = (mouseX - panX.value) / oldZoom
  const pointY = (mouseY - panY.value) / oldZoom

  // Calculate new pan to keep the point under the mouse
  panX.value = mouseX - pointX * newZoom
  panY.value = mouseY - pointY * newZoom

  zoom.value = newZoom

  // Force repaint to fix rendering quality immediately
  if (containerRef.value) {
    containerRef.value.style.transform = `translate3d(${panX.value}px, ${panY.value}px, 0) scale(${newZoom})`
  }
}

const handleMouseDown = (e: MouseEvent) => {
  // Only start dragging on left mouse button
  if (e.button !== 0) return

  // Check if clicking on a node (skip dragging if on interactive element)
  const target = e.target as HTMLElement
  if (!isSpacePressed.value && (target.closest('.department-node') || target.closest('.employee-node'))) {
    return
  }

  isDragging.value = true
  hasDragged.value = false
  dragStart.value = {
    x: e.clientX - panX.value,
    y: e.clientY - panY.value
  }
}

const handleMouseMove = (e: MouseEvent) => {
  if (!isDragging.value) return

  // Update pan position
  const newPanX = e.clientX - dragStart.value.x
  const newPanY = e.clientY - dragStart.value.y

  // Check if actually moved (to distinguish click from drag)
  if (Math.abs(newPanX - panX.value) > 2 || Math.abs(newPanY - panY.value) > 2) {
    hasDragged.value = true
  }

  panX.value = newPanX
  panY.value = newPanY
}

const handleMouseUp = () => {
  isDragging.value = false
}

const handleMouseLeave = () => {
  isDragging.value = false
}

// Reset zoom and pan to fit
const resetView = () => {
  fitToWidth()
}

// Computed style for transform with GPU acceleration
const transformStyle = computed(() => ({
  transform: `translate3d(${panX.value}px, ${panY.value}px, 0) scale(${zoom.value})`
}))

// Computed style for wrapper cursor
const wrapperCursor = computed(() => {
  if (isDragging.value) return 'grabbing'
  if (isSpacePressed.value) return 'grab'
  return 'default'
})

// Refit when data changes significantly
watch(departmentOrgChartData, () => {
  // Next tick to wait for DOM of charts
  nextTick(() => setTimeout(fitToWidth, 0))
})

// Force repaint when zoom changes to fix rendering quality
watch(zoom, (newZoom) => {
  if (containerRef.value) {
    // Use requestAnimationFrame to ensure repaint happens
    requestAnimationFrame(() => {
      if (containerRef.value) {
        // Force browser to recalculate and re-render with GPU acceleration
        containerRef.value.style.transform = `translate3d(${panX.value}px, ${panY.value}px, 0) scale(${newZoom})`
        // Trigger reflow to force immediate repaint
        void containerRef.value.offsetHeight
      }
    })
  }
})

</script>

<template>
  <div class="space-y-4 overflow-x-hidden max-w-full">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
          Struktura organizacyjna
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
          {{ allEmployees.length }} pracowników w {{ departments.length }} działach
        </p>
      </div>
    </div>

    <!-- View Mode Tabs & Search -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4 max-w-full overflow-hidden">
      <div class="flex flex-col gap-4">
        <!-- Tabs -->
        <div class="flex flex-wrap gap-2">
          <button
            :class="[
              'px-4 py-2 rounded-lg font-medium transition-colors text-sm',
              viewMode === 'tree'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
            ]"
            @click="viewMode = 'tree'"
          >
            <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
            </svg>
            Drzewo organizacyjne
          </button>
          <button
            :class="[
              'px-4 py-2 rounded-lg font-medium transition-colors text-sm',
              viewMode === 'departments'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
            ]"
            @click="viewMode = 'departments'"
          >
            <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
            Według działów
          </button>
          <button
            :class="[
              'px-4 py-2 rounded-lg font-medium transition-colors text-sm',
              viewMode === 'list'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
            ]"
            @click="viewMode = 'list'"
          >
            <svg class="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16" />
            </svg>
            Lista pracowników
          </button>
        </div>

        <!-- Search & Filters -->
        <div v-if="viewMode === 'list'" class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label for="search" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Szukaj pracownika
            </label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
              <input
                id="search"
                v-model="searchQuery"
                type="text"
                placeholder="Imię, nazwisko, email..."
                class="pl-10 w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              >
            </div>
          </div>

          <div>
            <label for="department" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Dział
            </label>
            <select
              id="department"
              v-model="selectedDepartment"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option :value="null">
                Wszystkie działy
              </option>
              <option v-for="dept in departmentsFlat" :key="dept.id" :value="dept.id">
                {{ dept.name }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12">
      <div class="flex flex-col items-center justify-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie danych...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-6">
      <div class="flex items-center gap-3">
        <svg class="w-6 h-6 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div>
          <h3 class="text-lg font-semibold text-red-900 dark:text-red-200">Błąd ładowania danych</h3>
          <p class="text-sm text-red-700 dark:text-red-300">{{ error }}</p>
        </div>
      </div>
    </div>

    <!-- Main Content Area - Full Width -->
    <div v-else class="w-full max-w-full overflow-hidden">
        <!-- Departments View -->
        <div v-if="viewMode === 'departments'" class="space-y-4">
          <div
            v-for="dept in departmentsFlat"
            :key="dept.id"
            class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
            :class="{ 'ml-8': dept.level && dept.level > 0 }"
          >
            <div class="flex items-center justify-between mb-4">
              <div class="flex-1">
                <div class="flex items-center gap-2">
                  <span v-if="dept.level && dept.level > 0" class="text-gray-400 dark:text-gray-600">
                    {{ '└─'.repeat(dept.level) }}
                  </span>
                  <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
                    {{ dept.name }}
                  </h3>
                </div>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ dept.description }}
                </p>
              </div>
              <div class="text-right">
                <p class="text-2xl font-bold text-gray-900 dark:text-white">
                  {{ getEmployeesByDepartment(dept.id).length }}
                </p>
                <p class="text-xs text-gray-500 dark:text-gray-400">pracowników</p>
              </div>
            </div>

            <!-- Department Manager -->
            <div v-if="getManagerByDepartment(dept)" class="mb-4 p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
              <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Kierownik działu</p>
              <div
                class="flex items-center gap-3 cursor-pointer"
                @click="selectEmployee(getManagerByDepartment(dept))"
              >
                <div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold overflow-hidden">
                  <img
                    v-if="getManagerByDepartment(dept).profilePhotoUrl"
                    :src="getManagerByDepartment(dept).profilePhotoUrl"
                    :alt="`${getManagerByDepartment(dept).firstName} ${getManagerByDepartment(dept).lastName}`"
                    class="w-full h-full object-cover"
                  />
                  <span v-else>
                    {{ getInitials(getManagerByDepartment(dept)) }}
                  </span>
                </div>
                <div>
                  <p class="font-medium text-gray-900 dark:text-white">
                    {{ getManagerByDepartment(dept).firstName }} {{ getManagerByDepartment(dept).lastName }}
                  </p>
                  <p class="text-sm text-gray-600 dark:text-gray-400">
                    {{ getManagerByDepartment(dept).position }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Department Employees -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div
                v-for="employee in getEmployeesByDepartment(dept.id).filter(e => e.id !== dept.departmentHeadId)"
                :key="employee.id"
                class="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer transition-colors"
                @click="selectEmployee(employee)"
              >
                <div class="w-10 h-10 rounded-full bg-gray-300 dark:bg-gray-600 flex items-center justify-center text-gray-700 dark:text-gray-300 font-semibold text-sm overflow-hidden">
                  <img
                    v-if="employee.profilePhotoUrl"
                    :src="employee.profilePhotoUrl"
                    :alt="`${employee.firstName} ${employee.lastName}`"
                    class="w-full h-full object-cover"
                  />
                  <span v-else>
                    {{ getInitials(employee) }}
                  </span>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="font-medium text-gray-900 dark:text-white truncate text-sm">
                    {{ employee.firstName }} {{ employee.lastName }}
                  </p>
                  <p class="text-xs text-gray-600 dark:text-gray-400 truncate">
                    {{ employee.position }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- List View -->
        <div v-else-if="viewMode === 'list'" class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead class="bg-gray-50 dark:bg-gray-700">
                <tr>
                  <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Pracownik
                  </th>
                  <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Stanowisko
                  </th>
                  <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Dział
                  </th>
                  <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Email
                  </th>
                  <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                    Telefon
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                <tr
                  v-for="employee in filteredEmployees"
                  :key="employee.id"
                  class="hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer transition-colors"
                  @click="selectEmployee(employee)"
                >
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center gap-3">
                      <div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold text-sm overflow-hidden">
                        <img
                          v-if="employee.profilePhotoUrl"
                          :src="employee.profilePhotoUrl"
                          :alt="`${employee.firstName} ${employee.lastName}`"
                          class="w-full h-full object-cover"
                        />
                        <span v-else>
                          {{ getInitials(employee) }}
                        </span>
                      </div>
                      <div>
                        <p class="font-medium text-gray-900 dark:text-white">
                          {{ employee.firstName }} {{ employee.lastName }}
                        </p>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                    {{ employee.position }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span class="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
                      {{ employee.department }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                    {{ employee.email }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                    {{ employee.phoneNumber || '-' }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Empty State -->
          <div v-if="filteredEmployees.length === 0" class="text-center py-12">
            <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
              Brak pracowników
            </h3>
            <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
              Nie znaleziono pracowników spełniających kryteria wyszukiwania.
            </p>
          </div>
        </div>

        <!-- Tree View -->
        <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden max-w-full">
          <div v-if="departmentOrgChartData.length > 0" class="p-4 sm:p-6 max-w-full box-border">
            <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 mb-4">
              <div class="flex-shrink min-w-0">
                <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
                  Struktura organizacyjna - Drzewo działów
                </h2>
                <p class="text-xs sm:text-sm text-gray-600 dark:text-gray-400 mt-1 flex-wrap">
                  <span class="inline-flex items-center gap-1">
                    <svg class="w-3 h-3 sm:w-4 sm:h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 15l-2 5L9 9l11 4-5 2zm0 0l5 5M7.188 2.239l.777 2.897M5.136 7.965l-2.898-.777M13.95 4.05l-2.122 2.122m-5.657 5.656l-2.12 2.122" />
                    </svg>
                    Przeciągnij
                  </span>
                  <span class="mx-1 sm:mx-2">•</span>
                  <span class="inline-flex items-center gap-1">
                    <svg class="w-3 h-3 sm:w-4 sm:h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                    Scroll
                  </span>
                  <span class="mx-1 sm:mx-2">•</span>
                  <span class="inline-flex items-center gap-1">
                    <kbd class="px-1 py-0.5 text-xs font-semibold bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded">Space</kbd>
                    + drag
                  </span>
                </p>
              </div>

              <!-- Zoom Controls -->
              <div class="flex items-center gap-2 flex-shrink-0">
                <span class="text-sm text-gray-600 dark:text-gray-400 mr-2">
                  {{ Math.round(zoom * 100) }}%
                </span>
                <button
                  @click="zoom = Math.min(3, zoom + 0.2)"
                  class="p-2 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors"
                  title="Powiększ"
                >
                  <svg class="w-5 h-5 text-gray-700 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0zM10 7v6m3-3H7" />
                  </svg>
                </button>
                <button
                  @click="zoom = Math.max(0.3, zoom - 0.2)"
                  class="p-2 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors"
                  title="Pomniejsz"
                >
                  <svg class="w-5 h-5 text-gray-700 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0zM13 10H7" />
                  </svg>
                </button>
                <button
                  @click="resetView"
                  class="p-2 bg-blue-100 dark:bg-blue-900 hover:bg-blue-200 dark:hover:bg-blue-800 text-blue-700 dark:text-blue-300 rounded-lg transition-colors"
                  title="Resetuj widok"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                  </svg>
                </button>
              </div>
            </div>

            <div
              ref="wrapperRef"
              class="org-chart-wrapper"
              :style="{ cursor: wrapperCursor }"
              @wheel="handleWheel"
              @mousedown="handleMouseDown"
              @mousemove="handleMouseMove"
              @mouseup="handleMouseUp"
              @mouseleave="handleMouseLeave"
            >
              <div ref="containerRef" class="org-chart-container" :style="transformStyle">
              <OrganizationChart
                v-for="rootDept in departmentOrgChartData"
                :key="rootDept.key"
                :value="rootDept"
                collapsible
                selection-mode="single"
                @node-select="handleDepartmentNodeClick"
                class="mb-8"
              >
                <template #department="slotProps">
                  <div class="department-node">
                    <div class="department-name">
                      {{ slotProps.node.data.name }}
                    </div>
                    <div class="department-manager">
                      👤 {{ slotProps.node.data.manager }}
                    </div>
                    <div class="department-stats">
                      <span class="employee-badge">
                        👥 {{ slotProps.node.data.employeeCount }} pracowników
                      </span>
                    </div>
                  </div>
                </template>

                <template #employee="slotProps">
                  <div class="employee-node" @click="handleEmployeeNodeClick(slotProps.node)">
                    <div class="employee-avatar">
                      <img
                        v-if="slotProps.node.data.profilePhotoUrl"
                        :src="slotProps.node.data.profilePhotoUrl"
                        :alt="`${slotProps.node.data.firstName} ${slotProps.node.data.lastName}`"
                        class="employee-avatar-img"
                      />
                      <span v-else>
                        {{ slotProps.node.data.firstName[0] }}{{ slotProps.node.data.lastName[0] }}
                      </span>
                    </div>
                    <div class="employee-name">
                      {{ slotProps.node.data.firstName }} {{ slotProps.node.data.lastName }}
                    </div>
                    <div class="employee-position">
                      {{ slotProps.node.data.position || 'Brak stanowiska' }}
                    </div>
                    <div v-if="slotProps.node.data.isHead" class="employee-head-badge">
                      Kierownik
                    </div>
                  </div>
                </template>
              </OrganizationChart>
              </div>
            </div>
          </div>

          <div v-else class="text-center py-12">
            <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
            <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
              Brak działów
            </h3>
            <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
              Nie znaleziono działów w systemie
            </p>
          </div>
        </div>
      </div>


    <!-- Employee Details Modal -->
    <Teleport to="body">
      <div
        v-if="showEmployeeModal && selectedEmployee"
        class="fixed inset-0 z-50 overflow-y-auto"
        @click.self="closeEmployeeModal"
      >
        <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
          <!-- Background overlay -->
          <div class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75" @click="closeEmployeeModal" />

          <!-- Modal panel -->
          <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
            <!-- Header -->
            <div class="bg-gradient-to-r from-blue-500 to-blue-600 px-6 py-4">
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-semibold text-white">
                  Szczegóły pracownika
                </h3>
                <button
                  type="button"
                  class="text-white hover:text-gray-200 focus:outline-none"
                  @click="closeEmployeeModal"
                >
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            <!-- Content -->
            <div class="px-6 py-6 space-y-6">
              <!-- Avatar & Basic Info -->
              <div class="text-center">
                <div class="inline-flex w-24 h-24 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 items-center justify-center text-3xl font-bold text-white shadow-lg overflow-hidden">
                  <img
                    v-if="selectedEmployee.profilePhotoUrl"
                    :src="selectedEmployee.profilePhotoUrl"
                    :alt="`${selectedEmployee.firstName} ${selectedEmployee.lastName}`"
                    class="w-full h-full object-cover"
                  />
                  <span v-else>
                    {{ getInitials(selectedEmployee) }}
                  </span>
                </div>
                <h4 class="mt-4 text-2xl font-bold text-gray-900 dark:text-white">
                  {{ selectedEmployee.firstName }} {{ selectedEmployee.lastName }}
                </h4>
                <p class="mt-1 text-gray-600 dark:text-gray-400">
                  {{ selectedEmployee.position }}
                </p>
                <span
                  class="inline-block mt-3 px-4 py-1.5 text-sm font-medium rounded-full text-white bg-blue-500 shadow-sm"
                >
                  {{ selectedEmployee.department }}
                </span>
              </div>

              <!-- Contact Info -->
              <div class="space-y-4 pt-4 border-t border-gray-200 dark:border-gray-700">
                <div class="flex items-center gap-4">
                  <div class="flex-shrink-0 w-10 h-10 rounded-full bg-blue-100 dark:bg-blue-900 flex items-center justify-center">
                    <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Email</p>
                    <a
                      :href="`mailto:${selectedEmployee.email}`"
                      class="text-sm text-blue-600 dark:text-blue-400 hover:underline truncate block font-medium"
                    >
                      {{ selectedEmployee.email }}
                    </a>
                  </div>
                </div>

                <div v-if="selectedEmployee.phoneNumber" class="flex items-center gap-4">
                  <div class="flex-shrink-0 w-10 h-10 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
                    <svg class="w-5 h-5 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                    </svg>
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Telefon</p>
                    <a
                      :href="`tel:${selectedEmployee.phoneNumber}`"
                      class="text-sm text-green-600 dark:text-green-400 hover:underline font-medium"
                    >
                      {{ selectedEmployee.phoneNumber }}
                    </a>
                  </div>
                </div>
              </div>

              <!-- Supervisor -->
              <div v-if="selectedEmployee.supervisor" class="pt-4 border-t border-gray-200 dark:border-gray-700">
                <p class="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase mb-3">
                  Przełożony
                </p>
                <div
                  class="flex items-center gap-3 p-3 rounded-lg bg-gray-50 dark:bg-gray-700/50 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer transition-colors"
                  @click="selectEmployee(selectedEmployee.supervisor)"
                >
                  <div class="w-12 h-12 rounded-full bg-gradient-to-br from-gray-400 to-gray-500 flex items-center justify-center text-white font-semibold">
                    {{ getInitials(selectedEmployee.supervisor) }}
                  </div>
                  <div class="flex-1">
                    <p class="text-sm font-semibold text-gray-900 dark:text-white">
                      {{ selectedEmployee.supervisor.firstName }} {{ selectedEmployee.supervisor.lastName }}
                    </p>
                    <p class="text-xs text-gray-600 dark:text-gray-400">
                      {{ selectedEmployee.supervisor.position }}
                    </p>
                  </div>
                  <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                  </svg>
                </div>
              </div>

              <!-- Team -->
              <div v-if="selectedEmployee.subordinates && selectedEmployee.subordinates.length > 0" class="pt-4 border-t border-gray-200 dark:border-gray-700">
                <p class="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase mb-3">
                  Zarządza zespołem
                </p>
                <div class="flex items-center gap-2 mb-4">
                  <div class="text-3xl font-bold text-gray-900 dark:text-white">
                    {{ selectedEmployee.subordinates.length }}
                  </div>
                  <span class="text-sm text-gray-600 dark:text-gray-400">
                    {{ selectedEmployee.subordinates.length === 1 ? 'osoba' : selectedEmployee.subordinates.length < 5 ? 'osoby' : 'osób' }}
                  </span>
                </div>
                <div class="space-y-2 max-h-48 overflow-y-auto">
                  <div
                    v-for="sub in selectedEmployee.subordinates"
                    :key="sub.id"
                    class="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700/50 cursor-pointer transition-colors"
                    @click="selectEmployee(sub)"
                  >
                    <div class="w-10 h-10 rounded-full bg-gradient-to-br from-gray-300 to-gray-400 dark:from-gray-600 dark:to-gray-700 flex items-center justify-center text-gray-700 dark:text-gray-300 font-semibold text-sm">
                      {{ getInitials(sub) }}
                    </div>
                    <div class="flex-1">
                      <p class="text-sm font-medium text-gray-900 dark:text-white">
                        {{ sub.firstName }} {{ sub.lastName }}
                      </p>
                      <p class="text-xs text-gray-600 dark:text-gray-400">
                        {{ sub.position }}
                      </p>
                    </div>
                    <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </div>
              </div>
            </div>

            <!-- Footer -->
            <div class="bg-gray-50 dark:bg-gray-700/50 px-6 py-4">
              <button
                type="button"
                class="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
                @click="closeEmployeeModal"
              >
                Zamknij
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Department Details Modal -->
    <Teleport to="body">
      <div
        v-if="showDepartmentModal && selectedDepartmentNode"
        class="fixed inset-0 z-50 overflow-y-auto"
        @click.self="showDepartmentModal = false"
      >
        <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
          <!-- Background overlay -->
          <div class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75" @click="showDepartmentModal = false" />

          <!-- Modal panel -->
          <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full">
            <!-- Header -->
            <div class="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-semibold text-white">
                  Szczegóły działu
                </h3>
                <button
                  type="button"
                  class="text-white hover:text-gray-200 focus:outline-none"
                  @click="showDepartmentModal = false"
                >
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            <!-- Content -->
            <div class="px-6 py-6">
              <!-- Department Info -->
              <div class="mb-6">
                <h4 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                  {{ selectedDepartmentNode.name }}
                </h4>
                <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
                  {{ selectedDepartmentNode.description }}
                </p>

                <div class="grid grid-cols-2 gap-4">
                  <div class="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4">
                    <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Kierownik działu</p>
                    <p class="font-semibold text-gray-900 dark:text-white">
                      {{ getManagerByDepartment(selectedDepartmentNode)?.firstName || 'Brak' }}
                      {{ getManagerByDepartment(selectedDepartmentNode)?.lastName || 'kierownika' }}
                    </p>
                  </div>
                  <div class="bg-green-50 dark:bg-green-900/20 rounded-lg p-4">
                    <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Liczba pracowników</p>
                    <p class="font-semibold text-gray-900 dark:text-white">
                      {{ getEmployeesByDepartment(selectedDepartmentNode.id).length }} pracowników
                    </p>
                  </div>
                </div>
              </div>

              <!-- Employees List -->
              <div class="border-t border-gray-200 dark:border-gray-700 pt-4">
                <h5 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                  Pracownicy działu
                </h5>

                <div v-if="getEmployeesByDepartment(selectedDepartmentNode.id).length > 0" class="space-y-2 max-h-96 overflow-y-auto">
                  <div
                    v-for="employee in getEmployeesByDepartment(selectedDepartmentNode.id)"
                    :key="employee.id"
                    class="flex items-center gap-3 p-3 rounded-lg bg-gray-50 dark:bg-gray-700/50 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer transition-colors"
                    @click="selectEmployee(employee); showDepartmentModal = false"
                  >
                    <div class="w-12 h-12 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white font-semibold">
                      {{ getInitials(employee) }}
                    </div>
                    <div class="flex-1">
                      <p class="font-medium text-gray-900 dark:text-white">
                        {{ employee.firstName }} {{ employee.lastName }}
                        <span v-if="employee.id === selectedDepartmentNode.departmentHeadId" class="ml-2 text-xs bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 px-2 py-0.5 rounded-full">
                          Kierownik
                        </span>
                      </p>
                      <p class="text-sm text-gray-600 dark:text-gray-400">
                        {{ employee.position }}
                      </p>
                    </div>
                    <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </div>

                <div v-else class="text-center py-8">
                  <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                  </svg>
                  <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
                    Brak pracowników w tym dziale
                  </p>
                </div>
              </div>
            </div>

            <!-- Footer -->
            <div class="bg-gray-50 dark:bg-gray-700/50 px-6 py-4">
              <button
                type="button"
                class="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
                @click="showDepartmentModal = false"
              >
                Zamknij
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Quick Edit Modal -->
    <Teleport to="body">
      <div
        v-if="showQuickEditModal && quickEditEmployee"
        class="fixed inset-0 z-50 overflow-y-auto"
        @click.self="closeQuickEditModal"
      >
        <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
          <!-- Background overlay -->
          <div class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75" @click="closeQuickEditModal" />

          <!-- Modal panel -->
          <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
            <!-- Header -->
            <div class="bg-gradient-to-r from-blue-500 to-blue-600 px-6 py-4">
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-semibold text-white">
                  Szybka edycja
                </h3>
                <button
                  type="button"
                  class="text-white hover:text-gray-200 focus:outline-none"
                  @click="closeQuickEditModal"
                >
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            <!-- Content -->
            <div class="px-6 py-6">
              <!-- Employee Info -->
              <div class="mb-6 pb-4 border-b border-gray-200 dark:border-gray-700">
                <div class="flex items-center gap-3">
                  <div class="w-16 h-16 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white text-xl font-bold overflow-hidden">
                    <img
                      v-if="quickEditEmployee.profilePhotoUrl"
                      :src="quickEditEmployee.profilePhotoUrl"
                      :alt="`${quickEditEmployee.firstName} ${quickEditEmployee.lastName}`"
                      class="w-full h-full object-cover"
                    />
                    <span v-else>
                      {{ getInitials(quickEditEmployee) }}
                    </span>
                  </div>
                  <div>
                    <h4 class="text-xl font-bold text-gray-900 dark:text-white">
                      {{ quickEditEmployee.firstName }} {{ quickEditEmployee.lastName }}
                    </h4>
                    <p class="text-sm text-gray-600 dark:text-gray-400">
                      {{ quickEditEmployee.email }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Error Message -->
              <div v-if="quickEditError" class="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
                <p class="text-sm text-red-800 dark:text-red-200">{{ quickEditError }}</p>
              </div>

              <!-- Form -->
              <form @submit.prevent="saveQuickEdit" class="space-y-4">
                <!-- Department -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Dział <span class="text-red-500">*</span>
                  </label>
                  <select
                    v-model="quickEditForm.department"
                    required
                    class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                  >
                    <option value="">Wybierz dział</option>
                    <option
                      v-for="dept in departmentsFlat"
                      :key="dept.id"
                      :value="dept.name"
                    >
                      {{ dept.level > 0 ? '└─'.repeat(dept.level) + ' ' : '' }}{{ dept.name }}
                    </option>
                  </select>
                </div>

                <!-- Position -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Stanowisko <span class="text-red-500">*</span>
                  </label>
                  <PositionAutocomplete
                    :model-value="positionId"
                    @update:modelValue="handlePositionUpdate"
                    @update:positionName="handlePositionNameUpdate"
                    placeholder="Wpisz lub wybierz stanowisko..."
                    required
                  />
                </div>

                <!-- Actions -->
                <div class="flex justify-end gap-3 pt-4">
                  <button
                    type="button"
                    class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                    @click="closeQuickEditModal"
                    :disabled="quickEditLoading"
                  >
                    Anuluj
                  </button>
                  <button
                    type="submit"
                    class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50"
                    :disabled="quickEditLoading"
                  >
                    {{ quickEditLoading ? 'Zapisywanie...' : 'Zapisz' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
/* Organization Chart Wrapper - Pan & Zoom Container */
.org-chart-wrapper {
  width: 100%;
  max-width: 100%;
  height: calc(100vh - 450px); /* Full viewport height minus header and controls */
  min-height: 500px;
  max-height: 700px;
  overflow: hidden; /* No scrollbars */
  position: relative;
  margin: 0;
  background: linear-gradient(90deg, rgba(0,0,0,0.02) 1px, transparent 1px),
              linear-gradient(rgba(0,0,0,0.02) 1px, transparent 1px);
  background-size: 40px 40px;
  background-position: 0 0;
  user-select: none; /* Prevent text selection while dragging */
  border-radius: 8px;
  box-sizing: border-box;
}

/* Organization Chart Container */
.org-chart-container {
  width: max-content;
  height: max-content;
  min-width: 100%;
  min-height: 100%;
  padding: 20px;
  position: relative;
  transform-origin: 0 0;
  transition: none; /* Remove transition for smoother panning */
  will-change: transform;
  pointer-events: none; /* Disable pointer events on container, enable on children */
  box-sizing: border-box;

  /* Improve rendering quality at different scales */
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-rendering: optimizeLegibility;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
  transform-style: preserve-3d;
}

/* Department Node Styling */
.department-node {
  min-width: 220px;
  padding: 16px;
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
  border: 2px solid #1e40af;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(37, 99, 235, 0.2);
  cursor: pointer !important;
  transition: all 0.3s ease;
  text-align: center;
  color: white;
  user-select: none;
  pointer-events: auto;

  /* Sharp text rendering at all zoom levels */
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-rendering: optimizeLegibility;
}

.department-node:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(37, 99, 235, 0.3);
  background: linear-gradient(135deg, #1d4ed8 0%, #1e40af 100%);
}

/* Employee Node Styling */
.employee-node {
  min-width: 180px;
  padding: 14px;
  margin: 8px;
  background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%);
  border: 2px solid #374151;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(107, 114, 128, 0.2);
  cursor: pointer !important;
  transition: all 0.3s ease;
  text-align: center;
  color: white;
  user-select: none;
  pointer-events: auto;

  /* Sharp text rendering at all zoom levels */
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-rendering: optimizeLegibility;
}

.employee-node:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(107, 114, 128, 0.3);
  background: linear-gradient(135deg, #4b5563 0%, #374151 100%);
}

.employee-avatar {
  width: 48px;
  height: 48px;
  margin: 0 auto 8px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  font-weight: 700;
  border: 2px solid rgba(255, 255, 255, 0.5);
  overflow: hidden;
}

.employee-avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 50%;

  /* Crisp image rendering when scaled */
  image-rendering: -webkit-optimize-contrast;
  image-rendering: crisp-edges;
  -ms-interpolation-mode: nearest-neighbor;
}

.employee-name {
  font-size: 0.95rem;
  font-weight: 700;
  margin-bottom: 4px;
  line-height: 1.2;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.employee-position {
  font-size: 0.75rem;
  opacity: 0.9;
  font-weight: 500;
  margin-bottom: 4px;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.employee-head-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 3px 10px;
  background: rgba(255, 255, 255, 0.3);
  border-radius: 10px;
  display: inline-block;
  margin-top: 6px;
  border: 1px solid rgba(255, 255, 255, 0.4);
}

.department-name {
  font-size: 1.1rem;
  font-weight: 700;
  margin-bottom: 8px;
  line-height: 1.3;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.department-manager {
  font-size: 0.875rem;
  opacity: 0.95;
  margin-bottom: 8px;
  font-weight: 500;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.department-stats {
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.3);
}

.employee-badge {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 4px 12px;
  background: rgba(255, 255, 255, 0.25);
  border-radius: 12px;
  display: inline-block;
}

/* PrimeVue OrganizationChart overrides */
:deep(.p-organizationchart) {
  padding: 10px;
  max-width: 100%;
  box-sizing: border-box;
}

:deep(.p-organizationchart-table) {
  max-width: 100%;
}

:deep(.p-organizationchart-node-content) {
  border: none !important;
  padding: 0 !important;
}

/* Connection lines */
:deep(.p-organizationchart-connector-down) {
  width: 2px !important;
  height: 20px !important;
  background-color: #2563eb !important;
  margin: 0 auto !important;
}

:deep(.p-organizationchart-connector-left),
:deep(.p-organizationchart-connector-right) {
  border-top: 2px solid #2563eb !important;
}

:deep(.p-organizationchart-connector-left) {
  border-right: 2px solid #2563eb !important;
}

:deep(.p-organizationchart-connector-right) {
  border-left: 2px solid #2563eb !important;
}

/* Dark mode */
:global(.dark) .org-chart-wrapper {
  background: linear-gradient(90deg, rgba(255,255,255,0.05) 1px, transparent 1px),
              linear-gradient(rgba(255,255,255,0.05) 1px, transparent 1px);
  background-size: 40px 40px;
}

:global(.dark) .department-node {
  background: linear-gradient(135deg, #1e40af 0%, #1e3a8a 100%);
  border-color: #1d4ed8;
}

:global(.dark) .employee-node {
  background: linear-gradient(135deg, #4b5563 0%, #374151 100%);
  border-color: #6b7280;
}

:global(.dark) :deep(.p-organizationchart-connector-down) {
  background-color: #3b82f6 !important;
}

:global(.dark) :deep(.p-organizationchart-connector-left),
:global(.dark) :deep(.p-organizationchart-connector-right) {
  border-top-color: #3b82f6 !important;
}

:global(.dark) :deep(.p-organizationchart-connector-left) {
  border-right-color: #3b82f6 !important;
}

:global(.dark) :deep(.p-organizationchart-connector-right) {
  border-left-color: #3b82f6 !important;
}
</style>
