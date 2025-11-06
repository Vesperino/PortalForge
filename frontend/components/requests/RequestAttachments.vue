<script setup lang="ts">
import { ref } from 'vue'
import { Paperclip, Download, FileText, Image, File, Eye } from 'lucide-vue-next'
import DocumentViewer from '~/components/common/DocumentViewer.vue'

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
    return 'text-green-500 dark:text-green-400'
  } else if (['pdf'].includes(ext)) {
    return 'text-red-500 dark:text-red-400'
  } else {
    return 'text-gray-500 dark:text-gray-400'
  }
}

const isImage = (url: string): boolean => {
  const ext = getFileExtension(url)
  return ['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp'].includes(ext)
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
    <div class="flex items-center gap-3">
      <div class="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 dark:from-blue-600 dark:to-blue-700 rounded-lg flex items-center justify-center shadow-lg">
        <Paperclip class="w-5 h-5 text-white" />
      </div>
      <div>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
          Załączniki
        </h3>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          {{ attachments.length }} {{ attachments.length === 1 ? 'plik' : attachments.length < 5 ? 'pliki' : 'plików' }}
        </p>
      </div>
    </div>

    <!-- Attachments Grid -->
    <div v-if="attachments.length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <div
        v-for="(attachment, index) in attachments"
        :key="index"
        class="group relative bg-white dark:bg-gray-800 rounded-xl border-2 border-gray-200 dark:border-gray-700 overflow-hidden hover:border-blue-400 dark:hover:border-blue-500 transition-all duration-300 hover:shadow-xl hover:-translate-y-1"
      >
        <!-- Thumbnail/Preview -->
        <div class="aspect-video bg-gradient-to-br from-gray-100 to-gray-200 dark:from-gray-700 dark:to-gray-800 relative overflow-hidden">
          <!-- Image Preview -->
          <img
            v-if="isImage(attachment)"
            :src="attachment"
            :alt="getFileName(attachment)"
            class="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
          />

          <!-- PDF Icon -->
          <div
            v-else-if="getFileExtension(attachment) === 'pdf'"
            class="w-full h-full flex items-center justify-center"
          >
            <div class="text-center">
              <FileText class="w-16 h-16 mx-auto mb-2 text-red-500 dark:text-red-400" />
              <span class="text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase">PDF</span>
            </div>
          </div>

          <!-- Other File Icon -->
          <div
            v-else
            class="w-full h-full flex items-center justify-center"
          >
            <div class="text-center">
              <File class="w-16 h-16 mx-auto mb-2 text-gray-500 dark:text-gray-400" />
              <span class="text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase">
                {{ getFileExtension(attachment) }}
              </span>
            </div>
          </div>

          <!-- Hover Overlay with Actions -->
          <div class="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center gap-2">
            <!-- Preview Button -->
            <button
              v-if="canPreview(attachment)"
              class="p-3 bg-blue-600 hover:bg-blue-700 rounded-lg transition-all shadow-lg hover:shadow-xl hover:scale-110 text-white"
              title="Podgląd"
              @click="openViewer(index)"
            >
              <Eye class="w-5 h-5" />
            </button>

            <!-- Download Button -->
            <button
              class="p-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-all shadow-lg hover:shadow-xl hover:scale-110 text-white"
              title="Pobierz"
              @click="downloadFile(attachment, $event)"
            >
              <Download class="w-5 h-5" />
            </button>
          </div>
        </div>

        <!-- File Info -->
        <div class="p-4 bg-white dark:bg-gray-800">
          <div class="flex items-start gap-3">
            <component
              :is="getFileIcon(attachment)"
              class="w-6 h-6 flex-shrink-0 mt-0.5"
              :class="getFileIconColor(attachment)"
            />
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 dark:text-white truncate" :title="getFileName(attachment)">
                {{ getFileName(attachment) }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400 uppercase font-medium mt-0.5">
                {{ getFileExtension(attachment) }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- No Attachments -->
    <div v-else class="text-center py-16 px-4">
      <div class="w-20 h-20 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-4">
        <Paperclip class="w-10 h-10 text-gray-400 dark:text-gray-500" />
      </div>
      <h4 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
        Brak załączników
      </h4>
      <p class="text-sm text-gray-500 dark:text-gray-400">
        Ten wniosek nie zawiera żadnych załączników
      </p>
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
