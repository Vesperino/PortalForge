<script setup lang="ts">
import type { ApprovalHistoryItem } from '~/types/requests'

interface Props {
  items: ApprovalHistoryItem[]
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  click: [requestId: string]
}>()

const handleClick = (requestId: string): void => {
  emit('click', requestId)
}
</script>

<template>
  <div data-testid="approval-history-list">
    <div v-if="loading" class="text-center py-12">
      <div
        class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"
      />
    </div>

    <div
      v-else-if="items.length === 0"
      class="text-center py-12 text-gray-500 dark:text-gray-400"
    >
      Brak historii zatwierdzonych/odrzuconych wniosków.
    </div>

    <div v-else class="space-y-4">
      <ApprovalHistoryCard
        v-for="item in items"
        :key="`${item.requestId}-${item.stepId}`"
        :item="item"
        @click="handleClick"
      />
    </div>
  </div>
</template>
