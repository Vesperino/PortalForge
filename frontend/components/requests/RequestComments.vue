<script setup lang="ts">
import { ref, computed } from 'vue'
import { MessageCircle, Send, Paperclip, X, Eye, Download } from 'lucide-vue-next'
import DocumentViewer from '~/components/common/DocumentViewer.vue'

interface Comment {
  id: string
  userId: string
  userName: string
  comment: string
  attachments: string[]
  createdAt: string
}

interface Props {
  comments: Comment[]
  canAddComment: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  addComment: [data: { comment: string, attachments: File[] }]
}>()

const newComment = ref('')
const selectedFiles = ref<File[]>([])
const fileInput = ref<HTMLInputElement | null>(null)
const isSubmitting = ref(false)

const showAttachmentsViewer = ref(false)
const viewerAttachments = ref<string[]>([])
const viewerInitialIndex = ref(0)

const sortedComments = computed(() => {
  return [...props.comments].sort((a, b) =>
    new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
  )
})

const canSubmit = computed(() => {
  return (newComment.value.trim() !== '' || selectedFiles.value.length > 0) && !isSubmitting.value
})

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  const days = Math.floor(diff / (1000 * 60 * 60 * 24))

  if (days === 0) {
    const hours = Math.floor(diff / (1000 * 60 * 60))
    if (hours === 0) {
      const minutes = Math.floor(diff / (1000 * 60))
      return minutes === 0 ? 'Przed chwilą' : `${minutes} min temu`
    }
    return `${hours} godz. temu`
  } else if (days === 1) {
    return 'Wczoraj'
  } else if (days < 7) {
    return `${days} dni temu`
  } else {
    return date.toLocaleDateString('pl-PL', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files) {
    selectedFiles.value = [...selectedFiles.value, ...Array.from(target.files)]
  }
}

const removeFile = (index: number) => {
  selectedFiles.value = selectedFiles.value.filter((_, i) => i !== index)
}

const openFilePicker = () => {
  fileInput.value?.click()
}

const handleSubmit = async () => {
  if (!canSubmit.value) return

  isSubmitting.value = true
  try {
    emit('addComment', {
      comment: newComment.value.trim(),
      attachments: selectedFiles.value
    })
    newComment.value = ''
    selectedFiles.value = []
    if (fileInput.value) {
      fileInput.value.value = ''
    }
  } finally {
    isSubmitting.value = false
  }
}

const getFileExtension = (url: string): string => {
  const parts = url.split('.')
  return parts.length > 1 ? parts[parts.length - 1].toLowerCase() : ''
}

const getFileName = (url: string): string => {
  const parts = url.split('/')
  return parts[parts.length - 1]
}

const canPreview = (url: string): boolean => {
  const ext = getFileExtension(url)
  return ['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp', 'pdf'].includes(ext)
}

const openAttachmentsViewer = (attachments: string[], index: number) => {
  viewerAttachments.value = attachments
  viewerInitialIndex.value = index
  showAttachmentsViewer.value = true
}

const downloadFile = (url: string) => {
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
      <MessageCircle class="w-5 h-5 text-gray-500 dark:text-gray-400" />
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
        Komentarze ({{ comments.length }})
      </h3>
    </div>

    <!-- Comments List -->
    <div v-if="comments.length > 0" class="space-y-4">
      <div
        v-for="comment in sortedComments"
        :key="comment.id"
        class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4"
      >
        <!-- Comment Header -->
        <div class="flex items-center justify-between mb-2">
          <div class="flex items-center gap-2">
            <div class="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white font-semibold">
              {{ comment.userName.split(' ').map(n => n[0]).join('').toUpperCase() }}
            </div>
            <div>
              <p class="font-medium text-gray-900 dark:text-white">
                {{ comment.userName }}
              </p>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                {{ formatDate(comment.createdAt) }}
              </p>
            </div>
          </div>
        </div>

        <!-- Comment Content -->
        <p v-if="comment.comment" class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
          {{ comment.comment }}
        </p>
        <p v-else class="text-gray-500 dark:text-gray-400 italic text-sm">
          [Komentarz bez tekstu - tylko załączniki]
        </p>

        <!-- Attachments -->
        <div v-if="comment.attachments && comment.attachments.length > 0" class="mt-3">
          <p class="text-sm text-gray-500 dark:text-gray-400 mb-2 flex items-center gap-2">
            <Paperclip class="w-4 h-4" />
            Załączniki ({{ comment.attachments.length }}):
          </p>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
            <div
              v-for="(attachment, idx) in comment.attachments"
              :key="idx"
              class="flex items-center gap-2 p-2 bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-600 group"
            >
              <div class="flex-1 min-w-0">
                <p class="text-sm text-gray-700 dark:text-gray-300 truncate">
                  {{ getFileName(attachment) }}
                </p>
                <p class="text-xs text-gray-500 dark:text-gray-400 uppercase">
                  {{ getFileExtension(attachment) }}
                </p>
              </div>
              <div class="flex items-center gap-1 flex-shrink-0">
                <button
                  v-if="canPreview(attachment)"
                  class="p-1.5 hover:bg-gray-100 dark:hover:bg-gray-700 rounded transition-colors text-gray-500 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-400"
                  title="Podgląd"
                  @click="openAttachmentsViewer(comment.attachments, idx)"
                >
                  <Eye class="w-4 h-4" />
                </button>
                <button
                  class="p-1.5 hover:bg-gray-100 dark:hover:bg-gray-700 rounded transition-colors text-gray-500 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-400"
                  title="Pobierz"
                  @click="downloadFile(attachment)"
                >
                  <Download class="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- No Comments -->
    <div v-else class="text-center py-8 text-gray-500 dark:text-gray-400">
      <MessageCircle class="w-12 h-12 mx-auto mb-2 opacity-50" />
      <p>Brak komentarzy</p>
    </div>

    <!-- Add Comment Form -->
    <div v-if="canAddComment" class="border-t border-gray-200 dark:border-gray-700 pt-4">
      <form class="space-y-3" @submit.prevent="handleSubmit">
        <textarea
          v-model="newComment"
          rows="3"
          placeholder="Dodaj komentarz (opcjonalnie, jeśli dodajesz załącznik)..."
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          :disabled="isSubmitting"
        />

        <!-- Selected Files Preview -->
        <div v-if="selectedFiles.length > 0" class="space-y-2">
          <p class="text-sm font-medium text-gray-700 dark:text-gray-300">
            Wybrane pliki ({{ selectedFiles.length }}):
          </p>
          <div class="space-y-2">
            <div
              v-for="(file, index) in selectedFiles"
              :key="index"
              class="flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-700 rounded-lg border border-gray-200 dark:border-gray-600"
            >
              <div class="flex items-center gap-2 flex-1 min-w-0">
                <Paperclip class="w-4 h-4 text-gray-500 dark:text-gray-400 flex-shrink-0" />
                <span class="text-sm text-gray-700 dark:text-gray-300 truncate">
                  {{ file.name }}
                </span>
                <span class="text-xs text-gray-500 dark:text-gray-400 flex-shrink-0">
                  ({{ (file.size / 1024).toFixed(1) }} KB)
                </span>
              </div>
              <button
                type="button"
                class="p-1 hover:bg-gray-200 dark:hover:bg-gray-600 rounded transition-colors text-gray-500 hover:text-red-600 dark:text-gray-400 dark:hover:text-red-400 flex-shrink-0"
                title="Usuń plik"
                @click="removeFile(index)"
              >
                <X class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        <!-- Hidden File Input -->
        <input
          ref="fileInput"
          type="file"
          multiple
          class="hidden"
          accept="image/*,.pdf,.doc,.docx,.xls,.xlsx,.txt"
          @change="handleFileSelect"
        />

        <!-- Actions -->
        <div class="flex items-center justify-between">
          <button
            type="button"
            class="inline-flex items-center gap-2 px-4 py-2 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg transition"
            :disabled="isSubmitting"
            @click="openFilePicker"
          >
            <Paperclip class="w-4 h-4" />
            Dodaj załącznik
          </button>

          <button
            type="submit"
            :disabled="!canSubmit"
            class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg transition"
          >
            <Send class="w-4 h-4" />
            {{ isSubmitting ? 'Wysyłanie...' : 'Wyślij komentarz' }}
          </button>
        </div>
      </form>
    </div>

    <!-- Document Viewer Modal -->
    <DocumentViewer
      v-if="showAttachmentsViewer"
      :attachments="viewerAttachments"
      :initial-index="viewerInitialIndex"
      @close="showAttachmentsViewer = false"
    />
  </div>
</template>
