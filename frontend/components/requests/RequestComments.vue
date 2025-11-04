<script setup lang="ts">
import { ref, computed } from 'vue'
import { MessageCircle, Send } from 'lucide-vue-next'

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
  addComment: [comment: string]
}>()

const newComment = ref('')
const isSubmitting = ref(false)

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

const handleSubmit = async () => {
  if (!newComment.value.trim() || isSubmitting.value) return

  isSubmitting.value = true
  try {
    emit('addComment', newComment.value.trim())
    newComment.value = ''
  } finally {
    isSubmitting.value = false
  }
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
        <p class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
          {{ comment.comment }}
        </p>

        <!-- Attachments -->
        <div v-if="comment.attachments && comment.attachments.length > 0" class="mt-3">
          <p class="text-sm text-gray-500 dark:text-gray-400 mb-2">
            Załączniki ({{ comment.attachments.length }}):
          </p>
          <div class="flex flex-wrap gap-2">
            <a
              v-for="(attachment, idx) in comment.attachments"
              :key="idx"
              :href="attachment"
              target="_blank"
              class="text-sm text-blue-600 dark:text-blue-400 hover:underline"
            >
              Załącznik {{ idx + 1 }}
            </a>
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
      <form @submit.prevent="handleSubmit" class="space-y-3">
        <textarea
          v-model="newComment"
          rows="3"
          placeholder="Dodaj komentarz..."
          class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
          :disabled="isSubmitting"
        />
        <div class="flex justify-end">
          <button
            type="submit"
            :disabled="!newComment.trim() || isSubmitting"
            class="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg transition"
          >
            <Send class="w-4 h-4" />
            {{ isSubmitting ? 'Wysyłanie...' : 'Wyślij komentarz' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
