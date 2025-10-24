<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import type { Employee } from '~/types'
import * as echarts from 'echarts/core'
import { TreeChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  ToolboxComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  TitleComponent,
  TooltipComponent,
  GridComponent,
  TreeChart,
  CanvasRenderer,
  ToolboxComponent
])

interface Props {
  employee: Employee
  onSelectEmployee: (employee: Employee) => void
}

const props = defineProps<Props>()

const chartRef = ref<HTMLDivElement | null>(null)
let chartInstance: echarts.ECharts | null = null
let removeResizeListener: (() => void) | null = null

const BASE_NODE_WIDTH = 240
const BASE_LAYER_HEIGHT = 240
const BASE_LABEL_WIDTH = 190
const BASE_NAME_FONT = 13
const BASE_POSITION_FONT = 11
const BASE_DEPARTMENT_FONT = 10
const MIN_SCALE_FLOOR = 0.02
const MAX_SCALE_CEILING = 8

let currentVisualScale = 1

const { getDepartments } = useMockData()
const departments = getDepartments()

const getDepartmentColor = (employee: Employee) => {
  return employee.department?.color || '#3b82f6'
}

const clampScale = (scale: number) => Math.min(1.2, Math.max(scale, 0.4))

const buildLabelConfig = (color: string) => {
  const scale = clampScale(currentVisualScale)
  const labelWidth = Math.max(140, Math.round(BASE_LABEL_WIDTH * scale))
  const nameFontSize = Math.max(11, Math.round(BASE_NAME_FONT * Math.max(scale, 0.85)))
  const positionFontSize = Math.max(10, Math.round(BASE_POSITION_FONT * Math.max(scale, 0.8)))
  const departmentFontSize = Math.max(9, Math.round(BASE_DEPARTMENT_FONT * Math.max(scale, 0.75)))
  const primaryLineHeight = Math.round(20 * Math.max(scale, 0.8))
  const secondaryLineHeight = Math.round(18 * Math.max(scale, 0.75))
  const verticalPadding = Math.round(8 * Math.max(scale, 0.8))

  return {
    backgroundColor: 'rgba(255, 255, 255, 0.95)',
    borderColor: color,
    borderWidth: Math.max(1, 1.2 * scale),
    borderRadius: 6,
    padding: [verticalPadding, 10, verticalPadding, 10],
    width: labelWidth,
    overflow: 'truncate',
    color: '#1f2937',
    fontSize: nameFontSize,
    fontWeight: 'bold',
    rich: {
      name: {
        fontSize: nameFontSize,
        fontWeight: 'bold',
        color: '#1f2937',
        lineHeight: primaryLineHeight,
        width: labelWidth,
        overflow: 'truncate'
      },
      position: {
        fontSize: positionFontSize,
        color: '#4b5563',
        lineHeight: secondaryLineHeight,
        width: labelWidth,
        overflow: 'truncate'
      },
      dept: {
        fontSize: departmentFontSize,
        color: '#6b7280',
        lineHeight: secondaryLineHeight,
        width: labelWidth,
        overflow: 'truncate'
      }
    }
  }
}

// Convert employee hierarchy to ECharts tree data format
const convertToTreeData = (employee: Employee): any => {
  const color = getDepartmentColor(employee)

  const node: any = {
    name: `${employee.firstName} ${employee.lastName}`,
    value: employee.id,
    itemStyle: {
      color: color,
      borderColor: color,
      borderWidth: getNodeBorderWidth(currentVisualScale)
    },
    label: {
      ...buildLabelConfig(color),
      formatter: (params: any) => {
        const emp = findEmployeeById(params.value)
        if (!emp) return params.name
        const fullName = `${emp.firstName} ${emp.lastName}`
        const position = emp.position?.name || ''
        const dept = emp.department?.name || ''
        return `{name|${fullName}}\n{position|${position}}\n{dept|${dept}}`
      }
    },
    tooltip: {
      formatter: (params: any) => {
        const emp = findEmployeeById(params.value)
        if (!emp) return params.name
        return `
          <div style="padding: 8px; max-width: 250px;">
            <div style="font-weight: bold; font-size: 14px; margin-bottom: 4px;">${emp.firstName} ${emp.lastName}</div>
            <div style="font-size: 12px; color: #666; margin-bottom: 2px;">${emp.position?.name || ''}</div>
            <div style="font-size: 11px; color: #999;">${emp.department?.name || ''}</div>
            <div style="font-size: 11px; color: #999; margin-top: 4px;">${emp.email}</div>
          </div>
        `
      }
    },
    employeeData: employee
  }

  if (employee.subordinates && employee.subordinates.length > 0) {
    node.children = employee.subordinates.map(sub => convertToTreeData(sub))
  }

  return node
}

// Helper function to find employee by ID
const findEmployeeById = (id: number): Employee | null => {
  const search = (emp: Employee): Employee | null => {
    if (emp.id === id) return emp
    if (emp.subordinates) {
      for (const sub of emp.subordinates) {
        const found = search(sub)
        if (found) return found
      }
    }
    return null
  }
  return search(props.employee)
}

const calculateTreeMetrics = (employee: Employee) => {
  let maxDepth = 0
  const levelCounts: Record<number, number> = {}

  const traverse = (node: Employee, depth: number) => {
    maxDepth = Math.max(maxDepth, depth)
    levelCounts[depth] = (levelCounts[depth] || 0) + 1
    node.subordinates?.forEach(sub => traverse(sub, depth + 1))
  }

  traverse(employee, 0)

  const maxBreadth = Object.values(levelCounts).reduce((acc, count) => Math.max(acc, count), 0) || 1

  return { maxDepth, maxBreadth }
}

const treeMetrics = computed(() => calculateTreeMetrics(props.employee))

const computeLayoutMetrics = () => {
  const { maxDepth, maxBreadth } = treeMetrics.value
  // Znacznie zwiększone odstępy, aby węzły się nie nakładały
  const nodeGap = 250 + maxBreadth * 30  // Duży odstęp poziomy między węzłami
  const layerGap = 280 + maxDepth * 25   // Duży odstęp pionowy między poziomami
  const requiredWidth = Math.max(maxBreadth, 1) * (BASE_NODE_WIDTH + nodeGap) + 500
  const requiredHeight = Math.max(maxDepth + 1, 1) * (BASE_LAYER_HEIGHT + layerGap) + 450
  return { nodeGap, layerGap, requiredWidth, requiredHeight }
}

const getAutoScaleConfig = () => {
  const containerWidth = chartRef.value?.clientWidth ?? 800
  const containerHeight = chartRef.value?.clientHeight ?? 600

  const { requiredWidth, requiredHeight } = computeLayoutMetrics()

  const widthScale = containerWidth / requiredWidth
  const heightScale = containerHeight / requiredHeight
  // Automatyczne dopasowanie skali, aby cały wykres był widoczny
  const autoScale = Math.max(MIN_SCALE_FLOOR, Math.min(0.7, widthScale, heightScale))
  const minScale = Math.max(MIN_SCALE_FLOOR, autoScale * 0.3)

  return { autoScale, minScale }
}

const getSymbolSize = (scale: number) => Math.max(10, Math.round(20 * Math.max(scale, 0.7)))
const getLineWidth = (scale: number) => Math.max(1.5, Number((2.5 * Math.max(scale, 0.7)).toFixed(1)))
const getNodeBorderWidth = (scale: number) => Math.max(1.5, Number((2.5 * Math.max(scale, 0.7)).toFixed(1)))

const buildSeriesLabel = (scale: number, isLeaf = false) => {
  const effective = clampScale(scale)
  const baseDistance = isLeaf ? 22 : 26
  const distance = Math.max(12, Math.round(baseDistance * effective))
  return {
    position: isLeaf ? 'bottom' : 'top',
    distance,
    verticalAlign: 'middle',
    align: 'center'
  }
}

const refreshSeriesVisuals = (scale: number, includeData = false) => {
  currentVisualScale = clampScale(scale)
  if (!chartInstance) return

  const symbolSize = getSymbolSize(currentVisualScale)
  const lineWidth = getLineWidth(currentVisualScale)

  chartInstance.setOption({
    series: [
      {
        zoom: currentVisualScale,
        symbolSize,
        label: buildSeriesLabel(currentVisualScale, false),
        leaves: {
          label: buildSeriesLabel(currentVisualScale, true)
        },
        lineStyle: {
          color: '#cbd5e1',
          width: lineWidth,
          curveness: 0.5
        },
        emphasis: {
          focus: 'descendant',
          lineStyle: {
            width: lineWidth + 1.5,
            color: '#3b82f6'
          },
          itemStyle: {
            shadowBlur: 10,
            shadowColor: 'rgba(0, 0, 0, 0.3)'
          }
        },
        itemStyle: {
          borderWidth: getNodeBorderWidth(currentVisualScale)
        },
        ...(includeData ? { data: [convertToTreeData(props.employee)] } : {})
      }
    ]
  }, false, false)
}

const updateScaleLimits = (minScale: number) => {
  if (!chartInstance) return

  chartInstance.setOption({
    series: [
      {
        scaleLimit: {
          min: minScale,
          max: MAX_SCALE_CEILING
        }
      }
    ]
  }, false, false)
}

const applyInitialZoom = (targetScale: number) => {
  if (!chartInstance || !chartRef.value) return

  if (targetScale >= 1) {
    return
  }

  try {
    chartInstance.dispatchAction({
      type: 'treeRoam',
      zoom: targetScale,
      originX: chartRef.value.clientWidth / 2,
      originY: 0
    })
  } catch (error) {
    console.warn('Failed to apply initial tree zoom', error)
  }
}

const handleTreeRoam = () => {
  if (!chartInstance) return
  const option = chartInstance.getOption()
  const series = Array.isArray(option.series) ? option.series[0] : null
  const zoom = typeof series?.zoom === 'number' ? series.zoom : currentVisualScale
  const clamped = Math.max(MIN_SCALE_FLOOR, Math.min(zoom, MAX_SCALE_CEILING))
  if (Math.abs(clamped - currentVisualScale) > 0.01) {
    refreshSeriesVisuals(clamped, true)
  }
}

const initChart = () => {
  if (!chartRef.value) return

  if (chartInstance) {
    chartInstance.off('treeRoam', handleTreeRoam)
    removeResizeListener?.()
    chartInstance.dispose()
    removeResizeListener = null
  }

  chartInstance = echarts.init(chartRef.value)

  const { autoScale, minScale } = getAutoScaleConfig()
  const { nodeGap, layerGap } = computeLayoutMetrics()
  currentVisualScale = clampScale(autoScale)

  const initialData = convertToTreeData(props.employee)
  const initialSymbolSize = getSymbolSize(currentVisualScale)
  const initialLineWidth = getLineWidth(currentVisualScale)

  const option = {
    tooltip: {
      trigger: 'item',
      triggerOn: 'mousemove',
      confine: true
    },
    toolbox: {
      show: true,
      feature: {
        restore: { show: true, title: 'Resetuj widok' },
        saveAsImage: { show: true, title: 'Zapisz jako obraz', pixelRatio: 2 }
      },
      right: 16,
      top: 16
    },
    series: [
      {
        type: 'tree',
        data: [initialData],
        top: '3%',
        left: '2%',
        bottom: '3%',
        right: '2%',
        symbolSize: initialSymbolSize,
        orient: 'vertical',
        expandAndCollapse: true,
        initialTreeDepth: 2,
        animationDuration: 450,
        animationDurationUpdate: 450,
        layout: 'orthogonal',
        roam: true,
        scaleLimit: {
          min: minScale,
          max: MAX_SCALE_CEILING
        },
        nodeGap,
        layerGap,
        label: buildSeriesLabel(currentVisualScale, false),
        leaves: {
          label: buildSeriesLabel(currentVisualScale, true)
        },
        lineStyle: {
          color: '#cbd5e1',
          width: initialLineWidth,
          curveness: 0.5
        },
        emphasis: {
          focus: 'descendant',
          lineStyle: {
            width: initialLineWidth + 1.5,
            color: '#3b82f6'
          },
          itemStyle: {
            shadowBlur: 10,
            shadowColor: 'rgba(0, 0, 0, 0.3)'
          }
        },
        itemStyle: {
          borderWidth: getNodeBorderWidth(currentVisualScale)
        }
      }
    ]
  }

  chartInstance.setOption(option)
  applyInitialZoom(autoScale)
  refreshSeriesVisuals(currentVisualScale, true)

  chartInstance.on('click', (params: any) => {
    if (params.data && params.data.employeeData) {
      props.onSelectEmployee(params.data.employeeData)
    }
  })

  const handleResize = () => {
    chartInstance?.resize()
    const { autoScale: resizedAuto, minScale: updatedMin } = getAutoScaleConfig()
    updateScaleLimits(updatedMin)
    const option = chartInstance?.getOption()
    const currentZoom = Array.isArray(option?.series) && typeof option.series[0]?.zoom === 'number'
      ? option.series[0].zoom
      : currentVisualScale
    const targetScale = Math.min(resizedAuto, currentZoom)
    chartInstance?.setOption({ series: [{ zoom: targetScale }] }, false, false)
    refreshSeriesVisuals(targetScale, true)
  }

  window.addEventListener('resize', handleResize)
  removeResizeListener = () => {
    window.removeEventListener('resize', handleResize)
  }

  chartInstance.on('treeRoam', handleTreeRoam)
}

onMounted(() => {
  initChart()
})

watch(() => props.employee, () => {
  initChart()
}, { deep: true })

// Cleanup on unmount
onUnmounted(() => {
  removeResizeListener?.()
  if (chartInstance) {
    chartInstance.off('treeRoam', handleTreeRoam)
    chartInstance.dispose()
    chartInstance = null
  }
})
</script>

<template>

  <div class="w-full">

    <div class="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 shadow-lg overflow-hidden">

      <div

        ref="chartRef"

        class="w-full"

        style="min-height: clamp(560px, 70vh, 820px); cursor: grab;"

      />

    </div>

  </div>

</template>





