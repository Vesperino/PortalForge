<script setup lang="ts">
import { Plus, List, ClipboardList, CheckCircle } from 'lucide-vue-next'
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

const isActive = (tab: RequestTab): boolean => activeTab.value === tab
</script>

<template>
  <div
    class="mb-6 bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-1.5 inline-flex gap-1"
    data-testid="request-tabs"
  >
    <button
      :class="[
        'flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all duration-200',
        isActive('new')
          ? 'bg-blue-600 text-white shadow-sm'
          : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
      ]"
      @click="activeTab = 'new'"
    >
      <Plus class="w-4 h-4" />
      <span>Nowy wniosek</span>
    </button>

    <button
      :class="[
        'flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all duration-200',
        isActive('my-requests')
          ? 'bg-blue-600 text-white shadow-sm'
          : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
      ]"
      @click="activeTab = 'my-requests'"
    >
      <List class="w-4 h-4" />
      <span>Moje wnioski</span>
      <span
        v-if="myRequestsCount > 0"
        :class="[
          'px-2 py-0.5 text-xs rounded-full',
          isActive('my-requests')
            ? 'bg-white/20 text-white'
            : 'bg-gray-200 dark:bg-gray-600 text-gray-700 dark:text-gray-200'
        ]"
      >
        {{ myRequestsCount }}
      </span>
    </button>

    <button
      v-if="pendingApprovalsCount > 0"
      :class="[
        'flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all duration-200',
        isActive('to-approve')
          ? 'bg-orange-500 text-white shadow-sm'
          : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
      ]"
      @click="activeTab = 'to-approve'"
    >
      <ClipboardList class="w-4 h-4" />
      <span>Do zatwierdzenia</span>
      <span
        :class="[
          'px-2 py-0.5 text-xs rounded-full',
          isActive('to-approve')
            ? 'bg-white/20 text-white'
            : 'bg-orange-100 dark:bg-orange-900/50 text-orange-700 dark:text-orange-300'
        ]"
      >
        {{ pendingApprovalsCount }}
      </span>
    </button>

    <button
      v-if="approvalsHistoryCount > 0"
      :class="[
        'flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all duration-200',
        isActive('approved-by-me')
          ? 'bg-green-600 text-white shadow-sm'
          : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
      ]"
      @click="activeTab = 'approved-by-me'"
    >
      <CheckCircle class="w-4 h-4" />
      <span>Historia</span>
      <span
        :class="[
          'px-2 py-0.5 text-xs rounded-full',
          isActive('approved-by-me')
            ? 'bg-white/20 text-white'
            : 'bg-green-100 dark:bg-green-900/50 text-green-700 dark:text-green-300'
        ]"
      >
        {{ approvalsHistoryCount }}
      </span>
    </button>
  </div>
</template>
