<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { Employee } from '~/types'

interface Props {
  employee: Employee
  onSelectEmployee?: (employee: Employee) => void
}

const props = defineProps<Props>()
const emit = defineEmits<{
  selectEmployee: [employee: Employee]
}>()

// Konwertuj dane pracownika na format hierarchiczny
const convertToOrgData = (emp: Employee): any => {
  const node: any = {
    id: emp.id,
    name: `${emp.firstName} ${emp.lastName}`,
    title: emp.position?.name || '',
    department: emp.department?.name || '',
    email: emp.email,
    phone: emp.phone || '',
    color: emp.department?.color || '#3b82f6',
    employee: emp,
    children: []
  }

  if (emp.subordinates && emp.subordinates.length > 0) {
    node.children = emp.subordinates.map(sub => convertToOrgData(sub))
  }

  return node
}

const orgData = computed(() => convertToOrgData(props.employee))

const handleNodeClick = (employee: Employee) => {
  emit('selectEmployee', employee)
  if (props.onSelectEmployee) {
    props.onSelectEmployee(employee)
  }
}

const getInitials = (name: string) => {
  const parts = name.split(' ')
  return parts.map(p => p[0]).join('').toUpperCase()
}

// Rekurencyjne renderowanie węzłów
const renderNode = (node: any, level: number = 0) => {
  return {
    node,
    level,
    hasChildren: node.children && node.children.length > 0
  }
}

// Płaska lista wszystkich węzłów do renderowania
const flattenNodes = (node: any, level: number = 0, result: any[] = []): any[] => {
  result.push({ ...node, level })
  if (node.children) {
    node.children.forEach((child: any) => flattenNodes(child, level + 1, result))
  }
  return result
}

const allNodes = computed(() => flattenNodes(orgData.value))
</script>

<template>
  <div class="w-full h-full overflow-auto bg-gray-50 dark:bg-gray-900 p-8">
    <div class="org-chart-container">
      <!-- Root Node -->
      <div class="flex flex-col items-center">
        <div
          class="org-node cursor-pointer transform hover:scale-105 transition-all duration-200"
          :style="{ borderColor: orgData.color }"
          @click="handleNodeClick(orgData.employee)"
        >
          <div class="flex items-center gap-3 mb-2">
            <div
              class="w-12 h-12 rounded-full flex items-center justify-center text-white font-bold text-lg"
              :style="{ backgroundColor: orgData.color }"
            >
              {{ getInitials(orgData.name) }}
            </div>
            <div class="flex-1">
              <h3 class="font-bold text-gray-900 dark:text-white">
                {{ orgData.name }}
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ orgData.title }}
              </p>
            </div>
          </div>
          <div class="mt-2 pt-2 border-t border-gray-200 dark:border-gray-700">
            <span
              class="inline-block px-3 py-1 text-xs font-medium rounded-full text-white"
              :style="{ backgroundColor: orgData.color }"
            >
              {{ orgData.department }}
            </span>
          </div>
        </div>

        <!-- Vertical Line -->
        <div
          v-if="orgData.children && orgData.children.length > 0"
          class="org-line-vertical"
          :style="{ backgroundColor: orgData.color }"
        />

        <!-- Horizontal Line -->
        <div
          v-if="orgData.children && orgData.children.length > 1"
          class="org-line-horizontal"
          :style="{ backgroundColor: orgData.color }"
        />

        <!-- Children -->
        <div
          v-if="orgData.children && orgData.children.length > 0"
          class="flex gap-8 mt-4"
        >
          <OrgChart
            v-for="child in orgData.children"
            :key="child.id"
            :employee="child.employee"
            :on-select-employee="handleNodeClick"
            @select-employee="handleNodeClick"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.org-chart-container {
  min-width: max-content;
}

.org-node {
  background: white;
  border: 2px solid;
  border-radius: 12px;
  padding: 16px;
  min-width: 280px;
  max-width: 320px;
  box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
}

.dark .org-node {
  background: #1f2937;
}

.org-line-vertical {
  width: 2px;
  height: 40px;
}

.org-line-horizontal {
  height: 2px;
  position: absolute;
  width: calc(100% - 40px);
  margin-top: -22px;
  left: 50%;
  transform: translateX(-50%);
}
</style>
