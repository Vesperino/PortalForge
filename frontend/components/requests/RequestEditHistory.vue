<script setup lang="ts">
import { computed } from 'vue'
import { History, User, Calendar } from 'lucide-vue-next'

interface EditHistoryEntry {
  id: string
  editedByUserId: string
  editedByUserName: string
  editedAt: string
  oldFormData: string
  newFormData: string
  changeReason?: string
}

interface Props {
  editHistory: EditHistoryEntry[]
}

const props = defineProps<Props>()

const sortedHistory = computed(() => {
  return [...props.editHistory].sort((a, b) =>
    new Date(b.editedAt).getTime() - new Date(a.editedAt).getTime()
  )
})

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getChangeSummary = (oldData: string, newData: string): string[] => {
  try {
    const oldObj = JSON.parse(oldData)
    const newObj = JSON.parse(newData)
    const changes: string[] = []

    // Compare keys
    const allKeys = new Set([...Object.keys(oldObj), ...Object.keys(newObj)])

    allKeys.forEach(key => {
      const oldValue = oldObj[key]
      const newValue = newObj[key]

      if (oldValue !== newValue) {
        const fieldName = formatFieldName(key)
        if (oldValue === undefined) {
          changes.push(`Dodano ${fieldName}: ${formatValue(newValue)}`)
        } else if (newValue === undefined) {
          changes.push(`Usunięto ${fieldName}`)
        } else {
          changes.push(`Zmieniono ${fieldName}: "${formatValue(oldValue)}" → "${formatValue(newValue)}"`)
        }
      }
    })

    return changes.length > 0 ? changes : ['Brak widocznych zmian']
  } catch {
    return ['Nie można porównać zmian']
  }
}

const formatFieldName = (key: string): string => {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, str => str.toUpperCase())
    .trim()
}

const formatValue = (value: unknown): string => {
  if (value === null || value === undefined) return '-'
  if (typeof value === 'boolean') return value ? 'Tak' : 'Nie'
  if (typeof value === 'object') return JSON.stringify(value)

  // Try to format as date if it looks like an ISO date
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}/.test(value)) {
    try {
      const date = new Date(value)
      return date.toLocaleDateString('pl-PL', { year: 'numeric', month: 'long', day: 'numeric' })
    } catch {
      return value
    }
  }

  return String(value)
}
</script>

<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="flex items-center gap-2">
      <History class="w-5 h-5 text-gray-500 dark:text-gray-400" />
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
        Historia edycji ({{ editHistory.length }})
      </h3>
    </div>

    <!-- History Timeline -->
    <div v-if="editHistory.length > 0" class="space-y-4">
      <div
        v-for="(entry, index) in sortedHistory"
        :key="entry.id"
        class="relative pl-8 pb-6 border-l-2 border-gray-200 dark:border-gray-700"
        :class="{ 'pb-0': index === sortedHistory.length - 1 }"
      >
        <!-- Timeline Dot -->
        <div class="absolute -left-2 top-0 w-4 h-4 bg-blue-500 rounded-full border-4 border-white dark:border-gray-900" />

        <!-- Entry Content -->
        <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
          <!-- Entry Header -->
          <div class="flex items-start justify-between mb-3">
            <div class="flex items-center gap-2">
              <User class="w-4 h-4 text-gray-500 dark:text-gray-400" />
              <span class="font-medium text-gray-900 dark:text-white">
                {{ entry.editedByUserName }}
              </span>
            </div>
            <div class="flex items-center gap-1 text-sm text-gray-500 dark:text-gray-400">
              <Calendar class="w-4 h-4" />
              <span>{{ formatDate(entry.editedAt) }}</span>
            </div>
          </div>

          <!-- Change Reason -->
          <div v-if="entry.changeReason" class="mb-3 p-3 bg-blue-50 dark:bg-blue-900/20 rounded border border-blue-200 dark:border-blue-800">
            <p class="text-sm font-medium text-blue-900 dark:text-blue-200 mb-1">
              Powód zmiany:
            </p>
            <p class="text-sm text-blue-800 dark:text-blue-300">
              {{ entry.changeReason }}
            </p>
          </div>

          <!-- Changes Summary -->
          <div class="space-y-2">
            <p class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Wprowadzone zmiany:
            </p>
            <ul class="space-y-1">
              <li
                v-for="(change, idx) in getChangeSummary(entry.oldFormData, entry.newFormData)"
                :key="idx"
                class="text-sm text-gray-600 dark:text-gray-400 pl-4 relative before:content-['•'] before:absolute before:left-0"
              >
                {{ change }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <!-- No History -->
    <div v-else class="text-center py-8 text-gray-500 dark:text-gray-400">
      <History class="w-12 h-12 mx-auto mb-2 opacity-50" />
      <p>Brak historii edycji</p>
      <p class="text-sm mt-1">Wniosek nie był jeszcze edytowany</p>
    </div>
  </div>
</template>
