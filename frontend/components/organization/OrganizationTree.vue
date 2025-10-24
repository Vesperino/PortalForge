<script setup lang="ts">
import type { Employee } from '~/types'

interface Props {
  employee: Employee
  onSelectEmployee?: (employee: Employee) => void
}

const props = defineProps<Props>()

const getInitials = (employee: Employee) => {
  return `${employee.firstName[0]}${employee.lastName[0]}`
}

const handleClick = (employee: Employee) => {
  if (props.onSelectEmployee) {
    props.onSelectEmployee(employee)
  }
}
</script>

<template>
  <div class="flex flex-col items-center">
    <!-- Employee Card -->
    <div
      class="relative bg-white dark:bg-gray-800 border-2 border-gray-300 dark:border-gray-600 rounded-lg p-4 cursor-pointer hover:shadow-lg hover:border-blue-500 dark:hover:border-blue-400 transition-all max-w-xs w-full text-center group"
      @click="handleClick(employee)"
    >
      <div class="w-16 h-16 mx-auto mb-2 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white font-bold text-xl shadow-md">
        {{ getInitials(employee) }}
      </div>
      <h3 class="font-semibold text-gray-900 dark:text-white text-sm">
        {{ employee.firstName }} {{ employee.lastName }}
      </h3>
      <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">
        {{ employee.position?.name }}
      </p>
      <span class="inline-block mt-2 px-2 py-1 text-xs font-medium rounded-full bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
        {{ employee.department?.name }}
      </span>
    </div>

    <!-- Subordinates -->
    <div v-if="employee.subordinates && employee.subordinates.length > 0" class="mt-8 relative">
      <!-- Vertical line from parent -->
      <div class="absolute top-0 left-1/2 w-0.5 h-8 bg-gray-300 dark:bg-gray-600 -translate-x-1/2 -translate-y-8" />
      
      <!-- Horizontal line connecting children -->
      <div
        v-if="employee.subordinates.length > 1"
        class="absolute top-0 left-0 right-0 h-0.5 bg-gray-300 dark:bg-gray-600"
        :style="{
          left: `calc(${100 / employee.subordinates.length / 2}%)`,
          right: `calc(${100 / employee.subordinates.length / 2}%)`
        }"
      />

      <!-- Children Grid -->
      <div
        class="grid gap-8"
        :class="{
          'grid-cols-1': employee.subordinates.length === 1,
          'grid-cols-2': employee.subordinates.length === 2,
          'grid-cols-3': employee.subordinates.length === 3,
          'grid-cols-4': employee.subordinates.length >= 4
        }"
      >
        <div
          v-for="subordinate in employee.subordinates"
          :key="subordinate.id"
          class="relative"
        >
          <!-- Vertical line to child -->
          <div class="absolute top-0 left-1/2 w-0.5 h-8 bg-gray-300 dark:bg-gray-600 -translate-x-1/2 -translate-y-8" />
          
          <!-- Recursive component -->
          <OrganizationTree
            :employee="subordinate"
            :on-select-employee="onSelectEmployee"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Ensure proper spacing and alignment */
.grid {
  align-items: start;
}
</style>

