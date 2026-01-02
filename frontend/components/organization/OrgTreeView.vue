<script setup lang="ts">
import type { OrganizationChartNode } from 'primevue/organizationchart'
import type { DepartmentTreeDto } from '~/types/department'
import type { OrganizationEmployee } from '~/composables/useOrganization'

interface Props {
  chartData: OrganizationChartNode[]
}

defineProps<Props>()

const emit = defineEmits<{
  selectDepartment: [department: DepartmentTreeDto]
  selectEmployee: [employee: OrganizationEmployee]
}>()

const { getDepartmentFromLookup, findEmployeeById } = useOrganization()

const zoom = ref(1)
const panX = ref(0)
const panY = ref(0)
const wrapperRef = ref<HTMLElement | null>(null)
const containerRef = ref<HTMLElement | null>(null)
const isDragging = ref(false)
const hasDragged = ref(false)
const dragStart = ref({ x: 0, y: 0 })
const isSpacePressed = ref(false)

const handleKeyDown = (e: KeyboardEvent) => {
  if (e.code === 'Space' && !isSpacePressed.value) {
    isSpacePressed.value = true
    e.preventDefault()
  }
}

const handleKeyUp = (e: KeyboardEvent) => {
  if (e.code === 'Space') {
    isSpacePressed.value = false
    isDragging.value = false
  }
}

const fitToWidth = () => {
  const wrapper = wrapperRef.value
  const container = containerRef.value
  if (!wrapper || !container) return

  const contentWidth = container.scrollWidth || container.offsetWidth
  const contentHeight = container.scrollHeight || container.offsetHeight
  const wrapperWidth = wrapper.clientWidth
  const wrapperHeight = wrapper.clientHeight

  if (!contentWidth || !wrapperWidth) return

  const widthScale = wrapperWidth / contentWidth
  const heightScale = contentHeight ? (wrapperHeight / contentHeight) : 1
  const targetScale = Math.min(widthScale, heightScale)
  const newZoom = Math.min(3, Math.max(0.3, targetScale * 0.98))

  zoom.value = newZoom

  const scaledWidth = contentWidth * newZoom
  const scaledHeight = contentHeight * newZoom
  panX.value = (wrapperWidth - scaledWidth) / 2
  panY.value = Math.max(20, (wrapperHeight - scaledHeight) / 2)
}

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown)
  window.addEventListener('keyup', handleKeyUp)
  window.addEventListener('resize', fitToWidth)

  setTimeout(() => {
    fitToWidth()
  }, 100)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown)
  window.removeEventListener('keyup', handleKeyUp)
  window.removeEventListener('resize', fitToWidth)
})

const handleWheel = (e: WheelEvent) => {
  e.preventDefault()

  const wrapper = e.currentTarget as HTMLElement
  const rect = wrapper.getBoundingClientRect()
  const mouseX = e.clientX - rect.left
  const mouseY = e.clientY - rect.top

  const delta = e.deltaY * -0.001
  const oldZoom = zoom.value
  const newZoom = Math.min(Math.max(0.3, oldZoom + delta), 3)

  const pointX = (mouseX - panX.value) / oldZoom
  const pointY = (mouseY - panY.value) / oldZoom

  panX.value = mouseX - pointX * newZoom
  panY.value = mouseY - pointY * newZoom

  zoom.value = newZoom

  if (containerRef.value) {
    containerRef.value.style.transform = `translate3d(${panX.value}px, ${panY.value}px, 0) scale(${newZoom})`
  }
}

const handleMouseDown = (e: MouseEvent) => {
  if (e.button !== 0) return

  const target = e.target as HTMLElement
  if (!isSpacePressed.value && (target.closest('.department-node') || target.closest('.employee-node'))) {
    return
  }

  isDragging.value = true
  hasDragged.value = false
  dragStart.value = {
    x: e.clientX - panX.value,
    y: e.clientY - panY.value
  }
}

const handleMouseMove = (e: MouseEvent) => {
  if (!isDragging.value) return

  const newPanX = e.clientX - dragStart.value.x
  const newPanY = e.clientY - dragStart.value.y

  if (Math.abs(newPanX - panX.value) > 2 || Math.abs(newPanY - panY.value) > 2) {
    hasDragged.value = true
  }

  panX.value = newPanX
  panY.value = newPanY
}

const handleMouseUp = () => {
  isDragging.value = false
}

const handleMouseLeave = () => {
  isDragging.value = false
}

const resetView = () => {
  fitToWidth()
}

const zoomIn = () => {
  zoom.value = Math.min(3, zoom.value + 0.2)
}

const zoomOut = () => {
  zoom.value = Math.max(0.3, zoom.value - 0.2)
}

const transformStyle = computed(() => ({
  transform: `translate3d(${panX.value}px, ${panY.value}px, 0) scale(${zoom.value})`
}))

const wrapperCursor = computed(() => {
  if (isDragging.value) return 'grabbing'
  if (isSpacePressed.value) return 'grab'
  return 'default'
})

const handleDepartmentNodeClick = (event: { node?: OrganizationChartNode } | OrganizationChartNode) => {
  const node = 'node' in event ? event.node : event
  if (!node) return
  const department = getDepartmentFromLookup(node.key as string)
  if (department) {
    emit('selectDepartment', department)
  }
}

const handleEmployeeNodeClick = (node: OrganizationChartNode) => {
  const employee = findEmployeeById(node.data.id)
  if (employee) {
    emit('selectEmployee', employee)
  }
}

watch(zoom, (newZoom) => {
  if (containerRef.value) {
    requestAnimationFrame(() => {
      if (containerRef.value) {
        containerRef.value.style.transform = `translate3d(${panX.value}px, ${panY.value}px, 0) scale(${newZoom})`
        void containerRef.value.offsetHeight
      }
    })
  }
})

defineExpose({
  fitToWidth,
  resetView
})
</script>

<template>
  <div class="p-4 sm:p-6 max-w-full box-border">
    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 mb-4">
      <div class="flex-shrink min-w-0">
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
          Struktura organizacyjna - Drzewo dzialow
        </h2>
        <p class="text-xs sm:text-sm text-gray-600 dark:text-gray-400 mt-1 flex-wrap">
          <span class="inline-flex items-center gap-1">
            <svg class="w-3 h-3 sm:w-4 sm:h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 15l-2 5L9 9l11 4-5 2zm0 0l5 5M7.188 2.239l.777 2.897M5.136 7.965l-2.898-.777M13.95 4.05l-2.122 2.122m-5.657 5.656l-2.12 2.122" />
            </svg>
            Przeciagnij
          </span>
          <span class="mx-1 sm:mx-2">-</span>
          <span class="inline-flex items-center gap-1">
            <svg class="w-3 h-3 sm:w-4 sm:h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            Scroll
          </span>
          <span class="mx-1 sm:mx-2">-</span>
          <span class="inline-flex items-center gap-1">
            <kbd class="px-1 py-0.5 text-xs font-semibold bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded">Space</kbd>
            + drag
          </span>
        </p>
      </div>

      <div class="flex items-center gap-2 flex-shrink-0">
        <span class="text-sm text-gray-600 dark:text-gray-400 mr-2">
          {{ Math.round(zoom * 100) }}%
        </span>
        <button
          class="p-2 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors"
          title="Powieksz"
          data-testid="zoom-in-btn"
          @click="zoomIn"
        >
          <svg class="w-5 h-5 text-gray-700 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0zM10 7v6m3-3H7" />
          </svg>
        </button>
        <button
          class="p-2 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors"
          title="Pomniejsz"
          data-testid="zoom-out-btn"
          @click="zoomOut"
        >
          <svg class="w-5 h-5 text-gray-700 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0zM13 10H7" />
          </svg>
        </button>
        <button
          class="p-2 bg-blue-100 dark:bg-blue-900 hover:bg-blue-200 dark:hover:bg-blue-800 text-blue-700 dark:text-blue-300 rounded-lg transition-colors"
          title="Resetuj widok"
          data-testid="reset-view-btn"
          @click="resetView"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
        </button>
      </div>
    </div>

    <div
      ref="wrapperRef"
      class="org-chart-wrapper"
      :style="{ cursor: wrapperCursor }"
      @wheel="handleWheel"
      @mousedown="handleMouseDown"
      @mousemove="handleMouseMove"
      @mouseup="handleMouseUp"
      @mouseleave="handleMouseLeave"
    >
      <div ref="containerRef" class="org-chart-container" :style="transformStyle">
        <OrganizationChart
          v-for="rootDept in chartData"
          :key="rootDept.key"
          :value="rootDept"
          collapsible
          selection-mode="single"
          class="mb-8"
          @node-select="handleDepartmentNodeClick"
        >
          <template #department="slotProps">
            <div class="department-node">
              <div class="department-name">
                {{ slotProps.node.data.name }}
              </div>
              <div class="department-manager">
                Dyrektor: {{ slotProps.node.data.director }}
              </div>
              <div class="department-stats">
                <span class="employee-badge">
                  {{ slotProps.node.data.employeeCount }} pracownikow
                </span>
              </div>
            </div>
          </template>

          <template #employee="slotProps">
            <div class="employee-node" @click="handleEmployeeNodeClick(slotProps.node)">
              <div class="employee-avatar">
                <img
                  v-if="slotProps.node.data.profilePhotoUrl"
                  :src="slotProps.node.data.profilePhotoUrl"
                  :alt="`${slotProps.node.data.firstName} ${slotProps.node.data.lastName}`"
                  class="employee-avatar-img"
                >
                <span v-else>
                  {{ slotProps.node.data.firstName[0] }}{{ slotProps.node.data.lastName[0] }}
                </span>
              </div>
              <div class="employee-name">
                {{ slotProps.node.data.firstName }} {{ slotProps.node.data.lastName }}
              </div>
              <div class="employee-position">
                {{ slotProps.node.data.position || 'Brak stanowiska' }}
              </div>
              <div v-if="slotProps.node.data.isHead" class="employee-head-badge">
                Kierownik
              </div>
            </div>
          </template>
        </OrganizationChart>
      </div>
    </div>
  </div>
</template>

<style scoped>
.org-chart-wrapper {
  width: 100%;
  max-width: 100%;
  height: calc(100vh - 450px);
  min-height: 500px;
  max-height: 700px;
  overflow: hidden;
  position: relative;
  margin: 0;
  background: linear-gradient(90deg, rgba(0,0,0,0.02) 1px, transparent 1px),
              linear-gradient(rgba(0,0,0,0.02) 1px, transparent 1px);
  background-size: 40px 40px;
  background-position: 0 0;
  user-select: none;
  border-radius: 8px;
  box-sizing: border-box;
}

.org-chart-container {
  width: max-content;
  height: max-content;
  min-width: 100%;
  min-height: 100%;
  padding: 20px;
  position: relative;
  transform-origin: 0 0;
  transition: none;
  will-change: transform;
  pointer-events: none;
  box-sizing: border-box;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-rendering: optimizeLegibility;
  backface-visibility: hidden;
  -webkit-backface-visibility: hidden;
  transform-style: preserve-3d;
}

.department-node {
  min-width: 220px;
  padding: 16px;
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
  border: 2px solid #1e40af;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(37, 99, 235, 0.2);
  cursor: pointer !important;
  transition: all 0.3s ease;
  text-align: center;
  color: white;
  user-select: none;
  pointer-events: auto;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-rendering: optimizeLegibility;
}

.department-node:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(37, 99, 235, 0.3);
  background: linear-gradient(135deg, #1d4ed8 0%, #1e40af 100%);
}

.employee-node {
  min-width: 180px;
  padding: 14px;
  margin: 8px;
  background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%);
  border: 2px solid #374151;
  border-radius: 10px;
  box-shadow: 0 4px 6px rgba(107, 114, 128, 0.2);
  cursor: pointer !important;
  transition: all 0.3s ease;
  text-align: center;
  color: white;
  user-select: none;
  pointer-events: auto;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-rendering: optimizeLegibility;
}

.employee-node:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(107, 114, 128, 0.3);
  background: linear-gradient(135deg, #4b5563 0%, #374151 100%);
}

.employee-avatar {
  width: 48px;
  height: 48px;
  margin: 0 auto 8px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  font-weight: 700;
  border: 2px solid rgba(255, 255, 255, 0.5);
  overflow: hidden;
}

.employee-avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 50%;
  image-rendering: -webkit-optimize-contrast;
  image-rendering: crisp-edges;
  -ms-interpolation-mode: nearest-neighbor;
}

.employee-name {
  font-size: 0.95rem;
  font-weight: 700;
  margin-bottom: 4px;
  line-height: 1.2;
}

.employee-position {
  font-size: 0.75rem;
  opacity: 0.9;
  font-weight: 500;
  margin-bottom: 4px;
}

.employee-head-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 3px 10px;
  background: rgba(255, 255, 255, 0.3);
  border-radius: 10px;
  display: inline-block;
  margin-top: 6px;
  border: 1px solid rgba(255, 255, 255, 0.4);
}

.department-name {
  font-size: 1.1rem;
  font-weight: 700;
  margin-bottom: 8px;
  line-height: 1.3;
}

.department-manager {
  font-size: 0.875rem;
  opacity: 0.95;
  margin-bottom: 8px;
  font-weight: 500;
}

.department-stats {
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.3);
}

.employee-badge {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 4px 12px;
  background: rgba(255, 255, 255, 0.25);
  border-radius: 12px;
  display: inline-block;
}

:deep(.p-organizationchart) {
  padding: 10px;
  max-width: 100%;
  box-sizing: border-box;
}

:deep(.p-organizationchart-table) {
  max-width: 100%;
}

:deep(.p-organizationchart-node-content) {
  border: none !important;
  padding: 0 !important;
}

:deep(.p-organizationchart-connector-down) {
  width: 2px !important;
  height: 20px !important;
  background-color: #2563eb !important;
  margin: 0 auto !important;
}

:deep(.p-organizationchart-connector-left),
:deep(.p-organizationchart-connector-right) {
  border-top: 2px solid #2563eb !important;
}

:deep(.p-organizationchart-connector-left) {
  border-right: 2px solid #2563eb !important;
}

:deep(.p-organizationchart-connector-right) {
  border-left: 2px solid #2563eb !important;
}

:global(.dark) .org-chart-wrapper {
  background: linear-gradient(90deg, rgba(255,255,255,0.05) 1px, transparent 1px),
              linear-gradient(rgba(255,255,255,0.05) 1px, transparent 1px);
  background-size: 40px 40px;
}

:global(.dark) .department-node {
  background: linear-gradient(135deg, #1e40af 0%, #1e3a8a 100%);
  border-color: #1d4ed8;
}

:global(.dark) .employee-node {
  background: linear-gradient(135deg, #4b5563 0%, #374151 100%);
  border-color: #6b7280;
}

:global(.dark) :deep(.p-organizationchart-connector-down) {
  background-color: #3b82f6 !important;
}

:global(.dark) :deep(.p-organizationchart-connector-left),
:global(.dark) :deep(.p-organizationchart-connector-right) {
  border-top-color: #3b82f6 !important;
}

:global(.dark) :deep(.p-organizationchart-connector-left) {
  border-right-color: #3b82f6 !important;
}

:global(.dark) :deep(.p-organizationchart-connector-right) {
  border-left-color: #3b82f6 !important;
}
</style>
