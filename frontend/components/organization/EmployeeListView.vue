<script setup lang="ts">
import type { OrganizationEmployee } from '~/composables/useOrganization'

interface Props {
  employees: OrganizationEmployee[]
}

defineProps<Props>()

const emit = defineEmits<{
  selectEmployee: [employee: OrganizationEmployee]
}>()

const { getInitials } = useOrganization()

const handleSelectEmployee = (employee: OrganizationEmployee) => {
  emit('selectEmployee', employee)
}
</script>

<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
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
              Dzial
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
            v-for="employee in employees"
            :key="employee.id"
            class="hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer transition-colors"
            data-testid="employee-row"
            @click="handleSelectEmployee(employee)"
          >
            <td class="px-6 py-4 whitespace-nowrap">
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold text-sm overflow-hidden">
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

    <div v-if="employees.length === 0" class="text-center py-12">
      <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">
        Brak pracownikow
      </h3>
      <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
        Nie znaleziono pracownikow spelniajacych kryteria wyszukiwania.
      </p>
    </div>
  </div>
</template>
