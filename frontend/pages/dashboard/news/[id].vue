<script setup lang="ts">
import type { News } from '~/types'
import { useAuthStore } from '~/stores/auth'

definePageMeta({
  layout: 'default',
  middleware: 'auth'
})

const config = useRuntimeConfig()
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { fetchNewsById, deleteNews } = useNewsApi()
const googleMapsApiKey = config.public.googleMapsApiKey || ''

const newsId = computed(() => Number.parseInt(route.params.id as string, 10))
const news = ref<News | null>(null)
const isLoading = ref(false)
const isDeleting = ref(false)
const error = ref<string | null>(null)
const showDeleteModal = ref(false)

const resolveErrorMessage = (input: unknown, fallback: string) => {
  if (input && typeof input === 'object' && 'message' in input) {
    const message = (input as { message?: unknown }).message
    if (typeof message === 'string' && message.trim().length > 0) {
      return message
    }
  }

  return fallback
}

const canManageNews = computed(() => {
  const role = authStore.user?.role
  if (!role) {
    return false
  }

  const normalized = role.toString().toLowerCase()
  return normalized === 'admin' || normalized === 'marketing'
})

async function loadNews() {
  isLoading.value = true
  error.value = null

  try {
    news.value = await fetchNewsById(newsId.value)
  } catch (err: unknown) {
    error.value = resolveErrorMessage(err, 'Nie udało się załadować aktualności.')
    console.error('fetchNewsById error:', err)
  } finally {
    isLoading.value = false
  }
}

async function handleDelete() {
  if (!news.value || !canManageNews.value) {
    showDeleteModal.value = false
    return
  }

  isDeleting.value = true

  try {
    await deleteNews(news.value.id)
    router.push('/dashboard/news')
  } catch (err: unknown) {
    error.value = resolveErrorMessage(err, 'Nie udało się usunąć aktualności.')
    console.error('deleteNews error:', err)
  } finally {
    isDeleting.value = false
    showDeleteModal.value = false
  }
}

function handleEdit() {
  if (!canManageNews.value) {
    return
  }
  router.push(`/dashboard/news/edit/${newsId.value}`)
}

function handleBack() {
  router.push('/dashboard/news')
}

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
    announcement: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    product: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    hr: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    tech: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    event: 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200'
  }

  if (category in colors) {
    return colors[category]
  }

  const capitalized = category.charAt(0).toUpperCase() + category.slice(1).toLowerCase()
  if (capitalized in colors) {
    return colors[capitalized]
  }

  return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200'
}

const getCategoryLabel = (category: string) => {
  const labels: Record<string, string> = {
    announcement: 'Announcement',
    product: 'Product',
    hr: 'HR',
    tech: 'Tech',
    event: 'Event'
  }

  const key = category.toLowerCase()
  return labels[key] ?? category
}

const getAuthorName = (currentNews: News) => {
  if (currentNews.authorName && currentNews.authorName.trim().length > 0) {
    return currentNews.authorName
  }

  if (currentNews.author) {
    return `${currentNews.author.firstName} ${currentNews.author.lastName}`
  }

  return 'Unknown author'
}

onMounted(() => {
  loadNews()
})
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6">
    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto" />
      <p class="mt-4 text-gray-600 dark:text-gray-400">
        Ładowanie szczegółów aktualności...
      </p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-100 dark:bg-red-900 rounded-lg shadow-md p-6">
      <p class="text-red-800 dark:text-red-200">{{ error }}</p>
      <div class="mt-4 flex gap-2">
        <button class="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700" @click="loadNews">
          Spróbuj ponownie
        </button>
        <button class="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700" @click="handleBack">
          Powrót do listy
        </button>
      </div>
    </div>

    <!-- News Content -->
    <template v-else-if="news">
      <!-- Header with Actions -->
      <div class="flex items-center justify-between">
        <button
          class="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition"
          @click="handleBack"
        >
          Powrót do listy
        </button>
        <div v-if="canManageNews" class="flex gap-2">
          <button
            class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition font-medium"
            @click="handleEdit"
          >
            Edytuj
          </button>
          <button
            class="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition font-medium"
            @click="showDeleteModal = true"
          >
            Usuń
          </button>
        </div>
      </div>

      <!-- Main Article -->
      <article class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden">
        <div v-if="news.imageUrl" class="h-96 overflow-hidden">
          <img
            :src="news.imageUrl"
            :alt="news.title"
            class="w-full h-full object-cover"
          >
        </div>

        <div class="p-8 space-y-6">
          <div class="flex flex-wrap items-center gap-2">
            <span
              :class="getCategoryColor(news.category)"
              class="px-3 py-1 text-xs font-medium rounded-full"
            >
              {{ getCategoryLabel(news.category) }}
            </span>
            <span
              v-if="news.isEvent"
              class="px-3 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200"
            >
              📅 Wydarzenie
            </span>
            <span
              v-if="news.departmentId"
              class="px-3 py-1 text-xs font-medium rounded-full bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200"
            >
              🏢 Dział {{ news.departmentId }}
            </span>
          </div>

          <h1 class="text-4xl font-bold text-gray-900 dark:text-white">
            {{ news.title }}
          </h1>

          <div class="flex flex-wrap items-center gap-4 text-sm text-gray-500 dark:text-gray-400 pb-6 border-b border-gray-200 dark:border-gray-700">
            <span class="flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
              {{ getAuthorName(news) }}
            </span>
            <span class="flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              {{ formatDate(news.createdAt) }}
            </span>
            <span v-if="news.updatedAt" class="flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              Updated: {{ formatDate(news.updatedAt) }}
            </span>
            <span class="flex items-center gap-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
              </svg>
              {{ news.views }} views
            </span>
          </div>

          <div v-if="news.excerpt" class="text-xl text-gray-700 dark:text-gray-300 leading-relaxed italic">
            {{ news.excerpt }}
          </div>

          <div
            class="prose dark:prose-invert max-w-none text-gray-900 dark:text-gray-100"
            v-html="news.content"
          />

          <!-- Event Details Section -->
          <div v-if="news.isEvent" class="pt-6 border-t border-gray-200 dark:border-gray-700">
            <div class="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-6 space-y-4">
              <div class="flex items-center gap-2 mb-4">
                <svg class="w-6 h-6 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <h2 class="text-2xl font-bold text-blue-700 dark:text-blue-400">
                  Szczegóły wydarzenia
                </h2>
              </div>

              <div v-if="news.eventHashtag" class="flex items-start gap-3">
                <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 20l4-16m2 16l4-16M6 9h14M4 15h14" />
                </svg>
                <div>
                  <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Hashtag</p>
                  <p class="text-lg text-blue-600 dark:text-blue-400 font-semibold">{{ news.eventHashtag }}</p>
                </div>
              </div>

              <div v-if="news.eventDateTime" class="flex items-start gap-3">
                <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <div>
                  <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Data i godzina</p>
                  <p class="text-lg text-gray-900 dark:text-gray-100">{{ formatDate(news.eventDateTime) }}</p>
                </div>
              </div>

              <div v-if="news.eventLocation" class="flex items-start gap-3">
                <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <div class="flex-1">
                  <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Lokalizacja</p>
                  <p class="text-lg text-gray-900 dark:text-gray-100">{{ news.eventLocation }}</p>

                  <!-- Google Maps Embed -->
                  <div v-if="googleMapsApiKey" class="mt-4 rounded-lg overflow-hidden border border-gray-300 dark:border-gray-600">
                    <iframe
                      :src="`https://www.google.com/maps/embed/v1/place?key=${googleMapsApiKey}&q=${encodeURIComponent(news.eventLocation)}`"
                      width="100%"
                      height="300"
                      style="border:0;"
                      allowfullscreen
                      loading="lazy"
                      referrerpolicy="no-referrer-when-downgrade"
                      class="w-full"
                    />
                  </div>
                  <div v-else class="mt-4 p-4 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg">
                    <p class="text-sm text-yellow-800 dark:text-yellow-200">
                      <span class="font-semibold">Uwaga:</span> Aby wyświetlić mapę, skonfiguruj Google Maps API key w zmiennych środowiskowych.
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-if="news.eventId" class="pt-6 border-t border-gray-200 dark:border-gray-700">
            <button
              class="w-full flex items-center justify-center gap-2 px-6 py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg transition-colors"
              @click="router.push(`/dashboard/calendar?eventId=${news.eventId}`)"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              Zobacz powiązane wydarzenie
            </button>
          </div>
        </div>
      </article>
    </template>

    <!-- Delete Confirmation Modal -->
    <Teleport to="body">
      <div
        v-if="showDeleteModal"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
        @click.self="showDeleteModal = false"
      >
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full p-6">
          <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-4">
            Potwierdź usunięcie
          </h3>
          <p class="text-gray-600 dark:text-gray-400 mb-6">
            Czy na pewno chcesz usunąć tę aktualność? Ta operacja nie może być cofnięta.
          </p>
          <div class="flex gap-4 justify-end">
            <button
              class="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition font-medium"
              :disabled="isDeleting"
              @click="showDeleteModal = false"
            >
              Anuluj
            </button>
            <button
              class="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 disabled:bg-gray-400 disabled:cursor-not-allowed transition font-medium"
              :disabled="isDeleting"
              @click="handleDelete"
            >
              {{ isDeleting ? 'Usuwanie...' : 'Usuń' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>


