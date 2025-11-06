<script setup lang="ts">
import type { Position } from '~/types/position'
import AutoComplete from 'primevue/autocomplete'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'

const props = defineProps<{
  modelValue: string | null
  placeholder?: string
  required?: boolean
  disabled?: boolean
  initialPositionName?: string  // Support for initial position name without ID
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: string | null): void
  (e: 'update:positionName', name: string): void
}>()

const { searchPositions, createPosition } = usePositions()

const selectedPosition = ref<Position | null>(null)
const positionInput = ref<string>(props.initialPositionName || '')
const suggestions = ref<Position[]>([])
const showCreateModal = ref(false)
const isInitialized = ref(false)

const newPosition = ref({
  name: '',
  description: ''
})

// Search positions as user types
async function onSearch(event: { query: string }) {
  if (event.query.trim().length > 0) {
    suggestions.value = await searchPositions(event.query)
  } else {
    suggestions.value = await searchPositions('')
  }
}

// When user selects a position from dropdown
function onSelect(event: { value: Position }) {
  selectedPosition.value = event.value
  // Keep input as plain string to avoid sending objects in payload
  positionInput.value = event.value.name
  emit('update:modelValue', event.value.id)
  emit('update:positionName', event.value.name)
}

// Keep position name in sync when typing; coerce objects to string; clear ID if no exact match
watch(positionInput, (val: any) => {
  const name = typeof val === 'string' ? val : (val && typeof val === 'object' && 'name' in val ? (val as any).name : '')
  if (typeof val !== 'string') {
    // Coerce model back to string to prevent sending objects in requests
    positionInput.value = name
    return
  }
  emit('update:positionName', name || '')
  const exact = suggestions.value.find(p => p.name.toLowerCase() === (name || '').toLowerCase())
  if (!exact) emit('update:modelValue', null)
})

// Open modal to create new position
function openCreateModal() {
  newPosition.value.name = positionInput.value
  newPosition.value.description = ''
  showCreateModal.value = true
}

// Create new position
async function handleCreatePosition() {
  if (!newPosition.value.name.trim()) {
    return
  }

  const positionId = await createPosition({
    name: newPosition.value.name,
    description: newPosition.value.description || null,
    isActive: true
  })

  if (positionId) {
    // Close modal
    showCreateModal.value = false

    // Set the new position
    positionInput.value = newPosition.value.name
    emit('update:modelValue', positionId)
    emit('update:positionName', newPosition.value.name)

    // Clear form
    newPosition.value = { name: '', description: '' }
  }
}

// Watch for external changes to modelValue
watch(() => props.modelValue, async (newVal, oldVal) => {
  // Avoid unnecessary updates if value hasn't changed
  if (newVal === oldVal) return

  if (newVal && !selectedPosition.value) {
    // Load the position by ID if needed
    const allPositions = await searchPositions('')
    const position = allPositions.find(p => p.id === newVal)
    if (position && positionInput.value !== position.name) {
      selectedPosition.value = position
      positionInput.value = position.name
      isInitialized.value = true
    }
  } else if (!newVal && !isInitialized.value) {
    // Only clear if not initialized with initial position name
    // This prevents clearing when component is first loaded with initialPositionName but no ID
    if (!props.initialPositionName || positionInput.value === '') {
      selectedPosition.value = null
      positionInput.value = ''
    }
  }
}, { immediate: false })

// Watch for initialPositionName changes
watch(() => props.initialPositionName, async (newName) => {
  if (newName && !positionInput.value && !isInitialized.value) {
    positionInput.value = newName

    // Try to find matching position by name to get ID
    const allPositions = await searchPositions('')
    const position = allPositions.find(p =>
      p.name.toLowerCase() === newName.toLowerCase()
    )
    if (position) {
      selectedPosition.value = position
      emit('update:modelValue', position.id)
      emit('update:positionName', position.name)
    }
    isInitialized.value = true
  }
}, { immediate: true })
</script>

<template>
  <div class="position-autocomplete">
    <div class="flex gap-2">
      <AutoComplete
        v-model="positionInput"
        :suggestions="suggestions"
        option-label="name"
        :placeholder="placeholder || 'Wpisz stanowisko...'"
        :required="required"
        :disabled="disabled"
        class="flex-1"
        :append-to="'self'"
        :pt="{
          root: { class: 'w-full' },
          input: { class: 'w-full px-4 py-2.5 border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all' },
          panel: { class: 'max-h-64 overflow-auto' }
        }"
        @complete="onSearch"
        @item-select="onSelect"
      >
        <template #option="slotProps">
          <div class="flex flex-col">
            <span class="font-semibold">{{ slotProps.option.name }}</span>
            <span v-if="slotProps.option.description" class="text-sm text-gray-500">
              {{ slotProps.option.description }}
            </span>
          </div>
        </template>
      </AutoComplete>

      <Button
        icon="pi pi-plus"
        severity="success"
        outlined
        :disabled="disabled"
        title="Dodaj nowe stanowisko"
        @click="openCreateModal"
      />
    </div>

    <!-- Create Position Modal -->
    <Dialog
      v-model:visible="showCreateModal"
      modal
      header="Dodaj nowe stanowisko"
      :style="{ width: '30rem' }"
    >
      <div class="flex flex-col gap-4">
        <div class="flex flex-col gap-2">
          <label for="position-name" class="font-semibold">Nazwa stanowiska</label>
          <InputText
            id="position-name"
            v-model="newPosition.name"
            placeholder="np. Senior Developer"
            required
          />
        </div>

        <div class="flex flex-col gap-2">
          <label for="position-description" class="font-semibold">Opis (opcjonalnie)</label>
          <Textarea
            id="position-description"
            v-model="newPosition.description"
            rows="3"
            placeholder="Dodaj opis stanowiska..."
          />
        </div>

        <div class="flex justify-end gap-2">
          <Button
            label="Anuluj"
            severity="secondary"
            outlined
            @click="showCreateModal = false"
          />
          <Button
            label="Dodaj"
            severity="success"
            :disabled="!newPosition.name.trim()"
            @click="handleCreatePosition"
          />
        </div>
      </div>
    </Dialog>
  </div>
</template>

<style scoped>
.position-autocomplete {
  width: 100%;
}

/* Ensure the suggestions panel appears above custom modals */
:global(.p-autocomplete-panel) {
  z-index: 1000 !important;
}
</style>
