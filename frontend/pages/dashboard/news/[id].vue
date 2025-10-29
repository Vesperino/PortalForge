<script setup lang="ts">
import type { News } from '~/types'

definePageMeta({
  layout: 'default',
  middleware: 'auth'
})

const route = useRoute()
const router = useRouter()
const { fetchNewsById, deleteNews } = useNewsApi()
const { data: authData } = useAuth()

const newsId = parseInt(route.params.id as string)
const news = ref<News | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)
const showDeleteConfirm = ref(false)
const isDeleting = ref(false)

// Check if user can edit/delete (Admin, HR, Marketing)
const canEdit = computed(() => {
  const userRole = authData.value?.user?.user_metadata?.role
  return ['Admin', 'Hr', 'Marketing'].includes(userRole)
})

async function loadNews() {
  isLoading.value = true
  error.value = null

  try {
    news.value = await fetchNewsById(newsId)
  } catch (err) {
    error.value = 'Failed to load news article'
    console.error(err)
  } finally {
    isLoading.value = false
  }
}

async function handleDelete() {
  if (!news.value) return

  isDeleting.value = true
  try {
    await deleteNews(news.value.id)
    await router.push('/dashboard/news')
  } catch (err) {
    error.value = 'Nie udało się usunąć aktualności'
    console.error(err)
  } finally {
    isDeleting.value = false
    showDeleteConfirm.value = false
  }
}

function goToEdit() {
  router.push(`/dashboard/news/edit/${newsId}`)
}

onMounted(() => {
  loadNews()
})

const formatDate = (dateString: string | Date) => {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(date)
}

const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    'announcement': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    'Announcement': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    'product': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'Product': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'hr': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    'Hr': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    'tech': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    'Tech': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    'event': 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
    'Event': 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200'
  }
  return colors[category] || 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
}

const getCategoryLabel = (category: string) => {
  const labels: Record<string, string> = {
    'announcement': 'Ogłoszenie',
    'Announcement': 'Ogłoszenie',
    'product': 'Produkt',
    'Product': 'Produkt',
    'hr': 'HR',
    'Hr': 'HR',
    'tech': 'Tech',
    'Tech': 'Tech',
    'event': 'Event',
    'Event': 'Event'
  }
  return labels[category] || category
}

const goBack = () => {
  router.push('/dashboard/news')
}

function getAuthorInitials(authorName: string) {
  const parts = authorName.split(' ')
  if (parts.length >= 2) {
    return parts[0][0] + parts[parts.length - 1][0]
  }
  return authorName.substring(0, 2).toUpperCase()
}
</script>

<template>
  <div class="space-y-6">
    <!-- Back Button -->
    <div>
      <button
        type="button"
        class="flex items-center gap-2 text-blue-600 dark:text-blue-400 hover:underline focus:outline-none"
        @click="goBack"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
        </svg>
        Powrót do aktualności
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"></div>
      <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie aktualności...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-100 dark:bg-red-900 rounded-lg shadow-md p-6">
      <p class="text-red-800 dark:text-red-200">{{ error }}</p>
      <button @click="loadNews" class="mt-4 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700">
        Spróbuj ponownie
      </button>
    </div>

    <!-- News Article -->
    <article v-else-if="news" class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
      <!-- Featured Image -->
      <div v-if="news.imageUrl" class="h-96 overflow-hidden">
        <img
          :src="news.imageUrl"
          :alt="news.title"
          class="w-full h-full object-cover"
        >
      </div>

      <!-- Article Content -->
      <div class="p-8">
        <!-- Meta Information -->
        <div class="flex items-center gap-3 mb-6">
          <span
            :class="getCategoryColor(news.category)"
            class="px-3 py-1 text-sm font-medium rounded-full"
          >
            {{ getCategoryLabel(news.category) }}
          </span>
          <span v-if="news.eventId" class="px-3 py-1 text-sm font-medium rounded-full bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200">
            Wydarzenie
          </span>
        </div>

        <!-- Title -->
        <h1 class="text-4xl font-bold text-gray-900 dark:text-white mb-4">
          {{ news.title }}
        </h1>

        <!-- Author and Date -->
        <div class="flex items-center gap-6 mb-8 pb-8 border-t border-gray-200 dark:border-gray-700">
          <div class="flex items-center gap-3">
            <div class="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-semibold">
              {{ getAuthorInitials(news.authorName) }}
            </div>
            <div>
              <p class="font-medium text-gray-900 dark:text-white">
                {{ news.authorName }}
              </p>
            </div>
          </div>
          <div class="flex items-center gap-4 text-gray-600 dark:text-gray-400">
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span class="text-sm">{{ formatDate(news.createdAt) }}</span>
            </div>
            <div class="flex items-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
              <span class="text-sm">{{ news.views }} wyświetleń</span>
            </div>
          </div>
        </div>

        <!-- Article Content -->
        <div class="prose prose-lg dark:prose-invert max-w-none">
          <p class="text-xl text-gray-700 dark:text-gray-300 leading-relaxed mb-6">
            {{ news.excerpt }}
          </p>
          <div class="text-gray-700 dark:text-gray-300 leading-relaxed" v-html="news.content"></div>
        </div>
      </div>
    </article>

    <!-- Share & Actions -->
    <div v-if="news" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            Udostępnij
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Podziel się tą aktualnością z zespołem
          </p>
        </div>
        <div class="flex gap-2">
          <button
            type="button"
            class="px-4 py-2 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
            @click="() => navigator.clipboard.writeText(window.location.href)"
          >
            Kopiuj link
          </button>
        </div>
      </div>
    </div>

    <!-- Admin Actions -->
    <div v-if="news && canEdit" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            Zarządzanie
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Edytuj lub usuń tę aktualność
          </p>
        </div>
        <div class="flex gap-3">
          <button
            type="button"
            class="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            @click="goToEdit"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
            Edytuj
          </button>
          <button
            type="button"
            class="flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
            @click="showDeleteConfirm = true"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
            Usuń
          </button>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div
      v-if="showDeleteConfirm"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="showDeleteConfirm = false"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full p-6">
        <div class="flex items-start gap-4">
          <div class="flex-shrink-0 w-12 h-12 rounded-full bg-red-100 dark:bg-red-900 flex items-center justify-center">
            <svg class="w-6 h-6 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          <div class="flex-1">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              Usuń aktualność
            </h3>
            <p class="text-gray-600 dark:text-gray-400 mb-6">
              Czy na pewno chcesz usunąć tę aktualność? Ta akcja jest nieodwracalna.
            </p>
            <div class="flex gap-3 justify-end">
              <button
                type="button"
                class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-200 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
                :disabled="isDeleting"
                @click="showDeleteConfirm = false"
              >
                Anuluj
              </button>
              <button
                type="button"
                class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                :disabled="isDeleting"
                @click="handleDelete"
              >
                <span v-if="isDeleting">Usuwanie...</span>
                <span v-else>Usuń</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
