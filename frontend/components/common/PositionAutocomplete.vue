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
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: string | null): void
  (e: 'update:positionName', name: string): void
}>()

const { searchPositions, createPosition } = usePositions()

const selectedPosition = ref<Position | null>(null)
const positionInput = ref<string>('')
const suggestions = ref<Position[]>([])
const showCreateModal = ref(false)

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
  emit('update:modelValue', event.value.id)
  emit('update:positionName', event.value.name)
}

// When user types manually without selecting
function onInput(event: any) {
  const inputValue = event.target?.value || event.value

  if (typeof inputValue === 'string') {
    positionInput.value = inputValue
    emit('update:positionName', inputValue)

    // If input doesn't match any existing position, clear the ID
    const exactMatch = suggestions.value.find(p =>
      p.name.toLowerCase() === inputValue.toLowerCase()
    )
    if (!exactMatch) {
      emit('update:modelValue', null)
    }
  }
}

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
watch(() => props.modelValue, async (newVal) => {
  if (newVal && !selectedPosition.value) {
    // Load the position by ID if needed
    const allPositions = await searchPositions('')
    const position = allPositions.find(p => p.id === newVal)
    if (position) {
      selectedPosition.value = position
      positionInput.value = position.name
    }
  } else if (!newVal) {
    selectedPosition.value = null
    positionInput.value = ''
  }
})
</script>

<template>
  <div class="position-autocomplete">
    <div class="flex gap-2">
      <AutoComplete
        v-model="positionInput"
        :suggestions="suggestions"
        optionLabel="name"
        :placeholder="placeholder || 'Wpisz stanowisko...'"
        :required="required"
        :disabled="disabled"
        class="flex-1"
        @complete="onSearch"
        @item-select="onSelect"
        @input="onInput"
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
        @click="openCreateModal"
        title="Dodaj nowe stanowisko"
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
</style>
