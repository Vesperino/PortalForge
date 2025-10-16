<script setup lang="ts">
interface Props {
  variant?: 'success' | 'error' | 'warning' | 'info'
  dismissible?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'info',
  dismissible: false
})

interface Emits {
  (e: 'dismiss'): void
}

const emit = defineEmits<Emits>()

const isVisible = ref(true)

const alertClasses = computed(() => {
  const baseClasses = 'p-4 rounded-md border'

  const variantClasses = {
    success: 'bg-green-50 border-green-200 text-green-800',
    error: 'bg-red-50 border-red-200 text-red-800',
    warning: 'bg-yellow-50 border-yellow-200 text-yellow-800',
    info: 'bg-blue-50 border-blue-200 text-blue-800'
  }

  return `${baseClasses} ${variantClasses[props.variant]}`
})

const iconPath = computed(() => {
  switch (props.variant) {
    case 'success':
      return 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z'
    case 'error':
      return 'M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z'
    case 'warning':
      return 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z'
    case 'info':
    default:
      return 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'
  }
})

const handleDismiss = () => {
  isVisible.value = false
  emit('dismiss')
}
</script>

<template>
  <div
    v-if="isVisible"
    :class="alertClasses"
    role="alert"
  >
    <div class="flex items-start">
      <svg
        class="w-5 h-5 mr-3 flex-shrink-0"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          :d="iconPath"
        />
      </svg>

      <div class="flex-1">
        <slot />
      </div>

      <button
        v-if="dismissible"
        type="button"
        class="ml-3 flex-shrink-0 hover:opacity-75 focus:outline-none"
        aria-label="Zamknij"
        @click="handleDismiss"
      >
        <svg
          class="w-5 h-5"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M6 18L18 6M6 6l12 12"
          />
        </svg>
      </button>
    </div>
  </div>
</template>
