<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="isOpen"
        class="fixed inset-0 z-[10002] flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm"
        @click.self="handleCancel"
      >
        <div
          class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full"
          role="dialog"
          aria-modal="true"
          :aria-labelledby="titleId"
        >
          <!-- Header -->
          <div class="p-6 pb-4">
            <h3
              :id="titleId"
              class="text-lg font-semibold text-gray-900 dark:text-white"
            >
              {{ title }}
            </h3>
            <p
              v-if="message"
              class="mt-2 text-sm text-gray-600 dark:text-gray-400"
            >
              {{ message }}
            </p>

            <!-- Input -->
            <input
              ref="inputRef"
              v-model="inputValue"
              type="text"
              class="mt-4 w-full px-3 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              :placeholder="placeholder"
              @keyup.enter="handleConfirm"
              @keyup.escape="handleCancel"
            >
          </div>

          <!-- Actions -->
          <div class="flex gap-3 px-6 pb-6 justify-end">
            <button
              type="button"
              class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
              @click="handleCancel"
            >
              Anuluj
            </button>
            <button
              type="button"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
              @click="handleConfirm"
            >
              OK
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue'

export interface PromptModalProps {
  isOpen: boolean
  title: string
  message?: string
  placeholder?: string
  defaultValue?: string
}

const props = withDefaults(defineProps<PromptModalProps>(), {
  message: undefined,
  placeholder: '',
  defaultValue: ''
})

const emit = defineEmits<{
  confirm: [value: string]
  cancel: []
}>()

const inputRef = ref<HTMLInputElement | null>(null)
const inputValue = ref(props.defaultValue)
const titleId = computed(() => `prompt-modal-title-${Math.random().toString(36).substr(2, 9)}`)

// Focus input when modal opens
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    inputValue.value = props.defaultValue
    nextTick(() => {
      inputRef.value?.focus()
      inputRef.value?.select()
    })
  }
})

const handleConfirm = () => {
  emit('confirm', inputValue.value)
  inputValue.value = ''
}

const handleCancel = () => {
  emit('cancel')
  inputValue.value = ''
}
</script>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active > div,
.modal-leave-active > div {
  transition: transform 0.2s ease, opacity 0.2s ease;
}

.modal-enter-from > div,
.modal-leave-to > div {
  opacity: 0;
  transform: scale(0.95);
}
</style>
