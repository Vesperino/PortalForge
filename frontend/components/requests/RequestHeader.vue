<script setup lang="ts">
import { ArrowLeft } from 'lucide-vue-next'
import type { Request } from '~/types/requests'

interface Props {
  request: Request
}

defineProps<Props>()

const emit = defineEmits<{
  back: []
}>()

const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const handleBack = (): void => {
  emit('back')
}
</script>

<template>
  <div data-testid="request-header">
    <button
      class="mb-6 flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors"
      data-testid="request-header-back-button"
      @click="handleBack"
    >
      <ArrowLeft class="w-5 h-5" />
      <span>Powrot do wnioskow</span>
    </button>

    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <div class="flex items-start justify-between mb-4">
        <div>
          <h1
            class="text-2xl font-bold text-gray-900 dark:text-white mb-2"
            data-testid="request-header-title"
          >
            {{ request.requestTemplateName }}
          </h1>
          <p
            class="text-gray-600 dark:text-gray-400"
            data-testid="request-header-number"
          >
            {{ request.requestNumber }}
          </p>
        </div>
        <RequestStatusBadge :status="request.status" />
      </div>

      <div class="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
        <div>
          <p class="text-gray-500 dark:text-gray-400">Data zlozenia</p>
          <p
            class="font-medium text-gray-900 dark:text-white"
            data-testid="request-header-date"
          >
            {{ formatDate(request.submittedAt) }}
          </p>
        </div>
        <div>
          <p class="text-gray-500 dark:text-gray-400">Wnioskodawca</p>
          <p
            class="font-medium text-gray-900 dark:text-white"
            data-testid="request-header-submitter"
          >
            {{ request.submittedByName }}
          </p>
        </div>
        <div>
          <p class="text-gray-500 dark:text-gray-400">Priorytet</p>
          <p class="font-medium text-gray-900 dark:text-white">
            <span
              :class="request.priority === 'Urgent' ? 'text-red-600 dark:text-red-400' : ''"
              data-testid="request-header-priority"
            >
              {{ request.priority === 'Urgent' ? 'Pilne' : 'Standard' }}
            </span>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
