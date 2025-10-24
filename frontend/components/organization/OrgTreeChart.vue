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

// Pan and zoom functionality
const chartContainer = ref<HTMLElement | null>(null)
const chartContent = ref<HTMLElement | null>(null)
const scale = ref(1)
const position = ref({ x: 0, y: 0 })
const isPanning = ref(false)
const startPos = ref({ x: 0, y: 0 })

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

// Pan functionality
const handleMouseDown = (e: MouseEvent) => {
  // Check if clicked on a node or node-related element
  const target = e.target as HTMLElement
  const isNode = target.closest('.custom-node') || target.closest('.p-organizationchart-node')

  // Only pan with left mouse button when NOT clicking on nodes
  if (e.button === 0 && !isNode) {
    isPanning.value = true
    startPos.value = {
      x: e.clientX - position.value.x,
      y: e.clientY - position.value.y
    }
    e.preventDefault()
  }
}

const handleMouseMove = (e: MouseEvent) => {
  if (isPanning.value) {
    position.value = {
      x: e.clientX - startPos.value.x,
      y: e.clientY - startPos.value.y
    }
  }
}

const handleMouseUp = () => {
  isPanning.value = false
}

const handleWheel = (e: WheelEvent) => {
  e.preventDefault()
  const delta = e.deltaY > 0 ? -0.1 : 0.1
  const newScale = Math.max(0.5, Math.min(2, scale.value + delta))
  scale.value = newScale
}

// Reset view
const resetView = () => {
  scale.value = 1
  position.value = { x: 0, y: 0 }
}

// Computed transform style
const transformStyle = computed(() => {
  return `translate(${position.value.x}px, ${position.value.y}px) scale(${scale.value})`
})

// Add event listeners
onMounted(() => {
  if (chartContainer.value) {
    chartContainer.value.addEventListener('wheel', handleWheel, { passive: false })
    window.addEventListener('mouseup', handleMouseUp)
    window.addEventListener('mousemove', handleMouseMove)
  }
})

onUnmounted(() => {
  if (chartContainer.value) {
    chartContainer.value.removeEventListener('wheel', handleWheel)
  }
  window.removeEventListener('mouseup', handleMouseUp)
  window.removeEventListener('mousemove', handleMouseMove)
})
</script>

<template>
  <div class="org-chart-wrapper">
    <!-- Controls -->
    <div class="chart-controls">
      <button
        class="control-btn"
        @click="scale = Math.min(2, scale + 0.1)"
        title="Powiększ"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0zM10 7v6m3-3H7" />
        </svg>
      </button>
      <button
        class="control-btn"
        @click="scale = Math.max(0.5, scale - 0.1)"
        title="Pomniejsz"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0zM13 10H7" />
        </svg>
      </button>
      <button
        class="control-btn"
        @click="resetView"
        title="Resetuj widok"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
      </button>
      <div class="zoom-indicator">{{ Math.round(scale * 100) }}%</div>
    </div>

    <!-- Chart container with pan/zoom -->
    <div
      ref="chartContainer"
      class="org-tree-chart-container"
      :class="{ 'is-panning': isPanning }"
      @mousedown="handleMouseDown"
    >
      <div
        ref="chartContent"
        class="org-tree-chart-content"
        :style="{ transform: transformStyle }"
      >
        <OrganizationChart
          :value="data"
          collapsible
          selection-mode="single"
          @node-select="handleNodeClick"
        >
          <template #person="slotProps">
            <div
              class="custom-node"
              :style="{
                backgroundColor: slotProps.node.data.color + '20',
                borderColor: slotProps.node.data.color
              }"
            >
              <div class="node-name" :style="{ color: slotProps.node.data.color }">
                {{ slotProps.node.data.name }}
              </div>
              <div class="node-title">{{ slotProps.node.data.title }}</div>
            </div>
          </template>
        </OrganizationChart>
      </div>
    </div>

    <div class="chart-hint">
      💡 Użyj kółka myszy aby powiększyć/pomniejszyć, przeciągnij myszką aby przesunąć
    </div>
  </div>
</template>

<style scoped>
.org-chart-wrapper {
  position: relative;
  width: 100%;
  height: 700px;
  background: #f9fafb;
  border-radius: 12px;
  overflow: hidden;
}

:global(.dark) .org-chart-wrapper {
  background: #1f2937;
}

/* Controls */
.chart-controls {
  position: absolute;
  top: 20px;
  right: 20px;
  display: flex;
  gap: 8px;
  z-index: 10;
  background: white;
  padding: 8px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

:global(.dark) .chart-controls {
  background: #374151;
}

.control-btn {
  padding: 8px;
  background: #f3f4f6;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.control-btn:hover {
  background: #e5e7eb;
  transform: scale(1.05);
}

:global(.dark) .control-btn {
  background: #4b5563;
  color: white;
}

:global(.dark) .control-btn:hover {
  background: #6b7280;
}

.zoom-indicator {
  display: flex;
  align-items: center;
  padding: 0 12px;
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
}

:global(.dark) .zoom-indicator {
  color: #e5e7eb;
}

/* Chart container */
.org-tree-chart-container {
  width: 100%;
  height: 100%;
  overflow: hidden;
  cursor: grab;
  position: relative;
}

.org-tree-chart-container.is-panning {
  cursor: grabbing;
}

.org-tree-chart-content {
  transform-origin: center;
  transition: transform 0.1s ease-out;
  display: inline-block;
  min-width: 100%;
  min-height: 100%;
  padding: 60px;
}

/* Hint */
.chart-hint {
  position: absolute;
  bottom: 20px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(0, 0, 0, 0.75);
  color: white;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 0.75rem;
  z-index: 10;
  pointer-events: none;
}

/* Custom nodes */
.custom-node {
  width: 140px;
  min-height: 60px;
  padding: 12px;
  border: 2px solid;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
}

.custom-node:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.node-name {
  font-size: 0.875rem;
  font-weight: 600;
  text-align: center;
  line-height: 1.2;
}

.node-title {
  font-size: 0.75rem;
  color: #6b7280;
  text-align: center;
  line-height: 1.2;
}

/* Dark mode */
:global(.dark) .node-title {
  color: #9ca3af;
}

/* PrimeVue OrganizationChart connection lines */
:deep(.p-organizationchart) {
  padding: 0;
}

:deep(.p-organizationchart-table) {
  border-spacing: 0;
  border-collapse: separate;
}

:deep(.p-organizationchart-node-content) {
  border: none !important;
  padding: 0 !important;
}

/* Vertical lines (down) */
:deep(.p-organizationchart-line-down) {
  background-color: #94a3b8;
  width: 2px;
  height: 20px;
  margin: 0 auto;
}

/* Horizontal lines (left and right connectors) */
:deep(.p-organizationchart-line-left),
:deep(.p-organizationchart-line-right) {
  background-color: #94a3b8;
  height: 2px;
  width: 100%;
}

/* Top border for child nodes */
:deep(.p-organizationchart-line-top) {
  border-top: 2px solid #94a3b8;
  height: 20px;
}

/* Dark mode lines */
:global(.dark) :deep(.p-organizationchart-line-down),
:global(.dark) :deep(.p-organizationchart-line-left),
:global(.dark) :deep(.p-organizationchart-line-right) {
  background-color: #64748b;
}

:global(.dark) :deep(.p-organizationchart-line-top) {
  border-top-color: #64748b;
}

/* Ensure lines container is visible */
:deep(.p-organizationchart-lines) {
  opacity: 1 !important;
  visibility: visible !important;
}

/* Connectors spacing */
:deep(.p-organizationchart-node-cell) {
  padding: 0 0.5rem;
}

:deep(.p-organizationchart-lines-cell) {
  padding: 0;
}
</style>
