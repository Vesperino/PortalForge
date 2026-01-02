<script setup lang="ts">
interface Props {
  showApproveModal: boolean
  showRejectModal: boolean
  isSubmitting: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  'update:showApproveModal': [value: boolean]
  'update:showRejectModal': [value: boolean]
  approve: [comment: string]
  reject: [reason: string]
}>()

const approveComment = ref('')
const rejectComment = ref('')

const handleCloseApproveModal = (): void => {
  emit('update:showApproveModal', false)
  approveComment.value = ''
}

const handleCloseRejectModal = (): void => {
  emit('update:showRejectModal', false)
  rejectComment.value = ''
}

const handleApprove = (): void => {
  emit('approve', approveComment.value)
  approveComment.value = ''
}

const handleReject = (): void => {
  if (!rejectComment.value.trim()) {
    return
  }
  emit('reject', rejectComment.value)
  rejectComment.value = ''
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="showApproveModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      data-testid="approval-actions-approve-modal"
      @click.self="handleCloseApproveModal"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full">
        <div class="p-6 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
            Zatwierdz wniosek
          </h3>
        </div>

        <div class="p-6 space-y-4">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Czy na pewno chcesz zatwierdzic ten wniosek?
          </p>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Komentarz (opcjonalnie)
            </label>
            <textarea
              v-model="approveComment"
              rows="3"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
              placeholder="Dodaj komentarz..."
              data-testid="approval-actions-approve-comment"
            />
          </div>
        </div>

        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700">
          <button
            class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            :disabled="isSubmitting"
            data-testid="approval-actions-approve-cancel"
            @click="handleCloseApproveModal"
          >
            Anuluj
          </button>
          <button
            class="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="isSubmitting"
            data-testid="approval-actions-approve-confirm"
            @click="handleApprove"
          >
            {{ isSubmitting ? 'Zatwierdzanie...' : 'Zatwierdz' }}
          </button>
        </div>
      </div>
    </div>

    <div
      v-if="showRejectModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      data-testid="approval-actions-reject-modal"
      @click.self="handleCloseRejectModal"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full">
        <div class="p-6 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
            Odrzuc wniosek
          </h3>
        </div>

        <div class="p-6 space-y-4">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Czy na pewno chcesz odrzucic ten wniosek? Podaj powod odrzucenia.
          </p>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Powod odrzucenia <span class="text-red-500">*</span>
            </label>
            <textarea
              v-model="rejectComment"
              rows="4"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent bg-white dark:bg-gray-900 text-gray-900 dark:text-white"
              placeholder="Wpisz powod odrzucenia wniosku..."
              required
              data-testid="approval-actions-reject-reason"
            />
          </div>
        </div>

        <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-700">
          <button
            class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            :disabled="isSubmitting"
            data-testid="approval-actions-reject-cancel"
            @click="handleCloseRejectModal"
          >
            Anuluj
          </button>
          <button
            class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="isSubmitting || !rejectComment.trim()"
            data-testid="approval-actions-reject-confirm"
            @click="handleReject"
          >
            {{ isSubmitting ? 'Odrzucanie...' : 'Odrzuc' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
