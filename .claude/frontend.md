# Frontend Rules - Nuxt 3 + Vue 3 + TypeScript + Tailwind

## Vue 3 Composition API

Use `<script setup>` syntax for all components:

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { Employee } from '~/types'

// Props
interface Props {
  employeeId: number
}
const props = defineProps<Props>()

// Emits
interface Emits {
  (e: 'update', employee: Employee): void
  (e: 'delete', id: number): void
}
const emit = defineEmits<Emits>()

// State
const employee = ref<Employee | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)

// Computed
const fullName = computed(() =>
  employee.value ? `${employee.value.firstName} ${employee.value.lastName}` : ''
)

// Methods
async function loadEmployee() {
  isLoading.value = true
  error.value = null

  try {
    const data = await $fetch<Employee>(`/api/employees/${props.employeeId}`)
    employee.value = data
  } catch (err) {
    error.value = 'Failed to load employee'
    console.error(err)
  } finally {
    isLoading.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadEmployee()
})
</script>

<template>
  <div class="employee-card">
    <div v-if="isLoading" class="loading">Loading...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else-if="employee" class="employee-details">
      <h2>{{ fullName }}</h2>
      <p>{{ employee.email }}</p>
      <button @click="emit('update', employee)">Edit</button>
      <button @click="emit('delete', employee.id)">Delete</button>
    </div>
  </div>
</template>
```

## TypeScript Standards

- Always define types and interfaces
- Use `type` for unions/intersections, `interface` for objects
- Avoid `any` - use `unknown` if type is truly unknown
- Use generics for reusable components

```typescript
// Types for API responses
export interface Employee {
  employeeId: number
  firstName: string
  lastName: string
  email: string
  departmentId: number
  department?: Department
}

export interface Department {
  departmentId: number
  name: string
  description: string
}

// API response wrapper
export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
  errors?: string[]
}

// Union types
export type EmployeeStatus = 'active' | 'inactive' | 'on-leave'

// Utility types
export type CreateEmployeeDto = Omit<Employee, 'employeeId' | 'department'>
export type UpdateEmployeeDto = Partial<CreateEmployeeDto>
```

## Composables

Create reusable composables for common logic:

```typescript
// composables/useEmployees.ts
export function useEmployees() {
  const employees = ref<Employee[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchEmployees() {
    isLoading.value = true
    error.value = null

    try {
      const data = await $fetch<ApiResponse<Employee[]>>('/api/employees')
      employees.value = data.data
    } catch (err) {
      error.value = 'Failed to fetch employees'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function createEmployee(dto: CreateEmployeeDto) {
    const data = await $fetch<ApiResponse<Employee>>('/api/employees', {
      method: 'POST',
      body: dto
    })
    employees.value.push(data.data)
    return data.data
  }

  return {
    employees: readonly(employees),
    isLoading: readonly(isLoading),
    error: readonly(error),
    fetchEmployees,
    createEmployee
  }
}
```

## Pinia Stores

Use Pinia for global state management:

```typescript
// stores/auth.ts
export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const isAuthenticated = computed(() => user.value !== null)

  // Actions
  async function login(email: string, password: string) {
    const { data } = await $fetch<ApiResponse<User>>('/api/auth/login', {
      method: 'POST',
      body: { email, password }
    })
    user.value = data
  }

  function logout() {
    user.value = null
  }

  return {
    user: readonly(user),
    isAuthenticated,
    login,
    logout
  }
})
```

## Tailwind CSS Best Practices

- Use utility classes directly in templates
- Group related utilities logically
- Use responsive prefixes (sm:, md:, lg:, xl:)
- Leverage dark mode with `dark:` prefix
- Use arbitrary values `[]` sparingly

```vue
<template>
  <div class="flex flex-col gap-4 p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md">
    <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
      {{ title }}
    </h2>

    <p class="text-sm text-gray-600 dark:text-gray-300 leading-relaxed">
      {{ description }}
    </p>

    <div class="flex gap-2 justify-end">
      <button class="px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded transition">
        Save
      </button>
      <button class="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition">
        Cancel
      </button>
    </div>
  </div>
</template>
```

## Accessibility Guidelines

- Use semantic HTML elements
- Implement proper ARIA labels and roles
- Ensure keyboard navigation works
- Maintain proper heading hierarchy (h1 → h2 → h3)
- Implement focus states for interactive elements

```vue
<template>
  <nav aria-label="Main navigation">
    <ul role="list" class="flex gap-4">
      <li>
        <a href="/employees" aria-current="page" class="focus:ring-2 focus:ring-blue-500">
          Employees
        </a>
      </li>
      <li>
        <a href="/departments" class="focus:ring-2 focus:ring-blue-500">
          Departments
        </a>
      </li>
    </ul>
  </nav>

  <button
    aria-label="Close dialog"
    @click="closeDialog"
    class="focus:ring-2 focus:ring-blue-500"
  >
    <span aria-hidden="true">&times;</span>
  </button>
</template>
```

## Component Organization

### File Structure

```
components/
├── employees/
│   ├── EmployeeCard.vue
│   ├── EmployeeList.vue
│   └── EmployeeForm.vue
├── common/
│   ├── BaseButton.vue
│   ├── BaseInput.vue
│   └── BaseModal.vue
└── layout/
    ├── Header.vue
    ├── Footer.vue
    └── Sidebar.vue
```

### Component Best Practices

- Use functional, declarative programming - avoid classes
- Prefer named exports for composables and utils
- Use descriptive variable names with auxiliary verbs (e.g., isLoading, hasError)
- File structure: imports, types, composables, main logic, template
- Use `defineProps` and `defineEmits` with TypeScript interfaces
- Implement proper prop validation
- Use `computed` for derived state instead of methods
- Leverage VueUse composables for common functionality
- Use Nuxt's auto-imported components - no need for manual imports
- Implement proper loading and error states in components

## Error Handling

### API Error Handling

```typescript
// composables/useApi.ts
export function useApi() {
  async function handleRequest<T>(promise: Promise<T>): Promise<T> {
    try {
      return await promise
    } catch (error) {
      if (error.statusCode === 401) {
        // Redirect to login
        await navigateTo('/login')
      } else if (error.statusCode === 403) {
        // Show forbidden message
        console.error('Access denied')
      } else {
        // Show generic error
        console.error('An error occurred:', error)
      }
      throw error
    }
  }

  return { handleRequest }
}
```

## Performance Optimization

- Use `v-show` for frequently toggled elements
- Use `v-if` for conditionally rendered elements
- Implement lazy loading for heavy components
- Use virtual scrolling for long lists
- Optimize images (use appropriate formats and sizes)
- Implement proper caching strategies

```vue
<script setup lang="ts">
// Lazy load heavy component
const HeavyChart = defineAsyncComponent(() =>
  import('~/components/HeavyChart.vue')
)
</script>

<template>
  <Suspense>
    <template #default>
      <HeavyChart />
    </template>
    <template #fallback>
      <div class="loading">Loading chart...</div>
    </template>
  </Suspense>
</template>
```

---

**Auto-attach**: `**/*.vue`, `**/*.ts` in frontend directory
