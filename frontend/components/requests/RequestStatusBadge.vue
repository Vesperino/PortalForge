<script setup lang="ts">
import type { RequestStatus, ApprovalStepStatus } from '~/types/requests'

interface Props {
  status: RequestStatus | ApprovalStepStatus
  size?: 'sm' | 'md' | 'lg'
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md'
})

const statusConfig: Record<string, { class: string; label: string }> = {
  Draft: {
    class: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200',
    label: 'Szkic'
  },
  InReview: {
    class: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    label: 'W trakcie oceny'
  },
  Approved: {
    class: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    label: 'Zatwierdzony'
  },
  Rejected: {
    class: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    label: 'Odrzucony'
  },
  AwaitingSurvey: {
    class: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    label: 'Wymaga quizu'
  },
  Pending: {
    class: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200',
    label: 'Oczekujący'
  },
  RequiresSurvey: {
    class: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    label: 'Wymaga quizu'
  },
  SurveyFailed: {
    class: 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
    label: 'Quiz niezdany'
  }
}

const sizeClasses: Record<string, string> = {
  sm: 'px-2 py-0.5 text-xs',
  md: 'px-3 py-1 text-sm',
  lg: 'px-4 py-1.5 text-base'
}

const badgeClass = computed(() => {
  const config = statusConfig[props.status] || statusConfig.Draft
  return `${config.class} ${sizeClasses[props.size]} font-medium rounded-full`
})

const badgeLabel = computed(() => {
  const config = statusConfig[props.status] || statusConfig.Draft
  return config.label
})
</script>

<template>
  <span :class="badgeClass" data-testid="request-status-badge">
    {{ badgeLabel }}
  </span>
</template>
