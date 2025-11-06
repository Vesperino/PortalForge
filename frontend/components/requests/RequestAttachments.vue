<script setup lang="ts">
import { ref } from 'vue'
import { Paperclip, Download, FileText, Image, File, Eye } from 'lucide-vue-next'
import FilePreviewModal from '~/components/common/FilePreviewModal.vue'

interface Props {
  attachments: string[]
}

defineProps<Props>()

const showPreviewModal = ref(false)
const previewFile = ref<{ url: string, name: string } | null>(null)

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

const openPreview = (url: string, event?: Event) => {
  if (event) {
    event.preventDefault()
  }
  previewFile.value = {
    url,
    name: getFileName(url)
  }
  showPreviewModal.value = true
}

const downloadFile = (url: string, event: Event) => {
  event.preventDefault()
  event.stopPropagation()
  window.open(url, '_blank')
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
        class="flex items-center gap-3 p-4 bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 rounded-lg border border-gray-200 dark:border-gray-600 transition group cursor-pointer"
        @click="openPreview(attachment)"
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

        <!-- Action Buttons -->
        <div class="flex gap-2 flex-shrink-0">
          <button
            title="Podgląd"
            class="p-1 text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition"
            @click="openPreview(attachment, $event)"
          >
            <Eye class="w-5 h-5" />
          </button>
          <button
            title="Pobierz"
            class="p-1 text-gray-400 hover:text-green-600 dark:hover:text-green-400 transition"
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

    <!-- File Preview Modal -->
    <FilePreviewModal
      v-if="previewFile"
      v-model:visible="showPreviewModal"
      :file-url="previewFile.url"
      :file-name="previewFile.name"
    />
  </div>
</template>
