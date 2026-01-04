<script setup lang="ts">
import type { DepartmentTreeDto } from '~/types/department'
import type { OrganizationEmployee } from '~/composables/useOrganization'

definePageMeta({
  layout: 'default',
  middleware: ['auth', 'verified']
})

const route = useRoute()

const {
  departments,
  allEmployees,
  isLoading,
  error,
  departmentsFlat,
  loadData,
  filterEmployees,
  buildOrgChartData
} = useOrganization()

const viewMode = ref<'tree' | 'departments' | 'list'>('departments')
const selectedDepartment = ref<string | null>(null)
const searchQuery = ref<string>('')

const selectedEmployee = ref<OrganizationEmployee | null>(null)
const showEmployeeModal = ref(false)
const selectedDepartmentNode = ref<DepartmentTreeDto | null>(null)
const showDepartmentModal = ref(false)

const filteredEmployees = computed(() => {
  return filterEmployees(allEmployees.value, {
    searchQuery: searchQuery.value,
    departmentId: selectedDepartment.value
  })
})

const departmentOrgChartData = computed(() => buildOrgChartData())

const selectEmployee = (employee: OrganizationEmployee) => {
  selectedEmployee.value = employee
  showEmployeeModal.value = true
}

const handleDepartmentSelect = (department: DepartmentTreeDto) => {
  selectedDepartmentNode.value = department
  showDepartmentModal.value = true
}

const treeViewRef = ref<{ fitToWidth: () => void; resetView: () => void } | null>(null)

onMounted(async () => {
  await loadData()

  nextTick(() => {
    setTimeout(() => {
      treeViewRef.value?.fitToWidth()
    }, 100)
  })
})

watch(departmentOrgChartData, () => {
  nextTick(() => {
    setTimeout(() => {
      treeViewRef.value?.fitToWidth()
    }, 0)
  })
})

watch(() => route.path, (newPath) => {
  if (newPath === '/dashboard/organization') {
    loadData()
  }
})
</script>

<template>
  <div class="space-y-4 overflow-x-hidden max-w-full">
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
          Struktura organizacyjna
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
          {{ allEmployees.length }} pracownikow w {{ departments.length }} dzialach
        </p>
      </div>
    </div>

    <!-- Debug panel - remove after fixing -->
    <div class="bg-yellow-100 dark:bg-yellow-900 p-4 rounded-lg text-sm">
      <p><strong>Debug:</strong></p>
      <p>isLoading: {{ isLoading }}</p>
      <p>error: {{ error }}</p>
      <p>viewMode: {{ viewMode }}</p>
      <p>departments.length: {{ departments.length }}</p>
      <p>departmentsFlat.length: {{ departmentsFlat.length }}</p>
      <p>allEmployees.length: {{ allEmployees.length }}</p>
      <p v-if="departmentsFlat.length > 0">First dept: {{ departmentsFlat[0]?.name }}</p>
    </div>

    <OrgFilters
      v-model:view-mode="viewMode"
      v-model:search-query="searchQuery"
      v-model:selected-department="selectedDepartment"
      :departments="departmentsFlat"
    />

    <div v-if="isLoading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12">
      <div class="flex flex-col items-center justify-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" />
        <p class="mt-4 text-gray-600 dark:text-gray-400">Ladowanie danych...</p>
      </div>
    </div>

    <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-6">
      <div class="flex items-center gap-3">
        <svg class="w-6 h-6 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div>
          <h3 class="text-lg font-semibold text-red-900 dark:text-red-200">Blad ladowania danych</h3>
          <p class="text-sm text-red-700 dark:text-red-300">{{ error }}</p>
        </div>
      </div>
    </div>

    <div v-else class="w-full max-w-full overflow-hidden">
      <DepartmentSidebar
        v-if="viewMode === 'departments'"
        :departments="departmentsFlat"
        @select-employee="selectEmployee"
      />

      <EmployeeListView
        v-else-if="viewMode === 'list'"
        :employees="filteredEmployees"
        @select-employee="selectEmployee"
      />

      <div v-else class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden max-w-full">
        <template v-if="departmentOrgChartData.length > 0">
          <Suspense>
            <LazyOrgTreeView
              ref="treeViewRef"
              :chart-data="departmentOrgChartData"
              @select-department="handleDepartmentSelect"
              @select-employee="selectEmployee"
            />
            <template #fallback>
              <div class="p-12 flex items-center justify-center min-h-[500px]">
                <div class="text-center">
                  <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto" />
                  <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie diagramu organizacji...</p>
                </div>
              </div>
            </template>
          </Suspense>
        </template>

        <div v-else class="text-center py-12">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
            Brak dzialow
          </h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Nie znaleziono dzialow w systemie
          </p>
        </div>
      </div>
    </div>

    <EmployeeDetailModal
      v-model:visible="showEmployeeModal"
      :employee="selectedEmployee"
      @select-employee="selectEmployee"
    />

    <DepartmentDetailModal
      v-model:visible="showDepartmentModal"
      :department="selectedDepartmentNode"
      @select-employee="selectEmployee"
    />
  </div>
</template>
