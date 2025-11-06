<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { MessageCircle, Send, Paperclip, X, FileText, Image as ImageIcon, Eye } from 'lucide-vue-next'
import FilePreviewModal from '~/components/common/FilePreviewModal.vue'

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

interface AttachmentFile {
  file: File
  preview?: string
  id: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  addComment: [comment: string, attachments: File[]]
}>()

const newComment = ref('')
const isSubmitting = ref(false)
const attachments = ref<AttachmentFile[]>([])
const fileInputRef = ref<HTMLInputElement | null>(null)
const textareaRef = ref<HTMLTextAreaElement | null>(null)
const showPreviewModal = ref(false)
const previewFile = ref<{ url: string, name: string } | null>(null)

const ALLOWED_TYPES = ['application/pdf', 'image/png', 'image/jpeg', 'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document']
const ALLOWED_EXTENSIONS = ['.pdf', '.png', '.jpg', '.jpeg', '.doc', '.docx']
const MAX_FILE_SIZE = 20 * 1024 * 1024 // 20MB

const sortedComments = computed(() => {
  return [...props.comments].sort((a, b) =>
    new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
  )
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

const handleFileSelect = async (event: Event) => {
  const input = event.target as HTMLInputElement
  if (!input.files) return

  const files = Array.from(input.files)
  await addFiles(files)

  // Reset input
  input.value = ''
}

const addFiles = async (files: File[]) => {
  for (const file of files) {
    // Validate file type
    const extension = '.' + file.name.split('.').pop()?.toLowerCase()
    if (!ALLOWED_EXTENSIONS.includes(extension) && !ALLOWED_TYPES.includes(file.type)) {
      alert(`Nieprawidłowy typ pliku: ${file.name}. Dozwolone typy: PDF, PNG, JPG, JPEG, DOC, DOCX`)
      continue
    }

    // Validate file size
    if (file.size > MAX_FILE_SIZE) {
      alert(`Plik ${file.name} jest za duży. Maksymalny rozmiar: 20MB`)
      continue
    }

    // Create preview for images
    let preview: string | undefined
    if (file.type.startsWith('image/')) {
      preview = URL.createObjectURL(file)
    }

    attachments.value.push({
      file,
      preview,
      id: Math.random().toString(36).substr(2, 9)
    })
  }
}

const removeAttachment = (id: string) => {
  const attachment = attachments.value.find(a => a.id === id)
  if (attachment?.preview) {
    URL.revokeObjectURL(attachment.preview)
  }
  attachments.value = attachments.value.filter(a => a.id !== id)
}

const handlePaste = async (event: ClipboardEvent) => {
  const items = event.clipboardData?.items
  if (!items) return

  const files: File[] = []
  for (const item of Array.from(items)) {
    if (item.type.startsWith('image/')) {
      const file = item.getAsFile()
      if (file) {
        // Create a better filename for pasted images
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-')
        const extension = file.type.split('/')[1]
        const renamedFile = new File([file], `screenshot-${timestamp}.${extension}`, { type: file.type })
        files.push(renamedFile)
      }
    }
  }

  if (files.length > 0) {
    await addFiles(files)
  }
}

const getFileIcon = (file: File) => {
  if (file.type.startsWith('image/')) return ImageIcon
  return FileText
}

const getAttachmentIcon = (url: string) => {
  const extension = url.split('.').pop()?.toLowerCase()
  if (['png', 'jpg', 'jpeg', 'gif', 'webp'].includes(extension || '')) {
    return ImageIcon
  }
  return FileText
}

const getAttachmentFileName = (url: string) => {
  // Extract filename from URL
  const parts = url.split('/')
  const filename = parts[parts.length - 1]
  // Remove the GUID prefix if present (format: guid-filename.ext)
  const match = filename.match(/^[a-f0-9]{32}-(.+)$/)
  return match ? match[1] : filename
}

const formatFileSize = (bytes: number) => {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

const handleSubmit = async () => {
  if ((!newComment.value.trim() && attachments.value.length === 0) || isSubmitting.value) return

  isSubmitting.value = true
  try {
    const files = attachments.value.map(a => a.file)
    emit('addComment', newComment.value.trim(), files)
    newComment.value = ''

    // Clear attachments and their previews
    attachments.value.forEach(a => {
      if (a.preview) URL.revokeObjectURL(a.preview)
    })
    attachments.value = []
  } finally {
    isSubmitting.value = false
  }
}

const openPreview = (url: string, event?: Event) => {
  if (event) {
    event.preventDefault()
    event.stopPropagation()
  }
  previewFile.value = {
    url,
    name: getAttachmentFileName(url)
  }
  showPreviewModal.value = true
}

// Setup clipboard paste listener
onMounted(() => {
  if (textareaRef.value) {
    textareaRef.value.addEventListener('paste', handlePaste)
  }
})

onUnmounted(() => {
  // Clean up object URLs
  attachments.value.forEach(a => {
    if (a.preview) URL.revokeObjectURL(a.preview)
  })

  if (textareaRef.value) {
    textareaRef.value.removeEventListener('paste', handlePaste)
  }
})
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
        <p class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
          {{ comment.comment }}
        </p>

        <!-- Attachments -->
        <div v-if="comment.attachments && comment.attachments.length > 0" class="mt-3">
          <p class="text-sm text-gray-500 dark:text-gray-400 mb-2">
            Załączniki ({{ comment.attachments.length }}):
          </p>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
            <div
              v-for="(attachment, idx) in comment.attachments"
              :key="idx"
              class="flex items-center gap-2 p-2 bg-white dark:bg-gray-600 rounded border border-gray-200 dark:border-gray-500 hover:bg-gray-50 dark:hover:bg-gray-500 transition cursor-pointer group"
              @click="openPreview(attachment)"
            >
              <component
                :is="getAttachmentIcon(attachment)"
                class="w-5 h-5 text-gray-600 dark:text-gray-300 flex-shrink-0"
              />
              <span class="text-sm text-gray-900 dark:text-white truncate flex-1">
                {{ getAttachmentFileName(attachment) }}
              </span>
              <button
                title="Podgląd"
                class="opacity-0 group-hover:opacity-100 p-1 text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition"
                @click="openPreview(attachment, $event)"
              >
                <Eye class="w-4 h-4" />
              </button>
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
        <div class="relative">
          <textarea
            ref="textareaRef"
            v-model="newComment"
            rows="3"
            placeholder="Dodaj komentarz... (Ctrl+V aby wkleić zrzut ekranu)"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
            :disabled="isSubmitting"
          />
        </div>

        <!-- Attachments Preview -->
        <div v-if="attachments.length > 0" class="space-y-2">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Załączniki ({{ attachments.length }}):
          </p>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
            <div
              v-for="attachment in attachments"
              :key="attachment.id"
              class="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-700 rounded-lg border border-gray-200 dark:border-gray-600"
            >
              <!-- Preview for images -->
              <div v-if="attachment.preview" class="w-12 h-12 rounded overflow-hidden flex-shrink-0">
                <img :src="attachment.preview" :alt="attachment.file.name" class="w-full h-full object-cover">
              </div>
              <!-- Icon for other files -->
              <div v-else class="w-12 h-12 flex items-center justify-center bg-gray-200 dark:bg-gray-600 rounded flex-shrink-0">
                <component :is="getFileIcon(attachment.file)" class="w-6 h-6 text-gray-600 dark:text-gray-300" />
              </div>

              <!-- File info -->
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
                  {{ attachment.file.name }}
                </p>
                <p class="text-xs text-gray-500 dark:text-gray-400">
                  {{ formatFileSize(attachment.file.size) }}
                </p>
              </div>

              <!-- Remove button -->
              <button
                type="button"
                :disabled="isSubmitting"
                class="flex-shrink-0 p-1 text-gray-400 hover:text-red-600 dark:hover:text-red-400 transition"
                @click="removeAttachment(attachment.id)"
              >
                <X class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex items-center justify-between gap-2">
          <!-- File upload button -->
          <div>
            <input
              ref="fileInputRef"
              type="file"
              multiple
              :accept="ALLOWED_EXTENSIONS.join(',')"
              class="hidden"
              @change="handleFileSelect"
            >
            <button
              type="button"
              :disabled="isSubmitting"
              class="inline-flex items-center gap-2 px-3 py-2 text-sm text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed transition"
              @click="fileInputRef?.click()"
            >
              <Paperclip class="w-4 h-4" />
              Dodaj plik
            </button>
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              PDF, PNG, JPG, DOC, DOCX (maks. 20MB)
            </p>
          </div>

          <!-- Submit button -->
          <button
            type="submit"
            :disabled="(!newComment.trim() && attachments.length === 0) || isSubmitting"
            class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg transition"
          >
            <Send class="w-4 h-4" />
            {{ isSubmitting ? 'Wysyłanie...' : 'Wyślij' }}
          </button>
        </div>
      </form>
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
