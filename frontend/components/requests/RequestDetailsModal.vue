<script setup lang="ts">
import { X } from 'lucide-vue-next'
import type { RequestWithDetails, RequestTemplate } from '~/types/requests'

interface Props {
  request: RequestWithDetails | null
  template: RequestTemplate | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
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

const priorityDisplay = computed(() => {
  if (!props.request) return { text: 'Standard', icon: '', class: '' }
  return props.request.priority === 'Urgent'
    ? { text: 'Pilne', icon: '🔴', class: 'text-red-600 dark:text-red-400' }
    : { text: 'Standard', icon: '🔵', class: '' }
})

const handleClose = (): void => {
  emit('close')
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="request"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
      data-testid="request-details-modal"
      @click.self="handleClose"
    >
      <div
        class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-6xl w-full max-h-[90vh] overflow-y-auto"
      >
        <div
          class="p-6 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between sticky top-0 bg-white dark:bg-gray-800 z-10"
        >
          <div class="flex items-center gap-4">
            <Icon
              :name="getIconifyName(request.requestTemplateIcon)"
              class="w-10 h-10"
            />
            <div>
              <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ request.requestTemplateName }}
              </h2>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ request.requestNumber }}
              </p>
            </div>
          </div>
          <button
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
            @click="handleClose"
          >
            <X class="w-6 h-6" />
          </button>
        </div>

        <div class="p-6 space-y-6">
          <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-6">
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
              <div>
                <p class="text-gray-500 dark:text-gray-400 mb-1">
                  Data złożenia
                </p>
                <p class="font-medium text-gray-900 dark:text-white">
                  {{ formatDate(request.submittedAt) }}
                </p>
              </div>
              <div>
                <p class="text-gray-500 dark:text-gray-400 mb-1">Wnioskodawca</p>
                <p class="font-medium text-gray-900 dark:text-white">
                  {{ request.submittedByName }}
                </p>
              </div>
              <div>
                <p class="text-gray-500 dark:text-gray-400 mb-1">Status</p>
                <RequestStatusBadge :status="request.status" />
              </div>
              <div>
                <p class="text-gray-500 dark:text-gray-400 mb-1">Priorytet</p>
                <p class="font-medium text-gray-900 dark:text-white">
                  <span :class="priorityDisplay.class">
                    {{ priorityDisplay.icon }} {{ priorityDisplay.text }}
                  </span>
                </p>
              </div>
            </div>
          </div>

          <div
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-6"
          >
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Historia zatwierdzeń
            </h3>
            <RequestTimeline :steps="request.approvalSteps" />
          </div>

          <div
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-6"
          >
            <RequestFormDataDisplay
              :form-data="request.formData"
              :fields="template?.fields"
            />
          </div>

          <div
            v-if="request.attachments && request.attachments.length > 0"
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-6"
          >
            <RequestAttachments :attachments="request.attachments" />
          </div>

          <div
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-6"
          >
            <RequestComments
              :comments="request.comments || []"
              :can-add-comment="false"
            />
          </div>

          <div
            v-if="request.editHistory && request.editHistory.length > 0"
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-6"
          >
            <RequestEditHistory :edit-history="request.editHistory" />
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>
