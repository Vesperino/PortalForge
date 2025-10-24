<script setup lang="ts">
import { ref, computed } from 'vue'
import type { Employee } from '~/types'

interface Props {
  employee: Employee
  onSelectEmployee?: (employee: Employee) => void
}

const props = defineProps<Props>()

const isExpanded = ref(true)

const getInitials = (employee: Employee) => {
  return `${employee.firstName[0]}${employee.lastName[0]}`
}

const getDepartmentColor = (employee: Employee) => {
  return employee.department?.color || '#3b82f6'
}

const hasSubordinates = computed(() => {
  return props.employee.subordinates && props.employee.subordinates.length > 0
})

const toggleExpand = (e: Event) => {
  e.stopPropagation()
  isExpanded.value = !isExpanded.value
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
      class="relative bg-white dark:bg-gray-800 border-2 rounded-lg p-4 cursor-pointer hover:shadow-lg transition-all max-w-xs w-full text-center group"
      :style="{ borderColor: getDepartmentColor(employee) }"
      @click="handleClick(employee)"
    >
      <!-- Avatar with photo or initials -->
      <div class="relative w-16 h-16 mx-auto mb-2">
        <img
          v-if="employee.avatar"
          :src="employee.avatar"
          :alt="`${employee.firstName} ${employee.lastName}`"
          class="w-16 h-16 rounded-full object-cover shadow-md"
        />
        <div
          v-else
          class="w-16 h-16 rounded-full flex items-center justify-center text-white font-bold text-xl shadow-md"
          :style="{ background: `linear-gradient(135deg, ${getDepartmentColor(employee)}, ${getDepartmentColor(employee)}dd)` }"
        >
          {{ getInitials(employee) }}
        </div>
      </div>

      <h3 class="font-semibold text-gray-900 dark:text-white text-sm">
        {{ employee.firstName }} {{ employee.lastName }}
      </h3>
      <p class="text-xs text-gray-600 dark:text-gray-400 mt-1">
        {{ employee.position?.name }}
      </p>
      <span
        class="inline-block mt-2 px-2 py-1 text-xs font-medium rounded-full text-white"
        :style="{ backgroundColor: getDepartmentColor(employee) }"
      >
        {{ employee.department?.name }}
      </span>

      <!-- Expand/Collapse button -->
      <button
        v-if="hasSubordinates"
        class="absolute -bottom-3 left-1/2 -translate-x-1/2 w-6 h-6 rounded-full bg-white dark:bg-gray-700 border-2 flex items-center justify-center hover:scale-110 transition-transform z-10"
        :style="{ borderColor: getDepartmentColor(employee) }"
        @click="toggleExpand"
      >
        <svg
          class="w-4 h-4 transition-transform"
          :class="{ 'rotate-180': !isExpanded }"
          :style="{ color: getDepartmentColor(employee) }"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
        </svg>
      </button>
    </div>

    <!-- Subordinates -->
    <Transition
      enter-active-class="transition-all duration-300 ease-out"
      enter-from-class="opacity-0 scale-95"
      enter-to-class="opacity-100 scale-100"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="opacity-100 scale-100"
      leave-to-class="opacity-0 scale-95"
    >
      <div v-if="hasSubordinates && isExpanded" class="mt-8 relative">
        <!-- Vertical line from parent -->
        <div
          class="absolute top-0 left-1/2 w-0.5 h-8 -translate-x-1/2 -translate-y-8"
          :style="{ backgroundColor: getDepartmentColor(employee) }"
        />

        <!-- Horizontal line connecting children -->
        <div
          v-if="employee.subordinates!.length > 1"
          class="absolute top-0 left-0 right-0 h-0.5"
          :style="{
            backgroundColor: getDepartmentColor(employee),
            left: `calc(${100 / employee.subordinates!.length / 2}%)`,
            right: `calc(${100 / employee.subordinates!.length / 2}%)`
          }"
        />

        <!-- Children Grid -->
        <div
          class="grid gap-8"
          :class="{
            'grid-cols-1': employee.subordinates!.length === 1,
            'grid-cols-2': employee.subordinates!.length === 2,
            'grid-cols-3': employee.subordinates!.length === 3,
            'grid-cols-4': employee.subordinates!.length >= 4
          }"
        >
          <div
            v-for="subordinate in employee.subordinates"
            :key="subordinate.id"
            class="relative"
          >
            <!-- Vertical line to child -->
            <div
              class="absolute top-0 left-1/2 w-0.5 h-8 -translate-x-1/2 -translate-y-8"
              :style="{ backgroundColor: getDepartmentColor(employee) }"
            />

            <!-- Recursive component -->
            <OrganizationTree
              :employee="subordinate"
              :on-select-employee="onSelectEmployee"
            />
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
/* Ensure proper spacing and alignment */
.grid {
  align-items: start;
}
</style>

