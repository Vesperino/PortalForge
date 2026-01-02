<script setup lang="ts">
import { Plus, List, ClipboardList } from 'lucide-vue-next'
import type { RequestTab } from '~/types/requests'

interface Props {
  modelValue: RequestTab
  myRequestsCount?: number
  pendingApprovalsCount?: number
  approvalsHistoryCount?: number
}

const props = withDefaults(defineProps<Props>(), {
  myRequestsCount: 0,
  pendingApprovalsCount: 0,
  approvalsHistoryCount: 0
})

const emit = defineEmits<{
  'update:modelValue': [value: RequestTab]
}>()

const activeTab = computed({
  get: () => props.modelValue,
  set: (value: RequestTab) => emit('update:modelValue', value)
})

const getTabClass = (tab: RequestTab): string => {
  const baseClass = 'py-4 px-1 border-b-2 font-medium text-sm transition-colors'
  const activeClass = 'border-blue-500 text-blue-600 dark:text-blue-400'
  const inactiveClass =
    'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'

  return `${baseClass} ${activeTab.value === tab ? activeClass : inactiveClass}`
}
</script>

<template>
  <div
    class="mb-6 border-b border-gray-200 dark:border-gray-700"
    data-testid="request-tabs"
  >
    <nav class="-mb-px flex space-x-8">
      <button :class="getTabClass('new')" @click="activeTab = 'new'">
        <Plus class="w-4 h-4 inline mr-2" />
        Nowy wniosek
      </button>

      <button
        :class="getTabClass('my-requests')"
        @click="activeTab = 'my-requests'"
      >
        <List class="w-4 h-4 inline mr-2" />
        Moje wnioski
        <span
          v-if="myRequestsCount > 0"
          class="ml-2 px-2 py-0.5 text-xs bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 rounded-full"
        >
          {{ myRequestsCount }}
        </span>
      </button>

      <button
        v-if="pendingApprovalsCount > 0"
        :class="getTabClass('to-approve')"
        @click="activeTab = 'to-approve'"
      >
        <ClipboardList class="w-4 h-4 inline mr-2" />
        Do zatwierdzenia
        <span
          class="ml-2 px-2 py-0.5 text-xs bg-orange-100 dark:bg-orange-900 text-orange-800 dark:text-orange-200 rounded-full"
        >
          {{ pendingApprovalsCount }}
        </span>
      </button>

      <button
        v-if="approvalsHistoryCount > 0"
        :class="getTabClass('approved-by-me')"
        @click="activeTab = 'approved-by-me'"
      >
        <Icon name="heroicons:check-circle" class="w-4 h-4 inline mr-2" />
        Zatwierdzone przeze mnie
        <span
          class="ml-2 px-2 py-0.5 text-xs bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 rounded-full"
        >
          {{ approvalsHistoryCount }}
        </span>
      </button>
    </nav>
  </div>
</template>
