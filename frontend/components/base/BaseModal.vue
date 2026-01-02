<script setup lang="ts">
import { computed, watch, onMounted, onUnmounted } from 'vue'

export type ModalSize = 'sm' | 'md' | 'lg' | 'xl' | 'full'

interface Props {
  isOpen: boolean
  title?: string
  size?: ModalSize
  showCloseButton?: boolean
  closeOnBackdrop?: boolean
  closeOnEscape?: boolean
  persistent?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: undefined,
  size: 'md',
  showCloseButton: true,
  closeOnBackdrop: true,
  closeOnEscape: true,
  persistent: false
})

const emit = defineEmits<{
  close: []
}>()

const sizeClasses = computed(() => {
  const sizes: Record<ModalSize, string> = {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-xl',
    full: 'max-w-full mx-4'
  }
  return sizes[props.size]
})

function handleClose(): void {
  if (!props.persistent) {
    emit('close')
  }
}

function handleBackdropClick(): void {
  if (props.closeOnBackdrop && !props.persistent) {
    emit('close')
  }
}

function handleKeyDown(event: KeyboardEvent): void {
  if (event.key === 'Escape' && props.closeOnEscape && !props.persistent) {
    emit('close')
  }
}

watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    document.body.style.overflow = 'hidden'
    window.addEventListener('keydown', handleKeyDown)
  } else {
    document.body.style.overflow = ''
    window.removeEventListener('keydown', handleKeyDown)
  }
})

onMounted(() => {
  if (props.isOpen) {
    document.body.style.overflow = 'hidden'
    window.addEventListener('keydown', handleKeyDown)
  }
})

onUnmounted(() => {
  document.body.style.overflow = ''
  window.removeEventListener('keydown', handleKeyDown)
})
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="isOpen"
        class="fixed inset-0 z-[10002] flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm"
        data-testid="base-modal-backdrop"
        @click.self="handleBackdropClick"
      >
        <div
          :class="[
            'bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full overflow-hidden',
            sizeClasses
          ]"
          role="dialog"
          aria-modal="true"
          data-testid="base-modal-container"
        >
          <!-- Header -->
          <div
            v-if="title || showCloseButton"
            class="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700"
          >
            <h3
              v-if="title"
              class="text-lg font-semibold text-gray-900 dark:text-white"
              data-testid="base-modal-title"
            >
              {{ title }}
            </h3>
            <div v-else />
            <button
              v-if="showCloseButton"
              type="button"
              class="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              data-testid="base-modal-close-btn"
              @click="handleClose"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <!-- Body -->
          <div class="p-6" data-testid="base-modal-body">
            <slot />
          </div>

          <!-- Footer -->
          <div
            v-if="$slots.footer"
            class="px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50"
            data-testid="base-modal-footer"
          >
            <slot name="footer" />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

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
