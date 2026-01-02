<script setup lang="ts">
import type { ApprovalHistoryItem } from '~/types/requests'

interface Props {
  item: ApprovalHistoryItem
}

defineProps<Props>()

const emit = defineEmits<{
  click: [requestId: string]
}>()

const { getIconifyName } = useIconMapping()

const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const handleClick = (requestId: string): void => {
  emit('click', requestId)
}
</script>

<template>
  <button
    class="w-full bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 hover:shadow-lg hover:border-blue-500 dark:hover:border-blue-400 transition-all text-left group"
    data-testid="approval-history-card"
    @click="handleClick(item.requestId)"
  >
    <div class="flex items-start justify-between mb-4">
      <div class="flex items-start gap-4">
        <Icon
          :name="getIconifyName(item.templateIcon)"
          class="w-12 h-12 flex-shrink-0 transition-transform group-hover:scale-110"
        />
        <div>
          <h3
            class="text-lg font-semibold text-gray-900 dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors"
          >
            {{ item.templateName }}
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            {{ item.requestNumber }}
          </p>
          <p class="text-sm text-gray-500 dark:text-gray-500 mt-1">
            Wnioskodawca: {{ item.submittedByName }}
          </p>
        </div>
      </div>

      <span
        :class="[
          'px-3 py-1 text-sm font-medium rounded-full',
          item.decision === 'Approved'
            ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
            : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
        ]"
      >
        {{ item.decision === 'Approved' ? 'Zatwierdzono' : 'Odrzucono' }}
      </span>
    </div>

    <div class="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
      <div>
        <p class="text-gray-500 dark:text-gray-400">Data złożenia</p>
        <p class="font-medium text-gray-900 dark:text-white">
          {{ formatDate(item.submittedAt) }}
        </p>
      </div>
      <div>
        <p class="text-gray-500 dark:text-gray-400">Data decyzji</p>
        <p class="font-medium text-gray-900 dark:text-white">
          {{ item.finishedAt ? formatDate(item.finishedAt) : '-' }}
        </p>
      </div>
      <div v-if="item.comment">
        <p class="text-gray-500 dark:text-gray-400">Komentarz</p>
        <p class="font-medium text-gray-900 dark:text-white truncate">
          {{ item.comment }}
        </p>
      </div>
    </div>

    <div
      class="flex items-center text-blue-600 dark:text-blue-400 group-hover:text-blue-700 dark:group-hover:text-blue-300 font-medium text-sm mt-4"
    >
      <span>Zobacz pełne szczegóły</span>
      <svg
        class="w-4 h-4 ml-2 transition-transform group-hover:translate-x-1"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M9 5l7 7-7 7"
        />
      </svg>
    </div>
  </button>
</template>
