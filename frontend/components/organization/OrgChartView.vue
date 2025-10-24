<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue'
import type { EChartsOption, EChartsType } from 'echarts'
import { use as useECharts } from 'echarts/core'
import { TreeChart } from 'echarts/charts'
import { TooltipComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import VueECharts from 'vue-echarts'
import type { Employee } from '~/types'
import { useColorMode } from '#imports'

useECharts([TreeChart, TooltipComponent, CanvasRenderer])

interface Props {
  employee: Employee
  onSelectEmployee?: (employee: Employee) => void
}

const props = defineProps<Props>()
const emit = defineEmits<{
  selectEmployee: [employee: Employee]
}>()

const colorMode = useColorMode()
const isDark = computed(() => colorMode.value === 'dark')

const chartRef = ref<InstanceType<typeof VueECharts> | null>(null)
const chartInstance = ref<EChartsType | null>(null)

const handleNodeSelect = (employee: Employee) => {
  emit('selectEmployee', employee)
  if (props.onSelectEmployee) {
    props.onSelectEmployee(employee)
  }
}

const handleChartNodeClick = (params: any) => {
  const employee = params?.data?.originalEmployee as Employee | undefined
  if (employee) {
    handleNodeSelect(employee)
  }
}

const buildTreeNode = (employee: Employee, darkMode: boolean): any => {
  const departmentName = employee.department?.name ?? ''
  const departmentColor = employee.department?.color ?? '#3b82f6'
  const positionName = employee.position?.name ?? ''

  const cardBackground = darkMode ? '#1f2937' : '#ffffff'
  const baseTextColor = darkMode ? '#f9fafb' : '#111827'
  const metaTextColor = darkMode ? '#9ca3af' : '#6b7280'
  const shadowColor = darkMode ? 'rgba(0,0,0,0.35)' : 'rgba(15,23,42,0.12)'

  const children = (employee.subordinates ?? []).map(child =>
    buildTreeNode(child, darkMode)
  )

  const node: any = {
    name: `${employee.firstName} ${employee.lastName}`,
    value: employee.id,
    rawFullName: `${employee.firstName} ${employee.lastName}`,
    rawPosition: positionName,
    rawDepartment: departmentName,
    departmentColor,
    originalEmployee: employee,
    children,
    symbol: 'circle',
    symbolSize: 18,
    label: {
      position: 'inside',
      align: 'center',
      verticalAlign: 'middle',
      backgroundColor: cardBackground,
      borderColor: departmentColor,
      borderWidth: 2,
      borderRadius: 12,
      padding: [14, 18],
      color: baseTextColor,
      fontSize: 13,
      lineHeight: 18,
      shadowColor,
      shadowBlur: 14,
      shadowOffsetX: 0,
      shadowOffsetY: 6,
      formatter: (params: any) => {
        const data = params.data as any
        const lines: string[] = [data.rawFullName]
        if (data.rawPosition) {
          lines.push(`{position|${data.rawPosition}}`)
        }
        if (data.rawDepartment) {
          lines.push(`{department|${data.rawDepartment}}`)
        }
        return lines.join('\n')
      },
      rich: {
        position: {
          color: metaTextColor,
          fontSize: 11,
          padding: [4, 0, 0, 0]
        },
        department: {
          color: '#ffffff',
          fontSize: 11,
          backgroundColor: departmentColor,
          borderRadius: 999,
          padding: [2, 10],
          marginTop: 6
        }
      }
    }
  }

  if (children.length === 0) {
    delete node.children
  }

  return node
}

const chartOptions = computed<EChartsOption>(() => {
  if (!props.employee) {
    return { series: [] }
  }

  const treeData = buildTreeNode(props.employee, isDark.value)

  const linkColor = isDark.value ? '#4b5563' : '#d1d5db'
  const tooltipText = isDark.value ? '#f9fafb' : '#1f2937'
  const tooltipSubtext = isDark.value ? '#d1d5db' : '#4b5563'
  const tooltipBackground = isDark.value ? '#111827' : '#f9fafb'
  const tooltipBorder = isDark.value ? '#374151' : '#e5e7eb'

  const option: EChartsOption = {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'item',
      triggerOn: 'mousemove',
      confine: true,
      padding: [10, 14],
      backgroundColor: tooltipBackground,
      borderColor: tooltipBorder,
      borderWidth: 1,
      textStyle: {
        color: tooltipText,
        fontSize: 12,
        lineHeight: 18
      },
      formatter: (params: any) => {
        const data = params.data as any
        const lines: string[] = [
          `<div style="font-weight:600;margin-bottom:4px;color:${tooltipText};">${data.rawFullName}</div>`
        ]
        if (data.rawPosition) {
          lines.push(`<div style="color:${tooltipSubtext};">${data.rawPosition}</div>`)
        }
        if (data.rawDepartment) {
          lines.push(`<div style="margin-top:6px;font-weight:600;color:${data.departmentColor};">${data.rawDepartment}</div>`)
        }
        if (data.originalEmployee?.email) {
          lines.push(`<div style="margin-top:6px;">${data.originalEmployee.email}</div>`)
        }
        if (data.originalEmployee?.phone) {
          lines.push(`<div style="color:${tooltipSubtext};">${data.originalEmployee.phone}</div>`)
        }
        return lines.join('')
      }
    },
    series: [
      {
        type: 'tree',
        data: [treeData],
        top: 40,
        left: '2%',
        bottom: 40,
        right: '18%',
        orient: 'TB',
        layout: 'orthogonal',
        initialTreeDepth: 8,
        symbol: 'emptyCircle',
        symbolSize: 8,
        roam: true,
        expandAndCollapse: true,
        animationDuration: 600,
        animationDurationUpdate: 750,
        lineStyle: {
          color: linkColor,
          width: 1.6
        },
        label: {
          position: 'inside',
          align: 'center',
          verticalAlign: 'middle',
          color: isDark.value ? '#f9fafb' : '#111827',
          fontSize: 12,
          backgroundColor: 'transparent'
        },
        leaves: {
          label: {
            position: 'inside',
            align: 'center',
            verticalAlign: 'middle'
          }
        },
        emphasis: {
          focus: 'descendant'
        }
      }
    ]
  }

  return option
})

const handleChartReady = (chart: EChartsType) => {
  chartInstance.value = chart
  chart.off('click', handleChartNodeClick)
  chart.on('click', handleChartNodeClick)
}

onBeforeUnmount(() => {
  chartInstance.value?.off('click', handleChartNodeClick)
})
</script>

<template>
  <div class="w-full h-full bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden">
    <VueECharts
      ref="chartRef"
      class="org-chart-container"
      :option="chartOptions"
      :autoresize="true"
      @chart-ready="handleChartReady"
    />
  </div>
</template>

<style scoped>
.org-chart-container {
  width: 100%;
  height: 100%;
  min-height: 600px;
}
</style>
