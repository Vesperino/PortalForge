<script setup lang="ts">
import type { DepartmentTreeDto } from '~/types/department'
import type { OrganizationEmployee } from '~/composables/useOrganization'

interface Props {
  department: DepartmentTreeDto | null
  visible: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  selectEmployee: [employee: OrganizationEmployee]
}>()

const { getEmployeesByDepartment, getManagerByDepartment, getDirectorByDepartment, getInitials } = useOrganization()

const closeModal = () => {
  emit('update:visible', false)
}

const handleSelectEmployee = (employee: OrganizationEmployee) => {
  emit('selectEmployee', employee)
  closeModal()
}

const employees = computed(() => {
  if (!props.department) return []
  return getEmployeesByDepartment(props.department.id)
})

const manager = computed(() => {
  if (!props.department) return null
  return getManagerByDepartment(props.department)
})

const director = computed(() => {
  if (!props.department) return null
  return getDirectorByDepartment(props.department)
})
</script>

<template>
  <Teleport to="body">
    <div
      v-if="visible && department"
      class="fixed inset-0 z-50 overflow-y-auto"
      @click.self="closeModal"
    >
      <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
        <div
          class="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75"
          @click="closeModal"
        />

        <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full">
          <div class="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
            <div class="flex items-center justify-between">
              <h3 class="text-lg font-semibold text-white">
                Szczegoly dzialu
              </h3>
              <button
                type="button"
                class="text-white hover:text-gray-200 focus:outline-none"
                data-testid="close-department-modal"
                @click="closeModal"
              >
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <div class="px-6 py-6">
            <div class="mb-6">
              <h4 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                {{ department.name }}
              </h4>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
                {{ department.description }}
              </p>

              <div class="grid grid-cols-2 gap-4">
                <div
                  class="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4"
                  :class="{ 'cursor-pointer': manager }"
                  @click="manager ? handleSelectEmployee(manager) : null"
                >
                  <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Kierownik dzialu</p>
                  <div v-if="manager" class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold overflow-hidden">
                      <img
                        v-if="manager.profilePhotoUrl"
                        :src="manager.profilePhotoUrl"
                        :alt="`${manager.firstName} ${manager.lastName}`"
                        class="w-full h-full object-cover"
                      >
                      <span v-else>{{ getInitials(manager) }}</span>
                    </div>
                    <div>
                      <p class="font-semibold text-gray-900 dark:text-white">{{ manager.firstName }} {{ manager.lastName }}</p>
                      <p class="text-sm text-gray-600 dark:text-gray-400">{{ manager.position }}</p>
                    </div>
                  </div>
                  <p v-else class="text-sm text-gray-500 dark:text-gray-400 italic">Brak kierownika</p>
                </div>

                <div
                  class="bg-purple-50 dark:bg-purple-900/20 rounded-lg p-4"
                  :class="{ 'cursor-pointer': director }"
                  @click="director ? handleSelectEmployee(director) : null"
                >
                  <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Dyrektor dzialu</p>
                  <div v-if="director" class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-full bg-purple-500 flex items-center justify-center text-white font-semibold overflow-hidden">
                      <img
                        v-if="director.profilePhotoUrl"
                        :src="director.profilePhotoUrl"
                        :alt="`${director.firstName} ${director.lastName}`"
                        class="w-full h-full object-cover"
                      >
                      <span v-else>{{ getInitials(director) }}</span>
                    </div>
                    <div>
                      <p class="font-semibold text-gray-900 dark:text-white">{{ director.firstName }} {{ director.lastName }}</p>
                      <p class="text-sm text-gray-600 dark:text-gray-400">{{ director.position }}</p>
                    </div>
                  </div>
                  <p v-else class="text-sm text-gray-500 dark:text-gray-400 italic">Brak dyrektora</p>
                </div>

                <div class="bg-green-50 dark:bg-green-900/20 rounded-lg p-4 col-span-2">
                  <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Liczba pracownikow</p>
                  <p class="font-semibold text-gray-900 dark:text-white">
                    {{ employees.length }} pracownikow
                  </p>
                </div>
              </div>
            </div>

            <div class="border-t border-gray-200 dark:border-gray-700 pt-4">
              <h5 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Pracownicy dzialu
              </h5>

              <div v-if="employees.length > 0" class="space-y-2 max-h-96 overflow-y-auto">
                <div
                  v-for="employee in employees"
                  :key="employee.id"
                  class="flex items-center gap-3 p-3 rounded-lg bg-gray-50 dark:bg-gray-700/50 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer transition-colors"
                  data-testid="modal-employee-item"
                  @click="handleSelectEmployee(employee)"
                >
                  <div class="w-12 h-12 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white font-semibold overflow-hidden">
                    <img
                      v-if="employee.profilePhotoUrl"
                      :src="employee.profilePhotoUrl"
                      :alt="`${employee.firstName} ${employee.lastName}`"
                      class="w-full h-full object-cover"
                    >
                    <span v-else>{{ getInitials(employee) }}</span>
                  </div>
                  <div class="flex-1">
                    <p class="font-medium text-gray-900 dark:text-white">
                      {{ employee.firstName }} {{ employee.lastName }}
                      <span
                        v-if="employee.id === department.departmentHeadId"
                        class="ml-2 text-xs bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 px-2 py-0.5 rounded-full"
                      >
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
                  Brak pracownikow w tym dziale
                </p>
              </div>
            </div>
          </div>

          <div class="bg-gray-50 dark:bg-gray-700/50 px-6 py-4">
            <button
              type="button"
              class="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
              data-testid="close-modal-btn"
              @click="closeModal"
            >
              Zamknij
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>
