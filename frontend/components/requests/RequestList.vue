<script setup lang="ts">
import { ClipboardList } from 'lucide-vue-next'
import type { Request } from '~/types/requests'

interface Props {
  requests: Request[]
  loading?: boolean
  emptyTitle?: string
  emptyMessage?: string
}

withDefaults(defineProps<Props>(), {
  loading: false,
  emptyTitle: 'Brak wniosków',
  emptyMessage: 'Nie masz jeszcze żadnych wniosków.'
})
</script>

<template>
  <div data-testid="request-list">
    <div v-if="loading" class="text-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto" />
    </div>

    <div v-else-if="requests.length === 0" class="text-center py-12">
      <ClipboardList class="w-16 h-16 mx-auto text-gray-400 mb-4" />
      <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">
        {{ emptyTitle }}
      </h3>
      <p class="text-gray-600 dark:text-gray-400">
        {{ emptyMessage }}
      </p>
    </div>

    <div v-else class="space-y-4">
      <RequestCard
        v-for="request in requests"
        :key="request.id"
        :request="request"
      />
    </div>
  </div>
</template>
