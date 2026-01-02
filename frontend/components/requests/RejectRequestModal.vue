<script setup lang="ts">
import type { Request } from '~/types/requests'

interface Props {
  request: Request | null
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  close: []
  confirm: [reason: string]
}>()

const reason = ref('')

const isOpen = computed(() => props.request !== null)

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
  <BaseModal
    :is-open="isOpen"
    title="Odrzuć wniosek"
    size="md"
    @close="handleClose"
  >
    <div data-testid="reject-request-modal">
      <p class="text-gray-600 dark:text-gray-300 mb-4">
        Podaj powód odrzucenia wniosku
        <strong>{{ request?.requestNumber }}</strong>:
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
          data-testid="reject-request-reason-input"
        />
      </div>
    </div>

    <template #footer>
      <div class="flex gap-3 justify-end">
        <button
          :disabled="loading"
          class="px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white rounded-lg font-medium transition-colors"
          data-testid="reject-request-cancel-btn"
          @click="handleClose"
        >
          Anuluj
        </button>
        <button
          :disabled="loading || !canReject"
          class="px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-gray-400 text-white rounded-lg font-medium transition-colors"
          data-testid="reject-request-confirm-btn"
          @click="handleConfirm"
        >
          {{ loading ? 'Odrzucanie...' : 'Odrzuć' }}
        </button>
      </div>
    </template>
  </BaseModal>
</template>
