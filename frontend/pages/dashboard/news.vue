<script setup lang="ts">
import type { News, NewsCategory } from '~/types'

definePageMeta({
  layout: 'default',
  // middleware: 'auth' // Disabled for testing
})

const { fetchAllNews } = useNewsApi()
// const router = useRouter() // removed unused

const selectedCategory = ref<string>('all')
const searchQuery = ref<string>('')
const currentPage = ref(1)
const itemsPerPage = 3
const allNews = ref<News[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

const categories = [
  { value: 'all', label: 'Wszystkie' },
  { value: 'announcement', label: 'Ogłoszenia' },
  { value: 'hr', label: 'HR' },
  { value: 'product', label: 'Produkt' },
  { value: 'tech', label: 'Technologia' },
  { value: 'event', label: 'Wydarzenia' }
]

async function loadNews() {
  isLoading.value = true
  error.value = null

  try {
    const category = selectedCategory.value !== 'all' ? selectedCategory.value as NewsCategory : undefined
    const newsData = await fetchAllNews(category)
    allNews.value = newsData
  } catch (err) {
    error.value = 'Failed to load news'
    console.error(err)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  loadNews()
})

watch(selectedCategory, () => {
  loadNews()
})

const filteredNews = computed(() => {
  if (!searchQuery.value) {
    return allNews.value
  }

  const query = searchQuery.value.toLowerCase()
  return allNews.value.filter(news =>
    news.title.toLowerCase().includes(query) ||
    news.excerpt?.toLowerCase().includes(query) ||
    news.content.toLowerCase().includes(query)
  )
})

const totalPages = computed(() => Math.ceil(filteredNews.value.length / itemsPerPage))

const paginatedNews = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filteredNews.value.slice(start, end)
})

const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

watch([searchQuery], () => {
  currentPage.value = 1
})

const formatDate = (dateString: string | Date) => {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  return new Intl.DateTimeFormat('pl-PL', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
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

const thisMonthCount = computed(() => {
  const now = new Date()
  return allNews.value.filter(n => {
    const createdDate = new Date(n.createdAt)
    return createdDate.getMonth() === now.getMonth() && createdDate.getFullYear() === now.getFullYear()
  }).length
})

const getAuthorName = (news: News) => {
  if (news.author) {
    return `${news.author.firstName} ${news.author.lastName}`
  }
  return 'Unknown'
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        Aktualności
      </h1>
      <NuxtLink
        to="/dashboard/news/create"
        class="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors text-center"
      >
        Dodaj aktualność
      </NuxtLink>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"/>
      <p class="mt-4 text-gray-600 dark:text-gray-400">Ładowanie aktualności...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-100 dark:bg-red-900 rounded-lg shadow-md p-6">
      <p class="text-red-800 dark:text-red-200">{{ error }}</p>
      <button class="mt-4 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700" @click="loadNews">
        Spróbuj ponownie
      </button>
    </div>

    <template v-else>
      <!-- Filters -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- Search -->
          <div>
            <label for="search" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Szukaj
            </label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
              <input
                id="search"
                v-model="searchQuery"
                type="text"
                placeholder="Szukaj aktualności..."
                class="pl-10 w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              >
            </div>
          </div>

          <!-- Category Filter -->
          <div>
            <label for="category" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Kategoria
            </label>
            <select
              id="category"
              v-model="selectedCategory"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            >
              <option v-for="cat in categories" :key="cat.value" :value="cat.value">
                {{ cat.label }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <!-- News Stats -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
          <p class="text-sm text-gray-600 dark:text-gray-400">Wszystkie aktualności</p>
          <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ allNews.length }}</p>
        </div>
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
          <p class="text-sm text-gray-600 dark:text-gray-400">Wyświetlane</p>
          <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ filteredNews.length }}</p>
        </div>
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
          <p class="text-sm text-gray-600 dark:text-gray-400">Ten miesiąc</p>
          <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ thisMonthCount }}</p>
        </div>
      </div>

      <!-- News List -->
      <div class="space-y-4">
        <!-- Empty state -->
        <div v-if="paginatedNews.length === 0" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
          <svg class="w-16 h-16 mx-auto text-gray-400 dark:text-gray-600 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z" />
          </svg>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            Brak aktualności
          </h3>
          <p class="text-gray-600 dark:text-gray-400">
            Nie znaleziono aktualności spełniających wybrane kryteria.
          </p>
        </div>

        <article
          v-for="news in paginatedNews"
          :key="news.id"
          class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow cursor-pointer"
          @click="navigateTo(`/dashboard/news/${news.id}`)"
        >
          <!-- News Image -->
          <div v-if="news.imageUrl" class="h-56 overflow-hidden">
            <img
              :src="news.imageUrl"
              :alt="news.title"
              class="w-full h-full object-cover"
            >
          </div>

          <div class="p-6">
            <!-- News Header -->
            <div class="flex items-start justify-between mb-4">
              <div class="flex-1">
                <div class="flex items-center gap-2 mb-2">
                  <span
                    :class="getCategoryColor(news.category)"
                    class="px-3 py-1 text-xs font-medium rounded-full"
                  >
                    {{ getCategoryLabel(news.category) }}
                  </span>
                  <span v-if="news.eventId" class="px-3 py-1 text-xs font-medium rounded-full bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200">
                    Wydarzenie
                  </span>
                </div>
                <h2 class="text-2xl font-semibold text-gray-900 dark:text-white mb-2">
                  {{ news.title }}
                </h2>
                <div class="flex items-center gap-4 text-sm text-gray-500 dark:text-gray-400">
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
                  <span class="flex items-center gap-1">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                    {{ news.views }}
                  </span>
                </div>
              </div>
            </div>

            <!-- News Excerpt -->
            <p class="text-gray-700 dark:text-gray-300 leading-relaxed mb-4">
              {{ news.excerpt }}
            </p>

            <!-- News Actions -->
            <div class="flex items-center justify-between pt-4 border-t border-gray-200 dark:border-gray-700">
              <NuxtLink
                :to="`/dashboard/news/${news.id}`"
                class="text-sm text-blue-600 dark:text-blue-400 hover:underline focus:outline-none focus:ring-2 focus:ring-blue-500 rounded px-2 py-1"
              >
                Czytaj więcej →
              </NuxtLink>
            </div>
          </div>
        </article>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
        <div class="flex items-center justify-between">
          <!-- Page info -->
          <div class="text-sm text-gray-700 dark:text-gray-300">
            Strona <span class="font-medium">{{ currentPage }}</span> z <span class="font-medium">{{ totalPages }}</span>
            <span class="text-gray-500 dark:text-gray-400 ml-2">
              ({{ paginatedNews.length }} z {{ filteredNews.length }} aktualności)
            </span>
          </div>

          <!-- Pagination controls -->
          <div class="flex items-center gap-2">
            <!-- Previous button -->
            <button
              :disabled="currentPage === 1"
              class="px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 dark:hover:bg-gray-700"
              :class="currentPage === 1 ? 'text-gray-400 dark:text-gray-600' : 'text-gray-700 dark:text-gray-300'"
              @click="goToPage(currentPage - 1)"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
            </button>

            <!-- Page numbers -->
            <div class="flex items-center gap-1">
              <button
                v-for="page in totalPages"
                :key="page"
                class="px-4 py-2 rounded-lg text-sm font-medium transition-colors"
                :class="page === currentPage
                  ? 'bg-blue-600 text-white'
                  : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'"
                @click="goToPage(page)"
              >
                {{ page }}
              </button>
            </div>

            <!-- Next button -->
            <button
              :disabled="currentPage === totalPages"
              class="px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 dark:hover:bg-gray-700"
              :class="currentPage === totalPages ? 'text-gray-400 dark:text-gray-600' : 'text-gray-700 dark:text-gray-300'"
              @click="goToPage(currentPage + 1)"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
