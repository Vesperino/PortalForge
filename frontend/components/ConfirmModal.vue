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
          <div class="flex items-start gap-4 p-6 pb-4">
            <!-- Icon -->
            <div
              class="flex-shrink-0 w-12 h-12 rounded-full flex items-center justify-center"
              :class="iconClasses"
            >
              <Icon :name="icon" class="w-6 h-6" />
            </div>

            <!-- Content -->
            <div class="flex-1 min-w-0">
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
            </div>
          </div>

          <!-- Actions -->
          <div class="flex gap-3 px-6 pb-6 justify-end">
            <button
              v-if="!hideCancel"
              type="button"
              class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
              data-testid="confirm-modal-cancel-btn"
              @click="handleCancel"
            >
              {{ cancelText }}
            </button>
            <button
              type="button"
              class="px-4 py-2 text-sm font-medium text-white rounded-lg focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors"
              :class="confirmButtonClasses"
              data-testid="confirm-modal-confirm-btn"
              @click="handleConfirm"
            >
              {{ confirmText }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'

export interface ConfirmModalProps {
  isOpen: boolean
  title: string
  message?: string
  confirmText?: string
  cancelText?: string
  variant?: 'danger' | 'warning' | 'info' | 'success'
  hideCancel?: boolean
}

const props = withDefaults(defineProps<ConfirmModalProps>(), {
  confirmText: 'Potwierdź',
  cancelText: 'Anuluj',
  variant: 'info',
  hideCancel: false
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

const titleId = computed(() => `modal-title-${Math.random().toString(36).substr(2, 9)}`)

const icon = computed(() => {
  const icons = {
    danger: 'heroicons:exclamation-triangle',
    warning: 'heroicons:exclamation-circle',
    info: 'heroicons:information-circle',
    success: 'heroicons:check-circle'
  }
  return icons[props.variant]
})

const iconClasses = computed(() => {
  const classes = {
    danger: 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
    warning: 'bg-yellow-100 dark:bg-yellow-900/30 text-yellow-600 dark:text-yellow-400',
    info: 'bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400',
    success: 'bg-green-100 dark:bg-green-900/30 text-green-600 dark:text-green-400'
  }
  return classes[props.variant]
})

const confirmButtonClasses = computed(() => {
  const classes = {
    danger: 'bg-red-600 hover:bg-red-700 focus:ring-red-500 dark:bg-red-500 dark:hover:bg-red-600',
    warning: 'bg-yellow-600 hover:bg-yellow-700 focus:ring-yellow-500 dark:bg-yellow-500 dark:hover:bg-yellow-600',
    info: 'bg-blue-600 hover:bg-blue-700 focus:ring-blue-500 dark:bg-blue-500 dark:hover:bg-blue-600',
    success: 'bg-green-600 hover:bg-green-700 focus:ring-green-500 dark:bg-green-500 dark:hover:bg-green-600'
  }
  return classes[props.variant]
})

const handleConfirm = () => {
  emit('confirm')
}

const handleCancel = () => {
  emit('cancel')
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
