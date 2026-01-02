<script setup lang="ts">
import type { DepartmentTreeDto } from '~/types/department'
import type { OrganizationEmployee } from '~/composables/useOrganization'

interface Props {
  departments: DepartmentTreeDto[]
}

defineProps<Props>()

const emit = defineEmits<{
  selectEmployee: [employee: OrganizationEmployee]
}>()

const { getEmployeesByDepartment, getManagerByDepartment, getDirectorByDepartment, getInitials } = useOrganization()

const handleSelectEmployee = (employee: OrganizationEmployee) => {
  emit('selectEmployee', employee)
}
</script>

<template>
  <div class="space-y-4">
    <div
      v-for="dept in departments"
      :key="dept.id"
      class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
      :class="{ 'ml-8': dept.level && dept.level > 0 }"
    >
      <div class="flex items-center justify-between mb-4">
        <div class="flex-1">
          <div class="flex items-center gap-2">
            <span v-if="dept.level && dept.level > 0" class="text-gray-400 dark:text-gray-600">
              {{ '\u2514\u2500'.repeat(dept.level) }}
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
          <div class="text-right">
            <p class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ getEmployeesByDepartment(dept.id).length }}
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-400">pracownikow</p>
          </div>
        </div>
      </div>

      <div v-if="getManagerByDepartment(dept)" class="mb-4 p-3 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
        <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Kierownik dzialu</p>
        <div
          class="flex items-center gap-3 cursor-pointer"
          data-testid="department-manager"
          @click="handleSelectEmployee(getManagerByDepartment(dept)!)"
        >
          <div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold overflow-hidden">
            <img
              v-if="getManagerByDepartment(dept)?.profilePhotoUrl"
              :src="getManagerByDepartment(dept)?.profilePhotoUrl"
              :alt="`${getManagerByDepartment(dept)?.firstName} ${getManagerByDepartment(dept)?.lastName}`"
              class="w-full h-full object-cover"
            >
            <span v-else>
              {{ getInitials(getManagerByDepartment(dept)) }}
            </span>
          </div>
          <div>
            <p class="font-medium text-gray-900 dark:text-white">
              {{ getManagerByDepartment(dept)?.firstName }} {{ getManagerByDepartment(dept)?.lastName }}
            </p>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              {{ getManagerByDepartment(dept)?.position }}
            </p>
          </div>
        </div>
      </div>

      <div v-if="getDirectorByDepartment(dept)" class="mb-4 p-3 bg-purple-50 dark:bg-purple-900/20 rounded-lg">
        <p class="text-xs text-gray-600 dark:text-gray-400 mb-1">Dyrektor dzialu</p>
        <div
          class="flex items-center gap-3 cursor-pointer"
          data-testid="department-director"
          @click="handleSelectEmployee(getDirectorByDepartment(dept)!)"
        >
          <div class="w-10 h-10 rounded-full bg-purple-500 flex items-center justify-center text-white font-semibold overflow-hidden">
            <img
              v-if="getDirectorByDepartment(dept)?.profilePhotoUrl"
              :src="getDirectorByDepartment(dept)?.profilePhotoUrl"
              :alt="`${getDirectorByDepartment(dept)?.firstName} ${getDirectorByDepartment(dept)?.lastName}`"
              class="w-full h-full object-cover"
            >
            <span v-else>
              {{ getInitials(getDirectorByDepartment(dept)) }}
            </span>
          </div>
          <div>
            <p class="font-medium text-gray-900 dark:text-white">
              {{ getDirectorByDepartment(dept)?.firstName }} {{ getDirectorByDepartment(dept)?.lastName }}
            </p>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              {{ getDirectorByDepartment(dept)?.position }}
            </p>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div
          v-for="employee in getEmployeesByDepartment(dept.id).filter(e => e.id !== dept.departmentHeadId)"
          :key="employee.id"
          class="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer transition-colors"
          data-testid="department-employee"
          @click="handleSelectEmployee(employee)"
        >
          <div class="w-10 h-10 rounded-full bg-gray-300 dark:bg-gray-600 flex items-center justify-center text-gray-700 dark:text-gray-300 font-semibold text-sm overflow-hidden">
            <img
              v-if="employee.profilePhotoUrl"
              :src="employee.profilePhotoUrl"
              :alt="`${employee.firstName} ${employee.lastName}`"
              class="w-full h-full object-cover"
            >
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
</template>
