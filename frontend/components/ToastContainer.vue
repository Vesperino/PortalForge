<template>
  <div
    class="toast-container fixed right-4 z-[10001] space-y-2 max-w-md"
    :style="{ top: offsetTop }"
  >
    <TransitionGroup
      name="toast"
      tag="div"
      class="space-y-2"
    >
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="flex items-start gap-3 p-4 rounded-lg shadow-lg border backdrop-blur-sm"
        :class="getToastClasses(toast.type)"
      >
        <!-- Icon -->
        <div class="flex-shrink-0">
          <Icon
            :name="getToastIcon(toast.type)"
            class="w-5 h-5"
          />
        </div>
        
        <!-- Content -->
        <div class="flex-1 min-w-0">
          <p class="text-sm font-semibold">
            {{ toast.title }}
          </p>
          <p v-if="toast.message" class="text-sm mt-1 opacity-90">
            {{ toast.message }}
          </p>
        </div>
        
        <!-- Close button -->
        <button
          class="flex-shrink-0 opacity-70 hover:opacity-100 transition-opacity"
          @click="remove(toast.id)"
        >
          <Icon name="heroicons:x-mark" class="w-5 h-5" />
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import type { ToastType } from '~/composables/useNotificationToast'

const { toasts, remove } = useNotificationToast()
const offsetTop = ref('1rem')

const computeOffsetTop = () => {
  if (typeof window === 'undefined') {
    return
  }

  const header = document.querySelector<HTMLElement>('.app-header')
  const safeMargin = 16

  if (!header) {
    offsetTop.value = `${safeMargin}px`
    return
  }

  const { bottom } = header.getBoundingClientRect()
  const totalOffset = Math.max(safeMargin, Math.round(bottom) + safeMargin)
  offsetTop.value = `${totalOffset}px`
}

onMounted(() => {
  computeOffsetTop()
  window.addEventListener('resize', computeOffsetTop)
})

onBeforeUnmount(() => {
  if (typeof window !== 'undefined') {
    window.removeEventListener('resize', computeOffsetTop)
  }
})

watch(
  () => toasts.value.length,
  (count) => {
    if (count > 0) {
      computeOffsetTop()
    }
  }
)

const getToastClasses = (type: ToastType) => {
  const classes = {
    success: 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800 text-green-800 dark:text-green-200',
    error: 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800 text-red-800 dark:text-red-200',
    warning: 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-200 dark:border-yellow-800 text-yellow-800 dark:text-yellow-200',
    info: 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800 text-blue-800 dark:text-blue-200'
  }
  return classes[type]
}

const getToastIcon = (type: ToastType) => {
  const icons = {
    success: 'heroicons:check-circle',
    error: 'heroicons:x-circle',
    warning: 'heroicons:exclamation-triangle',
    info: 'heroicons:information-circle'
  }
  return icons[type]
}
</script>

<style scoped>
.toast-container {
  top: 1rem;
}

@media (max-width: 640px) {
  .toast-container {
    left: 50%;
    right: auto;
    transform: translateX(-50%);
    width: calc(100vw - 2rem);
    max-width: none;
  }
}

.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.toast-move {
  transition: transform 0.3s ease;
}
</style>

