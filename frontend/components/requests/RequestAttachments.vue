<script setup lang="ts">
import { ref } from 'vue'
import { Paperclip, Download, FileText, Image, File, Eye } from 'lucide-vue-next'

interface Props {
  attachments: string[]
}

defineProps<Props>()

const showViewer = ref(false)
const viewerInitialIndex = ref(0)

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

const canPreview = (url: string): boolean => {
  const ext = getFileExtension(url)
  return ['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp', 'pdf'].includes(ext)
}

const openViewer = (index: number) => {
  viewerInitialIndex.value = index
  showViewer.value = true
}

const downloadFile = (url: string, event: Event) => {
  event.stopPropagation()
  const link = document.createElement('a')
  link.href = url
  link.download = getFileName(url)
  link.target = '_blank'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
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
      <div
        v-for="(attachment, index) in attachments"
        :key="index"
        class="flex items-center gap-3 p-4 bg-gray-50 dark:bg-gray-700 rounded-lg border border-gray-200 dark:border-gray-600 group"
      >
        <!-- File Icon -->
        <component
          :is="getFileIcon(attachment)"
          class="w-10 h-10 flex-shrink-0"
          :class="getFileIconColor(attachment)"
        />

        <!-- File Info -->
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
            {{ getFileName(attachment) }}
          </p>
          <p class="text-xs text-gray-500 dark:text-gray-400 uppercase">
            {{ getFileExtension(attachment) }}
          </p>
        </div>

        <!-- Action Buttons -->
        <div class="flex items-center gap-2 flex-shrink-0">
          <!-- Preview Button (for images and PDFs) -->
          <button
            v-if="canPreview(attachment)"
            class="p-2 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors text-gray-500 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-400"
            title="Podgląd"
            @click="openViewer(index)"
          >
            <Eye class="w-5 h-5" />
          </button>

          <!-- Download Button -->
          <button
            class="p-2 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors text-gray-500 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-400"
            title="Pobierz"
            @click="downloadFile(attachment, $event)"
          >
            <Download class="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>

    <!-- No Attachments -->
    <div v-else class="text-center py-8 text-gray-500 dark:text-gray-400">
      <Paperclip class="w-12 h-12 mx-auto mb-2 opacity-50" />
      <p>Brak załączników</p>
    </div>

    <!-- Document Viewer Modal -->
    <DocumentViewer
      v-if="showViewer"
      :attachments="attachments"
      :initial-index="viewerInitialIndex"
      @close="showViewer = false"
    />
  </div>
</template>
