<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import type { DepartmentTreeDto } from '~/types/department'

interface Props {
  department: DepartmentTreeDto
  level?: number
  selectable?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  level: 0,
  selectable: false
})

interface Emits {
  (e: 'add-child', departmentId: string): void
  (e: 'edit', departmentId: string): void
  (e: 'delete', departmentId: string): void
  (e: 'select', departmentId: string): void
  (e: 'employee-click', employeeId: string): void
}

const emit = defineEmits<Emits>()

// Expand/collapse state
const isExpanded = ref(true)
const showActions = ref(false)

// Check if department has children
const hasChildren = computed(() => {
  return props.department.children && props.department.children.length > 0
})

// Toggle expand/collapse
const toggleExpand = () => {
  if (hasChildren.value) {
    isExpanded.value = !isExpanded.value
  }
}

// Get indentation based on tree level
const indentStyle = computed(() => {
  return {
    paddingLeft: `${props.level * 24}px`
  }
})

// Get initials from department head name
const getDepartmentHeadInitials = (name: string | null | undefined): string => {
  if (!name) return 'N/A'

  const parts = name.trim().split(' ').filter(p => p.length > 0)
  if (parts.length >= 2) {
    const first = parts[0]?.[0] || ''
    const last = parts[parts.length - 1]?.[0] || ''
    if (first && last) {
      return `${first}${last}`.toUpperCase()
    }
  }
  return name.substring(0, Math.min(2, name.length)).toUpperCase() || 'N/A'
}

// Get background color for avatar based on level
const getAvatarColor = (level: number): string => {
  const colors = [
    'bg-blue-500',
    'bg-green-500',
    'bg-purple-500',
    'bg-orange-500',
    'bg-pink-500',
    'bg-indigo-500',
    'bg-teal-500',
    'bg-red-500'
  ] as const
  return colors[level % colors.length] as string
}

// Handle action menu clicks
const handleAddChild = () => {
  showActions.value = false
  emit('add-child', props.department.id)
}

const handleEdit = () => {
  showActions.value = false
  emit('edit', props.department.id)
}

const handleDelete = () => {
  showActions.value = false
  emit('delete', props.department.id)
}

const handleSelect = () => {
  emit('select', props.department.id)
}

// Close actions menu when clicking outside
const actionsRef = ref<HTMLElement | null>(null)

const closeActionsMenu = (event: MouseEvent) => {
  if (actionsRef.value && !actionsRef.value.contains(event.target as Node)) {
    showActions.value = false
  }
}

// Add event listener for clicking outside
onMounted(() => {
  document.addEventListener('click', closeActionsMenu)
})

onUnmounted(() => {
  document.removeEventListener('click', closeActionsMenu)
})
</script>

<template>
  <div class="department-tree">
    <!-- Department Node -->
    <div
      class="department-node flex items-center gap-3 p-3 hover:bg-gray-50 dark:hover:bg-gray-900 rounded-lg transition-colors group"
      :style="indentStyle"
    >
      <!-- Expand/Collapse Arrow -->
      <button
        v-if="hasChildren"
        class="w-6 h-6 flex items-center justify-center text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 transition-transform"
        :class="{ 'rotate-90': isExpanded }"
        @click="toggleExpand"
      >
        <svg
          class="w-4 h-4"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 5l7 7-7 7"
          />
        </svg>
      </button>

      <!-- Placeholder for departments without children -->
      <div v-else class="w-6 h-6" />

      <!-- Department Icon -->
      <div
        class="w-10 h-10 rounded-lg flex items-center justify-center text-white font-semibold"
        :class="level === 0 ? 'bg-blue-600' : 'bg-gray-500'"
      >
        <svg
          class="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
          />
        </svg>
      </div>

      <!-- Department Info -->
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2">
          <h3 class="text-sm font-semibold text-gray-900 dark:text-white truncate">
            {{ department.name }}
          </h3>
          <span
            v-if="!department.isActive"
            class="px-2 py-0.5 text-xs font-medium rounded-full bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300"
          >
            Nieaktywny
          </span>
        </div>
        <div class="flex items-center gap-3 mt-1 text-xs text-gray-600 dark:text-gray-400">
          <!-- Department Head -->
          <div v-if="department.departmentHeadName" class="flex items-center gap-1.5">
            <div
              class="w-5 h-5 rounded-full flex items-center justify-center text-white text-[10px] font-semibold"
              :class="getAvatarColor(level)"
            >
              {{ getDepartmentHeadInitials(department.departmentHeadName) }}
            </div>
            <span>{{ department.departmentHeadName }}</span>
          </div>
          <div v-else class="flex items-center gap-1.5 text-gray-400">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              />
            </svg>
            <span>Brak kierownika</span>
          </div>

          <!-- Employee Count -->
          <div class="flex items-center gap-1">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
              />
            </svg>
            <span>{{ department.employeeCount }}</span>
          </div>
        </div>
      </div>

      <!-- Select Button (when selectable) -->
      <button
        v-if="selectable"
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
        @click="handleSelect"
      >
        Wybierz
      </button>

      <!-- Actions Dropdown (when not selectable) -->
      <div
        v-else
        ref="actionsRef"
        class="relative opacity-0 group-hover:opacity-100 transition-opacity"
      >
        <button
          class="w-8 h-8 flex items-center justify-center text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors"
          @click.stop="showActions = !showActions"
        >
          <svg
            class="w-5 h-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"
            />
          </svg>
        </button>

        <!-- Dropdown Menu -->
        <div
          v-if="showActions"
          class="absolute right-0 top-full mt-1 w-48 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 py-1 z-10"
        >
          <button
            class="w-full px-4 py-2 text-left text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 flex items-center gap-2 transition-colors"
            @click="handleAddChild"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 4v16m8-8H4"
              />
            </svg>
            Dodaj poddział
          </button>
          <button
            class="w-full px-4 py-2 text-left text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 flex items-center gap-2 transition-colors"
            @click="handleEdit"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
              />
            </svg>
            Edytuj
          </button>
          <button
            class="w-full px-4 py-2 text-left text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 flex items-center gap-2 transition-colors"
            @click="handleDelete"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
              />
            </svg>
            Usuń
          </button>
        </div>
      </div>
    </div>

    <!-- Employees List -->
    <div
      v-if="isExpanded && department.employees && department.employees.length > 0"
      class="employees-list ml-12 mt-2"
    >
      <div
        v-for="employee in department.employees"
        :key="employee.id"
        class="employee-item flex items-center gap-3 p-2 hover:bg-gray-50 dark:hover:bg-gray-900 rounded-lg transition-colors cursor-pointer"
        :style="indentStyle"
        @click="emit('employee-click', employee.id)"
      >
        <!-- Employee Icon -->
        <div
          class="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-semibold"
          :class="getAvatarColor(level + 1)"
        >
          {{ employee.firstName[0] }}{{ employee.lastName[0] }}
        </div>

        <!-- Employee Info -->
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
            {{ employee.firstName }} {{ employee.lastName }}
          </p>
          <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
            {{ employee.position || 'Brak stanowiska' }}
          </p>
        </div>

        <!-- Badge if department head -->
        <span
          v-if="department.departmentHeadId === employee.id"
          class="px-2 py-0.5 text-xs font-medium rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300"
        >
          Kierownik
        </span>
      </div>
    </div>

    <!-- Children Departments (Recursive) -->
    <div
      v-if="hasChildren && isExpanded"
      class="department-children"
    >
      <DepartmentTree
        v-for="child in department.children"
        :key="child.id"
        :department="child"
        :level="level + 1"
        :selectable="selectable"
        @add-child="emit('add-child', $event)"
        @edit="emit('edit', $event)"
        @delete="emit('delete', $event)"
        @select="emit('select', $event)"
        @employee-click="emit('employee-click', $event)"
      />
    </div>
  </div>
</template>

<style scoped>
.department-tree {
  position: relative;
}

.department-node {
  position: relative;
}

/* Subtle border for nested items */
.department-children {
  position: relative;
}

.department-children::before {
  content: '';
  position: absolute;
  left: 15px;
  top: 0;
  bottom: 0;
  width: 1px;
  background: linear-gradient(
    to bottom,
    transparent 0%,
    rgb(229, 231, 235) 10%,
    rgb(229, 231, 235) 90%,
    transparent 100%
  );
}

.dark .department-children::before {
  background: linear-gradient(
    to bottom,
    transparent 0%,
    rgb(55, 65, 81) 10%,
    rgb(55, 65, 81) 90%,
    transparent 100%
  );
}
</style>
