<script setup lang="ts">
import type { News } from '~/types'

definePageMeta({
  layout: 'default',
  middleware: 'auth'
})

const route = useRoute()
const router = useRouter()
const { fetchNewsById } = useNewsApi()

const newsId = parseInt(route.params.id as string)
const news = ref<News | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)

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
  </div>
</template>
