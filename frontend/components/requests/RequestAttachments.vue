<script setup lang="ts">
import { Paperclip, Download, FileText, Image, File } from 'lucide-vue-next'

interface Props {
  attachments: string[]
}

defineProps<Props>()

const getFileExtension = (url: string): string => {
  const parts = url.split('.')
  return parts.length > 1 ? parts[parts.length - 1].toLowerCase() : ''
}

const getFileName = (url: string): string => {
  const parts = url.split('/')
  return parts[parts.length - 1]
}

const getFileIcon = (url: string) => {
  const ext = getFileExtension(url)

  if (['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp'].includes(ext)) {
    return Image
  } else if (['pdf'].includes(ext)) {
    return FileText
  } else {
    return File
  }
}

const getFileIconColor = (url: string): string => {
  const ext = getFileExtension(url)

  if (['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp'].includes(ext)) {
    return 'text-green-500'
  } else if (['pdf'].includes(ext)) {
    return 'text-red-500'
  } else {
    return 'text-gray-500'
  }
}
</script>

<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="flex items-center gap-2">
      <Paperclip class="w-5 h-5 text-gray-500 dark:text-gray-400" />
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
        Załączniki ({{ attachments.length }})
      </h3>
    </div>

    <!-- Attachments List -->
    <div v-if="attachments.length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <a
        v-for="(attachment, index) in attachments"
        :key="index"
        :href="attachment"
        target="_blank"
        class="flex items-center gap-3 p-4 bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 rounded-lg border border-gray-200 dark:border-gray-600 transition group"
      >
        <!-- File Icon -->
        <component
          :is="getFileIcon(attachment)"
          class="w-10 h-10 flex-shrink-0"
          :class="getFileIconColor(attachment)"
        />

        <!-- File Info -->
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-gray-900 dark:text-white truncate group-hover:text-blue-600 dark:group-hover:text-blue-400">
            {{ getFileName(attachment) }}
          </p>
          <p class="text-xs text-gray-500 dark:text-gray-400 uppercase">
            {{ getFileExtension(attachment) }}
          </p>
        </div>

        <!-- Download Icon -->
        <Download class="w-5 h-5 text-gray-400 group-hover:text-blue-600 dark:group-hover:text-blue-400 flex-shrink-0" />
      </a>
    </div>

    <!-- No Attachments -->
    <div v-else class="text-center py-8 text-gray-500 dark:text-gray-400">
      <Paperclip class="w-12 h-12 mx-auto mb-2 opacity-50" />
      <p>Brak załączników</p>
    </div>
  </div>
</template>
