<script setup lang="ts">
import type { Employee } from '~/types'

definePageMeta({
  layout: 'default',
  middleware: ['auth']
})

const { getOrganizationTree, getDepartments, getEmployees } = useMockData()

const viewMode = ref<'tree' | 'departments' | 'list'>('tree')
const selectedDepartment = ref<number | null>(null)
const searchQuery = ref<string>('')

const organizationTree = getOrganizationTree()
const departments = getDepartments()
const allEmployees = getEmployees()

const selectedEmployee = ref<Employee | null>(null)
const showEmployeeModal = ref(false)

const filteredEmployees = computed(() => {
  let filtered = allEmployees

  if (selectedDepartment.value) {
    filtered = filtered.filter(e => e.departmentId === selectedDepartment.value)
  }

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(e =>
      e.firstName.toLowerCase().includes(query) ||
      e.lastName.toLowerCase().includes(query) ||
      e.email.toLowerCase().includes(query) ||
      e.position?.name.toLowerCase().includes(query) ||
      e.department?.name.toLowerCase().includes(query)
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
  return `${employee.firstName[0]}${employee.lastName[0]}`
}

const getEmployeesByDepartment = (departmentId: number) => {
  return allEmployees.filter(e => e.departmentId === departmentId)
}




</script>

<template>
  <div class="space-y-4">
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
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
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
              <option v-for="dept in departments" :key="dept.id" :value="dept.id">
                {{ dept.name }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content Area - Full Width -->
    <div class="w-full">
        <!-- Departments View -->
        <div v-if="viewMode === 'departments'" class="space-y-4">
          <div
            v-for="dept in departments"
            :key="dept.id"
            class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
          >
            <div class="flex items-center justify-between mb-4">
              <div>
                <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
                  {{ dept.name }}
                </h3>
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
            <div v-if="dept.manager" class="mb-4 p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
              <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Kierownik działu</p>
              <div
                class="flex items-center gap-3 cursor-pointer"
                @click="selectEmployee(dept.manager)"
              >
                <div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold">
                  {{ getInitials(dept.manager) }}
                </div>
                <div>
                  <p class="font-medium text-gray-900 dark:text-white">
                    {{ dept.manager.firstName }} {{ dept.manager.lastName }}
                  </p>
                  <p class="text-sm text-gray-600 dark:text-gray-400">
                    {{ dept.manager.position?.name }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Department Employees -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div
                v-for="employee in getEmployeesByDepartment(dept.id).filter(e => e.id !== dept.managerId)"
                :key="employee.id"
                class="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer transition-colors"
                @click="selectEmployee(employee)"
              >
                <div class="w-10 h-10 rounded-full bg-gray-300 dark:bg-gray-600 flex items-center justify-center text-gray-700 dark:text-gray-300 font-semibold text-sm">
                  {{ getInitials(employee) }}
                </div>
                <div class="flex-1 min-w-0">
                  <p class="font-medium text-gray-900 dark:text-white truncate text-sm">
                    {{ employee.firstName }} {{ employee.lastName }}
                  </p>
                  <p class="text-xs text-gray-600 dark:text-gray-400 truncate">
                    {{ employee.position?.name }}
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
                      <div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold text-sm">
                        {{ getInitials(employee) }}
                      </div>
                      <div>
                        <p class="font-medium text-gray-900 dark:text-white">
                          {{ employee.firstName }} {{ employee.lastName }}
                        </p>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                    {{ employee.position?.name }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span class="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
                      {{ employee.department?.name }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                    {{ employee.email }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-700 dark:text-gray-300">
                    {{ employee.phone || '-' }}
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
        <div v-else>
          <div v-if="organizationTree" class="h-[calc(100vh-280px)] min-h-[600px]">
            <ClientOnly>
              <OrgChartView
                :employee="organizationTree"
                :on-select-employee="selectEmployee"
                @select-employee="selectEmployee"
              />
              <template #fallback>
                <div class="flex items-center justify-center h-full bg-white dark:bg-gray-800 rounded-lg shadow-lg">
                  <div class="text-center">
                    <svg class="animate-spin h-12 w-12 text-blue-600 mx-auto mb-4" fill="none" viewBox="0 0 24 24">
                      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    <p class="text-gray-600 dark:text-gray-400">Ładowanie wykresu organizacyjnego...</p>
                  </div>
                </div>
              </template>
            </ClientOnly>
          </div>

          <div v-else class="text-center py-12 bg-white dark:bg-gray-800 rounded-lg shadow-md">
            <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
              Brak danych
            </h3>
            <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
              Brak danych o strukturze organizacyjnej
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
                <div class="inline-flex w-24 h-24 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 items-center justify-center text-3xl font-bold text-white shadow-lg">
                  {{ getInitials(selectedEmployee) }}
                </div>
                <h4 class="mt-4 text-2xl font-bold text-gray-900 dark:text-white">
                  {{ selectedEmployee.firstName }} {{ selectedEmployee.lastName }}
                </h4>
                <p class="mt-1 text-gray-600 dark:text-gray-400">
                  {{ selectedEmployee.position?.name }}
                </p>
                <span
                  class="inline-block mt-3 px-4 py-1.5 text-sm font-medium rounded-full text-white shadow-sm"
                  :style="{ backgroundColor: selectedEmployee.department?.color || '#3b82f6' }"
                >
                  {{ selectedEmployee.department?.name }}
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

                <div v-if="selectedEmployee.phone" class="flex items-center gap-4">
                  <div class="flex-shrink-0 w-10 h-10 rounded-full bg-green-100 dark:bg-green-900 flex items-center justify-center">
                    <svg class="w-5 h-5 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                    </svg>
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Telefon</p>
                    <a
                      :href="`tel:${selectedEmployee.phone}`"
                      class="text-sm text-green-600 dark:text-green-400 hover:underline font-medium"
                    >
                      {{ selectedEmployee.phone }}
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
                      {{ selectedEmployee.supervisor.position?.name }}
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
                        {{ sub.position?.name }}
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
  </div>
</template>
