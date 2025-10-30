# Requests System - Frontend Architecture

## Struktura Katalogów

```
frontend/
├── types/
│   └── requests.ts                   # TypeScript interfaces
│
├── composables/
│   └── useRequestsApi.ts            # API integration
│
├── middleware/
│   └── request-templates-admin.ts   # Permission check
│
├── components/
│   ├── IconPicker.vue               # Icon selection (80+ icons)
│   ├── RequestTimeline.vue          # Approval progress visualization
│   └── QuizModal.vue                # Interactive quiz
│
└── pages/
    ├── admin/
    │   ├── index.vue                # (updated: added card)
    │   └── request-templates/
    │       ├── index.vue            # Templates list
    │       └── create.vue           # Template creator wizard
    └── dashboard/
        └── requests/
            ├── index.vue            # Main page (tabs)
            └── submit/[id].vue      # Request submission form
```

## Type System

### Core Types

```typescript
// Enums
type DepartmentRole = 'Employee' | 'Manager' | 'Director'
type RequestStatus = 'Draft' | 'InReview' | 'Approved' | 'Rejected' | 'AwaitingSurvey'
type FieldType = 'Text' | 'Textarea' | 'Number' | 'Select' | 'Date' | 'Checkbox'

// Main interfaces
interface RequestTemplate {
  id: string
  name: string
  icon: string
  category: string
  departmentId?: string
  fields: RequestTemplateField[]
  approvalStepTemplates: RequestApprovalStepTemplate[]
  quizQuestions: QuizQuestion[]
}

interface Request {
  id: string
  requestNumber: string
  status: RequestStatus
  approvalSteps: RequestApprovalStep[]
}
```

## Composables

### useRequestsApi

```typescript
const { 
  // Templates
  getAvailableTemplates,
  getAllTemplates,
  getTemplateById,
  createTemplate,
  
  // Requests
  getMyRequests,
  getRequestsToApprove,
  submitRequest,
  approveRequestStep 
} = useRequestsApi()
```

### Usage Examples

```typescript
// Get templates for current user
const templates = await getAvailableTemplates()

// Submit request
const result = await submitRequest({
  requestTemplateId: 'uuid',
  priority: 'Standard',
  formData: { field1: 'value1' }
})

// Approve step
await approveRequestStep(requestId, stepId, {
  comment: 'Approved'
})
```

## Components

### 1. IconPicker

**Purpose:** Visual icon selection from 80+ Lucide icons

**Props:**
```typescript
{
  modelValue?: string  // Selected icon name
}
```

**Emits:**
```typescript
{
  'update:modelValue': [value: string]
}
```

**Features:**
- Search functionality
- Categorized icons
- Visual preview
- Selected state highlighting

**Usage:**
```vue
<IconPicker v-model="form.icon" />
```

### 2. RequestTimeline

**Purpose:** Visual representation of approval progress

**Props:**
```typescript
{
  steps: RequestApprovalStep[]
}
```

**Features:**
- Colored status indicators
- Icons per status (✓ approved, ⏳ in review, ❌ rejected)
- Dates and timestamps
- Comments display
- Quiz score badges
- Animated current step

**Usage:**
```vue
<RequestTimeline :steps="request.approvalSteps" />
```

### 3. QuizModal

**Purpose:** Interactive quiz interface with scoring

**Props:**
```typescript
{
  questions: QuizQuestion[]
  passingScore: number
}
```

**Emits:**
```typescript
{
  close: []
  submit: [score: number, passed: boolean, answers: Record<string, string>]
}
```

**Features:**
- Multiple choice questions
- Progress bar
- Real-time validation
- Score calculation
- Pass/fail determination
- Correct/incorrect answer highlighting

**Usage:**
```vue
<QuizModal
  :questions="quizQuestions"
  :passing-score="80"
  @close="closeModal"
  @submit="handleSubmit"
/>
```

## Pages Architecture

### Admin: Template List (admin/request-templates.vue)

**Features:**
- Grid view of templates
- Search bar
- Category filters
- Template cards showing:
  - Icon, name, description
  - Category badge
  - Department (or "all")
  - Active/inactive status
  - Field count, approval steps count
- Link to create new template
- Link to edit template

**State Management:**
```typescript
const templates = ref<RequestTemplate[]>([])
const loading = ref(true)
const searchQuery = ref('')
const filterCategory = ref('')

const filteredTemplates = computed(() => {
  // Filter + search logic
})
```

### Admin: Template Creator (admin/request-templates/create.vue)

**4-Step Wizard:**

**Step 1: Basic Info**
- Name, description, category
- IconPicker component
- Department selection (null = all)
- Estimated processing days
- Requires approval checkbox

**Step 2: Form Builder**
- Add/remove fields
- Drag & drop reordering (vuedraggable)
- Field configuration per type
- Validation rules

**Step 3: Approval Flow**
- Add approval steps
- Select approver role (Manager/Director)
- Enable quiz per step

**Step 4: Quiz**
- Set passing score (slider 0-100%)
- Add questions
- Add answer options (2-6 per question)
- Mark correct answers (checkbox)

**Navigation:**
- Previous/Next buttons
- Progress indicator
- Save on final step

### User: Requests Dashboard (dashboard/requests.vue)

**Tab 1: Nowy wniosek**
- Search bar
- Template grid with cards
- Click card → navigate to submit page

**Tab 2: Moje wnioski**
- Search bar
- Status filter dropdown
- Request cards showing:
  - Template icon and name
  - Request number
  - Status badge
  - Submission date
  - Priority badge
  - Current approval step
  - Progress (e.g., "2/3 approved")
- Click card → open details modal

**Request Details Modal:**
- Header with request info
- RequestTimeline component
- Form data display (JSON formatted)
- Close button

### User: Submit Request (dashboard/requests/submit/[id].vue)

**Features:**
- Load template by ID from route params
- Dynamic form generation based on template.fields
- Support for all 6 field types
- Field validation (required, min/max)
- Priority selection
- Submit to API
- Success redirect

**Field Rendering:**
```vue
<div v-if="field.fieldType === 'Text'">
  <input v-model="formData[field.id]" :required="field.isRequired" />
</div>
<div v-else-if="field.fieldType === 'Select'">
  <select v-model="formData[field.id]">
    <option v-for="opt in parseOptions(field.options)" :value="opt.value">
      {{ opt.label }}
    </option>
  </select>
</div>
<!-- ... other types -->
```

## State Management

### No Global Store Needed

Request system uses component-level state:
- API calls via composable
- Results stored in component refs
- No need for Pinia store (requests are contextual)

### Reactivity Patterns

```typescript
// Computed filters
const filteredTemplates = computed(() => {
  let result = templates.value
  if (filterCategory.value) {
    result = result.filter(t => t.category === filterCategory.value)
  }
  if (searchQuery.value) {
    result = result.filter(t => 
      t.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }
  return result
})

// Watch for changes
watch(() => props.modelValue, (newVal) => {
  selectedIcon.value = newVal || ''
})
```

## Styling Patterns

### Tailwind Utilities

```vue
<!-- Card with hover effects -->
<div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border 
     border-gray-200 dark:border-gray-700 hover:shadow-md 
     hover:border-blue-500 transition-all">

<!-- Status badges -->
<span :class="[
  'px-3 py-1 text-sm font-medium rounded-full',
  status === 'Approved' 
    ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
    : 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
]">
```

### Dark Mode Support

All components support dark mode via Tailwind's `dark:` variants:
- `dark:bg-gray-800` - Dark backgrounds
- `dark:text-white` - Light text
- `dark:border-gray-700` - Dark borders

## Navigation & Routing

### Route Structure

```
/admin/request-templates          → Lista szablonów (admin)
/admin/request-templates/create   → Kreator szablonu
/dashboard/requests               → Główna strona (tabs)
/dashboard/requests/submit/:id    → Formularz składania
```

### Middleware Protection

```typescript
// Admin templates require permission
definePageMeta({
  middleware: ['auth', 'admin', 'request-templates-admin']
})

// Regular requests require auth + verified
definePageMeta({
  middleware: ['auth', 'verified']
})
```

## Icon System

### Lucide Vue Next Integration

```typescript
import * as LucideIcons from 'lucide-vue-next'

const getIconComponent = (iconName: string) => {
  const IconComponent = (LucideIcons as any)[iconName]
  return IconComponent || LucideIcons.FileText  // Fallback
}
```

### Dynamic Icon Rendering

```vue
<component :is="getIconComponent(template.icon)" class="w-10 h-10" />
```

### Available Icon Categories

- **Hardware**: Laptop, Monitor, Smartphone, Server, HardDrive
- **Documents**: FileText, File, Folder, ClipboardList
- **People**: Users, User, UserPlus, UserCheck
- **Time**: Calendar, Clock, Bell
- **Communication**: Mail, MessageSquare, Phone, Video
- **Tools**: Settings, Tool, Wrench, Package
- **Business**: ShoppingCart, CreditCard, BarChart
- **Security**: Shield, Lock, Key
- **Education**: Book, GraduationCap, Award

## Form Builder Logic

### Drag & Drop Implementation

```vue
<template>
  <draggable
    v-model="form.fields"
    item-key="order"
    handle=".drag-handle"
  >
    <template #item="{ element: field, index }">
      <div class="field-item">
        <div class="drag-handle">
          <GripVertical class="w-5 h-5" />
        </div>
        <!-- Field configuration -->
      </div>
    </template>
  </draggable>
</template>
```

### Dynamic Field Addition

```typescript
const addField = () => {
  form.value.fields.push({
    label: '',
    fieldType: 'Text',
    placeholder: '',
    isRequired: false,
    order: form.value.fields.length + 1
  })
}

const removeField = (index: number) => {
  form.value.fields.splice(index, 1)
  // Reorder remaining fields
  form.value.fields.forEach((f, i) => f.order = i + 1)
}
```

## Error Handling

### API Error Handling

```typescript
const loadTemplates = async () => {
  try {
    loading.value = true
    error.value = ''
    templates.value = await getAllTemplates()
  } catch (err: any) {
    console.error('Error loading templates:', err)
    error.value = 'Nie udało się załadować szablonów'
  } finally {
    loading.value = false
  }
}
```

### User Feedback

```typescript
// Success
alert(`Wniosek ${result.requestNumber} został złożony pomyślnie!`)
navigateTo('/dashboard/requests')

// Error
alert('Błąd podczas składania wniosku')
```

## Performance Optimizations

### Computed Properties for Filtering

```typescript
// Only recalculates when dependencies change
const filteredTemplates = computed(() => {
  return templates.value.filter(/* ... */)
})
```

### Lazy Loading

```typescript
// Load data only when tab is active
watch(activeTab, async (newTab) => {
  if (newTab === 'my-requests' && myRequests.value.length === 0) {
    await loadMyRequests()
  }
})
```

### Icon Tree Shaking

Lucide Vue Next supports tree shaking - only imported icons are bundled.

## Accessibility

### Keyboard Navigation

- Tab navigation through forms
- Enter to submit
- Escape to close modals

### ARIA Labels

```vue
<button aria-label="Zamknij quiz" @click="closeModal">
  <X class="w-6 h-6" />
</button>
```

### Form Labels

```vue
<label for="templateName" class="block text-sm font-medium">
  Nazwa szablonu
</label>
<input id="templateName" v-model="form.name" />
```

## Mobile Responsiveness

### Grid Breakpoints

```vue
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  <!-- Templates cards -->
</div>
```

### Mobile Menu

```vue
<div class="flex flex-col sm:flex-row gap-4">
  <!-- Actions that stack on mobile -->
</div>
```

## Testing Frontend

### Component Testing (Future)

```typescript
import { mount } from '@vue/test-utils'
import IconPicker from '~/components/IconPicker.vue'

describe('IconPicker', () => {
  it('emits selected icon', async () => {
    const wrapper = mount(IconPicker)
    await wrapper.find('[data-icon="Laptop"]').trigger('click')
    expect(wrapper.emitted('update:modelValue')).toBeTruthy()
  })
})
```

### E2E Testing (Future)

```typescript
test('User can submit request', async ({ page }) => {
  await page.goto('/dashboard/requests')
  await page.click('text=Nowy wniosek')
  await page.click('text=Zamówienie sprzętu IT')
  await page.fill('[name="device"]', 'Laptop')
  await page.click('button:has-text("Złóż wniosek")')
  await expect(page.locator('text=REQ-')).toBeVisible()
})
```

## Best Practices

### 1. Type Safety

```typescript
// Always type props and emits
interface Props {
  steps: RequestApprovalStep[]
}
const props = defineProps<Props>()

interface Emits {
  close: []
  submit: [score: number, passed: boolean]
}
const emit = defineEmits<Emits>()
```

### 2. Composition API

```typescript
// Use setup script syntax
<script setup lang="ts">
const loading = ref(false)
const data = ref<Template[]>([])

const filtered = computed(() => {
  return data.value.filter(/* ... */)
})

onMounted(async () => {
  await loadData()
})
</script>
```

### 3. Error Boundaries

```typescript
// Always wrap async calls
try {
  const result = await apiCall()
} catch (error) {
  console.error('Error:', error)
  showErrorMessage()
} finally {
  loading.value = false
}
```

### 4. Loading States

```vue
<div v-if="loading">Loading...</div>
<div v-else-if="error">Error: {{ error }}</div>
<div v-else>
  <!-- Content -->
</div>
```

## Common Patterns

### Modal Pattern

```vue
<template>
  <!-- Backdrop -->
  <div class="fixed inset-0 bg-black bg-opacity-50 z-50" 
       @click.self="close">
    <!-- Modal content -->
    <div class="bg-white rounded-lg max-w-4xl w-full">
      <!-- Content -->
    </div>
  </div>
</template>
```

### Card Pattern

```vue
<div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm 
     border border-gray-200 dark:border-gray-700 
     hover:shadow-md transition-shadow p-6">
  <!-- Card content -->
</div>
```

### Badge Pattern

```vue
<span :class="[
  'px-2 py-1 text-xs font-medium rounded-full',
  getBadgeClass(status)
]">
  {{ status }}
</span>
```

## Styling Guide

### Color Scheme

- **Primary (Blue)**: Actions, links, selected states
- **Success (Green)**: Approved, passed
- **Danger (Red)**: Rejected, failed
- **Warning (Orange)**: Pending review
- **Info (Purple)**: Quiz-related
- **Neutral (Gray)**: Pending, inactive

### Spacing

- **Gap between elements**: `gap-4` (1rem)
- **Card padding**: `p-6` (1.5rem)
- **Section margins**: `mb-8` (2rem)

### Typography

- **Headings**: `text-3xl font-bold` (h1), `text-xl font-semibold` (h2)
- **Body**: `text-sm` or `text-base`
- **Labels**: `text-sm font-medium`

## Dependencies

### lucide-vue-next

```bash
npm install lucide-vue-next
```

**Usage:**
```typescript
import { Laptop, FileText, Users } from 'lucide-vue-next'

<template>
  <Laptop class="w-6 h-6 text-blue-600" />
</template>
```

### vuedraggable

```bash
npm install vuedraggable@next
```

**Usage:**
```vue
<draggable v-model="items" item-key="id" handle=".handle">
  <template #item="{ element }">
    <div>
      <div class="handle">☰</div>
      {{ element.name }}
    </div>
  </template>
</draggable>
```

## Future Frontend Enhancements

1. **Rich Text Editor** for text fields
2. **File Upload** component
3. **Date Range** picker
4. **Conditional Fields** (show field based on another field's value)
5. **Field Templates** (save common field configurations)
6. **Template Preview** before submission
7. **Request Draft** saving
8. **Keyboard Shortcuts**
9. **Accessibility** improvements (ARIA labels, keyboard nav)
10. **Print View** for requests

---

**Related Documentation:**
- [Backend Architecture](.ai/backend/requests-system.md)
- [Quick Reference](.ai/requests-system-quick-reference.md)
- [ADR 003](.ai/decisions/003-requests-system-architecture.md)

