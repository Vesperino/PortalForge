<script setup lang="ts">
import type { Request, RequestApprovalStep } from '~/types/requests'

interface Props {
  request: Request
}

const props = defineProps<Props>()

const { getIconifyName } = useIconMapping()

const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const getCurrentStepLabel = (request: Request): string => {
  const currentStep = request.approvalSteps.find(
    (s: RequestApprovalStep) => s.status === 'InReview'
  )
  return currentStep ? `U ${currentStep.approverName}` : '-'
}

const getApprovalProgress = (request: Request): string => {
  const approved = request.approvalSteps.filter(
    (s: RequestApprovalStep) => s.status === 'Approved'
  ).length
  const total = request.approvalSteps.length
  return `${approved}/${total}`
}

const priorityDisplay = computed(() => {
  return props.request.priority === 'Urgent'
    ? { text: 'Pilne', icon: '🔴', class: 'text-red-600 dark:text-red-400' }
    : { text: 'Standard', icon: '🔵', class: '' }
})
</script>

<template>
  <NuxtLink
    :to="`/dashboard/requests/${request.id}`"
    class="block w-full bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 hover:shadow-lg hover:border-blue-500 dark:hover:border-blue-400 transition-all group"
    data-testid="request-card"
  >
    <div class="flex items-start justify-between mb-4">
      <div class="flex items-start gap-4">
        <div class="relative">
          <Icon
            :name="getIconifyName(request.requestTemplateIcon)"
            class="w-12 h-12 flex-shrink-0 transition-transform group-hover:scale-110"
          />
        </div>
        <div>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
            {{ request.requestTemplateName }}
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            {{ request.requestNumber }}
          </p>
        </div>
      </div>

      <RequestStatusBadge :status="request.status" />
    </div>

    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4 text-sm">
      <div>
        <p class="text-gray-500 dark:text-gray-400">Data złożenia</p>
        <p class="font-medium text-gray-900 dark:text-white">
          {{ formatDate(request.submittedAt) }}
        </p>
      </div>
      <div>
        <p class="text-gray-500 dark:text-gray-400">Priorytet</p>
        <p class="font-medium text-gray-900 dark:text-white">
          <span :class="priorityDisplay.class">
            {{ priorityDisplay.icon }} {{ priorityDisplay.text }}
          </span>
        </p>
      </div>
      <div>
        <p class="text-gray-500 dark:text-gray-400">Etap</p>
        <p class="font-medium text-gray-900 dark:text-white">
          {{ getCurrentStepLabel(request) }}
        </p>
      </div>
      <div>
        <p class="text-gray-500 dark:text-gray-400">Postęp</p>
        <p class="font-medium text-gray-900 dark:text-white">
          {{ getApprovalProgress(request) }}
        </p>
      </div>
    </div>

    <div class="flex items-center text-blue-600 dark:text-blue-400 group-hover:text-blue-700 dark:group-hover:text-blue-300 font-medium text-sm">
      <span>Zobacz szczegóły</span>
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
  </NuxtLink>
</template>
