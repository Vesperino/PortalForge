# Frontend Documentation

## Overview
PortalForge Frontend - Nuxt 3 (Vue 3) application with TypeScript and Tailwind CSS

## Architecture

### Nuxt 3 Structure

```
frontend/
├── .nuxt/                # Build artifacts (generated)
├── .output/              # Production build (generated)
├── assets/               # Uncompiled assets (CSS, images)
├── components/           # Vue components (auto-imported)
│   ├── common/          # Shared components
│   ├── employee/        # Employee-related components
│   ├── calendar/        # Calendar components
│   └── news/            # News components
├── composables/          # Composition functions (auto-imported)
│   ├── useAuth.ts       # Authentication composable
│   ├── useApi.ts        # API client composable
│   └── useEmployee.ts   # Employee operations
├── layouts/              # Layout components
│   ├── default.vue      # Default layout
│   └── auth.vue         # Authentication layout
├── middleware/           # Route middleware
│   ├── auth.ts          # Authentication guard
│   └── role.ts          # Role-based guard
├── pages/                # File-based routing
│   ├── index.vue        # Home page
│   ├── login.vue        # Login page
│   ├── employees/       # Employee pages
│   ├── calendar/        # Calendar pages
│   └── news/            # News pages
├── plugins/              # Nuxt plugins
│   ├── api.ts           # API client plugin
│   └── supabase.ts      # Supabase client plugin
├── public/               # Static assets (served as-is)
├── stores/               # Pinia stores
│   ├── auth.ts          # Authentication state
│   ├── employee.ts      # Employee state
│   └── ui.ts            # UI state (modals, toasts)
├── types/                # TypeScript types and interfaces
│   ├── api.ts           # API response types
│   ├── models.ts        # Domain models
│   └── enums.ts         # Enumerations
├── utils/                # Utility functions
│   ├── format.ts        # Formatting utilities
│   └── validation.ts    # Validation utilities
├── .env                  # Environment variables (not committed)
├── .env.example          # Example environment variables
├── nuxt.config.ts        # Nuxt configuration
├── package.json          # Dependencies
├── tailwind.config.ts    # Tailwind configuration
└── tsconfig.json         # TypeScript configuration
```

## Current Status

### ✅ Completed
- Frontend folder structure created
- Nuxt 3 project initialization
- Tailwind CSS configuration
- **Authentication pages**:
  - `/auth/login` - Login page
  - `/auth/callback` - Email verification callback handler
  - `/auth/verify-email` - Email verification with resend functionality
- **Middleware**:
  - `auth.ts` - Authentication guard
  - `guest.ts` - Guest-only guard
  - `verified.ts` - Email verification check guard
- **Composables**:
  - `useAuth.ts` - Authentication logic with email verification flow
- **Stores**:
  - `auth.ts` - Authentication state management
- API integration with backend (Supabase Auth + .NET API)

### 🚧 In Progress
- Dashboard layout
- Homepage improvements

### ⏳ Planned
- Employee management pages
- Calendar view
- News feed
- Organizational chart visualization
- Advanced API features

## Key Technologies

- **Nuxt 3**: Vue 3 meta-framework with SSR
- **Vue 3**: Progressive JavaScript framework (Composition API)
- **TypeScript**: Type-safe JavaScript
- **Tailwind CSS**: Utility-first CSS framework
- **Pinia**: State management
- **VueUse**: Collection of Vue composition utilities
- **Vitest**: Unit testing framework
- **Playwright**: E2E testing framework
- **Zod**: Schema validation

## Pages & Routes (Planned)

### Public Routes (✅ Implemented)
- `/auth/login` - Login page
- `/auth/callback` - Email verification callback (from Supabase email link)
- `/auth/verify-email` - Email verification page with resend functionality

### Authenticated Routes
- `/` - Main homepage (requires verified email)
- `/dashboard` - Main dashboard (planned)
- `/employees` - Employee list (planned)
- `/employees/:id` - Employee details (planned)
- `/organization` - Org chart visualization (planned)
- `/calendar` - Events calendar (planned)
- `/news` - News feed (planned)
- `/profile` - User profile (planned)

### Role-Specific Routes
- `/admin/users` - User management (Admin only)
- `/admin/import` - CSV/Excel import (Admin/HR)
- `/admin/reports` - Activity reports (Admin)

## Component Guidelines

### Component Naming
- Use PascalCase: `EmployeeCard.vue`, `CalendarView.vue`
- Prefix base components: `BaseButton.vue`, `BaseInput.vue`
- Single-instance components: `TheHeader.vue`, `TheSidebar.vue`

### Component Structure

```vue
<script setup lang="ts">
// 1. Imports
import { ref, computed } from 'vue'
import type { Employee } from '@/types/models'

// 2. Props
interface Props {
  employee: Employee
  editable?: boolean
}
const props = withDefaults(defineProps<Props>(), {
  editable: false
})

// 3. Emits
const emit = defineEmits<{
  update: [employee: Employee]
  delete: [id: string]
}>()

// 4. State
const isEditing = ref(false)

// 5. Computed
const fullName = computed(() =>
  `${props.employee.firstName} ${props.employee.lastName}`
)

// 6. Methods
const handleEdit = () => {
  isEditing.value = true
}

// 7. Lifecycle hooks
onMounted(() => {
  console.log('Component mounted')
})
</script>

<template>
  <div class="employee-card">
    <h3 class="text-lg font-semibold">{{ fullName }}</h3>
    <p class="text-gray-600">{{ employee.position }}</p>
    <BaseButton v-if="editable" @click="handleEdit">
      Edit
    </BaseButton>
  </div>
</template>
```

## State Management (Pinia)

### Store Structure

```typescript
// stores/auth.ts
import { defineStore } from 'pinia'
import type { User } from '@/types/models'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const isAuthenticated = computed(() => !!user.value)

  // Actions
  const login = async (email: string, password: string) => {
    // Implementation
  }

  const logout = async () => {
    user.value = null
  }

  return {
    user,
    isAuthenticated,
    login,
    logout
  }
})
```

## API Integration

### API Client Pattern

```typescript
// composables/useApi.ts
export const useApi = () => {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiUrl

  const get = async <T>(url: string) => {
    return await $fetch<T>(`${baseURL}${url}`, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`
      }
    })
  }

  return { get, post, put, delete }
}

// Usage in component
const { data: employees } = await useAsyncData(
  'employees',
  () => useApi().get<Employee[]>('/api/employees')
)
```

## Styling Guidelines

### Tailwind CSS Best Practices

- **Use utility classes** first
- **Extract components** for repeated patterns
- **Use @apply** sparingly (only in base styles)
- **Follow mobile-first** responsive design
- **Use Tailwind's color palette**
- **Leverage dark mode** utilities

### Example Component Styling

```vue
<template>
  <!-- Card with responsive padding and shadows -->
  <div class="bg-white rounded-lg shadow-md p-4 md:p-6 hover:shadow-lg transition-shadow">
    <!-- Responsive text sizes -->
    <h2 class="text-xl md:text-2xl font-bold text-gray-900 mb-2">
      {{ title }}
    </h2>
    <!-- Responsive grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <!-- Grid items -->
    </div>
  </div>
</template>
```

## Form Handling

### Form Validation with Zod

```typescript
import { z } from 'zod'

const employeeSchema = z.object({
  firstName: z.string().min(2, 'First name must be at least 2 characters'),
  lastName: z.string().min(2, 'Last name must be at least 2 characters'),
  email: z.string().email('Invalid email address'),
  departmentId: z.string().uuid('Invalid department'),
  position: z.string().min(2, 'Position is required')
})

type EmployeeForm = z.infer<typeof employeeSchema>

// Usage in component
const formData = ref<EmployeeForm>({
  firstName: '',
  lastName: '',
  email: '',
  departmentId: '',
  position: ''
})

const handleSubmit = async () => {
  const result = employeeSchema.safeParse(formData.value)
  if (!result.success) {
    // Handle validation errors
    console.error(result.error.flatten())
    return
  }

  // Submit valid data
  await useApi().post('/api/employees', result.data)
}
```

## Testing Strategy

### Unit Tests (Vitest)
- Test composables
- Test utility functions
- Test component logic (with vue-test-utils)
- Mock API calls

### E2E Tests (Playwright)
- Test critical user flows
- Test authentication
- Test CRUD operations
- Test form submissions

### Test Example

```typescript
// tests/composables/useAuth.test.ts
import { describe, it, expect, vi } from 'vitest'
import { useAuthStore } from '@/stores/auth'

describe('useAuthStore', () => {
  it('should login user successfully', async () => {
    const store = useAuthStore()
    await store.login('test@example.com', 'password')

    expect(store.isAuthenticated).toBe(true)
    expect(store.user).toBeTruthy()
  })
})
```

## Performance Optimization

### Lazy Loading
```vue
<script setup>
// Lazy load heavy components
const HeavyChart = defineAsyncComponent(
  () => import('@/components/HeavyChart.vue')
)
</script>
```

### Image Optimization
```vue
<template>
  <!-- Use Nuxt Image for automatic optimization -->
  <NuxtImg
    src="/employee-photo.jpg"
    width="200"
    height="200"
    format="webp"
    loading="lazy"
  />
</template>
```

### Code Splitting
- Use route-based code splitting (automatic with Nuxt)
- Lazy load components that are not immediately visible
- Use dynamic imports for heavy libraries

## Accessibility (WCAG 2.1 AA)

- ✅ Use semantic HTML elements
- ✅ Provide alt text for images
- ✅ Ensure keyboard navigation
- ✅ Use ARIA labels where needed
- ✅ Maintain sufficient color contrast (4.5:1)
- ✅ Support screen readers
- ✅ Provide focus indicators
- ✅ Use skip navigation links

## Environment Variables

```bash
# .env.example
NUXT_PUBLIC_API_URL=http://localhost:5000
NUXT_PUBLIC_SUPABASE_URL=your-supabase-url
NUXT_PUBLIC_SUPABASE_KEY=your-supabase-anon-key
```

## Email Verification Flow

### Implementation Details

The email verification flow is fully implemented:

1. **Registration**: User registers at `/auth/login` (register form)
2. **Email Sent**: Supabase automatically sends verification email
3. **Redirect**: Frontend redirects to `/auth/verify-email?email=user@example.com`
4. **Timer**: 2-minute cooldown timer starts automatically
5. **Email Link**: User clicks verification link from email
6. **Callback**: Link redirects to `/auth/callback?token=xyz&type=signup`
7. **Verification**: Callback page verifies token via backend API
8. **Success**: User is redirected to `/auth/login` with success message

### Resend Email Feature

- **Rate Limiting**: 2-minute cooldown between resend attempts
- **Visual Timer**: MM:SS countdown display
- **Button State**: Disabled during cooldown with remaining time
- **Error Handling**: Clear error messages for failed attempts
- **Auto-start**: Timer starts automatically when redirected from registration

### Middleware Protection

- **`auth.ts`**: Checks if user is authenticated
- **`guest.ts`**: Redirects authenticated users away from auth pages
- **`verified.ts`**: Checks if email is verified, redirects to verify-email if not

---

**Last Updated**: 2025-10-17
