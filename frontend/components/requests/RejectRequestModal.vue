<script setup lang="ts">
import type { Request } from '~/types/requests'

interface Props {
  request: Request | null
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  close: []
  confirm: [reason: string]
}>()

const reason = ref('')

const canReject = computed((): boolean => {
  return reason.value.trim().length > 0
})

const handleClose = (): void => {
  reason.value = ''
  emit('close')
}

const handleConfirm = (): void => {
  if (!canReject.value) return
  emit('confirm', reason.value)
  reason.value = ''
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="request"
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4"
      data-testid="reject-request-modal"
      @click.self="handleClose"
    >
      <div
        class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full p-6"
      >
        <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-4">
          Odrzuć wniosek
        </h3>
        <p class="text-gray-600 dark:text-gray-300 mb-4">
          Podaj powód odrzucenia wniosku
          <strong>{{ request.requestNumber }}</strong
          >:
        </p>

        <div class="mb-4">
          <label
            class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
          >
            Powód odrzucenia *
          </label>
          <textarea
            v-model="reason"
            rows="4"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-red-500 dark:bg-gray-700 dark:text-white"
            placeholder="Opisz powód odrzucenia..."
            required
          />
        </div>

        <div class="flex gap-3">
          <button
            :disabled="loading || !canReject"
            class="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-gray-400 text-white rounded-lg font-medium transition-colors"
            @click="handleConfirm"
          >
            {{ loading ? 'Odrzucanie...' : 'Odrzuć' }}
          </button>
          <button
            :disabled="loading"
            class="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
            @click="handleClose"
          >
            Anuluj
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
