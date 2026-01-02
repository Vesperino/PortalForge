<script setup lang="ts">
import { CheckCircle, XCircle } from 'lucide-vue-next'
import type { Request, RequestApprovalStep } from '~/types/requests'

interface Props {
  request: Request
  currentStep: RequestApprovalStep | undefined
  isCurrentApprover: boolean
  requiresQuiz: boolean
  quizPassed: boolean
  quizScore: number | null | undefined
}

const props = defineProps<Props>()

const emit = defineEmits<{
  approve: [comment: string]
  reject: [reason: string]
}>()

const showApproveModal = ref(false)
const showRejectModal = ref(false)
const isSubmitting = ref(false)

const canApprove = computed((): boolean => {
  return props.isCurrentApprover
})

const handleApprove = (comment: string): void => {
  isSubmitting.value = true
  emit('approve', comment)
  showApproveModal.value = false
  isSubmitting.value = false
}

const handleReject = (reason: string): void => {
  isSubmitting.value = true
  emit('reject', reason)
  showRejectModal.value = false
  isSubmitting.value = false
}
</script>

<template>
  <div
    v-if="isCurrentApprover"
    class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6"
    data-testid="approval-actions"
  >
    <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
      Akcje
    </h3>
    <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
      Jestes aktualnym opiniujacym tego wniosku. Mozesz go zatwierdzic lub odrzucic.
    </p>

    <div
      v-if="requiresQuiz && !quizPassed"
      class="mb-4 p-4 border rounded-lg"
      :class="{
        'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800':
          quizScore === null || quizScore === undefined,
        'bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-800':
          quizScore !== null && quizScore !== undefined
      }"
    >
      <div class="flex items-start gap-3">
        <svg
          class="w-5 h-5 mt-0.5 flex-shrink-0"
          :class="{
            'text-blue-600 dark:text-blue-400': quizScore === null || quizScore === undefined,
            'text-amber-600 dark:text-amber-400': quizScore !== null && quizScore !== undefined
          }"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
          />
        </svg>
        <div>
          <p
            class="text-sm font-medium mb-1"
            :class="{
              'text-blue-900 dark:text-blue-100': quizScore === null || quizScore === undefined,
              'text-amber-900 dark:text-amber-100': quizScore !== null && quizScore !== undefined
            }"
          >
            {{ quizScore === null || quizScore === undefined ? 'Oczekiwanie na quiz' : 'Quiz niezaliczony' }}
          </p>
          <p
            class="text-sm"
            :class="{
              'text-blue-800 dark:text-blue-200': quizScore === null || quizScore === undefined,
              'text-amber-800 dark:text-amber-200': quizScore !== null && quizScore !== undefined
            }"
          >
            {{ quizScore === null || quizScore === undefined
              ? 'Wnioskodawca musi najpierw wypelnic wymagany quiz. Mozesz jednak podjac decyzje o zatwierdzeniu lub odrzuceniu mimo to.'
              : `Wnioskodawca nie zdal quizu (wynik: ${quizScore}%). Mimo to, jako opiniujacy mozesz podjac decyzje o zatwierdzeniu lub odrzuceniu wniosku wedlug wlasnego uznania.`
            }}
          </p>
        </div>
      </div>
    </div>

    <div class="flex gap-3">
      <button
        :disabled="!canApprove"
        class="px-6 py-3 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium flex items-center gap-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        data-testid="approval-actions-approve-button"
        @click="showApproveModal = true"
      >
        <CheckCircle class="w-5 h-5" />
        Zatwierdz
      </button>
      <button
        class="px-6 py-3 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium flex items-center gap-2 transition-colors"
        data-testid="approval-actions-reject-button"
        @click="showRejectModal = true"
      >
        <XCircle class="w-5 h-5" />
        Odrzuc
      </button>
    </div>

    <ApprovalActionsModals
      :show-approve-modal="showApproveModal"
      :show-reject-modal="showRejectModal"
      :is-submitting="isSubmitting"
      @update:show-approve-modal="showApproveModal = $event"
      @update:show-reject-modal="showRejectModal = $event"
      @approve="handleApprove"
      @reject="handleReject"
    />
  </div>
</template>
