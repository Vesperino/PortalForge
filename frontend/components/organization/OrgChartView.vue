<script setup lang="ts">
import type { Employee } from '~/types'
import { OrgChart } from 'd3-org-chart'

interface Props {
  employee: Employee
  onSelectEmployee?: (employee: Employee) => void
}

const props = defineProps<Props>()
const emit = defineEmits<{
  selectEmployee: [employee: Employee]
}>()

const chartContainer = ref<HTMLDivElement | null>(null)
let chartInstance: any = null

// Mapa pracowników dla szybkiego dostępu
const employeeMap = new Map<string, Employee>()

// Konwertuj dane pracownika do formatu d3-org-chart
const convertToOrgChartData = (employee: Employee, parentId: string | null = null): any => {
  const id = employee.id.toString()
  employeeMap.set(id, employee)

  const node: any = {
    id: id,
    parentId: parentId,
    name: `${employee.firstName} ${employee.lastName}`,
    positionName: employee.position?.name || '',
    department: employee.department?.name || '',
    departmentColor: employee.department?.color || '#3b82f6',
    email: employee.email,
    phone: employee.phone || '',
    _directSubordinates: employee.subordinates?.length || 0,
    _totalSubordinates: employee.subordinates?.length || 0
  }

  return node
}

// Spłaszcz hierarchię do tablicy
const flattenHierarchy = (employee: Employee, parentId: string | null = null): any[] => {
  const nodes: any[] = []
  const node = convertToOrgChartData(employee, parentId)
  nodes.push(node)

  if (employee.subordinates && employee.subordinates.length > 0) {
    employee.subordinates.forEach(sub => {
      nodes.push(...flattenHierarchy(sub, employee.id.toString()))
    })
  }

  return nodes
}

const initChart = () => {
  if (!chartContainer.value) return

  // Wyczyść mapę pracowników
  employeeMap.clear()

  // Spłaszcz hierarchię
  const data = flattenHierarchy(props.employee)

  const isDark = document.documentElement.classList.contains('dark')

  // Utwórz lub zaktualizuj wykres
  if (!chartInstance) {
    chartInstance = new OrgChart()
  }

  chartInstance
    .container(chartContainer.value)
    .data(data)
    .nodeWidth(() => 250)
    .nodeHeight(() => 150)
    .childrenMargin(() => 80)
    .compactMarginBetween(() => 40)
    .compactMarginPair(() => 60)
    .neighbourMargin(() => 80)
    .siblingsMargin(() => 80)
    .nodeContent((d: any) => {
      const employee = employeeMap.get(d.data.id)
      const bgColor = isDark ? '#1f2937' : '#ffffff'
      const textColor = isDark ? '#ffffff' : '#111827'
      const subtextColor = isDark ? '#9ca3af' : '#6b7280'
      const borderColor = d.data.departmentColor || '#3b82f6'

      return `
        <div style="
          width: 250px;
          height: 150px;
          background: ${bgColor};
          border: 2px solid ${borderColor};
          border-radius: 12px;
          padding: 16px;
          box-shadow: 0 2px 8px rgba(0,0,0,0.1);
          cursor: pointer;
          transition: all 0.2s;
          display: flex;
          flex-direction: column;
          justify-content: center;
          align-items: center;
          text-align: center;
        "
        onmouseover="this.style.transform='translateY(-4px)'; this.style.boxShadow='0 8px 16px rgba(0,0,0,0.15)'"
        onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 2px 8px rgba(0,0,0,0.1)'"
        >
          <div style="
            width: 48px;
            height: 48px;
            border-radius: 50%;
            background: linear-gradient(135deg, ${borderColor}, ${borderColor}dd);
            color: white;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 700;
            font-size: 18px;
            margin-bottom: 12px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
          ">
            ${d.data.name.split(' ').map((n: string) => n[0]).join('')}
          </div>
          <div style="
            font-weight: 600;
            font-size: 14px;
            color: ${textColor};
            margin-bottom: 4px;
            line-height: 1.3;
          ">
            ${d.data.name}
          </div>
          <div style="
            font-size: 12px;
            color: ${subtextColor};
            margin-bottom: 8px;
            line-height: 1.2;
          ">
            ${d.data.positionName}
          </div>
          <div style="
            font-size: 10px;
            padding: 4px 12px;
            border-radius: 12px;
            background: ${borderColor};
            color: white;
            font-weight: 500;
          ">
            ${d.data.department}
          </div>
        </div>
      `
    })
    .onNodeClick((d: any) => {
      const employee = employeeMap.get(d)
      if (employee) {
        emit('selectEmployee', employee)
        if (props.onSelectEmployee) {
          props.onSelectEmployee(employee)
        }
      }
    })
    .render()
}

onMounted(() => {
  nextTick(() => {
    initChart()
  })
})

watch(() => props.employee, () => {
  nextTick(() => {
    initChart()
  })
}, { deep: true })

// Obsługa dark mode
const isDark = ref(false)
onMounted(() => {
  isDark.value = document.documentElement.classList.contains('dark')
  
  const observer = new MutationObserver(() => {
    isDark.value = document.documentElement.classList.contains('dark')
    initChart() // Przerysuj wykres przy zmianie motywu
  })
  
  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class']
  })
  
  onUnmounted(() => {
    observer.disconnect()
  })
})

onUnmounted(() => {
  if (chartContainer.value) {
    chartContainer.value.innerHTML = ''
  }
  chartInstance = null
})
</script>

<template>
  <div class="w-full h-full bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-auto">
    <div
      ref="chartContainer"
      class="orgchart-container w-full h-full"
    />
  </div>
</template>

<style>
.orgchart-container {
  min-height: 600px;
}

/* Customizacja d3-org-chart */
.orgchart-container svg {
  width: 100%;
  height: 100%;
}
</style>

