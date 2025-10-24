<script setup lang="ts">
import type { Employee } from '~/types'
import type { OrganizationChartNode } from 'primevue/organizationchart'

interface Props {
  employee: Employee
  onSelectEmployee?: (employee: Employee) => void
}

const props = defineProps<Props>()
const emit = defineEmits<{
  selectEmployee: [employee: Employee]
}>()

// Store employee lookup map for click handling
const employeeLookup = new Map<string, Employee>()

// Convert Employee hierarchy to PrimeVue OrganizationChart format
const convertToOrgChartData = (emp: Employee): OrganizationChartNode => {
  const nodeKey = `emp-${emp.id}`
  employeeLookup.set(nodeKey, emp)

  const node: OrganizationChartNode = {
    key: nodeKey,
    type: 'person',
    data: {
      id: emp.id,
      name: `${emp.firstName} ${emp.lastName}`,
      title: emp.position?.name || '',
      department: emp.department?.name || '',
      color: emp.department?.color || '#3b82f6',
    },
    children: emp.subordinates?.map(sub => convertToOrgChartData(sub)) || []
  }

  return node
}

const data = computed(() => convertToOrgChartData(props.employee))

const handleNodeClick = (event: any) => {
  // PrimeVue passes event object with node property
  const node = event.node || event
  const employee = employeeLookup.get(node.key as string)
  if (employee) {
    emit('selectEmployee', employee)
    if (props.onSelectEmployee) {
      props.onSelectEmployee(employee)
    }
  }
}
</script>

<template>
  <div class="org-tree-chart-container">
    <OrganizationChart
      :value="data"
      collapsible
      selection-mode="single"
      @node-select="handleNodeClick"
    >
      <template #person="slotProps">
        <div
          class="custom-node"
          :style="{ borderColor: slotProps.node.data.color }"
        >
          <div class="node-header" :style="{ backgroundColor: slotProps.node.data.color }">
            <div class="node-name">{{ slotProps.node.data.name }}</div>
          </div>
          <div class="node-body">
            <div class="node-title">{{ slotProps.node.data.title }}</div>
            <div class="node-department" :style="{ color: slotProps.node.data.color }">
              {{ slotProps.node.data.department }}
            </div>
          </div>
        </div>
      </template>
    </OrganizationChart>
  </div>
</template>

<style scoped>
.org-tree-chart-container {
  width: 100%;
  overflow-x: auto;
  padding: 20px;
}

.custom-node {
  width: 200px;
  border: 2px solid;
  border-radius: 8px;
  overflow: hidden;
  background: white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  cursor: pointer;
  transition: all 0.3s ease;
}

.custom-node:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.node-header {
  padding: 0.75rem;
  color: white;
  font-weight: 600;
  text-align: center;
}

.node-name {
  font-size: 0.875rem;
  line-height: 1.25;
}

.node-body {
  padding: 0.75rem;
  background: white;
}

.node-title {
  font-size: 0.75rem;
  color: #6b7280;
  margin-bottom: 0.25rem;
  text-align: center;
}

.node-department {
  font-size: 0.7rem;
  font-weight: 600;
  text-align: center;
  padding: 0.25rem 0.5rem;
  border-radius: 9999px;
  background: rgba(59, 130, 246, 0.1);
}

/* Dark mode */
:global(.dark) .custom-node {
  background: #374151;
}

:global(.dark) .node-body {
  background: #374151;
}

:global(.dark) .node-title {
  color: #9ca3af;
}
</style>
