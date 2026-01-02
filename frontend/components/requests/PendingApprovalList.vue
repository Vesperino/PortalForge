<script setup lang="ts">
import type { Request } from '~/types/requests'

interface Props {
  requests: Request[]
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  approve: [request: Request]
  reject: [request: Request]
}>()

const handleApprove = (request: Request): void => {
  emit('approve', request)
}

const handleReject = (request: Request): void => {
  emit('reject', request)
}
</script>

<template>
  <div data-testid="pending-approval-list">
    <div v-if="loading" class="text-center py-12">
      <div
        class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"
      />
    </div>

    <div v-else-if="requests.length === 0" class="text-center py-12">
      <Icon
        name="heroicons:check-circle"
        class="w-16 h-16 mx-auto text-green-500 mb-4"
      />
      <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
        Brak wniosków do zatwierdzenia
      </h3>
      <p class="text-gray-600 dark:text-gray-400">
        Wszystkie wnioski zostały przetworzone
      </p>
    </div>

    <div v-else class="space-y-4">
      <PendingApprovalCard
        v-for="request in requests"
        :key="request.id"
        :request="request"
        @approve="handleApprove"
        @reject="handleReject"
      />
    </div>
  </div>
</template>
