<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import type { Employee } from '~/types'
import * as echarts from 'echarts/core'
import { TreeChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  GridComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  TitleComponent,
  TooltipComponent,
  GridComponent,
  TreeChart,
  CanvasRenderer
])

interface Props {
  employee: Employee
  onSelectEmployee: (employee: Employee) => void
}

const props = defineProps<Props>()

const chartRef = ref<HTMLDivElement | null>(null)
let chartInstance: echarts.ECharts | null = null

const { getDepartments } = useMockData()
const departments = getDepartments()

const getDepartmentColor = (employee: Employee) => {
  return employee.department?.color || '#3b82f6'
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
      borderWidth: 2
    },
    label: {
      backgroundColor: 'rgba(255, 255, 255, 0.9)',
      borderColor: color,
      borderWidth: 1,
      borderRadius: 4,
      padding: [8, 12],
      color: '#333',
      fontSize: 12,
      fontWeight: 'bold',
      formatter: (params: any) => {
        const emp = findEmployeeById(employee.id)
        if (!emp) return params.name
        return `{name|${params.name}}\n{position|${emp.position?.name || ''}}\n{dept|${emp.department?.name || ''}}`
      },
      rich: {
        name: {
          fontSize: 13,
          fontWeight: 'bold',
          color: '#1f2937',
          lineHeight: 20
        },
        position: {
          fontSize: 11,
          color: '#6b7280',
          lineHeight: 18
        },
        dept: {
          fontSize: 10,
          color: '#9ca3af',
          lineHeight: 16
        }
      }
    },
    tooltip: {
      formatter: (params: any) => {
        const emp = findEmployeeById(params.value)
        if (!emp) return params.name
        return `
          <div style="padding: 8px;">
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

const treeData = computed(() => convertToTreeData(props.employee))

const initChart = () => {
  if (!chartRef.value) return

  // Dispose existing chart
  if (chartInstance) {
    chartInstance.dispose()
  }

  chartInstance = echarts.init(chartRef.value)

  const option = {
    tooltip: {
      trigger: 'item',
      triggerOn: 'mousemove',
      confine: true
    },
    series: [
      {
        type: 'tree',
        data: [treeData.value],
        top: '5%',
        left: '10%',
        bottom: '5%',
        right: '20%',
        symbolSize: 12,
        orient: 'vertical',
        expandAndCollapse: true,
        initialTreeDepth: 3,
        animationDuration: 550,
        animationDurationUpdate: 750,
        label: {
          position: 'top',
          distance: 15,
          verticalAlign: 'middle',
          align: 'center'
        },
        leaves: {
          label: {
            position: 'bottom',
            verticalAlign: 'middle',
            align: 'center'
          }
        },
        lineStyle: {
          color: '#ccc',
          width: 2,
          curveness: 0.5
        },
        emphasis: {
          focus: 'descendant',
          lineStyle: {
            width: 3
          }
        }
      }
    ]
  }

  chartInstance.setOption(option)

  // Handle click events
  chartInstance.on('click', (params: any) => {
    if (params.data && params.data.employeeData) {
      props.onSelectEmployee(params.data.employeeData)
    }
  })

  // Handle window resize
  const handleResize = () => {
    chartInstance?.resize()
  }
  window.addEventListener('resize', handleResize)

  // Cleanup
  return () => {
    window.removeEventListener('resize', handleResize)
    chartInstance?.dispose()
  }
}

onMounted(() => {
  initChart()
})

watch(() => props.employee, () => {
  initChart()
}, { deep: true })

// Cleanup on unmount
onUnmounted(() => {
  if (chartInstance) {
    chartInstance.dispose()
  }
})
</script>

<template>
  <div class="w-full">
    <div class="mb-4 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
      <div class="flex items-start gap-3">
        <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div class="text-sm text-blue-800 dark:text-blue-200">
          <p class="font-medium mb-1">Interaktywne drzewo organizacyjne</p>
          <ul class="list-disc list-inside space-y-1 text-xs">
            <li>Kliknij na pracownika aby zobaczyć szczegóły</li>
            <li>Kliknij na węzeł aby rozwinąć/zwinąć podwładnych</li>
            <li>Użyj scroll aby przybliżyć/oddalić</li>
            <li>Przeciągnij aby przesunąć widok</li>
          </ul>
        </div>
      </div>
    </div>
    
    <div 
      ref="chartRef" 
      class="w-full bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700"
      style="height: 800px;"
    />
  </div>
</template>

